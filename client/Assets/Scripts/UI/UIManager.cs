using UnityEngine;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject LoginPanel;
        public GameObject MainMenuPanel;
        public GameObject GamePanel;

        [Header("Game UI")]
        public StatsPanel StatsPanel;
        public TimelinePanel TimelinePanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ShowLoginPanel()
        {
            LoginPanel.SetActive(true);
            MainMenuPanel.SetActive(false);
            GamePanel.SetActive(false);
        }

        public void ShowMainMenuPanel()
        {
            LoginPanel.SetActive(false);
            MainMenuPanel.SetActive(true);
            GamePanel.SetActive(false);
        }

        public void ShowGamePanel()
        {
            LoginPanel.SetActive(false);
            MainMenuPanel.SetActive(false);
            GamePanel.SetActive(true);
            RefreshUI();
        }

        public void RefreshUI()
        {
            StatsPanel?.UpdateStats();
            TimelinePanel?.UpdateTimeline();
        }

        public void ShowEventPopup(PlayerEvent playerEvent, EventTemplate eventTemplate)
        {
            Debug.Log($"Showing event: {eventTemplate.Title}");
        }
    }

    [System.Serializable]
    public class StatsPanel : MonoBehaviour
    {
        public void UpdateStats()
        {
            if (GameManager.Instance.PlayerProfile == null) return;

        }
    }

    [System.Serializable]
    public class TimelinePanel : MonoBehaviour
    {
        public void UpdateTimeline()
        {
            if (GameManager.Instance.CurrentGameState == null) return;
        }
    }
}
