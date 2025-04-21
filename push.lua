
-- Set path of 3rd party packages (lua_modules) and our modules (lib).
package.path = package.path .. ";.\\LuaApp\\lua_modules\\?.lua;.\\LuaApp\\lib\\?.lua"

require "requireNested" -- Allow for instantiation of modules in nested directories
require "DataCombine.DataCombineFactory"

if (#arg ~= 3) then
    error(
        "\nUSAGE:\n" ..
        "\t" .. arg[0] .. " <addonName> <jsonConfigPath> <characterName>-<realm>-<account>\n" ..
        "\t" .. arg[0] .. " <addonName> <jsonConfigPath> all"
    )
    os.exit(1)
end

local addonName = arg[1]
local jsonConfigPath = arg[2]
local character = arg[3]
local dataCombineObj = DataCombine.DataCombineFactory:create(addonName)
if dataCombineObj == nil then
    error(
        "DataCombine.DataCombineFactory:create() failed for addon '" .. addonName ..
        "': " .. DataCombine.DataCombineFactory.errorMsg
    )
    os.exit(1)
end

if (character == "all") then
    local combinedCharacters = dataCombineObj:combineAll(jsonConfigPath)
    if (combinedCharacters ~= nil) then
        print(
            "'" .. addonName .. "' data has been successfully combined for " ..
            "all characters by pushing from the character to all *other* accounts:\n"
        )
        for _, combinedCharacter in ipairs(combinedCharacters)
        do
            print(string.format(
                "\t%s-%s = %s",
                combinedCharacter.CharacterName,
                combinedCharacter.Realm,
                combinedCharacter.Account
            ))
        end
    else
        error(dataCombineObj.name .. ":combineAll() failed: " .. dataCombineObj.errorMsg)
        os.exit(1)
    end
else
    local characterSplitIter = string.gmatch(character, "[^%-]+")
    local characterName = characterSplitIter()
    local realm = characterSplitIter()
    local account = characterSplitIter()

    if (
        characterName == nil or
        realm == nil or
        account == nil
    ) then
        error("4th argument must be of the form '<characterName>-<realm>-<account>' or 'all'.")
        os.exit(1)
    end

    if (
        dataCombineObj:combine(
            characterName,
            realm,
            account,
            jsonConfigPath
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
end
