using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LifeCraft.Core;
using LifeCraft.Data;
using LifeCraft.Services;

namespace LifeCraft.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject LoginPanel;
        public GameObject MainMenuPanel;
        public GameObject CharacterCreationPanel;
        public GameObject GamePanel;

        [Header("Game UI")]
        public StatsPanel StatsPanel;
        public TimelinePanel TimelinePanel;
        public EventPopupPanel EventPopupPanel;

        public GameObject CareerDashboard;

        [Header("Message Toast")]
        [SerializeField] private GameObject messageToastPrefab;
        private GameObject activeToast;
        private Coroutine toastCoroutine;

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
            LoginPanel?.SetActive(true);
            MainMenuPanel?.SetActive(false);
            CharacterCreationPanel?.SetActive(false);
            GamePanel?.SetActive(false);
        }

        public void ShowMainMenuPanel()
        {
            LoginPanel?.SetActive(false);
            MainMenuPanel?.SetActive(true);
            CharacterCreationPanel?.SetActive(false);
            GamePanel?.SetActive(false);
        }

        public void ShowCharacterCreationPanel()
        {
            LoginPanel?.SetActive(false);
            MainMenuPanel?.SetActive(false);
            CharacterCreationPanel?.SetActive(true);
            GamePanel?.SetActive(false);
        }

        public void ShowGamePanel()
        {
            LoginPanel?.SetActive(false);
            MainMenuPanel?.SetActive(false);
            CharacterCreationPanel?.SetActive(false);
            GamePanel?.SetActive(true);
            RefreshUI();
        }

        public void RefreshUI()
        {
            StatsPanel?.UpdateStats();
            TimelinePanel?.UpdateTimeline();
        }

        public void ShowEventPopup(PlayerEvent playerEvent, EventTemplate eventTemplate)
        {
            EventPopupPanel?.ShowEvent(playerEvent, eventTemplate);
        }
        
        public void ShowMessage(string title, string message, float duration = 3f)
        {
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            
            if (activeToast != null)
            {
                Destroy(activeToast);
            }
            
            toastCoroutine = StartCoroutine(ShowMessageCoroutine(title, message, duration));
        }
        
        public void ShowError(string message)
        {
            ShowMessage("Error", message, 4f);
        }
        
        public void UpdateAvatarUI(Services.AvatarManager.AvailableAvatarsResponse data)
        {
            var avatarPanel = FindAnyObjectByType<AvatarPanel>();
            avatarPanel?.UpdateAvatarUI(data);
        }
        
        public void UpdateEquippedAvatar(Services.AvatarManager.CurrentAvatar equipped, string category)
        {
            var avatarPanel = FindAnyObjectByType<AvatarPanel>();
            avatarPanel?.UpdateEquippedAvatar(equipped, category);
        }
        
        private IEnumerator ShowMessageCoroutine(string title, string message, float duration)
        {
            activeToast = CreateToast(title, message);
            
            CanvasGroup toastCanvasGroup = activeToast.GetComponent<CanvasGroup>();
            if (toastCanvasGroup == null)
            {
                toastCanvasGroup = activeToast.AddComponent<CanvasGroup>();
            }
            
            toastCanvasGroup.alpha = 0f;
            float fadeTime = 0f;
            while (fadeTime < 0.3f)
            {
                fadeTime += Time.deltaTime;
                toastCanvasGroup.alpha = fadeTime / 0.3f;
                yield return null;
            }
            toastCanvasGroup.alpha = 1f;
            
            yield return new WaitForSeconds(duration);
            
            fadeTime = 0f;
            while (fadeTime < 0.3f)
            {
                fadeTime += Time.deltaTime;
                toastCanvasGroup.alpha = 1f - (fadeTime / 0.3f);
                yield return null;
            }
            
            Destroy(activeToast);
            activeToast = null;
            toastCoroutine = null;
        }
        
        private GameObject CreateToast(string title, string message)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>();
            }
            
            GameObject toast = new GameObject("MessageToast");
            toast.transform.SetParent(canvas.transform, false);
            
            RectTransform toastRect = toast.AddComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.1f, 0.85f);
            toastRect.anchorMax = new Vector2(0.9f, 0.95f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;
            
            Image toastBg = toast.AddComponent<Image>();
            toastBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(toast.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(16f, 0f);
            titleRect.offsetMax = new Vector2(-16f, -8f);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleLeft;
            
            GameObject messageObj = new GameObject("Message");
            messageObj.transform.SetParent(toast.transform, false);
            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0f, 0f);
            messageRect.anchorMax = new Vector2(1f, 0.5f);
            messageRect.offsetMin = new Vector2(16f, 8f);
            messageRect.offsetMax = new Vector2(-16f, 0f);
            
            Text messageText = messageObj.AddComponent<Text>();
            messageText.text = message;
            messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            messageText.fontSize = 14;
            messageText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            messageText.alignment = TextAnchor.MiddleLeft;
            
            return toast;
        }
    }

}
