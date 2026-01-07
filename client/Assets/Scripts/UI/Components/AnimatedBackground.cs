using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LifeCraft.UI.Components
{
    public class AnimatedBackground : MonoBehaviour
    {
        [Header("Gradient Settings")]
        [SerializeField] private Color topColor = new Color(0.188f, 0.153f, 0.133f, 1f);
        [SerializeField] private Color bottomColor = new Color(0.118f, 0.094f, 0.086f, 1f);
        [SerializeField] private Color accentColor = new Color(0.796f, 0.431f, 0.353f, 0.1f);
        
        [Header("Animation")]
        [SerializeField] private float gradientShiftSpeed = 0.3f;
        [SerializeField] private float gradientShiftAmount = 0.02f;
        [SerializeField] private bool animateGradient = true;
        
        [Header("Orbs")]
        [SerializeField] private int orbCount = 3;
        [SerializeField] private float orbMinSize = 200f;
        [SerializeField] private float orbMaxSize = 600f;
        [SerializeField] private float orbMinAlpha = 0.02f;
        [SerializeField] private float orbMaxAlpha = 0.08f;
        [SerializeField] private float orbMoveSpeed = 10f;
        
        private Image backgroundImage;
        private Image[] orbs;
        private Vector2[] orbVelocities;
        private RectTransform rectTransform;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            SetupBackground();
            SetupOrbs();
        }
        
        private void SetupBackground()
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            backgroundImage.color = bottomColor;
            backgroundImage.raycastTarget = false;
        }
        
        private void SetupOrbs()
        {
            orbs = new Image[orbCount];
            orbVelocities = new Vector2[orbCount];
            
            for (int i = 0; i < orbCount; i++)
            {
                CreateOrb(i);
            }
        }
        
        private void CreateOrb(int index)
        {
            GameObject orbObj = new GameObject($"BackgroundOrb_{index}");
            orbObj.transform.SetParent(transform, false);
            orbObj.transform.SetAsFirstSibling();
            
            RectTransform orbRect = orbObj.AddComponent<RectTransform>();
            float size = Random.Range(orbMinSize, orbMaxSize);
            orbRect.sizeDelta = new Vector2(size, size);
            
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            orbRect.anchoredPosition = new Vector2(
                Random.Range(-screenSize.x * 0.3f, screenSize.x * 0.3f),
                Random.Range(-screenSize.y * 0.3f, screenSize.y * 0.3f)
            );
            
            orbs[index] = orbObj.AddComponent<Image>();
            
            Color orbColor = index % 2 == 0 ? accentColor : 
                new Color(accentColor.r * 0.8f, accentColor.g * 1.2f, accentColor.b * 1.1f, accentColor.a);
            orbColor.a = Random.Range(orbMinAlpha, orbMaxAlpha);
            orbs[index].color = orbColor;
            orbs[index].raycastTarget = false;
            
            orbVelocities[index] = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized * Random.Range(0.5f, 1f);
        }
        
        private void Update()
        {
            if (animateGradient)
            {
                AnimateGradientShift();
            }
            
            AnimateOrbs();
        }
        
        private void AnimateGradientShift()
        {
            float shift = Mathf.Sin(Time.time * gradientShiftSpeed) * gradientShiftAmount;
            
            Color newColor = new Color(
                bottomColor.r + shift,
                bottomColor.g + shift * 0.8f,
                bottomColor.b + shift * 0.6f,
                bottomColor.a
            );
            
            backgroundImage.color = newColor;
        }
        
        private void AnimateOrbs()
        {
            if (orbs == null) return;
            
            for (int i = 0; i < orbs.Length; i++)
            {
                if (orbs[i] == null) continue;
                
                RectTransform orbRect = orbs[i].rectTransform;
                Vector2 pos = orbRect.anchoredPosition;
                
                pos += orbVelocities[i] * orbMoveSpeed * Time.deltaTime;
                
                Vector2 bounds = new Vector2(600f, 1000f);
                
                if (pos.x > bounds.x || pos.x < -bounds.x)
                {
                    orbVelocities[i].x *= -1f;
                    pos.x = Mathf.Clamp(pos.x, -bounds.x, bounds.x);
                }
                
                if (pos.y > bounds.y || pos.y < -bounds.y)
                {
                    orbVelocities[i].y *= -1f;
                    pos.y = Mathf.Clamp(pos.y, -bounds.y, bounds.y);
                }
                
                orbRect.anchoredPosition = pos;
            }
        }
        
        public void SetColors(Color top, Color bottom, Color accent)
        {
            topColor = top;
            bottomColor = bottom;
            accentColor = accent;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = bottomColor;
            }
        }
        
        public void FadeIn(float duration = 1f)
        {
            StartCoroutine(FadeInCoroutine(duration));
        }
        
        private IEnumerator FadeInCoroutine(float duration)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / duration;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
    }
}
