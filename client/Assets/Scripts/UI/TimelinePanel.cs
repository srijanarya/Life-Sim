using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    public class TimelinePanel : MonoBehaviour
    {
        [Header("UI References")]
        public ScrollRect TimelineScrollRect;
        public Transform ContentContainer;
        public TimelineItemPanel TimelineItemPrefab;
        public GameObject EmptyState;
        public GameObject LoadingSpinner;

        [Header("Colors")]
        public Color LifeEventColor = new Color(0.6f, 0.667f, 0.596f);
        public Color CareerEventColor = new Color(0.796f, 0.431f, 0.353f);
        public Color RelationshipEventColor = new Color(1f, 0.6f, 0.6f);
        public Color RandomEventColor = new Color(0.5f, 0.6f, 0.8f);
        public Color DailyChallengeColor = new Color(1f, 0.75f, 0.3f);

        [Header("Settings")]
        public float ItemSpacing = 20f;
        public int ItemsPerPage = 20;
        public float AnimationDelay = 0.1f;

        private List<PlayerEvent> cachedEvents = new List<PlayerEvent>();
        private bool isLoading = false;
        private ObjectPool<TimelineItemPanel> itemPool;

        private void Start()
        {
            InitializeObjectPool();
        }

        private void InitializeObjectPool()
        {
            itemPool = new ObjectPool<TimelineItemPanel>(
                () => Instantiate(TimelineItemPrefab, ContentContainer)
            );
        }

        public void UpdateTimeline()
        {
            if (GameManager.Instance?.CurrentGameState == null)
            {
                ShowEmptyState();
                return;
            }

            StartCoroutine(LoadTimelineEventsAsync());
        }

        private IEnumerator LoadTimelineEventsAsync()
        {
            if (isLoading) yield break;

            isLoading = true;
            ShowLoading();

            yield return new WaitForSeconds(0.5f);

            var gameState = GameManager.Instance.CurrentGameState;

            yield return ApiClient.Instance.Get<List<PlayerEvent>>(
                $"/api/game/{gameState.Id}/events",
                (events) =>
                {
                    cachedEvents = events;
                    DisplayEvents(events);
                    isLoading = false;
                },
                (error) =>
                {
                    Debug.LogError($"Failed to load timeline events: {error}");
                    ShowEmptyState();
                    isLoading = false;
                }
            );
        }

        private void DisplayEvents(List<PlayerEvent> events)
        {
            ClearTimeline();

            if (events == null || events.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideEmptyState();
            StartCoroutine(AnimateTimelineItems(events));
        }

        private IEnumerator AnimateTimelineItems(List<PlayerEvent> events)
        {
            float totalHeight = 0f;
            float timelineLineWidth = 2f;
            Vector2 lineStart = new Vector2(40f, 0f);

            for (int i = 0; i < events.Count; i++)
            {
                var eventData = events[i];
                var item = itemPool.Get();
                item.Initialize(eventData, GetEventColor(eventData));

                float itemHeight = item.GetHeight();
                totalHeight += itemHeight + ItemSpacing;

                item.transform.localPosition = new Vector3(0f, -totalHeight + itemHeight, 0f);

                yield return new WaitForSeconds(AnimationDelay);

                if (i == 0)
                {
                    CreateTimelineLine(lineStart, new Vector2(lineStart.x, -totalHeight + itemHeight * 0.5f), timelineLineWidth);
                }
                else
                {
                    float lineEndY = -totalHeight + itemHeight * 0.5f;
                    float lineStartY = -totalHeight + itemHeight + ItemSpacing - itemHeight * 0.5f;
                    CreateTimelineLine(new Vector2(lineStart.x, lineStartY), new Vector2(lineStart.x, lineEndY), timelineLineWidth);
                }
            }

            ContentContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, totalHeight);
        }

        private void CreateTimelineLine(Vector2 start, Vector2 end, float width)
        {
            GameObject line = new GameObject("TimelineLine");
            line.transform.SetParent(ContentContainer, false);

            RectTransform lineRect = line.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0f, 1f);
            lineRect.anchorMax = new Vector2(0f, 1f);
            lineRect.pivot = new Vector2(0f, 1f);
            lineRect.sizeDelta = new Vector2(width, Vector2.Distance(start, end));
            lineRect.localPosition = new Vector3(start.x, -start.y + (Vector2.Distance(start, end) * 0.5f), 0f);
            lineRect.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.up, end - start));

            Image lineImage = line.AddComponent<Image>();
            lineImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        }

        private Color GetEventColor(PlayerEvent eventData)
        {
            if (eventData.Template == null) return LifeEventColor;

            switch (eventData.Template.EventType)
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

        private void ClearTimeline()
        {
            foreach (Transform child in ContentContainer)
            {
                if (child.name == "TimelineLine")
                {
                    Destroy(child.gameObject);
                }
                else if (child.GetComponent<TimelineItemPanel>() != null)
                {
                    itemPool.Release(child.GetComponent<TimelineItemPanel>());
                }
            }
        }

        private void ShowLoading()
        {
            if (LoadingSpinner != null) LoadingSpinner.SetActive(true);
            if (EmptyState != null) EmptyState.SetActive(false);
        }

        private void ShowEmptyState()
        {
            if (LoadingSpinner != null) LoadingSpinner.SetActive(false);
            if (EmptyState != null) EmptyState.SetActive(true);
        }

        private void HideEmptyState()
        {
            if (EmptyState != null) EmptyState.SetActive(false);
        }

        private void OnDisable()
        {
            ClearTimeline();
        }

        private class ObjectPool<T> where T : Component
        {
            private Stack<T> pool = new Stack<T>();
            private System.Func<T> createFunc;

            public ObjectPool(System.Func<T> createFunc)
            {
                this.createFunc = createFunc;
            }

            public T Get()
            {
                if (pool.Count > 0)
                {
                    var item = pool.Pop();
                    item.gameObject.SetActive(true);
                    return item;
                }
                return createFunc();
            }

            public void Release(T item)
            {
                item.gameObject.SetActive(false);
                pool.Push(item);
            }
        }
    }
}
