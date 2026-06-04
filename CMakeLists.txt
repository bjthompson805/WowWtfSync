cmake_minimum_required(VERSION 3.16)
project(WowWtfSync VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

# ── Qt version detection ────────────────────────────────────────────────────
find_package(Qt6 QUIET COMPONENTS Core Widgets)
if(Qt6_FOUND)
    set(QT_VERSION_MAJOR 6)
    message(STATUS "Using Qt6")
else()
    find_package(Qt5 REQUIRED COMPONENTS Core Widgets)
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
    resources/resources.qrc
)

set(HEADERS
    src/CharacterEntry.h
    src/MainWindow.h
    src/ConfigManager.h
    src/Logger.h
    src/WtfScanner.h
    src/PushDialog.h
)

# ── Executable ───────────────────────────────────────────────────────────────
if(QT_VERSION_MAJOR EQUAL 6)
    qt_add_executable(WowWtfSync WIN32 MACOSX_BUNDLE ${SOURCES} ${HEADERS})
    target_link_libraries(WowWtfSync PRIVATE Qt6::Core Qt6::Widgets)
else()
    add_executable(WowWtfSync WIN32 MACOSX_BUNDLE ${SOURCES} ${HEADERS})
    target_link_libraries(WowWtfSync PRIVATE Qt5::Core Qt5::Widgets)
endif()

target_include_directories(WowWtfSync PRIVATE src)

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
