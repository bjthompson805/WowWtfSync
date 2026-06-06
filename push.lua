
local start = os.clock()

-- Derive the directory containing push.lua from arg[0] so that all module
-- paths are resolved relative to the script itself, not the working directory.
-- arg[0] is the absolute path passed by PushDialog: /path/to/build/push.lua
local pathSeparator = string.sub(package.config, 1, 1)
local scriptDir = (arg[0] or ""):match("^(.*[/\\])") or ("." .. pathSeparator)

-- Set path of 3rd party packages (lua_modules) and our modules (lib).
package.path = package.path
    .. ";" .. scriptDir .. "LuaApp" .. pathSeparator .. "lua_modules" .. pathSeparator .. "?.lua"
    .. ";" .. scriptDir .. "LuaApp" .. pathSeparator .. "lib"         .. pathSeparator .. "?.lua"

-- Add C module search path for printTableC and any future C extensions.
-- Both .so (Linux/macOS) and .dll (Windows) are listed so the correct one
-- is found automatically regardless of platform.
package.cpath = package.cpath
    .. ";" .. scriptDir .. "LuaApp" .. pathSeparator .. "lua_modules" .. pathSeparator .. "?.so"
    .. ";" .. scriptDir .. "LuaApp" .. pathSeparator .. "lua_modules" .. pathSeparator .. "?.dll"

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
local addonName = arg[1]:sub(1,1):upper() .. arg[1]:sub(2)
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
local totalTime = string.format("%.2f", os.clock() - start)
if (character == "all") then
    print(
        "'" .. addonName .. "' data has been successfully combined for " ..
        "all enabled characters by pushing from the character to all *other* accounts (took " .. totalTime .. " seconds)."
    )
else
    local characterName, realm, account = splitCharacter(character)
    print(string.format(
        "'%s' data for character '%s' on realm '%s' for account '%s' has been successfully " ..
        "combined to all other accounts (took %s seconds).",
        addonName,
        characterName,
        realm,
        account,
        totalTime
    ))
end
