using UnityEngine;
using Login;
using Game;

public class Launch : MonoBehaviour
{
    private int _loginConnId;
    private int _gameConnId;
    private string _token;
    private string _gameHost;
    private int _gamePort;

    void Start()
    {
        SocketService.Instance.OnConnectionStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (SocketService.Instance != null)
        {
            SocketService.Instance.OnConnectionStateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(int connId, SocketConnectionState oldState, SocketConnectionState newState)
    {
        Debug.Log($"[Net] connId={connId} {oldState} -> {newState}");

        if (newState == SocketConnectionState.Connected)
        {
            if (connId == _loginConnId)
            {
                SendLogin();
            }
            else if (connId == _gameConnId)
            {
                SendEnterGame();
            }
        }
    }

    // ============ 登录阶段 ============

    public void StartLogin()
    {
        Debug.Log("[Test] === Start Login ===");

        SocketService.Instance.RegisterHandler<S2C_Login>(1002, OnLoginResponse);

        var config = SocketConnectionConfig.Default("127.0.0.1", 8888);
        config.AutoReconnect = false;
        _loginConnId = SocketService.Instance.CreateAndConnect(config);
    }

    private void SendLogin()
    {
        var req = new C2S_Login { Username = "TestPlayer" };
        SocketService.Instance.Send(_loginConnId, 1001, req);
        Debug.Log("[Test] C2S_Login sent");
    }

    private void OnLoginResponse(int connId, S2C_Login resp)
    {
        Debug.Log($"[Test] S2C_Login: result={resp.Result} token={resp.Token} game={resp.GameHost}:{resp.GamePort}");

        if (resp.Result == 0)
        {
            _token = resp.Token;
            _gameHost = resp.GameHost;
            _gamePort = resp.GamePort;

            SocketService.Instance.UnregisterHandler(1002);
            SocketService.Instance.DestroyConnection(_loginConnId);
            _loginConnId = 0;

            Debug.Log("[Test] Login success, connecting to game server...");
            ConnectGameServer();
        }
        else
        {
            Debug.LogError($"[Test] Login failed: result={resp.Result}");
        }
    }

    // ============ 进入游戏阶段 ============

    private void ConnectGameServer()
    {
        Debug.Log($"[Test] === Connect Game Server {_gameHost}:{_gamePort} ===");

        SocketService.Instance.RegisterHandler<S2C_EnterGame>(2002, OnEnterGameResponse);

        var config = SocketConnectionConfig.Default(_gameHost, _gamePort);
        config.HeartbeatIntervalMs = 10000;
        config.HeartbeatTimeoutMs = 30000;
        config.HeartbeatRequestOpcode = 1;
        config.HeartbeatResponseOpcode = 2;
        _gameConnId = SocketService.Instance.CreateAndConnect(config);
    }

    private void SendEnterGame()
    {
        var req = new C2S_EnterGame { Token = _token };
        SocketService.Instance.Send(_gameConnId, 2001, req);
        Debug.Log("[Test] C2S_EnterGame sent");
    }

    private void OnEnterGameResponse(int connId, S2C_EnterGame resp)
    {
        Debug.Log($"[Test] S2C_EnterGame: result={resp.Result} playerId={resp.PlayerId} name={resp.PlayerName}");

        if (resp.Result == 0)
        {
            Debug.Log("[Test] === Enter Game Success! ===");
        }
        else
        {
            Debug.LogError($"[Test] Enter game failed: result={resp.Result}");
        }
    }

    // ============ UI 触发 ============

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 200, 60), "Start Login"))
        {
            StartLogin();
        }
    }
}
