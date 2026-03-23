//using System;
//using System.Threading.Tasks;
//using UnityEngine;

///// <summary>
///// 请求 phpServer 的示例。挂到任意 GameObject 上，点击按钮触发。
///// 启动前请先运行：cd phpServer && php -S 0.0.0.0:8080 -t public public/index.php
///// </summary>
//public class HttpServerExample : MonoBehaviour
//{
//    private const string BaseUrl = "http://localhost:8080";
//    private IHttpClient _http;
//    private string _logText = "";

//    void Awake()
//    {
//        _http = new UnityHttpClient();
//    }

//    void OnGUI()
//    {
//        float y = 20;
//        float btnW = 200;
//        float btnH = 40;
//        float gap = 10;

//        if (GUI.Button(new Rect(20, y, btnW, btnH), "GET /api/config"))
//        {
//            _ = RequestConfigAsync();
//        }
//        y += btnH + gap;

//        if (GUI.Button(new Rect(20, y, btnW, btnH), "GET /api/echo?foo=bar"))
//        {
//            _ = RequestEchoGetAsync();
//        }
//        y += btnH + gap;

//        if (GUI.Button(new Rect(20, y, btnW, btnH), "POST /api/login"))
//        {
//            _ = RequestLoginAsync();
//        }
//        y += btnH + gap;

//        if (GUI.Button(new Rect(20, y, btnW, btnH), "POST /api/echo (JSON)"))
//        {
//            _ = RequestEchoPostAsync();
//        }

//        // 显示日志
//        GUI.Label(new Rect(240, 20, 400, 400), _logText);
//    }

//    private void Log(string msg)
//    {
//        _logText = $"[{DateTime.Now:HH:mm:ss}] {msg}\n{_logText}";
//    }

//    private async Task RequestConfigAsync()
//    {
//        try
//        {
//            string json = await _http.GetStringAsync($"{BaseUrl}/api/config");
//            Log($"Config OK: {json}");
//        }
//        catch (OperationCanceledException)
//        {
//            Log("Config: 已取消");
//        }
//        catch (HttpModuleException ex)
//        {
//            Log($"Config 失败: {ex.Message}");
//        }
//    }

//    private async Task RequestEchoGetAsync()
//    {
//        try
//        {
//            string json = await _http.GetStringAsync($"{BaseUrl}/api/echo?foo=bar&baz=123");
//            Log($"Echo GET OK: {json}");
//        }
//        catch (OperationCanceledException)
//        {
//            Log("Echo GET: 已取消");
//        }
//        catch (HttpModuleException ex)
//        {
//            Log($"Echo GET 失败: {ex.Message}");
//        }
//    }

//    private async Task RequestLoginAsync()
//    {
//        try
//        {
//            string body = "{\"username\":\"TestPlayer\",\"password\":\"xxx\"}";
//            string json = await _http.PostJsonAsync($"{BaseUrl}/api/login", body);
//            Log($"Login OK: {json}");
//        }
//        catch (OperationCanceledException)
//        {
//            Log("Login: 已取消");
//        }
//        catch (HttpModuleException ex)
//        {
//            Log($"Login 失败: {ex.Message}");
//        }
//    }

//    private async Task RequestEchoPostAsync()
//    {
//        try
//        {
//            string body = "{\"name\":\"Unity\",\"value\":42}";
//            string json = await _http.PostJsonAsync($"{BaseUrl}/api/echo", body);
//            Log($"Echo POST OK: {json}");
//        }
//        catch (OperationCanceledException)
//        {
//            Log("Echo POST: 已取消");
//        }
//        catch (HttpModuleException ex)
//        {
//            Log($"Echo POST 失败: {ex.Message}");
//        }
//    }
//}
