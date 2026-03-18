local pb = require "pb"
local opcodes = require "opcodes"

local M = {}

--- Unpack body (without 2-byte length header) into opcode + decoded message.
--- body = [2-byte opcode BE] [N-byte protobuf payload]
function M.unpack(body)
    if #body < 2 then
        return nil, nil, "body too short"
    end
    local opcode = string.unpack(">I2", body)
    local proto_name = opcodes.proto_name[opcode]
    if not proto_name then
        return opcode, nil, "unknown opcode: " .. tostring(opcode)
    end
    local payload = body:sub(3)
    if #payload > 0 then
        local msg = pb.decode(proto_name, payload)
        return opcode, msg
    else
        return opcode, {}
    end
end

--- Encode opcode + message into body (without 2-byte length header).
--- Returns: [2-byte opcode BE] [N-byte protobuf payload]
function M.pack(opcode, msg)
    local proto_name = opcodes.proto_name[opcode]
    local payload = ""
    if msg and proto_name then
        payload = pb.encode(proto_name, msg)
    end
    return string.pack(">I2", opcode) .. payload
end

--- Encode a full network packet: [2-byte length BE] [body]
function M.pack_packet(opcode, msg)
    local body = M.pack(opcode, msg)
    return string.pack(">s2", body)
end

return M
