using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace LifeCraft.UI
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }
        
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;
        
        private CanvasGroup fadeCanvasGroup;
        private Image fadeImage;
        private bool isTransitioning = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                CreateFadeOverlay();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void CreateFadeOverlay()
        {
            GameObject canvasObj = new GameObject("TransitionCanvas");
            canvasObj.transform.SetParent(transform);
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform fadeRect = fadeObj.AddComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.offsetMin = Vector2.zero;
            fadeRect.offsetMax = Vector2.zero;
            
            fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = true;
            
            fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
        
        public void LoadScene(string sceneName, System.Action onComplete = null)
        {
            if (isTransitioning) return;
            StartCoroutine(TransitionToScene(sceneName, onComplete));
        }
        
        public void LoadScene(int sceneIndex, System.Action onComplete = null)
        {
            if (isTransitioning) return;
            StartCoroutine(TransitionToScene(sceneIndex, onComplete));
        }
        
        private IEnumerator TransitionToScene(string sceneName, System.Action onComplete)
        {
            isTransitioning = true;
            
            yield return StartCoroutine(FadeIn());
            
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
            loadOperation.allowSceneActivation = false;
            
            while (loadOperation.progress < 0.9f)
            {
                yield return null;
            }
            
            loadOperation.allowSceneActivation = true;
            
            yield return new WaitForSeconds(0.1f);
            
            yield return StartCoroutine(FadeOut());
            
            isTransitioning = false;
            onComplete?.Invoke();
        }
        
        private IEnumerator TransitionToScene(int sceneIndex, System.Action onComplete)
        {
            isTransitioning = true;
            
            yield return StartCoroutine(FadeIn());
            
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex);
            loadOperation.allowSceneActivation = false;
            
            while (loadOperation.progress < 0.9f)
            {
                yield return null;
            }
            
            loadOperation.allowSceneActivation = true;
            
            yield return new WaitForSeconds(0.1f);
            
            yield return StartCoroutine(FadeOut());
            
            isTransitioning = false;
            onComplete?.Invoke();
        }
        
        public IEnumerator FadeIn()
        {
            fadeCanvasGroup.blocksRaycasts = true;
            
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseInOutQuad(elapsed / transitionDuration);
                fadeCanvasGroup.alpha = t;
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
        }
        
        public IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseInOutQuad(elapsed / transitionDuration);
                fadeCanvasGroup.alpha = 1f - t;
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        
        private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
            if (fadeImage != null)
            {
                fadeImage.color = color;
            }
        }
        
        public bool IsTransitioning => isTransitioning;
    }
}
