using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Managers;

namespace Assets.Scripts.DailyRewards
{
    public class DailyRewardsView : BaseView
    {
        [Header("BACK BUTTON")]
        [SerializeField] private Button backButton;

        [Space, Header("DAY VIEW")]
        [SerializeField] private GridLayoutGroup layoutGroup;

        private DailyRewardsDayView[] dailyRewardsDays;

        void Awake()
        {
            SetDailyRewardsDays();
        }

        public override void UpdateView(BaseState state)
        {
            if (state is DailyRewardsState)
            {
                Show(state);
            }
            else
            {
                Hide();
            }
        }

        protected override void Show(BaseState state = null)
        {
            base.Show(state);

            DailyRewardsState dailyState = state as DailyRewardsState;

            backButton.onClick.AddListener(dailyState.OnBackButtonClick);
            backButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        }

        protected override void Hide(BaseState state = null)
        {
            base.Hide(state);

            backButton.onClick.RemoveAllListeners();
        }

        public void UpdateStatus(DailyRewardsEventManager eventManager)
        {
            SetDailyRewardsDays();

            var daysClaimed = eventManager.daysClaimed;

            for (var dayOn = 0; dayOn < dailyRewardsDays.Length; dayOn++)
            {
                DailyRewardsDayView dayView = dailyRewardsDays[dayOn];

                dayView?.UpdateStatus(eventManager);
            }
            //bonusDay.UpdateStatus(eventManager);

            //UpdateTimers(eventManager);
        }

        //public void UpdateTimers(DailyRewardsEventManager eventManager)
        //{
        //    secondsPerDayText.text = $"1 day = {eventManager.secondsPerDay} sec";

        //    if (eventManager.daysRemaining <= 0)
        //    {
        //        endedEventGameObject.SetActive(true);
        //        daysLeftText.text = "Days Left: 0";
        //        comeBackInText.text = "Event Over";
        //    }
        //    else
        //    {
        //        endedEventGameObject.SetActive(false);
        //        daysLeftText.text = $"Days Left: {eventManager.daysRemaining}";
        //        if (eventManager.secondsTillClaimable > 0)
        //        {
        //            comeBackInText.text = $"Come Back in: {eventManager.secondsTillClaimable:0.0} seconds";
        //        }
        //        else
        //        {
        //            comeBackInText.text = "Claim Now!";
        //        }
        //    }
        //}

        public void SetAllDaysUnclaimable()
        {
            SetUnclaimable();
            //bonusDay.SetUnclaimable();
        }

        //public void UpdateStatus(DailyRewardsEventManager eventManager)
        //{
        //    var daysClaimed = eventManager.daysClaimed;

        //    for (var dayOn = 0; dayOn < dailyRewardsDays.Length; dayOn++)
        //    {
        //        var dayView = dailyRewardsDays[dayOn];

        //        dayView.UpdateStatus(eventManager);
        //    }
        //}

        public void SetUnclaimable()
        {
            for (var dayOn = 0; dayOn < dailyRewardsDays.Length; dayOn++)
            {
                dailyRewardsDays[dayOn].SetUnclaimable();
            }
        }

        private void SetDailyRewardsDays()
        {
            if (dailyRewardsDays != null)
                return;

            dailyRewardsDays = layoutGroup.GetComponentsInChildren<DailyRewardsDayView>();

            for (var dayOn = 0; dayOn < dailyRewardsDays.Length; dayOn++)
            {
                dailyRewardsDays[dayOn].SetDayIndex(dayOn + 1);
            }
        }
    }
}
