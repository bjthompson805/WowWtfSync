
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineTitanGold")

local util = require("Util")
local pathSeparator = string.sub(package.config, 1, 1)

function thisClass:init() end -- Constructor

function thisClass:combineOne(characterName, realm, sourceAccount, faction, sourceStr, oldDestStr)
    -- Evaluate the source file (cache result to avoid redundant load() calls
    -- when multiple characters share the same source account in combineAll).
    if self._cachedSourceStr ~= sourceStr then
        local sourceTitanGoldFn = assert(load(sourceStr))
        sourceTitanGoldFn()
        self._cachedSourceTable = TitanSettings
        self._cachedSourceStr = sourceStr
    end
    local sourceTitanSettings = self._cachedSourceTable

    -- Evaluate the destination file
    destTitanGoldFn = assert(load(oldDestStr))
    destTitanGoldFn()
    local destTitanSettings = TitanSettings

    -- Combine
    local key = characterName .. "@" .. realm
    destTitanSettings["Players"][key] = sourceTitanSettings["Players"][key]

    -- Return the new destination string
    local newDestStr = "TitanSettings = " .. util:printTable(destTitanSettings)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return table.concat(
        {
            wtfAccountDir,
            account,
            "SavedVariables",
            "Titan.lua"
        },
        pathSeparator
    )
end

return thisClass
