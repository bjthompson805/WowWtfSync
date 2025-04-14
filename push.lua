
require ".\\LuaApp\\DataCombine\\DataCombine"

if (#arg ~= 5) then
    error("USAGE: " .. arg[0] .. " <wtfAccountDir> <characterName> <realm> <account> <addonName>")
    os.exit(1)
end

local wtfAccountDir = arg[1]
local characterName = arg[2]
local realm = arg[3]
local account = arg[4]
local addonName = arg[5]
local dataCombineObj = DataCombine:new()
dataCombineObj:combine(wtfAccountDir, characterName, realm, account, addonName)

print(string.format(
    "'%s' data for character '%s' on realm '%s' for account '%s' has been successfully " ..
    "combined to all other accounts.",
    addonName,
    characterName,
    realm,
    account
))