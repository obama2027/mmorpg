local skynet = require "skynet"
local socketdriver = require "skynet.socketdriver"
local proto_loader = require "proto_loader"
local opcodes = require "opcodes"
local packet = require "packet"

local GAME_PORT = 9999

local gate
local login_server
local connections = {}
local agents = {}

local function send_to_client(fd, opcode, msg)
    local body = packet.pack(opcode, msg)
    local data = string.pack(">s2", body)
    socketdriver.send(fd, data)
end

local function handle_enter_game(fd, msg)
    local token = msg.token or ""
    skynet.error(string.format("Game: enter_game fd=%d token=%s", fd, token))

    local info = skynet.call(login_server, "lua", "validate", token)
    if not info then
        skynet.error(string.format("Game: invalid token fd=%d", fd))
        send_to_client(fd, opcodes.S2C_EnterGame, {
            result = -1,
            player_id = 0,
            player_name = "",
        })
        skynet.call(gate, "lua", "kick", fd)
        return
    end

    local agent = skynet.newservice("agent")
    skynet.call(agent, "lua", "start", {
        fd = fd,
        gate = gate,
        player_id = info.player_id,
        player_name = info.username,
    })

    skynet.call(gate, "lua", "forward", fd, 0, agent)

    connections[fd] = { agent = agent, player_id = info.player_id }
    agents[agent] = fd

    send_to_client(fd, opcodes.S2C_EnterGame, {
        result = 0,
        player_id = info.player_id,
        player_name = info.username,
    })

    skynet.error(string.format("Game: player entered id=%d name=%s fd=%d", info.player_id, info.username, fd))
end

local SOCKET = {}

function SOCKET.open(fd, addr)
    skynet.error(string.format("Game: connected fd=%d addr=%s", fd, addr))
    skynet.call(gate, "lua", "accept", fd)
end

function SOCKET.close(fd)
    skynet.error(string.format("Game: disconnected fd=%d", fd))
    local c = connections[fd]
    if c and c.agent then
        skynet.send(c.agent, "lua", "disconnect")
        agents[c.agent] = nil
    end
    connections[fd] = nil
end

function SOCKET.error(fd, msg)
    skynet.error(string.format("Game: socket error fd=%d msg=%s", fd, tostring(msg)))
    SOCKET.close(fd)
end

function SOCKET.data(fd, data)
    local opcode, msg, err = packet.unpack(data)
    if err then
        skynet.error(string.format("Game: unpack error fd=%d err=%s", fd, err))
        return
    end

    if opcode == opcodes.C2S_EnterGame then
        handle_enter_game(fd, msg)
    elseif opcode == opcodes.C2S_Heartbeat then
        send_to_client(fd, opcodes.S2C_Heartbeat, {})
    else
        skynet.error(string.format("Game: unhandled opcode=%d fd=%d (no agent yet)", opcode, fd))
    end
end

local CMD = {}

function CMD.open(conf)
    login_server = conf.login_server

    gate = skynet.newservice("gate")
    skynet.call(gate, "lua", "open", {
        port = GAME_PORT,
        maxclient = 1024,
        nodelay = true,
        watchdog = skynet.self(),
    })
    skynet.error(string.format("Game gate listening on port %d", GAME_PORT))
end

function CMD.kick(agent)
    local fd = agents[agent]
    if fd then
        skynet.call(gate, "lua", "kick", fd)
    end
end

skynet.start(function()
    local proto_dir = skynet.getenv("proto_dir")
    proto_loader.load(proto_dir)

    skynet.dispatch("lua", function(session, source, cmd, subcmd, ...)
        if cmd == "socket" then
            local f = SOCKET[subcmd]
            if f then
                f(...)
            else
                skynet.error("Game: unknown socket subcmd=" .. tostring(subcmd))
            end
        else
            local f = CMD[cmd]
            if f then
                skynet.ret(skynet.pack(f(subcmd, ...)))
            else
                skynet.error("Game: unknown cmd=" .. tostring(cmd))
            end
        end
    end)
end)
