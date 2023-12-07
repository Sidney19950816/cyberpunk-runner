using System;
using System.Threading.Tasks;
using Assets.Scripts.Managers;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.DailyRewards
{
    public class DailyRewardsManager : MonoBehaviour
    {
        [SerializeField] private DailyRewardsView sceneView;

        [SerializeField] private DailyRewardsEventManager eventManager;

        #region Instance
        public static DailyRewardsManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            if (eventManager == null)
                eventManager = GetComponent<DailyRewardsEventManager>();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        public async Task RefreshStatus()
        {
            try
            {
                await eventManager.RefreshDailyRewardsEventStatus();

                if (this == null) return;

                Debug.Log("Initialization complete.");

                if (eventManager.isEnded)
                {
                    await eventManager.Demonstration_StartNextMonth();
                    if (this == null) return;
                }

                ShowStatus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        void Update()
        {
            if (!eventManager.isEventReady)
            {
                return;
            }

            // Only update if the event is actually active
            if (eventManager.isStarted && !eventManager.isEnded)
            {
                // Request periodic update to update timers and start new day, if necessary.
                if (eventManager.UpdateRewardsStatus(sceneView))
                {
                    // Update call returned true to signal start of new day so full update is required.
                    sceneView?.UpdateStatus(eventManager);
                }
                else
                {
                    // Update call signaled that only timers require updating (new day did not begin yet).
                    //sceneView.UpdateTimers(eventManager);
                }
            }
        }

        void ShowStatus()
        {
            sceneView?.UpdateStatus(eventManager);

            if (eventManager.firstVisit)
            {
                eventManager.MarkFirstVisitComplete();
            }
        }

        public async void OnClaimButtonPressed()
        {
            try
            {
                // Disable all claim buttons to prevent multiple collect requests.
                // Button is reenabled when the state is refreshed after the claim has been fully processed.
                sceneView.SetAllDaysUnclaimable();

                await eventManager.ClaimDailyReward();
                if (this == null) return;

                ShowStatus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async void OnOpenEventButtonPressed()
        {
            try
            {
                if (!eventManager.isEventReady)
                {
                    return;
                }

                if (eventManager.isEnded)
                {
                    await eventManager.Demonstration_StartNextMonth();
                    if (this == null) return;
                }

                ShowStatus();
                StateManager.SetState(new DailyRewardsState());

                //sceneView.OpenEventWindow();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
