
-- Set path of 3rd party packages (lua_modules) and our modules (lib).
package.path = package.path .. ";.\\LuaApp\\lua_modules\\?.lua;.\\LuaApp\\lib\\?.lua"

require "requireNested" -- Allow for instantiation of modules in nested directories
require "DataCombine.DataCombineFactory"

-------------------------------------------------------------
-- Functions
-------------------------------------------------------------
function splitCharacter(character)
    local characterSplitIter = string.gmatch(character, "[^%-]+")
    local characterName = characterSplitIter()
    local realm = characterSplitIter()
    local account = characterSplitIter()
    return characterName, realm, account
end

--------------------------------------------------------------
-- Main
--------------------------------------------------------------
if (#arg ~= 3) then
    error(
        "\nUSAGE:\n" ..
        "\t" .. arg[0] .. " <addonName> <jsonConfigPath> <characterName>-<realm>-<account>\n" ..
        "\t" .. arg[0] .. " <addonName> <jsonConfigPath> all"
    )
    os.exit(1)
end

-- Create the object for the given addon
local addonName = arg[1]
local jsonConfigPath = arg[2]
local character = arg[3]
local dataCombineObjs = DataCombine.DataCombineFactory:create(addonName)
if dataCombineObjs == nil then
    error(
        "DataCombine.DataCombineFactory:create() failed for addon '" .. addonName ..
        "': " .. DataCombine.DataCombineFactory.errorMsg
    )
    os.exit(1)
end

-- Combine
for _, dataCombineObj in ipairs(dataCombineObjs) do
    if (character == "all") then
        local combinedCharacters = dataCombineObj:combineAll(jsonConfigPath)
        if (combinedCharacters == nil) then
            error(dataCombineObj.name .. ":combineAll() failed: " .. dataCombineObj.errorMsg)
            os.exit(1)
        end
    else
        local characterName, realm, account = splitCharacter(character)

        if (
            characterName == nil or
            realm == nil or
            account == nil
        ) then
            error("4th argument must be of the form '<characterName>-<realm>-<account>' or 'all'.")
            os.exit(1)
        end

        local success = dataCombineObj:combine(
            characterName,
            realm,
            account,
            jsonConfigPath
        )
        if (success ~= true) then
            error(dataCombineObj.name .. ":combine() failed: " .. dataCombineObj.errorMsg)
            os.exit(1)
        end
    end
end

-- Print the success message
if (character == "all") then
    print(
        "'" .. addonName .. "' data has been successfully combined for " ..
        "all characters by pushing from the character to all *other* accounts."
    )
else
    local characterName, realm, account = splitCharacter(character)
    print(string.format(
        "'%s' data for character '%s' on realm '%s' for account '%s' has been successfully " ..
        "combined to all other accounts.",
        addonName,
        characterName,
        realm,
        account
    ))
end
