
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
        "\t" .. arg[0] .. " <addonName> <jsonConfigPath> all\n" ..
        "\t" .. arg[0] .. " all <jsonConfigPath> <characterName>-<realm>-<account>\n" ..
        "\t" .. arg[0] .. " all <jsonConfigPath> all\n" ..
        "\n"
    )
    os.exit(1)
end

local cliAddonName = string.lower(arg[1])
local jsonConfigPath = arg[2]
local character = arg[3]
local addonsToPush = {}
if (cliAddonName == "all") then
    addonsToPush = DataCombine.DataCombineFactory:getSupportedAddonNames()
else
    addonsToPush = { cliAddonName }
end

local addonPushes = {}
for _, addonToPush in ipairs(addonsToPush) do
    local dataCombineObjs = DataCombine.DataCombineFactory:create(addonToPush)
    local addonPushSuccess = false
    if dataCombineObjs == nil then
        table.insert(addonPushes, {
            addonName = addonToPush,
            status = "error",
            shortErrorMsg = DataCombine.DataCombineFactory.errorMsg,
            errorMsg = "DataCombine.DataCombineFactory:create() failed for addon '" .. addonToPush ..
                "': " .. DataCombine.DataCombineFactory.errorMsg
        })
        goto continue
    end

    -- Combine
    for _, dataCombineObj in ipairs(dataCombineObjs) do
        if (character == "all") then
            local combinedCharacters = dataCombineObj:combineAll(jsonConfigPath)
            if (combinedCharacters == nil) then
                table.insert(addonPushes, {
                    addonName = addonToPush,
                    status = "error",
                    shortErrorMsg = dataCombineObj.errorMsg,
                    errorMsg = dataCombineObj.name .. ":combineAll() failed: " .. dataCombineObj.errorMsg
                })
                goto continue
            end
        else
            local characterName, realm, account = splitCharacter(character)

            if (
                characterName == nil or
                realm == nil or
                account == nil
            ) then
                error("3rd argument must be of the form '<characterName>-<realm>-<account>' or 'all'.")
                os.exit(1)
            end

            local success = dataCombineObj:combine(
                characterName,
                realm,
                account,
                jsonConfigPath
            )
            if (success ~= true) then
                table.insert(addonPushes, {
                    addonName = addonToPush,
                    status = "error",
                    shortErrorMsg = dataCombineObj.errorMsg,
                    errorMsg = dataCombineObj.name .. ":combine() failed: " .. dataCombineObj.errorMsg
                })
                goto continue
            end
        end
    end

    table.insert(addonPushes, {
        addonName = addonToPush,
        status = "success"
    })

    ::continue::
end

-- If they only pushed 1 addon and it failed, then there were no successes, so the program entirely failed.
if (#addonPushes == 1 and addonPushes[1].status == "error") then
    error(addonPushes[1].errorMsg)
    os.exit(1)
end

-- Print the success message
local totalTime = string.format("%.2f", os.clock() - start)
if (character == "all") then
    if (cliAddonName == "all") then
        print(string.format(
            "All addon data has been successfully combined for the following addons for " ..
            "all enabled characters by pushing from the character to all *other* accounts " ..
            "(took %s seconds):\n\n",
            totalTime
        ))
        for _, addonPush in ipairs(addonPushes) do
            local statusMsg = addonPush.status == "success" and "Success" or "Failed: " .. addonPush.shortErrorMsg
            print(string.format("%s: %s", addonPush.addonName, statusMsg))
        end
    else
        print(string.format(
            "'%s' data has been successfully combined for " ..
            "all enabled characters by pushing from the character to all *other* accounts " ..
            "(took %s seconds).",
            cliAddonName:sub(1,1):upper() .. cliAddonName:sub(2),
            totalTime
        ))
    end
else
    local characterName, realm, account = splitCharacter(character)
    if (cliAddonName == "all") then
        print(string.format(
            "All addon data for character '%s' on realm '%s' for account '%s' has been successfully " ..
            "combined to all other accounts for the following addons (took %s seconds):\n\n",
            characterName,
            realm,
            account,
            totalTime
        ))
        for _, addonPush in ipairs(addonPushes) do
            local statusMsg = addonPush.status == "success" and "Success" or "Failed: " .. addonPush.shortErrorMsg
            print(string.format("%s: %s", addonPush.addonName, statusMsg))
        end
    else
        print(string.format(
            "%s data for character '%s' on realm '%s' for account '%s' has been successfully " ..
            "combined to all other accounts (took %s seconds).",
            cliAddonName:sub(1,1):upper() .. cliAddonName:sub(2),
            characterName,
            realm,
            account,
            totalTime
        ))
    end
end
