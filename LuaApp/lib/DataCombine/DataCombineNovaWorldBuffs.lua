
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineNovaWorldBuffs")

local util = require("Util")
local pathSeparator = string.sub(package.config, 1, 1)

function thisClass:init() end -- Constructor

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    -- Evaluate the source file (cache result to avoid redundant load() calls
    -- when multiple characters share the same source account in combineAll).
    if self._cachedSourceStr ~= sourceStr then
        local sourceNovaWorldBuffsFn = assert(load(sourceStr))
        sourceNovaWorldBuffsFn()
        self._cachedSourceTable = NWBdatabase
        self._cachedSourceStr = sourceStr
    end
    local sourceNWBdatabase = self._cachedSourceTable

    -- Evaluate the destination file
    destNovaWorldBuffsFn = assert(load(oldDestStr))
    destNovaWorldBuffsFn()
    local destNWBdatabase = NWBdatabase

    -- Combine
    if (sourceNWBdatabase["global"] == nil or
        sourceNWBdatabase["global"][realm] == nil or
        sourceNWBdatabase["global"][realm][faction] == nil or
        sourceNWBdatabase["global"][realm][faction]["myChars"] == nil
    ) then
        self.errorMsg = "Source faction '" .. realm .. "-" .. faction .. "' not " ..
            "found in the file for account '" .. sourceAccount .."'. You may need " ..
            "to log onto the character first."
        return nil
    end
    if (destNWBdatabase["global"] == nil) then
        destNWBdatabase["global"] = {}
    end
    if (destNWBdatabase["global"][realm] == nil) then
        destNWBdatabase["global"][realm] = {}
    end
    if (destNWBdatabase["global"][realm][faction] == nil) then
        destNWBdatabase["global"][realm][faction] = {}
    end
    if (destNWBdatabase["global"][realm][faction]["myChars"] == nil) then
        destNWBdatabase["global"][realm][faction]["myChars"] = {}
    end
    destNWBdatabase["global"][realm][faction]["myChars"][characterName] =
        sourceNWBdatabase["global"][realm][faction]["myChars"][characterName]

    -- Return the new destination string
    local newDestStr = "NWBdatabase = " .. util:printTable(destNWBdatabase)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return table.concat(
        {
            wtfAccountDir,
            account,
            "SavedVariables",
            "NovaWorldBuffs.lua"
        },
        pathSeparator
    )
end

return thisClass
