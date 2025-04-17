
-- This class represents the abstract base class of the addon-specific DataCombine modules.
-- Because there is no particular expectation for how the addons structure their data,
-- we will implement each addon separately into its own module. Even if the structure
-- is the same as another addon, we should copy and paste the code, in case the addon
-- ever changes it.

local class = require "30log"

local me = class("DataCombine.DataCombine")

function me:init() end -- Constructor

-- This method combines one character into all other accounts listed in the jsonConfigPath.
function me:combine(wtfAccountDir, characterName, realm, sourceAccount, jsonConfigPath)
    -- Get all unique accounts listed in the config
    local util = require("Util")
    local configTable = util:getJsonConfigTable(jsonConfigPath)
    local uniqueAccounts = {}
    for _, character in ipairs(configTable.AddedCharacters) do
        uniqueAccounts[character.Account] = true
    end
    uniqueAccounts[sourceAccount] = nil -- Remove the current account

    for destAccount in pairs(uniqueAccounts) do
        -- Read the old destination file
        local destPath = self:getPath(wtfAccountDir, destAccount)
        local fh = io.open(destPath, "r")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return false
        end
        local oldDestStr = fh:read("*all")
        fh:close()

        -- Read the source file
        local sourcePath = self:getPath(wtfAccountDir, sourceAccount)
        fh = io.open(sourcePath, "r")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return false
        end
        local sourceStr = fh:read("*all")
        fh:close()

        -- Combine the character from the source file into the destination file
        local newDestStr = self:combineOne(
            characterName,
            realm,
            sourceAccount,
            sourceStr,
            oldDestStr
        )

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
function me:combineAll(wtfAccountDir, jsonConfigPath)
    -- Get all unique accounts listed in the config
    local util = require("Util")
    local configTable = util:getJsonConfigTable(jsonConfigPath)
    local uniqueAccounts = {}
    for _, character in ipairs(configTable.AddedCharacters) do
        uniqueAccounts[character.Account] = true
    end
    
    local accountToNewDestStr = {}
    for account in pairs(uniqueAccounts) do
        local path = self:getPath(wtfAccountDir, account)
        local fh = io.open(path, "r")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. path .. "'"
            return nil
        end
        local str = fh:read("*all")
        fh:close()

        accountToNewDestStr[account] = str
    end

    -- Combine all characters into all *other* accounts
    for _,sourceCharacter in ipairs(configTable.AddedCharacters) do
        local sourceStr = accountToNewDestStr[sourceCharacter.Account]
        for destAccount, oldDestStr in pairs(accountToNewDestStr) do
            -- Skip account for this character
            if sourceCharacter.Account == destAccount then goto continue end

            -- Combine
            local newDestStr = self:combineOne(
                sourceCharacter.CharacterName,
                sourceCharacter.Realm,
                sourceCharacter.Account,
                sourceStr,
                oldDestStr
            )
            accountToNewDestStr[destAccount] = newDestStr

            ::continue::
        end
    end

    for account, newDestStr in pairs(accountToNewDestStr) do
        -- Save the new destination file
        local destPath = self:getPath(wtfAccountDir, account)
        local fh = io.open(destPath, "w")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return nil
        end
        fh:write(newDestStr)
        fh:close()
    end

    return configTable.AddedCharacters
end

function me:combineOne(characterName, realm, sourceAccount, sourceStr, oldDestStr)
    self.errorMsg = self.name .. ":combineOne() is an abstract method and must be implemented."
    return nil
end

function me:getPath(wtfAccountDir, account)
    self.errorMsg = self.name .. ":getPath() is an abstract method and must be implemented."
    return nil
end

return me
