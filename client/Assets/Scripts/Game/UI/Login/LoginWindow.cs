using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoginWindow : UIWindow
{
    public LoginModel Model { get; } = new LoginModel();

    private readonly LoginController _controller = new LoginController();

    public override void OnCreate()
    {
        base.OnCreate();

        _controller.Bind(this, Model);
        _controller.OnCreate();
    }

    protected override void OnBtnClick(Button btn)
    {
        if (btn == null)
        {
            return;
        }

        if (string.Equals(btn.name, "btnClose", StringComparison.OrdinalIgnoreCase))
        {
            _controller.OnCloseRequested();
            UIManager.Instance.Close(UIType.Login, destroy: false);
        }
    }

    public override void OnShow(object args)
    {
        gameObject.SetActive(true);
        _controller.OnShow(args);
    }

    public override void OnHide()
    {
        _controller.OnHide();
        gameObject.SetActive(false);
    }

    public override void OnDestroyUI()
    {
        _controller.OnDestroyUI();
        base.OnDestroyUI();
    }
}
