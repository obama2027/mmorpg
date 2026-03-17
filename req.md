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


1.