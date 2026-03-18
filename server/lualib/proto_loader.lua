local pb = require "pb"
local protoc = require "protoc"

local M = {}

function M.load(proto_dir)
    local p = protoc.new()
    p.include_imports = true

    local files = { "login.proto", "game.proto" }
    for _, file in ipairs(files) do
        local path = proto_dir .. "/" .. file
        local f = io.open(path, "r")
        if not f then
            error("Cannot open proto file: " .. path)
        end
        local content = f:read("*a")
        f:close()
        assert(p:load(content, file))
    end
end

return M
