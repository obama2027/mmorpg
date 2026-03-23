# PHP 简易 HTTP 服务器

基于 PHP 的简易 HTTP 服务，支持 GET/POST 请求，返回 JSON，便于 Unity 客户端 HTTP 模块联调测试。同时作为资源服务器，提供资源版本查询与文件下载，支持客户端热更新流程。

## 环境要求

- PHP 7.4+（推荐 8.x）
- 内置 json 扩展
- 资源上传 `/api/uploadRes` 需 **zip** 或 **phar** 扩展之一（解压 zip 包）
  - 若未启用：在 `php.ini` 中取消注释 `extension=zip`，或确保 `extension=phar` 已启用

## 启动

```bash
cd /mnt/d/work/AIProject/mmorpg/phpServer
php -S 0.0.0.0:8080 -t public public/index.php
```

- 监听 `http://localhost:8080`
- `-t public` 指定文档根目录
- `public/index.php` 为路由脚本，所有请求统一由此入口处理

## 资源目录结构

```
phpServer/Res/
├── Android/
│   ├── version.txt          # 当前最新版本号（如 1.0.2）
│   └── 1.0.2/               # 版本目录
│       ├── version.json     # 版本清单（dll、bundle 的 path、md5、size）
│       ├── base.dll.bytes
│       ├── game.dll.bytes
│       └── *.bundle
├── StandaloneWindows64/
│   ├── version.txt
│   └── 1.0.x/
│       └── ...
└── iOS/
    └── ...
```

## API 说明

### 通用 API

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/config | 返回配置 `{"ok":true,"data":{"version":"1.0"}}` |
| GET | /api/echo?key=value | 回显 GET 参数 |
| POST | /api/echo | 回显 POST 表单或 JSON body |
| POST | /api/login | 简易登录，不验证直接放行，返回 mock token |

### 资源上传（编辑器 Build Res 使用）

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/getNextResVersion?platform=xxx | 获取下一版本号（用于 UpLoad，version.txt 末段 +1） |
| POST | /api/uploadRes?platform=xxx | 上传资源 zip 包（body 为 application/zip），解压到 Res/\<platform\>/\<version\>/ |

### 资源下载（客户端热更新使用）

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/getResVersion?platform=xxx | 获取当前资源版本号，读取 Res/\<platform\>/version.txt |
| GET | /api/getVersionJson?platform=xxx&version=xxx | 获取指定版本的 version.json 内容 |
| GET | /api/downloadResFile?platform=xxx&version=xxx&file=xxx | 下载指定资源文件（二进制流） |

**参数说明：**

- `platform`：平台名，如 `Android`、`StandaloneWindows64`、`iOS`
- `version`：版本号，如 `1.0.2`
- `file`：文件名，如 `base.dll.bytes`、`game.dll.bytes`

## 客户端 Base.dll 更新流程

游戏启动时（isDebug=false），客户端会按以下顺序执行：

1. **首次复制**：若 persistentDataPath 下无 version.json，则将 StreamingAssets/\<Platform\>/ 全部复制到 persistentDataPath，最后写入 version.json。
2. **Base.dll 更新检查**：
   - 读取本地 persistentDataPath/version.json 中 base.dll.bytes 的 Md5
   - 调用 `GET /api/getResVersion?platform=xxx` 获取服务器最新版本号
   - 调用 `GET /api/getVersionJson?platform=xxx&version=xxx` 获取该版本的 version.json
   - 从服务器 version.json 中取 base.dll.bytes 的 Md5
   - 若本地 Md5 与服务器 Md5 不同，则调用 `GET /api/downloadResFile?platform=xxx&version=xxx&file=base.dll.bytes` 下载 base.dll.bytes 到 persistentDataPath
   - 下载成功后，更新本地 version.json 中 base.dll 的 Md5、Version 等字段

## 与 Unity 客户端对接

```csharp
// GET 配置
string json = await UnityHttpClient.Instance.GetStringAsync("http://localhost:8080/api/config");

// POST 登录
string resp = await UnityHttpClient.Instance.PostJsonAsync(
    "http://localhost:8080/api/login",
    "{\"username\":\"test\",\"password\":\"xxx\"}");
```

## 本地测试

```bash
# GET config
curl http://localhost:8080/api/config

# GET echo
curl "http://localhost:8080/api/echo?foo=bar"

# POST login (JSON)
curl -X POST http://localhost:8080/api/login -H "Content-Type: application/json" -d "{\"username\":\"player1\",\"password\":\"xxx\"}"

# 资源 API 测试
curl "http://localhost:8080/api/getResVersion?platform=Android"
curl "http://localhost:8080/api/getVersionJson?platform=Android&version=1.0.2"
curl -o base.dll.bytes "http://localhost:8080/api/downloadResFile?platform=Android&version=1.0.2&file=base.dll.bytes"
```
