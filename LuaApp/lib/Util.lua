
-- This class contains general utility functions associated with this specific application.

local class = require "30log"

-- Attempt to load the C extension for printTable. If the extension is present
-- (built via CMake and placed in LuaApp/lua_modules/), it is used automatically;
-- otherwise the pure-Lua implementation below serves as the fallback.
local _printTableC_ok, _printTableC = pcall(require, "printTableC")
if not _printTableC_ok then
    print("printTableC load failed: " .. tostring(_printTableC))
end

local thisClass = class("Util")

function thisClass:getJsonConfigTable(jsonConfigPath)
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
function thisClass:printTable(t, f)
    local serialized
    if _printTableC_ok then
        -- Fast path: C extension (single heap buffer, no Lua-VM overhead).
        serialized = _printTableC.printTable(t)
    else
        print("46")
        -- Fallback: pure-Lua implementation.
        local function printTableHelper(obj, cnt, out)
            cnt = cnt or 0
            out = out or {}

            if type(obj) == "table" then

                local indent = string.rep("\t", cnt)
                table.insert(out, "\n" .. indent .. "{\n")
                cnt = cnt + 1

                for k,v in pairs(obj) do
                    if type(k) == "string" then
                        table.insert(out, indent .. '["' .. k .. '"]' .. ' = ')
                    elseif type(k) == "number" then
                        table.insert(out, indent .. "[" .. k .. "] = ")
                    end

                    printTableHelper(v, cnt, out)
                    table.insert(out, ",\n")
                end

                cnt = cnt - 1
                table.insert(out, indent .. "}")

            elseif type(obj) == "string" then
                if obj:find('[\\%z\n\r"]') then
                    obj = string.gsub(obj, "\\", "\\\\")
                    obj = string.gsub(obj, "\000", "\\000")
                    obj = string.gsub(obj, "\n", "\\n")
                    obj = string.gsub(obj, "\r", "\\r")
                    obj = string.gsub(obj, '"', '\\"')
                end
                table.insert(out, '"' .. obj .. '"')
            else
                table.insert(out, tostring(obj))
            end

            return out
        end

        serialized = table.concat(printTableHelper(t))
    end

    if f == nil then
        return serialized
    else
        local fh = io.open(f, "w")
        if fh == nil then
            self.errorMsg = "Could not open file '" .. destPath .. "'"
            return false
        end
        fh:write("return ")
        fh:write(serialized)
        fh:close()

        return true
    end
end

return thisClass
