
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineAuctionator")

function thisClass:init() end -- Constructor

-- This method will copy any item scan days that exist in the source file but not in the
-- destination file.
function thisClass:combineOne(
    characterName,
    realm,
    sourceAccount,
    faction,
    sourceStr,
    oldDestStr,
    configTable,
    destAccount
)
    -- Return the destination string if the source file has already been combined
    -- for this realm, faction, and account.
    local alreadyCombinedKey = realm .. "_" .. faction .. "_" .. sourceAccount ..
        "_" .. destAccount
    if (self.alreadyCombined == nil) then
        self.alreadyCombined = {}
    end
    if (self.alreadyCombined[alreadyCombinedKey] == true) then
        return oldDestStr
    end

    -- Evaluate the source file
    local sourceAuctionatorFn = assert(load(sourceStr))
    sourceAuctionatorFn()
    local sourceAuctionatorPriceDatabase = AUCTIONATOR_PRICE_DATABASE

    -- Evaluate the destination file
    destAuctionatorFn = assert(load(oldDestStr))
    destAuctionatorFn()
    local destAuctionatorPriceDatabase = AUCTIONATOR_PRICE_DATABASE
    local extraVariables = {
        "AUCTIONATOR_CONFIG",
        "AUCTIONATOR_SAVEDVARS",
        "AUCTIONATOR_SHOPPING_LISTS",
        "AUCTIONATOR_POSTING_HISTORY",
        "AUCTIONATOR_VENDOR_PRICE_CACHE",
        "AUCTIONATOR_RECENT_SEARCHES",
        "AUCTIONATOR_SELLING_GROUPS"
    }
    local destAuctionatorExtraVariables = {}
    for _, varName in ipairs(extraVariables) do
        destAuctionatorExtraVariables[varName] = _G[varName]
    end

    local priceDbKey = realm .. " " .. faction
    local cbor = require "LibCBOR"
    if (type(destAuctionatorPriceDatabase[priceDbKey]) == "string") then
        -- Needs to be deserialized
        destAuctionatorPriceDatabase[priceDbKey] = cbor:Deserialize(destAuctionatorPriceDatabase[priceDbKey])
    end
    if (type(sourceAuctionatorPriceDatabase[priceDbKey]) == "string") then
        -- Needs to be deserialized
        sourceAuctionatorPriceDatabase[priceDbKey] = cbor:Deserialize(sourceAuctionatorPriceDatabase[priceDbKey])
    end

    if (type(sourceAuctionatorPriceDatabase[priceDbKey]) ~= "table") then
        -- Price database is empty, so skip it
        return oldDestStr
    end

    for itemID, sourceItemData in pairs(sourceAuctionatorPriceDatabase[priceDbKey]) do
        if (destAuctionatorPriceDatabase[priceDbKey] == nil) then
            destAuctionatorPriceDatabase[priceDbKey] = {}
        end

        if (destAuctionatorPriceDatabase[priceDbKey][itemID] == nil) then
            destAuctionatorPriceDatabase[priceDbKey][itemID] = sourceItemData
        else
            -- Combine the item data
            local destItemData = destAuctionatorPriceDatabase[priceDbKey][itemID]
            local destItemDataDeserialized = destItemData
            local sourceItemDataDeserialized = sourceItemData
            if (type(destItemDataDeserialized) == "string") then
                -- Needs to be deserialized
                destItemDataDeserialized = cbor:Deserialize(destItemDataDeserialized)
            end
            if (type(sourceItemDataDeserialized) == "string") then
                -- Needs to be deserialized
                sourceItemDataDeserialized = cbor:Deserialize(sourceItemDataDeserialized)
            end

            if (type(sourceItemDataDeserialized) ~= "table" or
                type(destItemDataDeserialized) ~= "table"
            ) then
                -- If the source or destination item data is not a table, then skip it
                -- as it is not valid.
                goto nextItem
            end

            local destMostRecentDay = self:getMostRecentDay(destItemDataDeserialized)
            local sourceMostRecentDay = self:getMostRecentDay(sourceItemDataDeserialized)

            -- Iterate through "a" (highest quantity seen). If it's not in the
            -- destination data, then add it, "h" (highest low price), and "l"
            -- (lowest low price) if it exists.
            for day, sourceHighestQuantitySeen in pairs(sourceItemDataDeserialized["a"]) do
                if (destItemDataDeserialized["a"][day] == nil) then
                    destItemDataDeserialized["a"][day] = sourceHighestQuantitySeen
                    destItemDataDeserialized["h"][day] = sourceItemDataDeserialized["h"][day]
                    if (sourceItemDataDeserialized["l"][day] ~= nil) then
                        destItemDataDeserialized["l"][day] = sourceItemDataDeserialized["l"][day]
                    end
                end
            end
            
            -- If the source data is more recent, then update the "m" (last seen minimum price).
            if (sourceMostRecentDay > destMostRecentDay) then
                destItemDataDeserialized["m"] = sourceItemDataDeserialized["m"]
            end

            local newDestItemData = destItemDataDeserialized
            if (type(destItemData) == "string") then
                -- Needs to be serialized
                newDestItemData = cbor:Serialize(newDestItemData)
            end

            destAuctionatorPriceDatabase[priceDbKey][itemID] = newDestItemData
        end

        ::nextItem::
    end

    -- Return the new destination string
    local util = require("Util")
    local newDestStr = "AUCTIONATOR_PRICE_DATABASE = " ..
        util:printTable(destAuctionatorPriceDatabase) .. "\n"
    for varName, varValue in pairs(destAuctionatorExtraVariables) do
        newDestStr = newDestStr .. varName .. " = " .. util:printTable(varValue) .. "\n"
    end
    self.alreadyCombined[alreadyCombinedKey] = true
    return newDestStr
end

function thisClass:getMostRecentDay(itemData)
    local mostRecentDay = 0
    for scanDay in pairs(itemData["a"]) do
        local scanDayInt = tonumber(scanDay)
        if (scanDayInt > mostRecentDay) then
            mostRecentDay = scanDayInt
        end
    end
    return mostRecentDay
end

function thisClass:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\Auctionator.lua"
end

return thisClass
