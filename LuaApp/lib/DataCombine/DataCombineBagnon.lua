
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineBagnon")

local util = require("Util")
local pathSeparator = string.sub(package.config, 1, 1)

function thisClass:init() end -- Constructor

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    -- Evaluate the source BagBrother file (cache result to avoid redundant load() calls
    -- when multiple characters share the same source account in combineAll).
    if self._cachedSourceStr ~= sourceStr then
        local sourceBagBrotherFn = assert(load(sourceStr))
        sourceBagBrotherFn()
        self._cachedSourceTable = BrotherBags
        self._cachedSourceStr = sourceStr
    end
    local sourceBrotherBags = self._cachedSourceTable

    -- Evaluate the destination BagBrother file
    destBagBrotherFn = assert(load(oldDestStr))
    destBagBrotherFn()
    local destBrotherBags = BrotherBags

    -- Combine
    if (sourceBrotherBags[realm] == nil) then
        self.errorMsg = "Source realm '" .. realm .. "' not found in the file for account '" ..
            sourceAccount .."'. You may need to log onto the character first."
        return nil
    end
    if (destBrotherBags[realm] == nil) then
        destBrotherBags[realm] = {}
    end
    destBrotherBags[realm][characterName] = sourceBrotherBags[realm][characterName]

    -- Return the new destination string
    local newDestStr = "BrotherBags = " .. util:printTable(destBrotherBags)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return table.concat(
        {
            wtfAccountDir,
            account,
            "SavedVariables",
            "BagBrother.lua"
        },
        pathSeparator
    )
end

return thisClass
