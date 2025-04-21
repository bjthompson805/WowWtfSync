
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineBagnon")

function thisClass:init() end -- Constructor

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    -- Evaluate the source BagBrother file
    local sourceBagBrotherFn = assert(load(sourceStr))
    sourceBagBrotherFn()
    local sourceBrotherBags = BrotherBags

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
    local util = require("Util")
    local newDestStr = "BrotherBags = " .. util:printTable(destBrotherBags)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\BagBrother.lua"
end

return thisClass
