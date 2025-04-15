
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local me = DataCombine.DataCombine:extend("DataCombine.DataCombineBagnon")

function me:init() end -- Constructor

function me:combine(wtfAccountDir, characterName, realm, account, addonName, jsonConfig)
    -- Evaluate the source BagBrother file
    local bagBrotherPath = wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\BagBrother.lua"
    local bagBrotherFn = assert(loadfile(bagBrotherPath))
    bagBrotherFn()
    local sourceBrotherBags = BrotherBags

    -- Get all unique accounts listed in the config
    local fh = io.open(jsonConfig, "r")
    if fh == nil then
        self.errorMsg = "Could not open file '" .. jsonConfig .. "'"
        return false
    end
    local jsonConfigContent = fh:read("*all")
    fh:close()
    local json = require 'json'
    local configTable = json.decode(jsonConfigContent)
    local uniqueAccounts = {}
    for _, character in ipairs(configTable) do
        uniqueAccounts[character.Account] = true
    end
    uniqueAccounts[account] = nil -- Remove the current account

    for account in pairs(uniqueAccounts) do
        -- Evaluate the destination BagBrother file
        bagBrotherPath = wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\BagBrother.lua"
        bagBrotherFn = assert(loadfile(bagBrotherPath))
        bagBrotherFn()
        local destBrotherBags = BrotherBags

        -- Combine
        destBrotherBags[realm][characterName] = sourceBrotherBags[realm][characterName]

        -- Save file
        self:printTable(destBrotherBags, bagBrotherPath, "BrotherBags")
    end

    return true
end

return me
