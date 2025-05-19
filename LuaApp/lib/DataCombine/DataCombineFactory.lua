
-- This is the factory design pattern for all addon-specific DataCombine modules.
-- It is used to create instances of the DataCombine class for each addon by passing
-- the case-insensitive addon name to the create() method.

local class = require "30log"

local thisClass = class("DataCombine.DataCombineFactory")

-- This method creates an instance of the given module name.
-- Input:
--     addonName [string] - name of the addon
-- Output:
--     obj [object|table] - a class instance of the given module; for some classes,
--         multiple instances are created and returned in a table
function thisClass:create(addonName)
    local addonName = string.lower(addonName)
    if (addonName == "bagnon") then
        require "DataCombine.DataCombineBagnon"
        return DataCombine.DataCombineBagnon:new()
    elseif (addonName == "auctionator") then
        require "DataCombine.DataCombineAuctionator"
        return DataCombine.DataCombineAuctionator:new()
    elseif (addonName == "titangold") then
        require "DataCombine.DataCombineTitanGold"
        return DataCombine.DataCombineTitanGold:new()
    elseif (addonName == "novaworldbuffs") then
        require "DataCombine.DataCombineNovaWorldBuffs"
        return DataCombine.DataCombineNovaWorldBuffs:new()
    elseif (addonName == "altoholic") then
        require "DataCombine.DataCombineAltoholic"
        local altoholicAddons = DataCombine.DataCombineAltoholic:getAltoholicAddons()
        local altoholicAddonObjects = {}
        for _, altoholicAddon in ipairs(altoholicAddons) do
            local altoholicAddonObject = DataCombine.DataCombineAltoholic:new(altoholicAddon)
            table.insert(altoholicAddonObjects, altoholicAddonObject)
        end
        return altoholicAddonObjects
    else
        thisClass.errorMsg = "Unknown addon name '" .. addonName .. "'"
        return nil
    end
end

return thisClass
