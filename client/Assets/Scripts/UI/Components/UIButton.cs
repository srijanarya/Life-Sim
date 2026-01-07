using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace LifeCraft.UI.Components
{
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Tertiary,
        Icon
    }
    
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Style")]
        [SerializeField] private ButtonStyle buttonStyle = ButtonStyle.Primary;
        
        [Header("Animation")]
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float animationDuration = 0.1f;
        [SerializeField] private bool enableHapticFeedback = true;
        
        [Header("Colors")]
        [SerializeField] private Color primaryColor = new Color(0.796f, 0.431f, 0.353f, 1f);
        [SerializeField] private Color secondaryColor = new Color(0.976f, 0.961f, 0.937f, 1f);
        [SerializeField] private Color tertiaryColor = new Color(0.6f, 0.667f, 0.596f, 1f);
        [SerializeField] private Color primaryTextColor = Color.white;
        [SerializeField] private Color secondaryTextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        private Button button;
        private Image backgroundImage;
        private Text buttonText;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Coroutine scaleAnimation;
        private bool isPressed = false;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            backgroundImage = GetComponent<Image>();
            buttonText = GetComponentInChildren<Text>();
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
        }
        
        private void Start()
        {
            ApplyStyle();
            EnsureMinimumTouchSize();
        }
        
        private void ApplyStyle()
        {
            switch (buttonStyle)
            {
                case ButtonStyle.Primary:
                    backgroundImage.color = primaryColor;
                    if (buttonText != null) buttonText.color = primaryTextColor;
                    break;
                    
                case ButtonStyle.Secondary:
                    backgroundImage.color = secondaryColor;
                    if (buttonText != null) buttonText.color = secondaryTextColor;
                    break;
                    
                case ButtonStyle.Tertiary:
                    backgroundImage.color = tertiaryColor;
                    if (buttonText != null) buttonText.color = primaryTextColor;
                    break;
                    
                case ButtonStyle.Icon:
                    backgroundImage.color = Color.clear;
                    break;
            }
        }
        
        private void EnsureMinimumTouchSize()
        {
            const float minTouchSize = 44f;
            
            Vector2 currentSize = rectTransform.sizeDelta;
            
            if (currentSize.x < minTouchSize || currentSize.y < minTouchSize)
            {
                rectTransform.sizeDelta = new Vector2(
                    Mathf.Max(currentSize.x, minTouchSize),
                    Mathf.Max(currentSize.y, minTouchSize)
                );
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;
            
            isPressed = true;
            AnimateToScale(originalScale * pressScale);
            
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback();
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            
            isPressed = false;
            AnimateToScale(originalScale);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isPressed) return;
            
            isPressed = false;
            AnimateToScale(originalScale);
        }
        
        private void AnimateToScale(Vector3 targetScale)
        {
            if (scaleAnimation != null)
            {
                StopCoroutine(scaleAnimation);
            }
            scaleAnimation = StartCoroutine(ScaleAnimation(targetScale));
        }
        
        private IEnumerator ScaleAnimation(Vector3 targetScale)
        {
            Vector3 startScale = rectTransform.localScale;
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / animationDuration);
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            rectTransform.localScale = targetScale;
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        
        private void TriggerHapticFeedback()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
            #endif
        }
        
        public void SetStyle(ButtonStyle style)
        {
            buttonStyle = style;
            ApplyStyle();
        }
        
        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
            
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = interactable ? 1f : 0.5f;
        }
    }
}
