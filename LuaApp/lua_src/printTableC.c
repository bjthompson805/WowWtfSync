/*
 * printTableC.c
 *
 * Lua C extension providing a fast table serializer used by Util.lua.
 * Equivalent to the pure-Lua printTable(), but writes into a single
 * heap-allocated buffer using C recursion, avoiding Lua-VM overhead,
 * intermediate table allocations, and redundant string copies.
 *
 * Build: compile as a shared Lua module named "printTableC".
 * Usage: local printTableC = require "printTableC"
 *        local str = printTableC.printTable(someTable)
 */

#include <lua.h>
#include <lauxlib.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>

/* ── Dynamic string buffer ──────────────────────────────────────────────── */

#define SB_INIT_CAP (128 * 1024)  /* 128 KB initial allocation */

typedef struct {
    char   *data;
    size_t  len;
    size_t  cap;
    int     oom;   /* out-of-memory flag */
} StrBuf;

static void sb_init(StrBuf *sb) {
    sb->data = (char *)malloc(SB_INIT_CAP);
    sb->len  = 0;
    sb->cap  = sb->data ? SB_INIT_CAP : 0;
    sb->oom  = (sb->data == NULL);
}

static void sb_free(StrBuf *sb) {
    free(sb->data);
    sb->data = NULL;
}

static void sb_grow(StrBuf *sb, size_t need) {
    size_t newcap = sb->cap ? sb->cap * 2 : SB_INIT_CAP;
    while (newcap < sb->len + need) newcap *= 2;
    char *newdata = (char *)realloc(sb->data, newcap);
    if (!newdata) { sb->oom = 1; return; }
    sb->data = newdata;
    sb->cap  = newcap;
}

static inline void sb_append(StrBuf *sb, const char *s, size_t n) {
    if (sb->oom) return;
    if (sb->len + n > sb->cap) sb_grow(sb, n);
    if (sb->oom) return;
    memcpy(sb->data + sb->len, s, n);
    sb->len += n;
}

static inline void sb_appendc(StrBuf *sb, char c) {
    if (sb->oom) return;
    if (sb->len + 1 > sb->cap) sb_grow(sb, 1);
    if (sb->oom) return;
    sb->data[sb->len++] = c;
}

static inline void sb_appendz(StrBuf *sb, const char *s) {
    sb_append(sb, s, strlen(s));
}

/* ── Indent helper ──────────────────────────────────────────────────────── */

#define MAX_DEPTH  200
#define TABS_CHUNK 32

static const char TABS[TABS_CHUNK + 1] =
    "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t"
    "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";

static void add_indent(StrBuf *sb, int depth) {
    while (depth > 0) {
        int chunk = depth > TABS_CHUNK ? TABS_CHUNK : depth;
        sb_append(sb, TABS, (size_t)chunk);
        depth -= chunk;
    }
}

/* ── Forward declaration ────────────────────────────────────────────────── */

static void serialize_value(lua_State *L, StrBuf *sb, int idx, int depth);

/* ── String serialization ───────────────────────────────────────────────── */

static void serialize_string(lua_State *L, StrBuf *sb, int idx) {
    size_t      len;
    const char *s = lua_tolstring(L, idx, &len);
    size_t      i;
    int         needs_escape = 0;

    for (i = 0; i < len; i++) {
        unsigned char c = (unsigned char)s[i];
        if (c == '\\' || c == '\0' || c == '\n' || c == '\r' || c == '"') {
            needs_escape = 1;
            break;
        }
    }

    sb_appendc(sb, '"');
    if (!needs_escape) {
        sb_append(sb, s, len);
    } else {
        for (i = 0; i < len; i++) {
            unsigned char c = (unsigned char)s[i];
            switch (c) {
                case '\\': sb_appendz(sb, "\\\\"); break;
                case '\0': sb_appendz(sb, "\\000"); break;
                case '\n': sb_appendz(sb, "\\n");  break;
                case '\r': sb_appendz(sb, "\\r");  break;
                case '"':  sb_appendz(sb, "\\\""); break;
                default:   sb_appendc(sb, (char)c); break;
            }
        }
    }
    sb_appendc(sb, '"');
}

/* ── Table serialization ────────────────────────────────────────────────── */

static void serialize_table(lua_State *L, StrBuf *sb, int idx, int depth) {
    char buf[64];

    /* Normalise to absolute index: idx must remain valid after lua_next pushes
     * key+value onto the stack.  */
    if (idx < 0) idx = lua_gettop(L) + idx + 1;

    sb_appendc(sb, '\n');
    add_indent(sb, depth);
    sb_appendz(sb, "{\n");

    lua_pushnil(L);                   /* initial key */
    while (lua_next(L, idx) != 0) {  /* key @ -2, value @ -1 */
        int ktype = lua_type(L, -2);

        add_indent(sb, depth + 1);

        if (ktype == LUA_TSTRING) {
            size_t      klen;
            const char *k = lua_tolstring(L, -2, &klen);
            sb_appendz(sb, "[\"");
            sb_append(sb, k, klen);
            sb_appendz(sb, "\"] = ");
        } else if (ktype == LUA_TNUMBER) {
#if LUA_VERSION_NUM >= 503
            if (lua_isinteger(L, -2))
                snprintf(buf, sizeof(buf), "[%lld] = ",
                         (long long)lua_tointeger(L, -2));
            else
                snprintf(buf, sizeof(buf), "[%.14g] = ",
                         (double)lua_tonumber(L, -2));
#else
            {
                lua_Number  n  = lua_tonumber(L, -2);
                lua_Integer ni = (lua_Integer)n;
                if ((lua_Number)ni == n)
                    snprintf(buf, sizeof(buf), "[%lld] = ", (long long)ni);
                else
                    snprintf(buf, sizeof(buf), "[%.14g] = ", (double)n);
            }
#endif
            sb_appendz(sb, buf);
        }
        /* Keys of other types are silently skipped (matches pure-Lua behaviour). */

        serialize_value(L, sb, -1, depth + 1);
        sb_appendz(sb, ",\n");

        lua_pop(L, 1);  /* pop value; keep key for lua_next */
    }

    add_indent(sb, depth);
    sb_appendc(sb, '}');
}

/* ── Value dispatch ─────────────────────────────────────────────────────── */

static void serialize_value(lua_State *L, StrBuf *sb, int idx, int depth) {
    char buf[64];
    int  t = lua_type(L, idx);

    if (depth > MAX_DEPTH) { sb_appendz(sb, "nil"); return; }

    switch (t) {
        case LUA_TTABLE:
            serialize_table(L, sb, idx, depth);
            break;

        case LUA_TSTRING:
            serialize_string(L, sb, idx);
            break;

        case LUA_TNUMBER:
#if LUA_VERSION_NUM >= 503
            if (lua_isinteger(L, idx))
                snprintf(buf, sizeof(buf), "%lld",
                         (long long)lua_tointeger(L, idx));
            else
                snprintf(buf, sizeof(buf), "%.14g",
                         (double)lua_tonumber(L, idx));
#else
            {
                lua_Number  n  = lua_tonumber(L, idx);
                lua_Integer ni = (lua_Integer)n;
                if ((lua_Number)ni == n)
                    snprintf(buf, sizeof(buf), "%lld", (long long)ni);
                else
                    snprintf(buf, sizeof(buf), "%.14g", (double)n);
            }
#endif
            sb_appendz(sb, buf);
            break;

        case LUA_TBOOLEAN:
            sb_appendz(sb, lua_toboolean(L, idx) ? "true" : "false");
            break;

        default:
            sb_appendz(sb, "nil");
            break;
    }
}

/* ── Public Lua function ────────────────────────────────────────────────── */

static int l_printTable(lua_State *L) {
    StrBuf sb;

    luaL_checktype(L, 1, LUA_TTABLE);

    sb_init(&sb);
    if (sb.oom)
        return luaL_error(L, "printTableC: initial allocation failed");

    serialize_value(L, &sb, 1, 0);

    if (sb.oom) {
        sb_free(&sb);
        return luaL_error(L, "printTableC: out of memory during serialization");
    }

    lua_pushlstring(L, sb.data, sb.len);
    sb_free(&sb);
    return 1;
}

/* ── Module registration ────────────────────────────────────────────────── */

static const luaL_Reg printTableC_lib[] = {
    { "printTable", l_printTable },
    { NULL,         NULL         }
};

#if defined(__GNUC__) || defined(__clang__)
__attribute__((visibility("default")))
#endif
LUALIB_API int luaopen_printTableC(lua_State *L) {
    luaL_newlib(L, printTableC_lib);
    return 1;
}
