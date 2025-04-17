
-- This class contains general utility functions associated with this specific application.

local class = require "30log"

local me = class("Util")

function me:getJsonConfigTable(jsonConfigPath)
    local fh = io.open(jsonConfigPath, "r")
    if fh == nil then
        self.errorMsg = "Could not open file '" .. jsonConfigPath .. "'"
        return false
    end
    local jsonConfigContent = fh:read("*all")
    fh:close()
    local json = require 'json'
    local configTable = json.decode(jsonConfigContent)

    return configTable
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
Taken from https://gist.github.com/marcotrosi/163b9e890e012c6a460a and modified.
--]]
-- t = table
-- f = filename [optional] (if omitted, a string is returned)
function me:printTable(t, f)
    local function printTableHelper(obj, cnt)
        local cnt = cnt or 0
        local tableStr = ""
        
        if type(obj) == "table" then
        
            tableStr = tableStr .. "\n" .. string.rep("\t", cnt) .. "{\n"
            cnt = cnt + 1
            
            for k,v in pairs(obj) do
                if type(k) == "string" then
                    tableStr = tableStr .. string.rep("\t",cnt) .. '["' .. k .. '"]' .. ' = '
                end
                
                if type(k) == "number" then
                    tableStr = tableStr .. string.rep("\t",cnt) .. "[" .. k .. "]" .. " = "
                end
                
                tableStr = tableStr .. printTableHelper(v, cnt)
                tableStr = tableStr .. ",\n"
            end
            
            cnt = cnt-1
            tableStr = tableStr .. string.rep("\t", cnt) .. "}"
        
        elseif type(obj) == "string" then
            tableStr = tableStr .. string.format("%q", obj)
        else
            tableStr = tableStr .. tostring(obj)
        end
        
        return tableStr
    end
    
    if f == nil then
        return printTableHelper(t)
    else
        local tableStr = printTableHelper(t)
        local fh = io.open(f, "w")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return false
        end
        fh:write("return ")
        fh:write(tableStr)
        fh:close()

        return true
    end
end

return me
