using UnityEngine;
using System.Collections;

namespace LifeCraft.Services
{
    public class RewardedAdsManager : MonoBehaviour
    {
        public static RewardedAdsManager Instance { get; private set; }
        
        [System.Serializable]
        public class AdRewards
        {
            public int premium;
            public int gameCurrency;
            public int health;
            public int happiness;
        }
        
        [System.Serializable]
        public class AdWatchResponse
        {
            public bool success;
            public AdRewards rewards;
            public int adsRemaining;
        }
        
        [System.Serializable]
        public class AdStatusResponse
        {
            public int adsWatchedToday;
            public int adsRemaining;
            public int maxAdsPerDay;
            public string nextResetTime;
            public bool canWatchAd;
            public int cooldownRemaining;
        }
        
        private string currentAdType;
        private bool isAdLoading = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void ShowRewardedAd(string adType)
        {
            if (isAdLoading)
            {
                Debug.LogWarning("Ad is already loading");
                return;
            }

            currentAdType = adType;
            
            isAdLoading = true;
            
            #if UNITY_IOS || UNITY_ANDROID
            Debug.Log("Showing rewarded ad: " + adType);
            OnAdCompleted();
            #endif
            
            #if UNITY_EDITOR
            OnAdCompleted();
            #endif
        }
        
        private void OnAdCompleted()
        {
            isAdLoading = false;
            Debug.Log("Ad completed successfully");
            
            #if UNITY_EDITOR || UNITY_STANDALONE
            StartCoroutine(GrantRewards(currentAdType));
            #endif
        }
        
        private void OnAdSkipped()
        {
            isAdLoading = false;
            Debug.Log("Ad was skipped");
        }
        
        private void OnAdFailed()
        {
            isAdLoading = false;
            Debug.LogError("Ad failed to show");
        }
        
        private System.Collections.IEnumerator GrantRewards(string adType)
        {
            var payload = new { adType };
            
            yield return ApiClient.Instance.Post<AdWatchResponse>(
                "/api/economy/rewards/ad-watch",
                payload,
                (response) => {
                    if (response.success)
                    {
                        Debug.Log($"Ad rewards granted: {JsonUtility.ToJson(response)}");
                        
                        UI.UIManager.Instance?.ShowRewardPopup(response.rewards);
                        UI.UIManager.Instance?.UpdateAdCount(response.adsRemaining);
                        RefreshAdStatus();
                    }
                },
                (error) => {
                    Debug.LogError($"Failed to grant ad rewards: {error}");
                    UI.UIManager.Instance?.ShowError("Failed to grant rewards");
                }
            );
        }
        
        public System.Collections.IEnumerator RefreshAdStatus()
        {
            yield return ApiClient.Instance.Get<AdStatusResponse>(
                "/api/economy/rewards/ad-status",
                (status) => {
                    UI.UIManager.Instance?.UpdateAdStatus(status);
                }
            );
        }
        
        void Start()
        {
            StartCoroutine(RefreshAdStatus());
        }
    }
}
