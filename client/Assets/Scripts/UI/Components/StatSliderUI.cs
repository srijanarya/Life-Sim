using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

namespace LifeCraft.UI
{
    public class StatSliderUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Action<int> OnValueChanged;
        
        private string statName;
        private string description;
        private Color statColor;
        private Color textColor;
        private Color backgroundColor;
        
        private int currentValue;
        private int minValue;
        private int maxValue;
        private int dynamicMaxValue;
        
        private RectTransform rectTransform;
        private RectTransform fillBar;
        private RectTransform handleKnob;
        private Text valueText;
        private Text nameText;
        private Image fillImage;
        private Image trackImage;
        
        private bool isDragging = false;
        private Coroutine valueAnimation;
        private Coroutine pulseAnimation;
        
        public void Initialize(string name, string desc, Color color, int startValue, int min, int max, Color text, Color bg)
        {
            statName = name;
            description = desc;
            statColor = color;
            textColor = text;
            backgroundColor = bg;
            
            minValue = min;
            maxValue = max;
            dynamicMaxValue = max;
            currentValue = startValue;
            
            rectTransform = GetComponent<RectTransform>();
            
            BuildSliderUI();
            UpdateVisuals(false);
        }
        
        private void BuildSliderUI()
        {
            GameObject labelRow = new GameObject("LabelRow");
            labelRow.transform.SetParent(transform, false);
            
            RectTransform labelRect = labelRow.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(0f, 22f);
            
            GameObject nameObj = new GameObject("StatName");
            nameObj.transform.SetParent(labelRect, false);
            
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(0.7f, 1f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            nameText = nameObj.AddComponent<Text>();
            nameText.text = statName;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 14;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = textColor;
            nameText.alignment = TextAnchor.MiddleLeft;
            
            GameObject valueObj = new GameObject("StatValue");
            valueObj.transform.SetParent(labelRect, false);
            
            RectTransform valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.7f, 0f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            
            valueText = valueObj.AddComponent<Text>();
            valueText.text = currentValue.ToString();
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 16;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = statColor;
            valueText.alignment = TextAnchor.MiddleRight;
            
            GameObject sliderTrack = new GameObject("SliderTrack");
            sliderTrack.transform.SetParent(transform, false);
            
            RectTransform trackRect = sliderTrack.AddComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.anchoredPosition = new Vector2(0f, 0f);
            trackRect.sizeDelta = new Vector2(0f, 28f);
            
            trackImage = sliderTrack.AddComponent<Image>();
            trackImage.color = backgroundColor;
            trackImage.raycastTarget = true;
            
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(trackRect, false);
            
            fillBar = fillObj.AddComponent<RectTransform>();
            fillBar.anchorMin = new Vector2(0f, 0f);
            fillBar.anchorMax = new Vector2(0f, 1f);
            fillBar.pivot = new Vector2(0f, 0.5f);
            fillBar.offsetMin = new Vector2(0f, 2f);
            fillBar.offsetMax = new Vector2(0f, -2f);
            fillBar.anchoredPosition = new Vector2(0f, 0f);
            
            fillImage = fillObj.AddComponent<Image>();
            fillImage.color = statColor;
            fillImage.raycastTarget = false;
            
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(trackRect, false);
            
            handleKnob = handleObj.AddComponent<RectTransform>();
            handleKnob.anchorMin = new Vector2(0f, 0.5f);
            handleKnob.anchorMax = new Vector2(0f, 0.5f);
            handleKnob.pivot = new Vector2(0.5f, 0.5f);
            handleKnob.sizeDelta = new Vector2(20f, 20f);
            
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            handleImage.raycastTarget = false;
            
            GameObject innerHandle = new GameObject("Inner");
            innerHandle.transform.SetParent(handleKnob, false);
            
            RectTransform innerRect = innerHandle.AddComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(3f, 3f);
            innerRect.offsetMax = new Vector2(-3f, -3f);
            
            Image innerImage = innerHandle.AddComponent<Image>();
            innerImage.color = statColor;
            innerImage.raycastTarget = false;
            
            GameObject ticksContainer = new GameObject("Ticks");
            ticksContainer.transform.SetParent(trackRect, false);
            
            RectTransform ticksRect = ticksContainer.AddComponent<RectTransform>();
            ticksRect.anchorMin = Vector2.zero;
            ticksRect.anchorMax = Vector2.one;
            ticksRect.offsetMin = Vector2.zero;
            ticksRect.offsetMax = Vector2.zero;
            
            int tickCount = maxValue - minValue;
            for (int i = 0; i <= tickCount; i++)
            {
                float normalizedPos = (float)i / tickCount;
                
                GameObject tick = new GameObject($"Tick_{i}");
                tick.transform.SetParent(ticksRect, false);
                
                RectTransform tickRect = tick.AddComponent<RectTransform>();
                tickRect.anchorMin = new Vector2(normalizedPos, 0f);
                tickRect.anchorMax = new Vector2(normalizedPos, 1f);
                tickRect.pivot = new Vector2(0.5f, 0.5f);
                tickRect.sizeDelta = new Vector2(1f, 0f);
                tickRect.anchoredPosition = Vector2.zero;
                
                Image tickImage = tick.AddComponent<Image>();
                tickImage.color = new Color(textColor.r, textColor.g, textColor.b, 0.15f);
                tickImage.raycastTarget = false;
            }
            
            ticksContainer.transform.SetAsFirstSibling();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            UpdateValueFromPointer(eventData);
            
            if (pulseAnimation != null) StopCoroutine(pulseAnimation);
            pulseAnimation = StartCoroutine(PulseHandle());
            
            #if UNITY_IOS && !UNITY_EDITOR
            #endif
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            UpdateValueFromPointer(eventData);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            
            if (pulseAnimation != null)
            {
                StopCoroutine(pulseAnimation);
                pulseAnimation = null;
            }
            
            StartCoroutine(SettleHandle());
        }
        
        private void UpdateValueFromPointer(PointerEventData eventData)
        {
            RectTransform trackRect = trackImage.GetComponent<RectTransform>();
            
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                return;
            }
            
            float trackWidth = trackRect.rect.width;
            float normalizedX = Mathf.Clamp01((localPoint.x + trackWidth * 0.5f) / trackWidth);
            
            int range = dynamicMaxValue - minValue;
            int newValue = Mathf.RoundToInt(normalizedX * range) + minValue;
            newValue = Mathf.Clamp(newValue, minValue, dynamicMaxValue);
            
            if (newValue != currentValue)
            {
                currentValue = newValue;
                UpdateVisuals(true);
                OnValueChanged?.Invoke(currentValue);
            }
        }
        
        private void UpdateVisuals(bool animate)
        {
            if (valueText != null)
            {
                valueText.text = currentValue.ToString();
            }
            
            float normalizedValue = (float)(currentValue - minValue) / (maxValue - minValue);
            
            if (fillBar != null)
            {
                RectTransform trackRect = trackImage.GetComponent<RectTransform>();
                float targetWidth = trackRect.rect.width * normalizedValue;
                
                if (animate && valueAnimation != null)
                {
                    StopCoroutine(valueAnimation);
                }
                
                if (animate && gameObject.activeInHierarchy)
                {
                    valueAnimation = StartCoroutine(AnimateFill(targetWidth, 0.15f));
                }
                else
                {
                    fillBar.sizeDelta = new Vector2(targetWidth, fillBar.sizeDelta.y);
                    UpdateHandlePosition(normalizedValue);
                }
            }
        }
        
        private IEnumerator AnimateFill(float targetWidth, float duration)
        {
            float startWidth = fillBar.sizeDelta.x;
            float elapsed = 0f;
            
            float normalizedTarget = (float)(currentValue - minValue) / (maxValue - minValue);
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                
                float currentWidth = Mathf.Lerp(startWidth, targetWidth, t);
                fillBar.sizeDelta = new Vector2(currentWidth, fillBar.sizeDelta.y);
                
                RectTransform trackRect = trackImage.GetComponent<RectTransform>();
                float normalizedPos = currentWidth / trackRect.rect.width;
                UpdateHandlePosition(normalizedPos);
                
                yield return null;
            }
            
            fillBar.sizeDelta = new Vector2(targetWidth, fillBar.sizeDelta.y);
            UpdateHandlePosition(normalizedTarget);
        }
        
        private void UpdateHandlePosition(float normalizedValue)
        {
            if (handleKnob == null || trackImage == null) return;
            
            RectTransform trackRect = trackImage.GetComponent<RectTransform>();
            float trackWidth = trackRect.rect.width;
            
            float xPos = (normalizedValue * trackWidth) - (trackWidth * 0.5f);
            handleKnob.anchoredPosition = new Vector2(xPos + trackWidth * 0.5f, 0f);
        }
        
        private IEnumerator PulseHandle()
        {
            if (handleKnob == null) yield break;
            
            Vector2 originalSize = handleKnob.sizeDelta;
            Vector2 expandedSize = originalSize * 1.2f;
            
            handleKnob.sizeDelta = expandedSize;
            
            while (isDragging)
            {
                yield return null;
            }
        }
        
        private IEnumerator SettleHandle()
        {
            if (handleKnob == null) yield break;
            
            Vector2 currentSize = handleKnob.sizeDelta;
            Vector2 targetSize = new Vector2(20f, 20f);
            float duration = 0.15f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(elapsed / duration);
                handleKnob.sizeDelta = Vector2.Lerp(currentSize, targetSize, t);
                yield return null;
            }
            
            handleKnob.sizeDelta = targetSize;
        }
        
        public void SetValue(int value, bool triggerCallback = true)
        {
            value = Mathf.Clamp(value, minValue, dynamicMaxValue);
            
            if (value != currentValue)
            {
                currentValue = value;
                UpdateVisuals(true);
                
                if (triggerCallback)
                {
                    OnValueChanged?.Invoke(currentValue);
                }
            }
        }
        
        public void SetMaxValue(int max)
        {
            dynamicMaxValue = Mathf.Clamp(max, minValue, maxValue);
            
            if (currentValue > dynamicMaxValue)
            {
                SetValue(dynamicMaxValue, true);
            }
            
            UpdateTrackAppearance();
        }
        
        private void UpdateTrackAppearance()
        {
            if (trackImage == null) return;
            
            float normalizedMax = (float)(dynamicMaxValue - minValue) / (maxValue - minValue);
            
            if (normalizedMax < 1f)
            {
                trackImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.5f);
            }
            else
            {
                trackImage.color = backgroundColor;
            }
        }
        
        public int GetValue()
        {
            return currentValue;
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
