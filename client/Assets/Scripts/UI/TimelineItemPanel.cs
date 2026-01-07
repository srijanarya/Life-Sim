using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    public class TimelineItemPanel : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI EventTitle;
        public TextMeshProUGUI YearMonthText;
        public TextMeshProUGUI AgeText;
        public TextMeshProUGUI StatChangesText;
        public Image EventTypeIndicator;
        public GameObject ExpandedDetails;

        [Header("Settings")]
        public float DefaultHeight = 120f;
        public float ExpandedHeight = 200f;
        public Color BackgroundColor = new Color(0.18f, 0.09f, 0.09f);

        private PlayerEvent eventData;
        private RectTransform rectTransform;
        private bool isExpanded = false;
        private Coroutine expandCoroutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (ExpandedDetails != null)
            {
                ExpandedDetails.SetActive(false);
            }
        }

        public void Initialize(PlayerEvent playerEvent, Color eventColor)
        {
            eventData = playerEvent;

            if (EventTitle != null && playerEvent.Template != null)
            {
                EventTitle.text = playerEvent.Template.Title;
            }

            if (YearMonthText != null)
            {
                YearMonthText.text = $"Year {playerEvent.YearOccurred}, Month {playerEvent.MonthOccurred}";
            }

            if (AgeText != null)
            {
                AgeText.text = $"Age {playerEvent.AgeOccurred}";
            }

            if (EventTypeIndicator != null)
            {
                EventTypeIndicator.color = eventColor;
            }

            if (StatChangesText != null)
            {
                string statChanges = FormatStatChanges();
                StatChangesText.text = statChanges;
            }

            GetComponent<Image>().color = BackgroundColor;
        }

        private string FormatStatChanges()
        {
            if (eventData.Decisions == null || eventData.Decisions.Count == 0)
            {
                return "";
            }

            var outcomes = eventData.Decisions[0].Outcomes as Dictionary<string, object>;
            if (outcomes == null) return "";

            var changes = new System.Text.StringBuilder();

            foreach (var kvp in outcomes)
            {
                if (kvp.Value is int)
                {
                    int value = (int)kvp.Value;
                    string sign = value > 0 ? "+" : "";
                    string formattedStat = FormatStatName(kvp.Key);
                    changes.AppendLine($"{sign}{value} {formattedStat}");
                }
            }

            return changes.ToString().Trim();
        }

        private string FormatStatName(string statKey)
        {
            return statKey.Replace("Boost", "").Replace("Penalty", "").ToLower();
        }

        public void ToggleExpand()
        {
            if (expandCoroutine != null)
            {
                StopCoroutine(expandCoroutine);
            }

            expandCoroutine = StartCoroutine(AnimateExpand());
        }

        private IEnumerator AnimateExpand()
        {
            isExpanded = !isExpanded;
            float targetHeight = isExpanded ? ExpandedHeight : DefaultHeight;
            float currentHeight = rectTransform.sizeDelta.y;
            float duration = 0.3f;
            float elapsedTime = 0f;

            if (ExpandedDetails != null)
            {
                ExpandedDetails.SetActive(isExpanded);
            }

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float easedT = EaseOutQuad(t);

                rectTransform.sizeDelta = new Vector2(
                    rectTransform.sizeDelta.x,
                    Mathf.Lerp(currentHeight, targetHeight, easedT)
                );

                yield return null;
            }

            rectTransform.sizeDelta = new Vector2(
                rectTransform.sizeDelta.x,
                targetHeight
            );
        }

        private float EaseOutQuad(float t)
        {
            return t * (2f - t);
        }

        public float GetHeight()
        {
            return isExpanded ? ExpandedHeight : DefaultHeight;
        }

        public void OnPointerClick()
        {
            ToggleExpand();
        }
    }
}
