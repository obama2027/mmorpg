# PHP 简易 HTTP 服务器

基于 PHP 的简易 HTTP 服务，支持 GET/POST 请求，返回 JSON，便于 Unity 客户端 HTTP 模块联调测试。

## 环境要求

- PHP 7.4+（推荐 8.x）
- 无额外扩展依赖（仅内置 json 扩展）

## 启动

```bash
cd /mnt/d/work/AIProject/mmorpg/phpserver
php -S 0.0.0.0:8080 -t public public/index.php
```

- 监听 `http://localhost:8080`
- `-t public` 指定文档根目录
- `public/index.php` 为路由脚本，所有请求统一由此入口处理

## API 说明

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/config | 返回配置 `{"ok":true,"data":{"version":"1.0"}}` |
| GET | /api/echo?key=value | 回显 GET 参数 |
| POST | /api/echo | 回显 POST 表单或 JSON body |
| POST | /api/login | 简易登录，不验证直接放行，返回 mock token |

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
```
