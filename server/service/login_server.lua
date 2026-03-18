local skynet = require "skynet"
local socket = require "skynet.socket"
local proto_loader = require "proto_loader"
local opcodes = require "opcodes"
local packet = require "packet"

local LOGIN_PORT = 8888
local GAME_HOST = "127.0.0.1"
local GAME_PORT = 9999

local tokens = {}
local player_id_seq = 0

local function generate_token()
    return string.format("%08x%08x", math.random(0, 0x7FFFFFFF), os.time())
end

local function handle_client(fd, addr)
    socket.start(fd)
    skynet.error(string.format("Login: connected fd=%d addr=%s", fd, addr))

    local header = socket.read(fd, 2)
    if not header or #header < 2 then
        skynet.error(string.format("Login: read header failed fd=%d", fd))
        socket.close(fd)
        return
    end

    local body_len = string.unpack(">I2", header)

    local body = socket.read(fd, body_len)
    if not body or #body < body_len then
        skynet.error(string.format("Login: read body failed fd=%d", fd))
        socket.close(fd)
        return
    end

    local opcode, msg, err = packet.unpack(body)
    if err then
        skynet.error(string.format("Login: unpack error fd=%d err=%s", fd, err))
        socket.close(fd)
        return
    end

    if opcode ~= opcodes.C2S_Login then
        skynet.error(string.format("Login: unexpected opcode=%d fd=%d", opcode, fd))
        socket.close(fd)
        return
    end

    local username = msg.username or "unknown"
    skynet.error(string.format("Login: user=%s fd=%d", username, fd))

    local token = generate_token()
    player_id_seq = player_id_seq + 1
    tokens[token] = {
        username = username,
        player_id = player_id_seq,
        time = os.time(),
    }

    local resp = packet.pack_packet(opcodes.S2C_Login, {
        result = 0,
        token = token,
        game_host = GAME_HOST,
        game_port = GAME_PORT,
    })
    socket.write(fd, resp)
    skynet.error(string.format("Login: success user=%s token=%s player_id=%d", username, token, player_id_seq))

    socket.close(fd)
end

local CMD = {}

function CMD.validate(token)
    local info = tokens[token]
    if info then
        tokens[token] = nil
        return info
    end
    return nil
end

skynet.start(function()
    local proto_dir = skynet.getenv("proto_dir")
    proto_loader.load(proto_dir)

    skynet.dispatch("lua", function(session, source, cmd, ...)
        local f = CMD[cmd]
        if f then
            skynet.ret(skynet.pack(f(...)))
        else
            skynet.error("Login: unknown cmd=" .. tostring(cmd))
        end
    end)

    math.randomseed(os.time())

    local listen_fd = socket.listen("0.0.0.0", LOGIN_PORT)
    skynet.error(string.format("Login server listening on port %d", LOGIN_PORT))
    socket.start(listen_fd, function(fd, addr)
        skynet.fork(handle_client, fd, addr)
    end)
end)
