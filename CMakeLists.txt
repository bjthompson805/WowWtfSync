cmake_minimum_required(VERSION 3.16)
project(WowWtfSync VERSION 1.0.0 LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

# ── Qt version detection ────────────────────────────────────────────────────
find_package(Qt6 QUIET COMPONENTS Core Widgets Network)
if(Qt6_FOUND)
    set(QT_VERSION_MAJOR 6)
    message(STATUS "Using Qt6")
else()
    find_package(Qt5 REQUIRED COMPONENTS Core Widgets Network)
    set(QT_VERSION_MAJOR 5)
    message(STATUS "Using Qt5")
endif()

# ── Sources ─────────────────────────────────────────────────────────────────
set(SOURCES
    src/main.cpp
    src/MainWindow.cpp
    src/ConfigManager.cpp
    src/Logger.cpp
    src/WtfScanner.cpp
    src/PushDialog.cpp
    src/AuthManager.cpp
    src/LoginDialog.cpp
    src/AccountDialog.cpp
    resources/resources.qrc
)

set(HEADERS
    src/CharacterEntry.h
    src/MainWindow.h
    src/ConfigManager.h
    src/Logger.h
    src/WtfScanner.h
    src/PushDialog.h
    src/AuthManager.h
    src/LoginDialog.h
    src/AccountDialog.h
)

# ── Executable ───────────────────────────────────────────────────────────────
if(QT_VERSION_MAJOR EQUAL 6)
    qt_add_executable(WowWtfSync WIN32 MACOSX_BUNDLE ${SOURCES} ${HEADERS})
    target_link_libraries(WowWtfSync PRIVATE Qt6::Core Qt6::Widgets Qt6::Network)
else()
    add_executable(WowWtfSync WIN32 MACOSX_BUNDLE ${SOURCES} ${HEADERS})
    target_link_libraries(WowWtfSync PRIVATE Qt5::Core Qt5::Widgets Qt5::Network)
endif()

target_include_directories(WowWtfSync PRIVATE src)

# ── printTableC Lua C extension ─────────────────────────────────────────────
# Builds a native Lua module that replaces the pure-Lua printTable() serializer.
# Util.lua loads it with pcall(require, "printTableC") and falls back gracefully
# if the extension is not present.

find_package(Lua)
if(LUA_FOUND)
    message(STATUS "Lua headers: ${LUA_INCLUDE_DIR} (v${LUA_VERSION_STRING})")

    add_library(printTableC MODULE LuaApp/lua_src/printTableC.c)
    target_include_directories(printTableC PRIVATE ${LUA_INCLUDE_DIR})

    # No "lib" prefix — Lua's require expects "printTableC.so", not "libprintTableC.so"
    set_target_properties(printTableC PROPERTIES PREFIX "" OUTPUT_NAME "printTableC")

    if(APPLE)
        # macOS: do NOT link against Lua; the running interpreter provides its symbols.
        target_link_options(printTableC PRIVATE -undefined dynamic_lookup)
        set_target_properties(printTableC PROPERTIES SUFFIX ".so")
    elseif(WIN32)
        # Windows: link the Lua import library so the DLL resolves symbols at load time.
        target_link_libraries(printTableC PRIVATE ${LUA_LIBRARIES})
        set_target_properties(printTableC PROPERTIES SUFFIX ".dll")
    else()
        # Linux: the interpreter provides symbols at runtime; no explicit link needed.
        set_target_properties(printTableC PROPERTIES SUFFIX ".so")
    endif()

    # After building, copy the module into the lua_modules directory where push.lua
    # can find it via package.cpath.
    add_custom_command(TARGET printTableC POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E make_directory
            "${CMAKE_CURRENT_BINARY_DIR}/LuaApp/lua_modules"
        COMMAND ${CMAKE_COMMAND} -E copy
            "$<TARGET_FILE:printTableC>"
            "${CMAKE_CURRENT_BINARY_DIR}/LuaApp/lua_modules/"
        COMMENT "Deploying printTableC to LuaApp/lua_modules/"
    )
else()
    message(WARNING
        "Lua development headers not found — printTableC will not be built. "
        "Install lua-dev (or equivalent) and re-run cmake to enable it. "
        "Util.lua will fall back to the pure-Lua serializer automatically.")
endif()

# ── Lua assets (push.lua + LuaApp) ──────────────────────────────────────────
# These files are copied into the build directory so the executable can find
# them at runtime without any installer.
# Pass -DLUA_ASSET_SRC=/path/to/WowWtfSync at configure time.

set(LUA_ASSET_SRC "" CACHE PATH "Path to the WowWtfSync Lua asset folder (contains push.lua and LuaApp/)")

if(LUA_ASSET_SRC AND EXISTS "${LUA_ASSET_SRC}/push.lua")
    file(COPY ${LUA_ASSET_SRC}/push.lua DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
    file(COPY ${LUA_ASSET_SRC}/LuaApp   DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
    message(STATUS "Lua assets copied from: ${LUA_ASSET_SRC}")
else()
    message(WARNING
        "LUA_ASSET_SRC not set or push.lua not found. "
        "Pass -DLUA_ASSET_SRC=/path/to/WowWtfSync to include Lua assets.")
endif()

# ── macOS bundle settings ────────────────────────────────────────────────────
if(APPLE)
    set_target_properties(WowWtfSync PROPERTIES
        MACOSX_BUNDLE_GUI_IDENTIFIER com.wowwtfsync.app
        MACOSX_BUNDLE_BUNDLE_VERSION ${PROJECT_VERSION}
        MACOSX_BUNDLE_SHORT_VERSION_STRING ${PROJECT_VERSION}
    )
endif()
