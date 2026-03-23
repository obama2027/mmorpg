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

帮我开发http和网络下载模块，代码落地到代码落地到client/Assets/Scripts/Core/Net
1.添加http网络请求，请求失败默认重试3次
2.添加网络下载模块，用来下载assetsbundle等东西
3.处理异常情况，例如下载失败重试几次，还有其他异常一起处理一下


再server目录下，帮我实现一个简易的服务器，
1.基于skynet,可以通过socket tcp链接，协议解析用protobuf,  
2.支持多个客户端链接
3.先链接网关再链接游戏服务器

开发建议版本http服务器，目录再phpServer
1.开发语言用php
2.接受http请求，返回结果
3.支持get和post请求




1.用 WSL2
2..proto 文件客户端和服务端共享
3.登录先不用验证，有流程，直接放行就可以
4.完成功能后叫我怎么再我的电脑上部署和启动服务器



实现整个游戏启动得流程
1.游戏客户端代码分为Launch.dll和Game.dll,Launch.dll存放热更和网络下载代码，Game.dll存放所有游戏代码
2.资源服暂时放在本地，资源根目录位phpServer/Res
3.资源服目录参考test/resServer得目录结构，Android得version.txt存放游戏最新版本号,区分平台版本，例如ios,Android
4.

添加工具Tools/Build Res, 点击打开弹出一个editor界面，界面上面是一个列表，最下面是Build按钮，列表单项前面是复选框，中间是文字，最后是Run按钮
1.列表里面暂时有3项，update proto，update dll, update res
2.点update proto后面得run按钮调用生成协议功能
3.点update dll后面得run按钮调用HyBirdCLR的CompileDll->ActiveBuildTarget,并将生成好的Launch.dll和Game.dll复制到StreamingAssets/plamtform下
4.点update res后面得run按钮调用MMORPG->Bunlde->BunldeCurrentTarget
5.点最下面得build按钮则按列表选中得单项，按顺序执行


游戏启动时如果不在editor状态，或者是再editor状态但buildconfig.editorDevelopmentMode=true, 则判断persistentDataPath路径下有没有version.json, streamingassets/platform下的文件全部复制到persistentDataPath下，并给出log打印