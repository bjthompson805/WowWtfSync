
DataCombine = {}

function DataCombine:new()
    object = {}
    setmetatable(object, self)
    self.__index = self
    return object
end

function DataCombine:combine(wtfAccountDir, characterName, realm, account, addonName, jsonConfig)
    if (addonName == "Bagnon") then
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
    end

    return true
end

--[[
A simple function to print tables or to write tables into files.
Great for debugging but also for data storage.
When writing into files the 'return' keyword will be added automatically,
so the tables can be loaded with 'dofile()' into a variable.
The basic datatypes table, string, number, boolean and nil are supported.
The tables can be nested and have number and string indices.
This function has no protection when writing files without proper permissions and
when datatypes other then the supported ones are used.
--]]
-- t = table
-- f = filename [optional]
function DataCombine:printTable(t, f, variableName)

   local function printTableHelper(obj, cnt)

      local cnt = cnt or 0

      if type(obj) == "table" then

         io.write("\n", string.rep("\t", cnt), "{\n")
         cnt = cnt + 1

         for k,v in pairs(obj) do

            if type(k) == "string" then
               io.write(string.rep("\t",cnt), '["'..k..'"]', ' = ')
            end

            if type(k) == "number" then
               io.write(string.rep("\t",cnt), "["..k.."]", " = ")
            end

            printTableHelper(v, cnt)
            io.write(",\n")
         end

         cnt = cnt-1
         io.write(string.rep("\t", cnt), "}")

      elseif type(obj) == "string" then
         io.write(string.format("%q", obj))

      else
         io.write(tostring(obj))
      end 
   end

   if f == nil then
      printTableHelper(t)
   else
      io.output(f)
      io.write(variableName .. " = ")
      printTableHelper(t)
      io.output(io.stdout)
   end
end
