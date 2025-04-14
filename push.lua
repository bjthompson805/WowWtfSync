
package.path = package.path .. ";.\\LuaApp\\lib\\?.lua"

require ".\\LuaApp\\DataCombine\\DataCombine"

if (#arg ~= 6) then
    error("USAGE: " .. arg[0] .. " <wtfAccountDir> <characterName> <realm> <account> <addonName> <jsonConfig>")
    os.exit(1)
end

local wtfAccountDir = arg[1]
local characterName = arg[2]
local realm = arg[3]
local account = arg[4]
local addonName = arg[5]
local jsonConfig = arg[6]
local dataCombineObj = DataCombine:new()
if (
    dataCombineObj:combine(
        wtfAccountDir,
        characterName,
        realm,
        account,
        addonName,
        jsonConfig
    )
) then
    print(string.format(
        "'%s' data for character '%s' on realm '%s' for account '%s' has been successfully " ..
        "combined to all other accounts.",
        addonName,
        characterName,
        realm,
        account
    ))
else
    error("DataCombine:combine() failed: " .. dataCombineObj.errorMsg)
    os.exit(1)
end