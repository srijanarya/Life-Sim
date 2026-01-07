using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LifeCraft.Core;

namespace LifeCraft.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Visual Elements")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform logoTransform;
        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private Image backgroundGradient;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.8f;
        [SerializeField] private float logoAnimationDelay = 0.2f;
        [SerializeField] private float buttonStaggerDelay = 0.1f;
        [SerializeField] private float buttonScaleOnPress = 0.95f;
        [SerializeField] private float buttonAnimationDuration = 0.15f;
        
        [Header("Floating Particles")]
        [SerializeField] private RectTransform particleContainer;
        [SerializeField] private int particleCount = 8;
        
        private Coroutine[] buttonAnimations;
        private Vector3[] originalButtonScales;
        private bool isTransitioning = false;
        private bool hasSavedGame = false;
        
        private void Awake()
        {
            CacheOriginalButtonScales();
        }
        
        private void Start()
        {
            SetupButtonListeners();
            CheckForSavedGame();
            
            if (panelCanvasGroup != null)
            {
                StartCoroutine(PlayEntranceAnimation());
            }
            
            if (particleContainer != null)
            {
                StartCoroutine(AnimateFloatingParticles());
            }
        }
        
        private void OnEnable()
        {
            CheckForSavedGame();
            
            if (panelCanvasGroup != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(PlayEntranceAnimation());
            }
        }
        
        private void CacheOriginalButtonScales()
        {
            if (buttonContainer == null) return;
            
            int childCount = buttonContainer.childCount;
            originalButtonScales = new Vector3[childCount];
            buttonAnimations = new Coroutine[childCount];
            
            for (int i = 0; i < childCount; i++)
            {
                originalButtonScales[i] = buttonContainer.GetChild(i).localScale;
            }
        }
        
        private void SetupButtonListeners()
        {
            SetupButton(newGameButton, OnNewGameClicked, 0);
            SetupButton(continueButton, OnContinueClicked, 1);
            SetupButton(settingsButton, OnSettingsClicked, 2);
            SetupButton(quitButton, OnQuitClicked, 3);
        }
        
        private void SetupButton(Button button, UnityEngine.Events.UnityAction clickHandler, int index)
        {
            if (button == null) return;
            
            button.onClick.AddListener(clickHandler);
            SetupButtonPressAnimation(button, index);
        }
        
        private void SetupButtonPressAnimation(Button button, int index)
        {
            var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            AddTriggerEntry(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, 
                () => AnimateButtonPress(button.transform, index, true));
            
            AddTriggerEntry(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerUp, 
                () => AnimateButtonPress(button.transform, index, false));
            
            AddTriggerEntry(eventTrigger, UnityEngine.EventSystems.EventTriggerType.PointerExit, 
                () => AnimateButtonPress(button.transform, index, false));
        }
        
        private void AddTriggerEntry(UnityEngine.EventSystems.EventTrigger trigger, 
            UnityEngine.EventSystems.EventTriggerType eventType, System.Action callback)
        {
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener((_) => callback());
            trigger.triggers.Add(entry);
        }
        
        private void AnimateButtonPress(Transform buttonTransform, int index, bool isPressed)
        {
            if (buttonAnimations == null || index >= buttonAnimations.Length) return;
            
            if (buttonAnimations[index] != null)
            {
                StopCoroutine(buttonAnimations[index]);
            }
            
            Vector3 targetScale = isPressed 
                ? originalButtonScales[index] * buttonScaleOnPress 
                : originalButtonScales[index];
                
            buttonAnimations[index] = StartCoroutine(
                AnimateScale(buttonTransform, targetScale, buttonAnimationDuration)
            );
        }
        
        #region Button Click Handlers
        
        private void OnNewGameClicked()
        {
            if (isTransitioning) return;
            
            PlayButtonSound();
            StartCoroutine(TransitionToNewGame());
        }
        
        private void OnContinueClicked()
        {
            if (isTransitioning || !hasSavedGame) return;
            
            PlayButtonSound();
            StartCoroutine(TransitionToContinueGame());
        }
        
        private void OnSettingsClicked()
        {
            if (isTransitioning) return;
            
            PlayButtonSound();
            ShowSettings();
        }
        
        private void OnQuitClicked()
        {
            if (isTransitioning) return;
            
            PlayButtonSound();
            StartCoroutine(TransitionToQuit());
        }
        
        #endregion
        
        #region Game Flow
        
        private void CheckForSavedGame()
        {
            hasSavedGame = PlayerPrefs.HasKey("LastGameId") || 
                          (GameManager.Instance != null && GameManager.Instance.CurrentGameState != null);
            
            UpdateContinueButtonState();
        }
        
        private void UpdateContinueButtonState()
        {
            if (continueButton == null) return;
            
            continueButton.interactable = hasSavedGame;
            
            var canvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = hasSavedGame ? 1f : 0.5f;
        }
        
        private IEnumerator TransitionToNewGame()
        {
            isTransitioning = true;
            
            yield return StartCoroutine(FadePanel(0f, fadeInDuration * 0.6f));
            
            UIManager.Instance?.ShowCharacterCreationPanel();
            
            isTransitioning = false;
        }
        
        private IEnumerator TransitionToContinueGame()
        {
            isTransitioning = true;
            
            yield return StartCoroutine(FadePanel(0f, fadeInDuration * 0.6f));
            
            string lastGameId = PlayerPrefs.GetString("LastGameId", "");
            
            if (!string.IsNullOrEmpty(lastGameId))
            {
                yield return ApiClient.Instance.Get<Data.GameState>(
                    $"/api/game/{lastGameId}",
                    OnGameLoaded,
                    OnGameLoadFailed
                );
            }
            else if (GameManager.Instance?.CurrentGameState != null)
            {
                UIManager.Instance?.ShowGamePanel();
            }
            
            isTransitioning = false;
        }
        
        private void OnGameLoaded(Data.GameState gameState)
        {
            GameManager.Instance?.SetGameState(gameState);
            UIManager.Instance?.ShowGamePanel();
        }
        
        private void OnGameLoadFailed(string error)
        {
            Debug.LogError($"Failed to load game: {error}");
            UIManager.Instance?.ShowMessage("Error", "Failed to load saved game.", 3f);
            StartCoroutine(FadePanel(1f, fadeInDuration * 0.6f));
        }
        
        private void ShowSettings()
        {
            Debug.Log("Settings clicked - implement settings panel");
            UIManager.Instance?.ShowMessage("Coming Soon", "Settings panel is under construction.", 2f);
        }
        
        private IEnumerator TransitionToQuit()
        {
            isTransitioning = true;
            
            yield return StartCoroutine(FadePanel(0f, fadeInDuration));
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        #endregion
        
        #region Animations
        
        private IEnumerator PlayEntranceAnimation()
        {
            ResetAnimationState();
            
            yield return StartCoroutine(FadePanel(1f, fadeInDuration * 0.5f));
            
            yield return new WaitForSeconds(logoAnimationDelay);
            
            AnimateLogo();
            
            yield return new WaitForSeconds(0.2f);
            
            yield return StartCoroutine(StaggerButtonAnimations());
        }
        
        private void ResetAnimationState()
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
            }
            
            if (logoTransform != null)
            {
                logoTransform.localScale = Vector3.one * 0.8f;
                logoTransform.anchoredPosition += Vector2.up * 30f;
            }
            
            if (buttonContainer != null)
            {
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    buttonContainer.GetChild(i).localScale = Vector3.zero;
                }
            }
        }
        
        private void AnimateLogo()
        {
            if (logoTransform == null) return;
            
            StartCoroutine(AnimateScale(logoTransform, Vector3.one, 0.4f, EaseOutBack));
            StartCoroutine(AnimateAnchoredPosition(logoTransform, logoTransform.anchoredPosition - Vector2.up * 30f, 0.4f));
        }
        
        private IEnumerator StaggerButtonAnimations()
        {
            if (buttonContainer == null) yield break;
            
            for (int i = 0; i < buttonContainer.childCount; i++)
            {
                StartCoroutine(AnimateScale(buttonContainer.GetChild(i), Vector3.one, 0.3f, EaseOutBack));
                yield return new WaitForSeconds(buttonStaggerDelay);
            }
        }
        
        private IEnumerator FadePanel(float targetAlpha, float duration)
        {
            if (panelCanvasGroup == null) yield break;
            
            float startAlpha = panelCanvasGroup.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseInOutQuad(elapsed / duration);
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            panelCanvasGroup.alpha = targetAlpha;
        }
        
        private IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration, System.Func<float, float> easeFunction = null)
        {
            if (target == null) yield break;
            
            easeFunction ??= EaseOutQuad;
            
            Vector3 startScale = target.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easeFunction(Mathf.Clamp01(elapsed / duration));
                target.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
                yield return null;
            }
            
            target.localScale = targetScale;
        }
        
        private IEnumerator AnimateAnchoredPosition(RectTransform target, Vector2 targetPos, float duration)
        {
            if (target == null) yield break;
            
            Vector2 startPos = target.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                target.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            target.anchoredPosition = targetPos;
        }
        
        private IEnumerator AnimateFloatingParticles()
        {
            if (particleContainer == null) yield break;
            
            for (int i = 0; i < particleCount; i++)
            {
                CreateFloatingParticle(i);
            }
            
            yield return null;
        }
        
        private void CreateFloatingParticle(int index)
        {
            GameObject particle = new GameObject($"Particle_{index}");
            particle.transform.SetParent(particleContainer, false);
            
            RectTransform rect = particle.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Random.Range(20f, 40f), Random.Range(20f, 40f));
            rect.anchoredPosition = new Vector2(Random.Range(-200f, 200f), Random.Range(-400f, -100f));
            
            Image img = particle.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, Random.Range(0.05f, 0.15f));
            
            CanvasGroup cg = particle.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            
            StartCoroutine(FloatParticle(rect, cg, index));
        }
        
        private IEnumerator FloatParticle(RectTransform rect, CanvasGroup canvasGroup, int index)
        {
            yield return new WaitForSeconds(index * 0.3f);
            
            float floatSpeed = Random.Range(15f, 35f);
            float swayAmount = Random.Range(20f, 60f);
            float swaySpeed = Random.Range(0.5f, 1.5f);
            float startX = rect.anchoredPosition.x;
            float targetAlpha = Random.Range(0.05f, 0.2f);
            
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, targetAlpha, 1f));
            
            while (gameObject.activeInHierarchy)
            {
                float newY = rect.anchoredPosition.y + floatSpeed * Time.deltaTime;
                float newX = startX + Mathf.Sin(Time.time * swaySpeed) * swayAmount;
                
                rect.anchoredPosition = new Vector2(newX, newY);
                
                if (newY > 500f)
                {
                    yield return StartCoroutine(FadeCanvasGroup(canvasGroup, targetAlpha, 0f, 0.5f));
                    
                    startX = Random.Range(-200f, 200f);
                    rect.anchoredPosition = new Vector2(startX, Random.Range(-450f, -350f));
                    
                    yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, targetAlpha, 0.5f));
                }
                
                yield return null;
            }
        }
        
        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
        }
        
        #endregion
        
        #region Easing Functions
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        
        private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        #endregion
        
        #region Audio
        
        private void PlayButtonSound()
        {
        }
        
        #endregion
        
        private void OnDestroy()
        {
            newGameButton?.onClick.RemoveAllListeners();
            continueButton?.onClick.RemoveAllListeners();
            settingsButton?.onClick.RemoveAllListeners();
            quitButton?.onClick.RemoveAllListeners();
        }
    }
}
