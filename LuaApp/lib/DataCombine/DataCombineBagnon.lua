
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local me = DataCombine.DataCombine:extend("DataCombine.DataCombineBagnon")

function me:init() end -- Constructor

function me:combineOne(characterName, realm, sourceAccount, sourceStr, oldDestStr)
    -- Evaluate the source BagBrother file
    local sourceBagBrotherFn = assert(load(sourceStr))
    sourceBagBrotherFn()
    local sourceBrotherBags = BrotherBags

    -- Evaluate the destination BagBrother file
    destBagBrotherFn = assert(load(oldDestStr))
    destBagBrotherFn()
    local destBrotherBags = BrotherBags

    -- Combine
    destBrotherBags[realm][characterName] = sourceBrotherBags[realm][characterName]

    -- Return the new destination string
    local util = require("Util")
    local newDestStr = "BrotherBags = " .. util:printTable(destBrotherBags)
    return newDestStr
end

function me:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\BagBrother.lua"
end

return me
