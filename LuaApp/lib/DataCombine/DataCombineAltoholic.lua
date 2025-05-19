
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineAltoholic")

-- Constructor
function thisClass:init(altoholicAddon)
    if (altoholicAddon == nil) then
        error("Altoholic addon name is required.")
        return nil
    end
    self.altoholicAddon = altoholicAddon
end

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    local variableName = self.altoholicAddon .. "DB"

    -- Evaluate the source file
    local sourceFn = assert(load(sourceStr))
    sourceFn()
    local sourceVariable = _G[variableName]

    -- Evaluate the destination file
    destFn = assert(load(oldDestStr))
    destFn()
    local destVariable = _G[variableName]

    -- Combine the profileKey
    local profileKey = characterName .. " - " .. realm
    if (sourceVariable["profileKeys"] == nil or
        sourceVariable["profileKeys"][profileKey] == nil
    ) then
        self.errorMsg = "Source account '" .. sourceAccount .. "' does not contain character '" ..
            profileKey .. "' for the addon '" .. self.altoholicAddon .. "'. You may " ..
            "need to log onto the character first."
        return nil
    end
    if (destVariable["profileKeys"] == nil) then
        destVariable["profileKeys"] = {}
    end
    destVariable["profileKeys"][profileKey] = sourceVariable["profileKeys"][profileKey]
    
    -- Combine the character data
    if (self.altoholicAddon ~= "Altoholic") then -- Altoholic.lua only has profileKeys
        if (sourceVariable["global"] == nil or
            sourceVariable["global"]["Characters"] == nil
        ) then
            self.errorMsg = "Source account '" .. sourceAccount .. "' does not contain the " ..
                "expected keys for the addon '" .. self.altoholicAddon .. "'. You may " ..
                "need to log onto the character first."
            return nil
        end
        if (destVariable["global"] == nil) then
            destVariable["global"] = {}
        end
        if (destVariable["global"]["Characters"] == nil) then
            destVariable["global"]["Characters"] = {}
        end

        local key = "Default." .. realm .. "." .. characterName
        destVariable["global"]["Characters"][key] = sourceVariable["global"]["Characters"][key]
    end

    -- Return the new destination string
    local util = require("Util")
    local newDestStr = variableName .. " = " .. util:printTable(destVariable)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\" .. self.altoholicAddon ..
        ".lua"
end

function thisClass:getAltoholicAddons()
    return {
        "Altoholic",
        "DataStore",
        "DataStore_Agenda",
        "DataStore_Auctions",
        "DataStore_Containers",
        "DataStore_Characters",
        "DataStore_Crafts",
        "DataStore_Currencies",
        "DataStore_Inventory",
        "DataStore_Mails",
        "DataStore_Quests",
        "DataStore_Reputations",
        "DataStore_Spells",
        "DataStore_Talents"
    }
end

return thisClass
