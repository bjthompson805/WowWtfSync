# Getting started
1. Clone the repository
2. Install the Tauri dependencies + Rust [here](https://tauri.app/start/prerequisites/).
3. Install NPM (Node Package Manager) for your OS.
4. From the repo's directory, run `npm ci` to install the npm modules. Then run `npm run tauri dev` to start the app.

# How to build
## Windows
1. Download the Windows 64 bit lua EXE and DLL files and put lua.exe and lua.dll in C:\lua. This is so that the application has access to the Windows lua.exe without the user needing to install it. They will be bundled into the installers.
2. Download the Lua source code and put all the files in /src/ into C:\lua (you can also put them in C:\lua\src or C:\lua\include). This is so that the build process can compile printTableC.c.
3. Run `npm run tauri build`. This will build WowWtfSync.exe and printTableC.dll. It will also copy push.lua and LuaApp. It will also create the installers.
4. The files to distribute are the .msi and .exe files under src-tauri/target/release/bundle/.

## MacOS
1. Install lua with Homebrew: `brew install lua`
2. Run `npm run tauri build` to build the application, printTableC.so, and the installer.
3. The file to distribute is the .dmg file under src-tauri/target/release/bundle/.

## Linux
1. Install lua for your distro:
    1. Ubuntu:
        ```
        sudo apt update
        sudo apt install lua5.4 liblua5.4-dev
        ```
2. Run `npm run tauri build` to build the application, printTableC.so, and the installers.
3. The files to distribute are the .deb and .rpm files under src-tauri/target/release/bundle/.
