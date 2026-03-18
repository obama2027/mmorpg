你要开发一个mmorpg项目，分为前后端工程，前端代码都放在client, 后端代码放在server, 开发引擎用unity团结引擎1.6.9
有几点要求，
1.服务器用开源框架skynet
2.客户端热更新框架用HybridCLR
3.模块设计用MVC模式
4.资源加载方案用assetbundle
5.网络需要支持socket和http，socket需要支持多链接
6.代码风格保持一致


1.客户端热更新框架用HybridCLR
2.模块设计用MVC模式
3.网络模块，socket+http
4.资源加载用assetbundle



资源管理器
1.再AssetBundleBuilder.CS添加工具自动设置BundleName,规则是读取Assets/Env/BuildConfig的buildRootList字段，该字段下的每个item都是一个搜索路径，
搜索路径下的每个一级文件都打包成一个bundle,例如BuildConfig其中一行是GameAssets/UI，UI下有login和Main多个一级目录，以main为例子，遍历main文件夹下的所有.prefab文件，将bundleName设置成GameAssets/UI/Main


处理一下生成bundle的规则
1.将所有bundle生成到StreamingAssets/BundlePathUtility.GetRuntimePlatformName()/下
2.生成的bundle命名按bundleName.bundle, bundleName读取已设置好的bundleNmae, 如果文件名字不能出现/，就把所有/替换成!
3.mainfest就按BundlePathUtility.GetRuntimePlatformName()命名



帮我开发网络模块
1.代码落地到client/Assets/Scripts/Core/Net/Socket
2.开发socket模块，socket支持同时多个链接，支持ipv4/ipv6, 支持tcp/udp协议
3.socket收发协议不阻塞主线程
4.处理拆包粘包问题，每个协议约定前4个字节为包头，用来读取协议长度
5.处理断线重连和异常问题
6.注意内存问题，防止泄露和内存安全问题
7.用protobuf处理协议


再server目录下，帮我实现一个简易的服务器，
1.基于skynet,可以通过socket tcp链接，协议解析用protobuf,  
2.支持多个客户端链接
3.先链接网关再链接游戏服务器




1.用 WSL2
2..proto 文件客户端和服务端共享
3.登录先不用验证，有流程，直接放行就可以
4.完成功能后叫我怎么再我的电脑上部署和启动服务器