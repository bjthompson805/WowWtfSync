
-- Set path of 3rd party packages (lua_modules) and our modules (lib).
package.path = package.path .. ";.\\LuaApp\\lua_modules\\?.lua;.\\LuaApp\\lib\\?.lua"

require "requireNested" -- Allow for instantiation of modules in nested directories
require "DataCombine.DataCombineFactory"

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
local dataCombineObj = DataCombine.DataCombineFactory:create(addonName)
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
    error(dataCombineObj.name .. ":combine() failed: " .. dataCombineObj.errorMsg)
    os.exit(1)
end