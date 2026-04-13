using UnityEngine;

public sealed class LoginController : UIController<LoginModel, LoginWindow>
{
    public override void OnShow(object args)
    {
        Debug.Log("[LoginController] OnShow");
    }

    /// <summary>
    /// Business-side reaction before the view closes (e.g. cancel in-flight requests). Navigation stays on the window.
    /// </summary>
    public void OnCloseRequested()
    {
    }
}
