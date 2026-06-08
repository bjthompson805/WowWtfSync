cmake_minimum_required(VERSION 3.21)
project(WowWtfSync VERSION 0.1.0 LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# ─── Dependencies via FetchContent ────────────────────────────────────────────
include(FetchContent)

# SDL3
FetchContent_Declare(
    SDL3
    GIT_REPOSITORY https://github.com/libsdl-org/SDL.git
    GIT_TAG        release-3.2.14
    GIT_SHALLOW    TRUE
)
set(SDL_SHARED OFF CACHE BOOL "" FORCE)
set(SDL_STATIC ON  CACHE BOOL "" FORCE)
set(SDL_TEST   OFF CACHE BOOL "" FORCE)
FetchContent_MakeAvailable(SDL3)

# Dear ImGui
FetchContent_Declare(
    imgui
    GIT_REPOSITORY https://github.com/ocornut/imgui.git
    GIT_TAG        v1.91.9b
    GIT_SHALLOW    TRUE
)
FetchContent_MakeAvailable(imgui)

# nlohmann/json  (MIT, header-only)
FetchContent_Declare(
    nlohmann_json
    GIT_REPOSITORY https://github.com/nlohmann/json.git
    GIT_TAG        v3.11.3
    GIT_SHALLOW    TRUE
)
set(JSON_BuildTests OFF CACHE BOOL "" FORCE)
set(JSON_Install    OFF CACHE BOOL "" FORCE)
FetchContent_MakeAvailable(nlohmann_json)

# stb_image  (public domain, single header)
set(STB_IMAGE_H "${CMAKE_BINARY_DIR}/stb_image.h")
if(NOT EXISTS "${STB_IMAGE_H}")
    file(DOWNLOAD
        "https://raw.githubusercontent.com/nothings/stb/master/stb_image.h"
        "${STB_IMAGE_H}"
        STATUS stb_status
    )
    list(GET stb_status 0 stb_err)
    if(stb_err)
        message(FATAL_ERROR "Failed to download stb_image.h: ${stb_status}")
    endif()
endif()

# cpp-httplib  (MIT, single header — HTTPS requires OpenSSL)
set(HTTPLIB_H "${CMAKE_BINARY_DIR}/httplib.h")
if(NOT EXISTS "${HTTPLIB_H}")
    file(DOWNLOAD
        "https://raw.githubusercontent.com/yhirose/cpp-httplib/master/httplib.h"
        "${HTTPLIB_H}"
        STATUS httplib_status
    )
    list(GET httplib_status 0 httplib_err)
    if(httplib_err)
        message(FATAL_ERROR "Failed to download httplib.h: ${httplib_status}")
    endif()
endif()

# OpenSSL  (required for HTTPS in cpp-httplib)
# Linux:   sudo apt install libssl-dev
# macOS:   brew install openssl  (then set OPENSSL_ROOT_DIR if needed)
# Windows: vcpkg install openssl  or  choco install openssl
find_package(OpenSSL REQUIRED)

# ─── Embed app icon PNG as a C byte array ─────────────────────────────────────
set(ICON_PNG "${CMAKE_SOURCE_DIR}/resources/app-icon-mac.png")
file(READ "${ICON_PNG}" ICON_HEX HEX)
string(LENGTH "${ICON_HEX}" ICON_HEX_LEN)
math(EXPR ICON_BYTE_COUNT "${ICON_HEX_LEN} / 2")
string(REGEX REPLACE "([0-9a-fA-F][0-9a-fA-F])" "0x\\1," ICON_BYTES "${ICON_HEX}")
file(WRITE "${CMAKE_BINARY_DIR}/AppIconData.h"
    "#pragma once\n"
    "static const unsigned char  kAppIconPng[]  = { ${ICON_BYTES} };\n"
    "static const unsigned int   kAppIconPngSize = ${ICON_BYTE_COUNT};\n"
)

# ─── ImGui static library (core + SDL3 backends) ──────────────────────────────
add_library(imgui_lib STATIC
    ${imgui_SOURCE_DIR}/imgui.cpp
    ${imgui_SOURCE_DIR}/imgui_demo.cpp
    ${imgui_SOURCE_DIR}/imgui_draw.cpp
    ${imgui_SOURCE_DIR}/imgui_tables.cpp
    ${imgui_SOURCE_DIR}/imgui_widgets.cpp
    ${imgui_SOURCE_DIR}/backends/imgui_impl_sdl3.cpp
    ${imgui_SOURCE_DIR}/backends/imgui_impl_sdlrenderer3.cpp
)
target_include_directories(imgui_lib PUBLIC
    ${imgui_SOURCE_DIR}
    ${imgui_SOURCE_DIR}/backends
)
target_link_libraries(imgui_lib PUBLIC SDL3::SDL3-static)

# ─── Application ──────────────────────────────────────────────────────────────
add_executable(WowWtfSync
    src/main.cpp
    src/App.cpp
    src/Config.cpp
    src/Logger.cpp
    src/PushDialog.cpp
    src/ApiClient.cpp
    src/AuthSession.cpp
    src/stb_impl.cpp
)
target_include_directories(WowWtfSync PRIVATE
    src
    ${CMAKE_BINARY_DIR}   # AppIconData.h + stb_image.h + httplib.h
)
target_compile_definitions(WowWtfSync PRIVATE CPPHTTPLIB_OPENSSL_SUPPORT)
target_link_libraries(WowWtfSync PRIVATE
    imgui_lib
    SDL3::SDL3-static
    nlohmann_json::nlohmann_json
    OpenSSL::SSL
    OpenSSL::Crypto
)

# Windows: embed .ico, hide console window, link extra SSL libs
if(WIN32)
    target_sources(WowWtfSync PRIVATE resources/app-icon.rc)
    set_target_properties(WowWtfSync PROPERTIES WIN32_EXECUTABLE TRUE)
    target_link_libraries(WowWtfSync PRIVATE Crypt32 Ws2_32)
endif()

# macOS: .app bundle + Security framework
if(APPLE)
    set(APP_ICNS "${CMAKE_SOURCE_DIR}/resources/app-icon.icns")
    set_source_files_properties(${APP_ICNS} PROPERTIES
        MACOSX_PACKAGE_LOCATION "Resources"
    )
    target_sources(WowWtfSync PRIVATE ${APP_ICNS})

    set_target_properties(WowWtfSync PROPERTIES
        MACOSX_BUNDLE                   TRUE
        MACOSX_BUNDLE_BUNDLE_NAME       "WowWtfSync"
        MACOSX_BUNDLE_BUNDLE_VERSION    "${PROJECT_VERSION}"
        MACOSX_BUNDLE_SHORT_VERSION_STRING "${PROJECT_VERSION}"
        MACOSX_BUNDLE_GUI_IDENTIFIER    "com.brandont.WowWtfSync"
        MACOSX_BUNDLE_ICON_FILE         "app-icon.icns"
    )

    target_link_libraries(WowWtfSync PRIVATE "-framework Security")
endif()

# ─── Lua C extension: printTableC ────────────────────────────────────────────
find_package(Lua 5.4)
if(LUA_FOUND)
    add_library(printTableC MODULE
        LuaApp/lua_src/printTableC.c
    )
    target_include_directories(printTableC PRIVATE ${LUA_INCLUDE_DIR})
    set_target_properties(printTableC PROPERTIES
        PREFIX ""
        LIBRARY_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/LuaApp/lua_modules"
    )
    if(APPLE)
        set_target_properties(printTableC PROPERTIES
            SUFFIX     ".so"
            LINK_FLAGS "-undefined dynamic_lookup"
        )
    elseif(WIN32)
        target_link_libraries(printTableC PRIVATE ${LUA_LIBRARIES})
    endif()
    install(TARGETS printTableC
        LIBRARY DESTINATION LuaApp/lua_modules
        RUNTIME DESTINATION LuaApp/lua_modules
    )
    message(STATUS "printTableC: Lua ${LUA_VERSION_STRING} found — C extension will be built.")
else()
    message(WARNING
        "Lua 5.4 development headers not found — printTableC will not be built. "
        "Install liblua5.4-dev (Linux), brew install lua@5.4 (macOS), or "
        "LuaForWindows (Windows) to enable the C extension.")
endif()

# ─── Install ──────────────────────────────────────────────────────────────────
install(TARGETS WowWtfSync
    BUNDLE  DESTINATION .
    RUNTIME DESTINATION .
)
