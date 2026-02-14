
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineTitanGold")

function thisClass:init() end -- Constructor

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    -- Evaluate the source file
    local sourceTitanGoldFn = assert(load(sourceStr))
    sourceTitanGoldFn()
    local sourceTitanSettings = TitanSettings

    -- Evaluate the destination file
    destTitanGoldFn = assert(load(oldDestStr))
    destTitanGoldFn()
    local destTitanSettings = TitanSettings

    -- Combine
    local key = characterName .. "@" .. realm
    destTitanSettings["Players"][key] = sourceTitanSettings["Players"][key]

    -- Return the new destination string
    local util = require("Util")
    local newDestStr = "TitanSettings = " .. util:printTable(destTitanSettings)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\Titan.lua"
end

return thisClass
