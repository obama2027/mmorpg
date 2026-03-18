# MMORPG Server

基于 Skynet 的简易 MMORPG 服务端，包含登录网关和游戏服务器。

## 架构

```
客户端                        服务端 (Skynet)
  │                              │
  ├── TCP:8888 ──► login_server  │  登录网关（验证账号，发放 token）
  │                              │
  └── TCP:9999 ──► game_watchdog │  游戏网关
                       │         │
                       └► agent  │  玩家 agent（每玩家一个）
```

**登录流程：**

1. 客户端连接 `8888` 端口，发送 `C2S_Login`
2. `login_server` 验证（当前直接放行），返回 `S2C_Login`（含 token + 游戏服务器地址）
3. 客户端断开登录连接
4. 客户端连接 `9999` 端口，发送 `C2S_EnterGame`（含 token）
5. `game_watchdog` 验证 token，创建 `agent`，返回 `S2C_EnterGame`
6. 后续协议由 `agent` 处理（心跳等）

## 协议格式

```
[2字节 body 长度, 大端序] [2字节 opcode, 大端序] [N字节 protobuf payload]
```

与客户端 `SocketPacketFramer` 和 Skynet `netpack` 2 字节头一致。

## 在你的 PC 上部署和启动

### 第一步：安装 WSL2

在 PowerShell（管理员）中执行：

```powershell
wsl --install -d Ubuntu-22.04
```

安装完成后重启电脑，打开 Ubuntu 终端设置用户名和密码。

### 第二步：安装编译工具

在 WSL2 Ubuntu 终端中执行：

```bash
sudo apt update
sudo apt install -y git gcc make autoconf libreadline-dev
```

### 第三步：克隆 Skynet 和 lua-protobuf

进入项目的 server 目录（通过 WSL2 访问 Windows 文件系统）：

```bash
cd /mnt/d/work/AIProject/mmorpg/server

# 克隆 skynet
git clone https://github.com/cloudwu/skynet.git skynet

# 创建 3rd 目录并克隆 lua-protobuf
mkdir -p 3rd
git clone https://github.com/starwing/lua-protobuf.git 3rd/lua-protobuf
```

### 第四步：编译

```bash
cd /mnt/d/work/AIProject/mmorpg/server
make
```

这会依次编译 Skynet 和 lua-protobuf。首次编译约 1-2 分钟。

如果 `make` 命令报错，也可以分步编译：

```bash
cd skynet && make linux && cd ..
cd 3rd/lua-protobuf && make && cd ../..
```

### 第五步：启动服务器

```bash
cd /mnt/d/work/AIProject/mmorpg/server
./skynet/skynet etc/config
```

看到以下输出说明启动成功：

```
=== MMORPG Server Starting ===
Login server listening on port 8888
Game gate listening on port 9999
=== MMORPG Server Ready ===
```

### 第六步：停止服务器

在同一终端按 `Ctrl+C`，或者在另一个终端执行：

```bash
cd /mnt/d/work/AIProject/mmorpg/server
./stop.sh
```

## 客户端连接配置

Unity 客户端连接服务器时，使用以下配置：

```csharp
// 登录连接
var loginConfig = SocketConnectionConfig.Default("127.0.0.1", 8888);
loginConfig.AutoReconnect = false;
int loginConnId = SocketService.Instance.CreateAndConnect(loginConfig);

// 游戏连接（登录成功后，用返回的 host 和 port）
var gameConfig = SocketConnectionConfig.Default(gameHost, gamePort);
gameConfig.HeartbeatIntervalMs = 10000;
gameConfig.HeartbeatTimeoutMs = 30000;
gameConfig.HeartbeatRequestOpcode = 1;
gameConfig.HeartbeatResponseOpcode = 2;
int gameConnId = SocketService.Instance.CreateAndConnect(gameConfig);
```

## Opcode 定义

| Opcode | 名称 | 方向 | Proto |
|--------|------|------|-------|
| 1 | C2S_Heartbeat | 客户端→服务端 | `game.C2S_Heartbeat` |
| 2 | S2C_Heartbeat | 服务端→客户端 | `game.S2C_Heartbeat` |
| 1001 | C2S_Login | 客户端→服务端 | `login.C2S_Login` |
| 1002 | S2C_Login | 服务端→客户端 | `login.S2C_Login` |
| 2001 | C2S_EnterGame | 客户端→服务端 | `game.C2S_EnterGame` |
| 2002 | S2C_EnterGame | 服务端→客户端 | `game.S2C_EnterGame` |

## 目录结构

```
server/
├── skynet/                    # Skynet 源码（git clone）
├── 3rd/
│   └── lua-protobuf/          # lua-protobuf 库（git clone）
├── proto/                     # → ../proto/（共享 proto 文件）
├── etc/
│   └── config                 # Skynet 启动配置
├── lualib/
│   ├── proto_loader.lua       # Protobuf 加载器
│   ├── opcodes.lua            # Opcode 定义
│   └── packet.lua             # 包编解码
├── service/
│   ├── main.lua               # 启动入口
│   ├── login_server.lua       # 登录网关
│   ├── game_watchdog.lua      # 游戏网关 watchdog
│   └── agent.lua              # 玩家 agent
├── logs/                      # 日志目录
├── Makefile                   # 构建脚本
├── start.sh                   # 启动脚本
├── stop.sh                    # 停止脚本
└── README.md                  # 本文件
```

## 常见问题

### WSL2 网络不通

如果 Unity 客户端无法连接 WSL2 中的服务器：

1. 先尝试 `127.0.0.1`（新版 WSL2 默认支持 localhost 转发）
2. 如果不行，在 WSL2 中执行 `ip addr show eth0` 获取 WSL2 的 IP 地址
3. 客户端连接该 IP 地址

### 编译 lua-protobuf 报错

确保安装了 gcc：

```bash
sudo apt install -y gcc make
```

### 端口被占用

修改端口号：
- 登录端口：`server/service/login_server.lua` 中的 `LOGIN_PORT`
- 游戏端口：`server/service/game_watchdog.lua` 中的 `GAME_PORT`
- 同时更新 `server/service/login_server.lua` 中的 `GAME_PORT`（返回给客户端的游戏服务器端口）
