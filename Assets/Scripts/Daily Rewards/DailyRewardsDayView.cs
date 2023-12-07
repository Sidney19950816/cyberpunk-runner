using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.DailyRewards
{
    public class DailyRewardsDayView : MonoBehaviour
    {
        [Header("TEXTS")]
        [SerializeField] private Text dayText;
        [SerializeField] private TextMeshProUGUI rewardQuantity;
        [SerializeField] private Text loginText;

        [Space, Header("BUTTONS")]
        [SerializeField] private Button claimButton;
        [SerializeField] private Button claimedButton;

        [Space, Header("IMAGES")]
        [SerializeField] private Image claimedCheckmarkImage;
        [SerializeField] private Image currencyImage;

        [Space, Header("SPRITES")]
        [SerializeField] private Sprite rokenSprite;
        [SerializeField] private Sprite biochipSprite;

        public int dayIndex { get; protected set; }

        private void Start()
        {
            claimButton.onClick.AddListener(DailyRewardsManager.Instance.OnClaimButtonPressed);
        }

        public void SetDayIndex(int dayIndex)
        {
            this.dayIndex = dayIndex;

            dayText.text = $"Day {dayIndex}";
        }

        public void UpdateStatus(DailyRewardsEventManager eventManager)
        {
            var reward = eventManager.GetDailyRewards(dayIndex - 1)[0];
            rewardQuantity.text = $"{reward.quantity}";
            currencyImage.sprite = reward.id.Equals(UtilConstants.CURRENCY_ROKEN_ID) ? rokenSprite : biochipSprite;

            claimButton.interactable = true;

            var dayStatus = eventManager.GetDayStatus(dayIndex);

            claimButton.gameObject.SetActive(dayStatus == DailyRewardsEventManager.DayStatus.DayClaimable);
            claimButton.interactable = dayStatus == DailyRewardsEventManager.DayStatus.DayClaimable;
            claimedButton.gameObject.SetActive(dayStatus == DailyRewardsEventManager.DayStatus.DayClaimed);
            claimedCheckmarkImage.gameObject.SetActive(dayStatus == DailyRewardsEventManager.DayStatus.DayClaimed);
            loginText.gameObject.SetActive(dayStatus == DailyRewardsEventManager.DayStatus.DayUnclaimable);
        }

        public void SetUnclaimable()
        {
            claimButton.interactable = false;
        }
    }
}
