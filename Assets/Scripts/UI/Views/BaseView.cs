using UnityEngine;
using Assets.Scripts;

public abstract class BaseView : BaseBehaviour
{
    public abstract void UpdateView(BaseState state);

    protected virtual void Show(BaseState state = null)
    {
        gameObject?.SetActive(true);
    }

    protected virtual void Hide(BaseState state = null)
    {
        gameObject?.SetActive(false);
    }
}
