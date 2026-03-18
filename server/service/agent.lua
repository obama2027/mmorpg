local skynet = require "skynet"
local socketdriver = require "skynet.socketdriver"
local proto_loader = require "proto_loader"
local opcodes = require "opcodes"
local packet = require "packet"

local client_fd
local gate
local player_id
local player_name

local function send_to_client(opcode, msg)
    local body = packet.pack(opcode, msg)
    local data = string.pack(">s2", body)
    socketdriver.send(client_fd, data)
end

local MSG = {}

function MSG.heartbeat(msg)
    send_to_client(opcodes.S2C_Heartbeat, {})
end

local function dispatch_client(data)
    local opcode, msg, err = packet.unpack(data)
    if err then
        skynet.error(string.format("Agent[%d]: unpack error=%s", player_id, err))
        return
    end

    if opcode == opcodes.C2S_Heartbeat then
        MSG.heartbeat(msg)
    else
        skynet.error(string.format("Agent[%d]: unhandled opcode=%d", player_id, opcode))
    end
end

local CMD = {}

function CMD.start(conf)
    client_fd = conf.fd
    gate = conf.gate
    player_id = conf.player_id
    player_name = conf.player_name
    skynet.error(string.format("Agent[%d]: started name=%s fd=%d", player_id, player_name, client_fd))
end

function CMD.disconnect()
    skynet.error(string.format("Agent[%d]: client disconnected", player_id))
    skynet.exit()
end

skynet.start(function()
    local proto_dir = skynet.getenv("proto_dir")
    proto_loader.load(proto_dir)

    skynet.register_protocol {
        name = "client",
        id = skynet.PTYPE_CLIENT,
        unpack = function(msg, sz)
            return skynet.tostring(msg, sz)
        end,
    }

    skynet.dispatch("client", function(session, source, data)
        dispatch_client(data)
    end)

    skynet.dispatch("lua", function(session, source, cmd, ...)
        local f = CMD[cmd]
        if f then
            skynet.ret(skynet.pack(f(...)))
        else
            skynet.error("Agent: unknown cmd=" .. tostring(cmd))
        end
    end)
end)
