using System;

public abstract class UIModel
{
}

public abstract class UIController<TModel, TWindow>
    where TModel : UIModel
    where TWindow : UIWindow
{
    protected TModel Model { get; private set; }
    protected TWindow Window { get; private set; }

    /// <summary>
    /// Window owns the model instance; controller receives the same reference for business logic.
    /// </summary>
    public void Bind(TWindow window, TModel model)
    {
        Window = window;
        Model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public virtual void OnCreate()
    {
    }

    public virtual void OnShow(object args)
    {
    }

    public virtual void OnHide()
    {
    }

    public virtual void OnDestroyUI()
    {
    }
}
