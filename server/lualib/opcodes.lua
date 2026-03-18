local M = {}

M.C2S_Heartbeat     = 1
M.S2C_Heartbeat     = 2

M.C2S_Login         = 1001
M.S2C_Login         = 1002

M.C2S_EnterGame     = 2001
M.S2C_EnterGame     = 2002

M.proto_name = {
    [1]    = "game.C2S_Heartbeat",
    [2]    = "game.S2C_Heartbeat",
    [1001] = "login.C2S_Login",
    [1002] = "login.S2C_Login",
    [2001] = "game.C2S_EnterGame",
    [2002] = "game.S2C_EnterGame",
}

return M
