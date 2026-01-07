using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    public class EventPopupPanel : MonoBehaviour
    {
        [Header("UI Components")]
        public CanvasGroup PopupCanvasGroup;
        public TextMeshProUGUI EventTitle;
        public TextMeshProUGUI EventDescription;
        public Transform DecisionContainer;
        public DecisionButton DecisionButtonPrefab;
        public TextMeshProUGUI RarityLabel;
        public Image EventIcon;
        public GameObject Backdrop;

        [Header("Colors")]
        public Color LifeEventColor = new Color(0.6f, 0.667f, 0.596f);
        public Color CareerEventColor = new Color(0.796f, 0.431f, 0.353f);
        public Color RelationshipEventColor = new Color(1f, 0.6f, 0.6f);
        public Color RandomEventColor = new Color(0.5f, 0.6f, 0.8f);
        public Color DailyChallengeColor = new Color(1f, 0.75f, 0.3f);
        public Color CommonRarityColor = new Color(0.5f, 0.5f, 0.5f);
        public Color UncommonRarityColor = new Color(0.2f, 0.6f, 0.8f);
        public Color RareRarityColor = new Color(0.6f, 0.2f, 0.8f);
        public Color EpicRarityColor = new Color(0.6f, 0.4f, 0.8f);
        public Color LegendaryRarityColor = new Color(1f, 0.8f, 0.2f);

        [Header("Settings")]
        public float ShowAnimationDuration = 0.5f;
        public float HideAnimationDuration = 0.3f;
        public float ButtonSpawnDelay = 0.1f;

        private PlayerEvent currentEvent;
        private EventTemplate currentTemplate;
        private List<DecisionButton> decisionButtons = new List<DecisionButton>();
        private Coroutine showCoroutine;
        private Coroutine hideCoroutine;

        private void Awake()
        {
            if (Backdrop != null)
            {
                Backdrop.SetActive(false);
            }

            if (PopupCanvasGroup != null)
            {
                PopupCanvasGroup.alpha = 0f;
                PopupCanvasGroup.interactable = false;
                PopupCanvasGroup.blocksRaycasts = false;
            }
        }

        public void ShowEvent(PlayerEvent playerEvent, EventTemplate eventTemplate)
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
            }

            currentEvent = playerEvent;
            currentTemplate = eventTemplate;

            showCoroutine = StartCoroutine(ShowEventCoroutine());
        }

        private IEnumerator ShowEventCoroutine()
        {
            yield return null;

            if (Backdrop != null)
            {
                Backdrop.SetActive(true);
            }

            UpdateEventInfo();
            SpawnDecisionButtons();

            if (PopupCanvasGroup != null)
            {
                PopupCanvasGroup.interactable = true;
                PopupCanvasGroup.blocksRaycasts = true;

                float elapsed = 0f;
                while (elapsed < ShowAnimationDuration)
                {
                    elapsed += Time.deltaTime;
                    PopupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / ShowAnimationDuration);
                    yield return null;
                }

                PopupCanvasGroup.alpha = 1f;
            }

            PlayShowAnimation();
        }

        private void UpdateEventInfo()
        {
            if (currentTemplate == null) return;

            if (EventTitle != null)
            {
                EventTitle.text = currentTemplate.Title;
            }

            if (EventDescription != null)
            {
                EventDescription.text = currentTemplate.Description;
            }

            if (RarityLabel != null)
            {
                RarityLabel.text = currentTemplate.Rarity;
                RarityLabel.color = GetRarityColor(currentTemplate.Rarity);
            }

            if (EventIcon != null)
            {
                EventIcon.color = GetEventTypeColor(currentTemplate.EventType);
            }
        }

        private Color GetRarityColor(string rarity)
        {
            switch (rarity)
            {
                case "COMMON":
                    return CommonRarityColor;
                case "UNCOMMON":
                    return UncommonRarityColor;
                case "RARE":
                    return RareRarityColor;
                case "EPIC":
                    return EpicRarityColor;
                case "LEGENDARY":
                    return LegendaryRarityColor;
                default:
                    return CommonRarityColor;
            }
        }

        private Color GetEventTypeColor(string eventType)
        {
            switch (eventType)
            {
                case "LIFE_EVENT":
                    return LifeEventColor;
                case "CAREER_EVENT":
                    return CareerEventColor;
                case "RELATIONSHIP_EVENT":
                    return RelationshipEventColor;
                case "RANDOM_EVENT":
                    return RandomEventColor;
                case "DAILY_CHALLENGE":
                    return DailyChallengeColor;
                default:
                    return LifeEventColor;
            }
        }

        private void SpawnDecisionButtons()
        {
            ClearDecisionButtons();

            if (currentTemplate.Decisions == null || currentTemplate.Decisions.Count == 0)
            {
                Debug.LogWarning("No decisions available for event");
                return;
            }

            foreach (var decision in currentTemplate.Decisions)
            {
                StartCoroutine(SpawnDecisionButton(decision));
            }
        }

        private IEnumerator SpawnDecisionButton(DecisionTemplate decision)
        {
            yield return new WaitForSeconds(ButtonSpawnDelay);

            var button = Instantiate(DecisionButtonPrefab, DecisionContainer);
            button.Initialize(decision);
            decisionButtons.Add(button);
        }

        private void ClearDecisionButtons()
        {
            foreach (var button in decisionButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            decisionButtons.Clear();
        }

        public void Hide()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            hideCoroutine = StartCoroutine(HideCoroutine());
        }

        private IEnumerator HideCoroutine()
        {
            if (PopupCanvasGroup != null)
            {
                PopupCanvasGroup.interactable = false;
                PopupCanvasGroup.blocksRaycasts = false;

                float elapsed = 0f;
                while (elapsed < HideAnimationDuration)
                {
                    elapsed += Time.deltaTime;
                    PopupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / HideAnimationDuration);
                    yield return null;
                }

                PopupCanvasGroup.alpha = 0f;
            }

            if (Backdrop != null)
            {
                Backdrop.SetActive(false);
            }

            currentEvent = null;
            currentTemplate = null;
        }

        private void PlayShowAnimation()
        {
            if (EventTitle != null)
            {
                EventTitle.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(EventTitle.transform, Vector3.one, 0.3f, EaseOutBack));
            }

            if (EventDescription != null)
            {
                EventDescription.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateScale(EventDescription.transform, Vector3.one, 0.3f, EaseOutBack, 0.1f));
            }
        }

        private IEnumerator AnimateScale(Transform target, Vector3 endScale, float duration, System.Func<float, float> easing, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            Vector3 startScale = target.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easing(elapsed / duration);
                target.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            target.localScale = endScale;
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public void OnDecisionSelected(DecisionTemplate decision)
        {
            if (currentEvent == null || currentTemplate == null) return;

            StartCoroutine(ProcessDecision(decision));
        }

        private IEnumerator ProcessDecision(DecisionTemplate decision)
        {
            Hide();

            yield return new WaitForSeconds(HideAnimationDuration);

            GameManager.Instance?.MakeDecision(currentEvent.Id, decision.Id);
        }

        private void OnDestroy()
        {
            ClearDecisionButtons();
        }
    }

    [System.Serializable]
    public class DecisionButton : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI DecisionText;
        public TextMeshProUGUI OutcomesPreview;
        public Image Background;
        public Button Button;

        [Header("Colors")]
        public Color NormalColor = new Color(0.2f, 0.2f, 0.2f);
        public Color HighlightColor = new Color(0.8f, 0.4f, 0.4f);
        public Color TextColor = Color.white;

        [Header("Animation")]
        public float PressScale = 0.9f;

        private DecisionTemplate decisionData;

        public void Initialize(DecisionTemplate decision)
        {
            decisionData = decision;

            if (DecisionText != null)
            {
                DecisionText.text = decision.Text;
            }

            if (OutcomesPreview != null)
            {
                OutcomesPreview.text = FormatOutcomes(decision.Outcomes);
            }

            if (Button != null)
            {
                Button.onClick.AddListener(OnButtonClicked);
            }

            if (Background != null)
            {
                Background.color = NormalColor;
            }
        }

        private string FormatOutcomes(Dictionary<string, object> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0) return "";

            var preview = new System.Text.StringBuilder();
            int count = 0;

            foreach (var kvp in outcomes)
            {
                if (kvp.Value is int && (int)kvp.Value != 0)
                {
                    int value = (int)kvp.Value;
                    string sign = value > 0 ? "+" : "";
                    preview.Append($"{sign}{value} {FormatStatKey(kvp.Key)}");
                    count++;

                    if (count >= 2) break;
                }
            }

            return preview.ToString();
        }

        private string FormatStatKey(string key)
        {
            return key.ToLower()
                .Replace("boost", "")
                .Replace("penalty", "")
                .Replace("change", "")
                .Replace("intelligence", "Int")
                .Replace("charisma", "Cha")
                .Replace("physical", "Phy")
                .Replace("creativity", "Crea")
                .Replace("health", "Hlth")
                .Replace("happiness", "Hap");
        }

        private void OnButtonClicked()
        {
            if (Background != null)
            {
                Background.color = HighlightColor;
            }

            StartCoroutine(ButtonPressAnimation());
        }

        private IEnumerator ButtonPressAnimation()
        {
            if (transform == null) yield break;

            transform.localScale = Vector3.one * PressScale;
            yield return new WaitForSeconds(0.05f);
            transform.localScale = Vector3.one;

            InvokeButtonSelected();
        }

        private void InvokeButtonSelected()
        {
            var popup = GetComponentInParent<EventPopupPanel>();
            popup?.OnDecisionSelected(decisionData);
        }
    }
}
