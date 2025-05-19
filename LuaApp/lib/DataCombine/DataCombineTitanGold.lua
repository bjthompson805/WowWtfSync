
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineTitanGold")

function thisClass:init() end -- Constructor

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    -- Evaluate the source file
    local sourceTitanGoldFn = assert(load(sourceStr))
    sourceTitanGoldFn()
    local sourceGoldSave = GoldSave

    -- Evaluate the destination file
    destTitanGoldFn = assert(load(oldDestStr))
    destTitanGoldFn()
    local destGoldSave = GoldSave

    -- Combine
    local key = characterName .. "_" .. realm .. "::" .. faction
    destGoldSave[key] = sourceGoldSave[key]

    -- Return the new destination string
    local util = require("Util")
    local newDestStr = "GoldSave = " .. util:printTable(destGoldSave)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\TitanGold.lua"
end

return thisClass
