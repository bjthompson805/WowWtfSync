
require ".\\LuaApp\\DataCombine\\DataCombine"
local wtfAccountDir = arg[1]
local addonName = arg[2]
local dataCombineObj = DataCombine:new()
dataCombineObj:combine()

print("Done")