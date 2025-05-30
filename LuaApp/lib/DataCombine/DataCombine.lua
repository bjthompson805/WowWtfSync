
-- This class represents the abstract base class of the addon-specific DataCombine modules.
-- Because there is no particular expectation for how the addons structure their data,
-- we will implement each addon separately into its own module. Even if the structure
-- is the same as another addon, we should copy and paste the code, in case the addon
-- ever changes it.

local class = require "30log"

local thisClass = class("DataCombine.DataCombine")

function thisClass:init() end -- Constructor

-- This method combines the given character into all other accounts listed in the jsonConfigPath.
function thisClass:combine(characterName, realm, sourceAccount, jsonConfigPath)
    -- Get all unique accounts listed in the config
    local util = require("Util")
    local configTable = util:getJsonConfigTable(jsonConfigPath)
    local uniqueAccounts = {}
    local faction = nil

    -- Get all unique accounts as well as the faction of the character
    for _, character in ipairs(configTable.AddedCharacters) do
        uniqueAccounts[character.Account] = true

        if (character.CharacterName == characterName and
            character.Realm == realm and
            character.Account == sourceAccount
        ) then
            faction = character.Faction
        end
    end
    uniqueAccounts[sourceAccount] = nil -- Remove the current account

    if (faction == nil) then
        self.errorMsg = "Could not determine faction for the character " ..
            "because the character is not in the config file '" .. jsonConfigPath ..
            "'."
        return false
    end
    
    -- Aggregate data from other accounts into the source account, if needed by the addon.
    if (self:enableCharacterAccountFileAggregation()) then
        local success = self:aggregateCharacterAccountFile(
            characterName,
            realm,
            sourceAccount,
            faction,
            configTable
        )
        if (success ~= true) then
            self.errorMsg = "Could not aggregate source character '" .. characterName ..
                "-" .. realm .. "-" .. sourceAccount .. "': " .. self.errorMsg
            return false
        end
    end

    -- Read the source file
    local wtfAccountDir = configTable.WowWtfFolder .. "\\Account"
    local sourcePath = self:getPath(wtfAccountDir, sourceAccount)
    local fh = io.open(sourcePath, "rb")
    if fh == nil then
        self.errorMsg = "Could not open file '" .. sourcePath .. "'"
        return false
    end
    local sourceStr = fh:read("*all")
    fh:close()
    
    -- Combine the given character into all *other* accounts
    for destAccount in pairs(uniqueAccounts) do
        -- Read the old destination file
        local destPath = self:getPath(wtfAccountDir, destAccount)
        local fh = io.open(destPath, "rb")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return false
        end
        local oldDestStr = fh:read("*all")
        fh:close()

        -- Combine the character from the source file into the destination file
        local newDestStr = self:combineOne(
            characterName,
            realm,
            sourceAccount,
            faction,
            sourceStr,
            oldDestStr,
            configTable,
            destAccount
        )
        if (newDestStr == nil) then
            self.errorMsg = self.name .. ":combineOne() failed for character '" ..
                characterName .. "-" .. realm .. "-" .. sourceAccount .. "': " ..
                self.errorMsg
            return false
        end

        -- Save the new destination file
        fh = io.open(destPath, "w")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return false
        end
        fh:write(newDestStr)
        fh:close()
    end

    return true
end

-- This method combines all characters listed in the jsonConfigPath into all other accounts.
function thisClass:combineAll(jsonConfigPath)
    -- Get all unique accounts listed in the config
    local util = require("Util")
    local configTable = util:getJsonConfigTable(jsonConfigPath)
    local uniqueAccounts = {}
    for _, character in ipairs(configTable.AddedCharacters) do
        uniqueAccounts[character.Account] = true
    end
    
    -- Aggregate data from other accounts into the source account, if needed by the addon.
    if (self:enableCharacterAccountFileAggregation()) then
        for _, sourceCharacter in ipairs(configTable.AddedCharacters) do
            local success = self:aggregateCharacterAccountFile(
                sourceCharacter.CharacterName,
                sourceCharacter.Realm,
                sourceCharacter.Account,
                sourceCharacter.Faction,
                configTable
            )
            if (success ~= true) then
                self.errorMsg = "Could not aggregate source character '" .. sourceCharacter.CharacterName ..
                    "-" .. sourceCharacter.Realm .. "-" .. sourceCharacter.Account .. "': " .. self.errorMsg
                return nil
            end
        end
    end
    
    local wtfAccountDir = configTable.WowWtfFolder .. "\\Account"
    local accountToNewDestStr = {}
    for account in pairs(uniqueAccounts) do
        local path = self:getPath(wtfAccountDir, account)
        local fh = io.open(path, "rb")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. path .. "'"
            return nil
        end
        local str = fh:read("*all")
        fh:close()

        accountToNewDestStr[account] = str
    end

    -- Combine all characters into all *other* accounts
    for _, sourceCharacter in ipairs(configTable.AddedCharacters) do
        local sourceStr = accountToNewDestStr[sourceCharacter.Account]
        for destAccount, oldDestStr in pairs(accountToNewDestStr) do
            -- Skip account for this character
            if sourceCharacter.Account == destAccount then goto continue end

            -- Combine
            local newDestStr = self:combineOne(
                sourceCharacter.CharacterName,
                sourceCharacter.Realm,
                sourceCharacter.Account,
                sourceCharacter.Faction,
                sourceStr,
                oldDestStr,
                configTable,
                destAccount
            )
            if (newDestStr == nil) then
                self.errorMsg = self.name .. ":combineOne() failed for character '" ..
                    sourceCharacter.CharacterName .. "-" .. sourceCharacter.Realm ..
                    "-" .. sourceCharacter.Account .. "': " .. self.errorMsg
                return nil
            end
            accountToNewDestStr[destAccount] = newDestStr

            ::continue::
        end
    end

    for account, newDestStr in pairs(accountToNewDestStr) do
        -- Save the new destination file
        local destPath = self:getPath(wtfAccountDir, account)
        local fh = io.open(destPath, "wb")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return nil
        end
        fh:write(newDestStr)
        fh:close()
    end

    return configTable.AddedCharacters
end

-- This method is the addon-specific logic for combining one character, and is used by
-- combine() and combineAll().
-- Input:
--     characterName [string] - the character being copied from
--     realm [string] - the realm of the character being copied from
--     sourceAccount [string] - the account of the character being copied from
--     faction [string] - the faction of the character being copied from
--     sourceStr [string] - the data of the account being copied from
--     oldDestStr [string] - the data of the account being copied to
--     configTable [table] - the config table from config.json
--     destAccount [string] - the account being copied to
-- Output:
--     newDestStr [string] - the new data of the account being copied to
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
    self.errorMsg = self.name .. ":combineOne() is an abstract method and must be implemented."
    return nil
end

-- This method returns the path to the addon's lua data file for the given account.
function thisClass:getPath(wtfAccountDir, account)
    self.errorMsg = self.name .. ":getPath() is an abstract method and must be implemented."
    return nil
end

-- This method implements logic to change the file of the account
-- that a character belongs to prior to having it copied to other accounts, by aggregating
-- the character's data on other accounts with its data on the account it belongs to.
-- This is occasionally used when a character's data is able to be changed while logged
-- onto a different account, such as in DataStore_Mails (Altoholic).
-- Input:
--     characterName [string] - the character being copied from
--     characterRealm [string] - the realm of the character being copied from
--     characterAccount [string] - the account of the character being copied from
--     characterFaction [string] - the faction of the character being copied from
--     configTable [table] - the config table from config.json
-- Output:
--     success [boolean] - indicates whether the update to the character account file was successful
function thisClass:aggregateCharacterAccountFile(
    characterName,
    characterRealm,
    characterAccount,
    characterFaction,
    configTable
)
    -- Get all unique accounts
    local otherAccounts = {}
    for _, character in ipairs(configTable.AddedCharacters) do
        otherAccounts[character.Account] = true
    end
    otherAccounts[characterAccount] = nil

    -- Read the character account file
    local wtfAccountDir = configTable.WowWtfFolder .. "\\Account"
    local characterAccountFilePath = self:getPath(wtfAccountDir, characterAccount)
    local fh = io.open(characterAccountFilePath, "rb")
    if fh == nil then
        self.errorMsg = "Could not open file '" .. characterAccountFilePath .. "'"
        return false
    end
    local characterAccountFileStr = fh:read("*all")
    fh:close()
    
    local newCharacterAccountFileStr = characterAccountFileStr
    for otherAccount in pairs(otherAccounts) do
        -- Read the other account file
        local otherAccountFilePath = self:getPath(wtfAccountDir, otherAccount)
        local fh = io.open(otherAccountFilePath, "rb")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. otherAccountFilePath .. "'"
            return false
        end
        local otherAccountFileStr = fh:read("*all")
        fh:close()

        -- Combine the character from the other account file into the character account
        -- file (reverse direction!).
        newCharacterAccountFileStr = self:aggregateOne(
            characterName,
            characterRealm,
            characterAccount,
            characterFaction,
            newCharacterAccountFileStr,
            otherAccount,
            otherAccountFileStr,
            configTable
        )
        if (newCharacterAccountFileStr == nil) then
            self.errorMsg = self.name .. ":aggregateOne() failed for character '" ..
                characterName .. "-" .. characterRealm .. "-" .. characterAccount .. "': " ..
                self.errorMsg
            return false
        end
    end

    -- Save the new character account file
    fh = io.open(characterAccountFilePath, "w")
    if fh == nil then
        self.errorMsg = "Could not open file '" .. characterAccountFilePath .. "'"
        return false
    end
    fh:write(newCharacterAccountFileStr)
    fh:close()

    return true
end

-- This method is used to determine whether the source character should be aggregated
-- before being copied to other accounts. By default, this is false, but can be overridden
-- by the addon-specific implementation.
function thisClass:enableCharacterAccountFileAggregation()
    return false
end

return thisClass
