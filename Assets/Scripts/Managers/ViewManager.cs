using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.Managers
{
    public class ViewManager : MonoBehaviour
    {
        private List<BaseView> baseViews = new List<BaseView>();

        private void Start()
        {
            foreach (BaseView baseView in GetComponentsInChildren<BaseView>(true))
            {
                baseViews.Add(baseView);
                StateManager.OnGameStateChanged += baseView.UpdateView;
            }
        }

        private void OnDestroy()
        {
            foreach (BaseView baseView in baseViews)
            {
                StateManager.OnGameStateChanged -= baseView.UpdateView;
            }
        }
    }
}

