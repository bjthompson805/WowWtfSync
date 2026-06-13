# How to build

## Windows
1. Download the Windows 64 bit lua EXE and DLL files and put lua.exe and lua.dll in C:\lua. This is so that the application has access to the Windows lua.exe without the user needing to install it.
2. Download the Lua source code and put all the files in /src/ into C:\lua (you can also put them in C:\lua\src or C:\lua\include). This is so that the build process can compile printTableC.c.
3. Run `npm run tauri build`. This will build WowWtfSync.exe and printTableC.dll/so. It will also copy push.lua and LuaApp.
4. The files to distribute are the .msi and .exe files under src-tauri/target/release/bundle/.
