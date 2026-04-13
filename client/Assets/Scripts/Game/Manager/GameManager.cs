using System;

public class GameManager:Single<GameManager>
{

    public override void Init()
    {
        InitListener();
    }
    public void InitListener()
    {
        EventDispatcher.AddListener<StringEventArgs>(EventId.LoginIn, OnLoginIn);
        EventDispatcher.AddListener<StringEventArgs>(EventId.LoginOut, OnLoginOut);
    }

    public void RemoveListener()
    {
        EventDispatcher.RemoveListener<StringEventArgs>(EventId.LoginIn, OnLoginIn);
        EventDispatcher.RemoveListener<StringEventArgs>(EventId.LoginOut, OnLoginOut);
    }

    private void OnLoginOut(StringEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void OnLoginIn(StringEventArgs t)
    {
        throw new NotImplementedException();
    }

    public void StartLoginConnect() { 
    
    }
}
