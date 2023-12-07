using Assets.Scripts.Managers;

public class ItemShopState : BaseState
{
    public ShopCategory ShopCategory { get; private set; }

    public ItemShopState(ShopCategory shopCategory)
    {
        ShopCategory = shopCategory;
    }

    public override void OnStateEnter()
    {
        GameSceneManager.Instance.WorldController.gameObject.SetActive(false);
    }

    public override void OnStateExit()
    {
        GameSceneManager.Instance.WorldController.gameObject.SetActive(true);
    }

    public void OnBackButtonPressed()
    {
        StateManager.SetState(new MainMenuState());
    }
}
