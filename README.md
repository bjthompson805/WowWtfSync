# How to build for a release

## MacOS
```
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --parallel
cpack -G DragNDrop
```

## Linux
```
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --parallel
mkdir -p release/WowWtfSync
cp -r {WowWtfSync,push.lua,LuaApp} release/WowWtfSync
cd release
tar -czvf WowWtfSync-1.0.0-linux.tar.gz release/WowWtfSync
```

## Windows

