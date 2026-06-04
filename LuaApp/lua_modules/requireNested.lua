
-- This module overrides the require() function so that module namespaces can be used
-- when accessing modules during instantiation.
-- Example: A module is created under lib/LibraryX/ModuleY.lua. When you add the 'lib'
--          directory to the package.path as './lib/?.lua', you will be able to resolve
--          the directory structure and avoid naming conflicts when using the normal
--          'require()' function, by saying 'require("LibraryX.ModuleY").
--          But, when instantiating the object, the module would need to be accessed as
--          'ModuleY', i.e. 'local obj = ModuleY:new()'. This override allows you to
--          access the module as 'LibraryX.ModuleY:new()' without erroring out.
--
-- Author: Brandon Thompson
-- Creation Date: 2025-04-14

local oldrequire = require

require = function(moduleName)
    local requireReturn = {oldrequire(moduleName)} -- Store all return values
    local requiredModule = table.unpack(requireReturn)

    -- If we've come here, the require() was successful, so create the global table values.
    -- The default behavior of searchpath(), which is used by require(), is to use '.', so
    -- we don't need to worry about any other separator that a user of this function may use.
    local moduleNameParts = {}
    local index = 0
    for part in string.gmatch(moduleName, "[^%.]+")
    do
        moduleNameParts[index] = part
        index = index + 1
    end

    local currentTable = _G
    for i = 1, #moduleNameParts -- Start at child
    do
        local parentPackage = moduleNameParts[i - 1]
        if type(currentTable[parentPackage]) ~= "table" then -- Parent of first child will be true
            currentTable[parentPackage] = {}
        end
        currentTable = currentTable[parentPackage]
        if i == #moduleNameParts then
            local basePackage = moduleNameParts[i]
            currentTable[basePackage] = requiredModule
        end
    end

    return table.unpack(requireReturn) -- Return as it normally would
end
