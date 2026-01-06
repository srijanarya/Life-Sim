using UnityEngine;
using UnityEngine.UI;

namespace LifeCraft.UI
{
    public class RewardedAdsPanel : MonoBehaviour
    {
        public Text adsRemainingText;
        public Text nextResetText;
        public Button watchAdButton;
        public Button doubleRewardButton;
        public Button skipWaitButton;
        
        private Services.RewardedAdsManager.AdStatusResponse currentStatus;

        void Start()
        {
            watchAdButton.onClick.AddListener(OnWatchAdClick);
            doubleRewardButton.onClick.AddListener(OnDoubleRewardClick);
            skipWaitButton.onClick.AddListener(OnSkipWaitClick);
        }
        
        private void OnWatchAdClick()
        {
            Services.RewardedAdsManager.Instance.ShowRewardedAd("DAILY_AD");
        }
        
        private void OnDoubleRewardClick()
        {
            Services.RewardedAdsManager.Instance.ShowRewardedAd("DOUBLE_REWARD");
        }
        
        private void OnSkipWaitClick()
        {
            Services.RewardedAdsManager.Instance.ShowRewardedAd("SKIP_WAIT");
        }
        
        public void UpdateAdStatus(Services.RewardedAdsManager.AdStatusResponse status)
        {
            currentStatus = status;
            
            adsRemainingText.text = $"Ads: {status.adsRemaining}/{status.maxAdsPerDay}";
            
            System.DateTime nextReset;
            if (System.DateTime.TryParse(status.nextResetTime, out nextReset))
            {
                System.TimeSpan timeUntilReset = nextReset - System.DateTime.Now;
                nextResetText.text = $"Resets in: {timeUntilReset.Hours}h {timeUntilReset.Minutes}m";
            }
            
            bool canWatch = status.canWatchAd && status.cooldownRemaining <= 0;
            watchAdButton.interactable = canWatch;
            doubleRewardButton.interactable = canWatch;
            skipWaitButton.interactable = canWatch;
            
            if (status.cooldownRemaining > 0)
            {
                nextResetText.text += $" ({status.cooldownRemaining}s cooldown)";
            }
        }
        
        public void UpdateAdCount(int remaining)
        {
            if (currentStatus != null)
            {
                currentStatus.adsRemaining = remaining;
            }
            adsRemainingText.text = $"Ads: {remaining}/{currentStatus?.maxAdsPerDay ?? 3}";
        }
        
        public void ShowRewardPopup(Services.RewardedAdsManager.AdRewards rewards)
        {
            string rewardText = $"Earned {rewards.premium} premium currency!";
            
            if (rewards.gameCurrency > 0)
            {
                rewardText += $"\n+{rewards.gameCurrency} game currency";
            }
            
            if (rewards.health > 0)
            {
                rewardText += $"\n+{rewards.health} Health";
            }
            
            if (rewards.happiness > 0)
            {
                rewardText += $"\n+{rewards.happiness} Happiness";
            }
            
            UI.UIManager.Instance?.ShowMessage("Reward Granted!", rewardText, 3f);
        }
    }
}
