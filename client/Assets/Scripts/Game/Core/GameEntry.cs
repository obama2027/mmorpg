
public class GameEntry:Single<GameEntry>{

    public override void Init()
    {
        GameAssetService.Instance.Init();
        SocketService.Instance.Init();
        GameManager.Instance.Init();
        UIManager.Instance.Init();
        UIManager.Instance.Open(UIType.Login,UILayer.FullScreen);
    }
}