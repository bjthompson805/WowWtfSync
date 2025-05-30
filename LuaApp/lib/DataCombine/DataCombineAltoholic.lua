
require "DataCombine.DataCombine"

-- Inherit from DataCombine.DataCombine
local thisClass = DataCombine.DataCombine:extend("DataCombine.DataCombineAltoholic")

-- Constructor
function thisClass:init(altoholicAddon)
    if (altoholicAddon == nil) then
        error("Altoholic addon name is required.")
        return nil
    end
    self.altoholicAddon = altoholicAddon
end

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
    local variableName = self.altoholicAddon .. "DB"

    -- Evaluate the source file
    local sourceFn = assert(load(sourceStr))
    sourceFn()
    local sourceVariable = _G[variableName]

    -- Evaluate the destination file
    destFn = assert(load(oldDestStr))
    destFn()
    local destVariable = _G[variableName]

    -- Combine the profileKey
    local profileKey = characterName .. " - " .. realm
    if (sourceVariable["profileKeys"] == nil or
        sourceVariable["profileKeys"][profileKey] == nil
    ) then
        self.errorMsg = "Source account '" .. sourceAccount .. "' does not contain character '" ..
            profileKey .. "' for the addon '" .. self.altoholicAddon .. "'. You may " ..
            "need to log onto the character first."
        return nil
    end
    if (destVariable["profileKeys"] == nil) then
        destVariable["profileKeys"] = {}
    end
    destVariable["profileKeys"][profileKey] = sourceVariable["profileKeys"][profileKey]
    
    -- Combine the character data
    if (self.altoholicAddon ~= "Altoholic") then -- Altoholic.lua only has profileKeys
        if (sourceVariable["global"] == nil or
            sourceVariable["global"]["Characters"] == nil
        ) then
            self.errorMsg = "Source account '" .. sourceAccount .. "' does not contain the " ..
                "expected keys for the addon '" .. self.altoholicAddon .. "'. You may " ..
                "need to log onto the character first."
            return nil
        end
        if (destVariable["global"] == nil) then
            destVariable["global"] = {}
        end
        if (destVariable["global"]["Characters"] == nil) then
            destVariable["global"]["Characters"] = {}
        end

        local key = "Default." .. realm .. "." .. characterName
        destVariable["global"]["Characters"][key] = sourceVariable["global"]["Characters"][key]
    end

    -- Return the new destination string
    local util = require("Util")
    local newDestStr = variableName .. " = " .. util:printTable(destVariable)
    return newDestStr
end

function thisClass:getPath(wtfAccountDir, account)
    return wtfAccountDir .. "\\" .. account .. "\\SavedVariables\\" .. self.altoholicAddon ..
        ".lua"
end

function thisClass:getAltoholicAddons()
    return {
        "Altoholic",
        "DataStore",
        "DataStore_Agenda",
        "DataStore_Auctions",
        "DataStore_Containers",
        "DataStore_Characters",
        "DataStore_Crafts",
        "DataStore_Currencies",
        "DataStore_Inventory",
        "DataStore_Mails",
        "DataStore_Quests",
        "DataStore_Reputations",
        "DataStore_Spells",
        "DataStore_Talents"
    }
end

function thisClass:enableCharacterAccountFileAggregation()
    if (self.altoholicAddon == "DataStore_Mails") then
        return true
    end

    return false
end

-- For DataStore_Mails, we need to aggregate the mails from other accounts
-- into the character's account prior to overwriting the other accounts.
-- This is because other accounts may contain mail that it knows the character has in transit,
-- due to sending it to the character, but the character's account does not know about it yet.
function thisClass:aggregateOne(
    characterName,
    characterRealm,
    characterAccount,
    characterFaction,
    characterAccountFileStr,
    otherAccount,
    otherAccountFileStr,
    configTable
)
    -- Evaluate the source file
    local characterAccountFileFn = assert(load(characterAccountFileStr))
    characterAccountFileFn()
    local characterAccountVariable = DataStore_MailsDB

    -- Evaluate the destination file
    otherAccountFileFn = assert(load(otherAccountFileStr))
    otherAccountFileFn()
    local otherAccountVariable = DataStore_MailsDB
    
    local key = "Default." .. characterRealm .. "." .. characterName
    if (characterAccountVariable["global"] == nil or
        characterAccountVariable["global"]["Characters"] == nil or
        characterAccountVariable["global"]["Characters"][key] == nil
    ) then
        -- Normally we'd throw an error here, but Altoholic does not add keys if the character
        -- has never seen mail, so this is normal behavior and there's nothing to do.
        return characterAccountFileStr
    end
    
    if (otherAccountVariable["global"] == nil or
        otherAccountVariable["global"]["Characters"] == nil or
        otherAccountVariable["global"]["Characters"][key] == nil or
        otherAccountVariable["global"]["Characters"][key]["Mails"] == nil
    ) then
        -- There are no mails for this character on the other account, so nothing to do.
        return characterAccountFileStr
    end

    local otherAccountMails = otherAccountVariable["global"]["Characters"][key]["Mails"]
    local characterAccountMails = characterAccountVariable["global"]["Characters"][key]["Mails"] or {}
    local characterLastUpdate = characterAccountVariable["global"]["Characters"][key]["lastUpdate"] or 0
    for _, otherAccountMail in ipairs(otherAccountMails) do
        for _, characterAccountMail in ipairs(characterAccountMails) do
            -- Check if the mail already exists in the destination.
            -- We assume that there will not be two identical mails (same itemID and count,
            -- or same amount of money) sent from both accounts to the other account
            -- within the same second. This algorithm does not support that scenario.
            if (otherAccountMail["sender"] == characterAccountMail["sender"] and
                otherAccountMail["itemID"] == characterAccountMail["itemID"] and
                otherAccountMail["count"] == characterAccountMail["count"] and
                otherAccountMail["money"] == characterAccountMail["money"] and
                otherAccountMail["lastCheck"] == characterAccountMail["lastCheck"]
            ) then
                -- Skip
                goto nextSourceMail
            end
        end
                
        -- Handle the case where a mail should not be copied from the other account
        -- into the character account due to the character's lastUpdate being
        -- greater than the other account's mail's "lastCheck" (+1 hour for transit time).
        local secondsInOneHour = 60 * 60
        if (otherAccountMail["lastCheck"] + secondsInOneHour > characterLastUpdate) then
            -- Copy to destination
            table.insert(characterAccountMails, otherAccountMail)
        end

        ::nextSourceMail::
    end

    characterAccountVariable["global"]["Characters"][key]["Mails"] = characterAccountMails

    -- Return the new character account file string
    local util = require("Util")
    local newCharacterAccountFileStr = "DataStore_MailsDB = " .. util:printTable(characterAccountVariable)
    return newCharacterAccountFileStr
end

return thisClass
