cd D:\work\AIProject\mmorpg

# 生成 login 协议
.\protoc --proto_path=proto --csharp_out=client/Assets/Scripts/Proto proto/login.proto

# 生成 game 协议
.\protoc --proto_path=proto --csharp_out=client/Assets/Scripts/Proto proto/game.proto