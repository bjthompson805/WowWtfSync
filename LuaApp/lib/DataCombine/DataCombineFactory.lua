
-- This is the factory design pattern for all addon-specific DataCombine modules.
-- It is used to create instances of the DataCombine class for each addon by passing
-- the case-insensitive addon name to the create() method.

require "DataCombine.DataCombineBagnon"

local class = require "30log"

local me = class("DataCombine.DataCombineFactory")

function me:create(addonName)
    local addonName = string.lower(addonName)
    if (addonName == "bagnon") then
        return DataCombine.DataCombineBagnon:new()
    else
        self.errorMsg = "Unknown addon name '" .. addonName .. "'"
        return nil
    end
end

return me
