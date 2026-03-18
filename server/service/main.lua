local skynet = require "skynet"

skynet.start(function()
    skynet.error("=== MMORPG Server Starting ===")

    local login = skynet.newservice("login_server")
    skynet.error("Login server started, addr=" .. skynet.address(login))

    local game = skynet.newservice("game_watchdog")
    skynet.call(game, "lua", "open", { login_server = login })
    skynet.error("Game server started, addr=" .. skynet.address(game))

    skynet.error("=== MMORPG Server Ready ===")
    skynet.exit()
end)
