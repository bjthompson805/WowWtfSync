
DataCombine = {}

function DataCombine:new()
    object = {}
    setmetatable(object, self)
    self.__index = self
    return object
end

function DataCombine:combine()
    print("Combined")
end
