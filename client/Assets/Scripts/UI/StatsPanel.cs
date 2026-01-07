using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Core;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    [System.Serializable]
    public class StatsPanel : MonoBehaviour
    {
        #region Theme Colors
        
        private static readonly Color BackgroundDark = new Color(0.118f, 0.094f, 0.086f, 0.97f);
        private static readonly Color BackgroundCard = new Color(0.15f, 0.12f, 0.11f, 0.95f);
        private static readonly Color BackgroundLight = new Color(0.976f, 0.961f, 0.937f, 1f);
        
        private static readonly Color AccentPrimary = new Color(0.796f, 0.431f, 0.353f, 1f);
        private static readonly Color AccentSecondary = new Color(0.6f, 0.667f, 0.596f, 1f);
        private static readonly Color AccentTertiary = new Color(0.867f, 0.788f, 0.694f, 1f);
        
        private static readonly Color TextPrimary = new Color(0.976f, 0.961f, 0.937f, 1f);
        private static readonly Color TextSecondary = new Color(0.7f, 0.65f, 0.6f, 1f);
        private static readonly Color TextMuted = new Color(0.5f, 0.45f, 0.4f, 1f);
        
        private static readonly Color StatCritical = new Color(0.898f, 0.345f, 0.329f, 1f);
        private static readonly Color StatWarning = new Color(0.945f, 0.769f, 0.059f, 1f);
        private static readonly Color StatGood = new Color(0.486f, 0.702f, 0.478f, 1f);
        private static readonly Color StatExcellent = new Color(0.396f, 0.612f, 0.851f, 1f);
        
        private static readonly Color HealthColor = new Color(0.898f, 0.345f, 0.329f, 1f);
        private static readonly Color HappinessColor = new Color(0.945f, 0.769f, 0.059f, 1f);
        private static readonly Color WealthColor = new Color(0.486f, 0.702f, 0.478f, 1f);
        private static readonly Color IntelligenceColor = new Color(0.396f, 0.612f, 0.851f, 1f);
        private static readonly Color CharismaColor = new Color(0.796f, 0.431f, 0.353f, 1f);
        private static readonly Color PhysicalColor = new Color(0.6f, 0.667f, 0.596f, 1f);
        private static readonly Color CreativityColor = new Color(0.729f, 0.494f, 0.729f, 1f);
        
        #endregion
        
        #region Cached References
        
        private RectTransform panelRect;
        private CanvasGroup canvasGroup;
        private ScrollRect scrollRect;
        
        private Dictionary<string, CircularStatIndicator> primaryStats = new Dictionary<string, CircularStatIndicator>();
        private Dictionary<string, LinearStatBar> secondaryStats = new Dictionary<string, LinearStatBar>();
        
        private Text ageText;
        private Text careerText;
        private Text yearText;
        private Text gamesPlayedText;
        private Text playtimeText;
        
        private List<FloatingChangeIndicator> changeIndicatorPool = new List<FloatingChangeIndicator>();
        private const int INDICATOR_POOL_SIZE = 10;
        
        private Dictionary<string, int> previousStatValues = new Dictionary<string, int>();
        
        private bool isAnimatingEntrance = false;
        private Coroutine updateCoroutine;
        private float lastUpdateTime;
        private const float UPDATE_COOLDOWN = 0.1f;
        
        #endregion
        
        #region Lifecycle
        
        private void Awake()
        {
            panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = gameObject.AddComponent<RectTransform>();
            }
            
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        private void Start()
        {
            BuildUI();
            InitializeIndicatorPool();
            canvasGroup.alpha = 0f;
        }
        
        private void OnEnable()
        {
            if (primaryStats.Count > 0 && !isAnimatingEntrance)
            {
                StartCoroutine(PlayEntranceAnimation());
            }
            
            if (GameManager.Instance != null)
            {
                InvokeRepeating(nameof(CheckForStatChanges), 0.5f, 0.5f);
            }
        }
        
        private void OnDisable()
        {
            CancelInvoke(nameof(CheckForStatChanges));
        }
        
        #endregion
        
        #region UI Construction
        
        private void BuildUI()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            SetupMainPanel();
            CreateScrollView();
            CreateCharacterInfoSection();
            CreatePrimaryStatsSection();
            CreateSecondaryStatsSection();
            CreateMetaStatsSection();
        }
        
        private void SetupMainPanel()
        {
            Image bgImage = gameObject.GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = gameObject.AddComponent<Image>();
            }
            bgImage.color = BackgroundDark;
            bgImage.raycastTarget = true;
            
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }
        
        private void CreateScrollView()
        {
            GameObject scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(transform, false);
            
            RectTransform scrollRect_rt = scrollObj.AddComponent<RectTransform>();
            scrollRect_rt.anchorMin = Vector2.zero;
            scrollRect_rt.anchorMax = Vector2.one;
            scrollRect_rt.offsetMin = new Vector2(0f, 0f);
            scrollRect_rt.offsetMax = new Vector2(0f, 0f);
            
            scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 20f;
            
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            Image viewportMask = viewportObj.AddComponent<Image>();
            viewportMask.color = Color.white;
            
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            scrollRect.viewport = viewportRect;
            
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            
            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 24, 40);
            vlg.spacing = 24f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            
            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.content = contentRect;
        }
        
        private void CreateCharacterInfoSection()
        {
            RectTransform content = scrollRect.content;
            
            GameObject sectionObj = CreateSection("CharacterInfo", content);
            RectTransform sectionRect = sectionObj.GetComponent<RectTransform>();
            
            CreateSectionHeader(sectionObj.transform, "CHARACTER", AccentTertiary);
            
            GameObject cardObj = CreateCard(sectionObj.transform, "InfoCard", 100f);
            
            HorizontalLayoutGroup hlg = cardObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 12, 12);
            hlg.spacing = 0f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            
            ageText = CreateInfoItem(cardObj.transform, "AGE", "18", AccentPrimary);
            CreateVerticalDivider(cardObj.transform);
            yearText = CreateInfoItem(cardObj.transform, "YEAR", "2024", AccentSecondary);
            CreateVerticalDivider(cardObj.transform);
            careerText = CreateInfoItem(cardObj.transform, "CAREER", "Student", AccentTertiary);
            
            LayoutElement le = sectionObj.AddComponent<LayoutElement>();
            le.preferredHeight = 160f;
            le.flexibleWidth = 1f;
        }
        
        private void CreatePrimaryStatsSection()
        {
            RectTransform content = scrollRect.content;
            
            GameObject sectionObj = CreateSection("PrimaryStats", content);
            
            CreateSectionHeader(sectionObj.transform, "VITAL STATS", AccentPrimary);
            
            GameObject gridObj = new GameObject("StatsGrid");
            gridObj.transform.SetParent(sectionObj.transform, false);
            
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.sizeDelta = new Vector2(0f, 320f);
            
            GridLayoutGroup glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.padding = new RectOffset(8, 8, 8, 8);
            glg.cellSize = new Vector2(100f, 130f);
            glg.spacing = new Vector2(12f, 16f);
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.startAxis = GridLayoutGroup.Axis.Horizontal;
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            
            CreateCircularIndicator(gridObj.transform, "Health", "HEALTH", HealthColor, 100, true);
            CreateCircularIndicator(gridObj.transform, "Happiness", "HAPPY", HappinessColor, 100, true);
            CreateCircularIndicator(gridObj.transform, "Wealth", "WEALTH", WealthColor, 100, false);
            
            LayoutElement le = sectionObj.AddComponent<LayoutElement>();
            le.preferredHeight = 380f;
            le.flexibleWidth = 1f;
        }
        
        private void CreateSecondaryStatsSection()
        {
            RectTransform content = scrollRect.content;
            
            GameObject sectionObj = CreateSection("SecondaryStats", content);
            
            CreateSectionHeader(sectionObj.transform, "SKILLS", AccentSecondary);
            
            GameObject cardObj = CreateCard(sectionObj.transform, "SkillsCard", 200f);
            
            VerticalLayoutGroup vlg = cardObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            
            CreateLinearStatBar(cardObj.transform, "Intelligence", "Intelligence", IntelligenceColor, 100);
            CreateLinearStatBar(cardObj.transform, "Charisma", "Charisma", CharismaColor, 100);
            CreateLinearStatBar(cardObj.transform, "Physical", "Physical", PhysicalColor, 100);
            CreateLinearStatBar(cardObj.transform, "Creativity", "Creativity", CreativityColor, 100);
            
            LayoutElement le = sectionObj.AddComponent<LayoutElement>();
            le.preferredHeight = 280f;
            le.flexibleWidth = 1f;
        }
        
        private void CreateMetaStatsSection()
        {
            RectTransform content = scrollRect.content;
            
            GameObject sectionObj = CreateSection("MetaStats", content);
            
            CreateSectionHeader(sectionObj.transform, "STATISTICS", TextMuted);
            
            GameObject cardObj = CreateCard(sectionObj.transform, "MetaCard", 80f);
            
            HorizontalLayoutGroup hlg = cardObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 12, 12);
            hlg.spacing = 0f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            
            gamesPlayedText = CreateMetaItem(cardObj.transform, "GAMES PLAYED", "0");
            CreateVerticalDivider(cardObj.transform);
            playtimeText = CreateMetaItem(cardObj.transform, "PLAYTIME", "0h 0m");
            
            LayoutElement le = sectionObj.AddComponent<LayoutElement>();
            le.preferredHeight = 140f;
            le.flexibleWidth = 1f;
        }
        
        #endregion
        
        #region UI Helper Methods
        
        private GameObject CreateSection(string name, Transform parent)
        {
            GameObject sectionObj = new GameObject(name);
            sectionObj.transform.SetParent(parent, false);
            
            RectTransform rect = sectionObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            
            VerticalLayoutGroup vlg = sectionObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            
            return sectionObj;
        }
        
        private void CreateSectionHeader(Transform parent, string title, Color accentColor)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0f, 24f);
            
            HorizontalLayoutGroup hlg = headerObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            
            GameObject lineObj = new GameObject("AccentLine");
            lineObj.transform.SetParent(headerObj.transform, false);
            
            RectTransform lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.sizeDelta = new Vector2(3f, 16f);
            
            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = accentColor;
            
            LayoutElement lineLE = lineObj.AddComponent<LayoutElement>();
            lineLE.preferredWidth = 3f;
            
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 11;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = TextMuted;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            
            LayoutElement titleLE = titleObj.AddComponent<LayoutElement>();
            titleLE.preferredWidth = 150f;
        }
        
        private GameObject CreateCard(Transform parent, string name, float height)
        {
            GameObject cardObj = new GameObject(name);
            cardObj.transform.SetParent(parent, false);
            
            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(0f, height);
            
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = BackgroundCard;
            
            CreateCardBorder(cardObj.transform);
            
            LayoutElement le = cardObj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1f;
            
            return cardObj;
        }
        
        private void CreateCardBorder(Transform parent)
        {
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(parent, false);
            borderObj.transform.SetAsFirstSibling();
            
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-1f, -1f);
            borderRect.offsetMax = new Vector2(1f, 1f);
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = new Color(1f, 1f, 1f, 0.05f);
            borderImage.raycastTarget = false;
        }
        
        private Text CreateInfoItem(Transform parent, string label, string value, Color accentColor)
        {
            GameObject itemObj = new GameObject(label + "Item");
            itemObj.transform.SetParent(parent, false);
            
            RectTransform itemRect = itemObj.AddComponent<RectTransform>();
            
            VerticalLayoutGroup vlg = itemObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 9;
            labelText.fontStyle = FontStyle.Normal;
            labelText.color = TextMuted;
            labelText.alignment = TextAnchor.MiddleCenter;
            
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 14f;
            
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(itemObj.transform, false);
            
            Text valueText = valueObj.AddComponent<Text>();
            valueText.text = value;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 22;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = accentColor;
            valueText.alignment = TextAnchor.MiddleCenter;
            
            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.preferredHeight = 28f;
            
            return valueText;
        }
        
        private Text CreateMetaItem(Transform parent, string label, string value)
        {
            GameObject itemObj = new GameObject(label + "Item");
            itemObj.transform.SetParent(parent, false);
            
            VerticalLayoutGroup vlg = itemObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(itemObj.transform, false);
            
            Text valueText = valueObj.AddComponent<Text>();
            valueText.text = value;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 18;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = TextPrimary;
            valueText.alignment = TextAnchor.MiddleCenter;
            
            LayoutElement valueLE = valueObj.AddComponent<LayoutElement>();
            valueLE.preferredHeight = 24f;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 9;
            labelText.color = TextMuted;
            labelText.alignment = TextAnchor.MiddleCenter;
            
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.preferredHeight = 14f;
            
            return valueText;
        }
        
        private void CreateVerticalDivider(Transform parent)
        {
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(parent, false);
            
            RectTransform divRect = dividerObj.AddComponent<RectTransform>();
            
            Image divImage = dividerObj.AddComponent<Image>();
            divImage.color = new Color(1f, 1f, 1f, 0.1f);
            
            LayoutElement le = dividerObj.AddComponent<LayoutElement>();
            le.preferredWidth = 1f;
            le.flexibleHeight = 1f;
        }
        
        private void CreateCircularIndicator(Transform parent, string key, string label, Color color, int maxValue, bool isPercentage)
        {
            GameObject indicatorObj = new GameObject(key + "Indicator");
            indicatorObj.transform.SetParent(parent, false);
            
            CircularStatIndicator indicator = indicatorObj.AddComponent<CircularStatIndicator>();
            indicator.Initialize(label, color, maxValue, isPercentage);
            
            primaryStats[key] = indicator;
        }
        
        private void CreateLinearStatBar(Transform parent, string key, string label, Color color, int maxValue)
        {
            GameObject barObj = new GameObject(key + "Bar");
            barObj.transform.SetParent(parent, false);
            
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(0f, 36f);
            
            LinearStatBar bar = barObj.AddComponent<LinearStatBar>();
            bar.Initialize(label, color, maxValue);
            
            secondaryStats[key] = bar;
            
            LayoutElement le = barObj.AddComponent<LayoutElement>();
            le.preferredHeight = 36f;
            le.flexibleWidth = 1f;
        }
        
        private void InitializeIndicatorPool()
        {
            for (int i = 0; i < INDICATOR_POOL_SIZE; i++)
            {
                GameObject indicatorObj = new GameObject($"ChangeIndicator_{i}");
                indicatorObj.transform.SetParent(transform, false);
                
                FloatingChangeIndicator indicator = indicatorObj.AddComponent<FloatingChangeIndicator>();
                indicator.Initialize();
                indicatorObj.SetActive(false);
                
                changeIndicatorPool.Add(indicator);
            }
        }
        
        #endregion
        
        #region Public API
        
        public void UpdateStats()
        {
            if (GameManager.Instance?.PlayerProfile == null) return;
            
            if (Time.time - lastUpdateTime < UPDATE_COOLDOWN) return;
            lastUpdateTime = Time.time;
            
            PlayerProfile profile = GameManager.Instance.PlayerProfile;
            GameState gameState = GameManager.Instance.CurrentGameState;
            
            UpdateStatWithChange("Health", profile.Health, 100);
            UpdateStatWithChange("Happiness", profile.Happiness, 100);
            UpdateStatWithChange("Wealth", profile.Wealth, Mathf.Max(100, profile.Wealth));
            
            UpdateStatWithChange("Intelligence", profile.Intelligence, 100);
            UpdateStatWithChange("Charisma", profile.Charisma, 100);
            UpdateStatWithChange("Physical", profile.Physical, 100);
            UpdateStatWithChange("Creativity", profile.Creativity, 100);
            
            if (ageText != null) ageText.text = profile.Age.ToString();
            if (yearText != null && gameState != null) yearText.text = gameState.CurrentYear.ToString();
            if (careerText != null && gameState != null)
            {
                careerText.text = string.IsNullOrEmpty(gameState.CareerId) ? "Unemployed" : "Level " + gameState.CareerLevel;
            }
            
            if (gamesPlayedText != null) gamesPlayedText.text = profile.GamesPlayed.ToString();
            if (playtimeText != null) playtimeText.text = FormatPlaytime(profile.TotalPlaytime);
        }
        
        public void AnimateStatChange(string statName, int oldValue, int newValue)
        {
            int change = newValue - oldValue;
            if (change == 0) return;
            
            if (primaryStats.TryGetValue(statName, out CircularStatIndicator circIndicator))
            {
                circIndicator.SetValue(newValue, true);
                ShowChangeIndicator(circIndicator.transform, change);
            }
            else if (secondaryStats.TryGetValue(statName, out LinearStatBar barIndicator))
            {
                barIndicator.SetValue(newValue, true);
                ShowChangeIndicator(barIndicator.transform, change);
            }
        }
        
        public void ForceRefresh()
        {
            if (GameManager.Instance?.PlayerProfile == null) return;
            
            PlayerProfile profile = GameManager.Instance.PlayerProfile;
            
            if (primaryStats.TryGetValue("Health", out var health))
                health.SetValue(profile.Health, false);
            if (primaryStats.TryGetValue("Happiness", out var happiness))
                happiness.SetValue(profile.Happiness, false);
            if (primaryStats.TryGetValue("Wealth", out var wealth))
                wealth.SetValue(profile.Wealth, false);
            
            if (secondaryStats.TryGetValue("Intelligence", out var intel))
                intel.SetValue(profile.Intelligence, false);
            if (secondaryStats.TryGetValue("Charisma", out var charisma))
                charisma.SetValue(profile.Charisma, false);
            if (secondaryStats.TryGetValue("Physical", out var physical))
                physical.SetValue(profile.Physical, false);
            if (secondaryStats.TryGetValue("Creativity", out var creativity))
                creativity.SetValue(profile.Creativity, false);
            
            previousStatValues.Clear();
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateStatWithChange(string statName, int newValue, int maxValue)
        {
            int oldValue = previousStatValues.ContainsKey(statName) ? previousStatValues[statName] : newValue;
            previousStatValues[statName] = newValue;
            
            if (oldValue != newValue)
            {
                AnimateStatChange(statName, oldValue, newValue);
            }
            else
            {
                if (primaryStats.TryGetValue(statName, out var circIndicator))
                {
                    circIndicator.SetValue(newValue, false);
                    circIndicator.SetMaxValue(maxValue);
                }
                else if (secondaryStats.TryGetValue(statName, out var barIndicator))
                {
                    barIndicator.SetValue(newValue, false);
                }
            }
        }
        
        private void CheckForStatChanges()
        {
            if (GameManager.Instance?.PlayerProfile == null) return;
            UpdateStats();
        }
        
        private void ShowChangeIndicator(Transform target, int change)
        {
            FloatingChangeIndicator indicator = null;
            foreach (var ind in changeIndicatorPool)
            {
                if (!ind.gameObject.activeSelf)
                {
                    indicator = ind;
                    break;
                }
            }
            
            if (indicator == null)
            {
                indicator = changeIndicatorPool[0];
            }
            
            indicator.Show(target, change);
        }
        
        private string FormatPlaytime(int totalMinutes)
        {
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            
            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }
            return $"{minutes}m";
        }
        
        #endregion
        
        #region Animations
        
        private IEnumerator PlayEntranceAnimation()
        {
            isAnimatingEntrance = true;
            
            canvasGroup.alpha = 0f;
            
            float fadeTime = 0f;
            float fadeDuration = 0.4f;
            
            while (fadeTime < fadeDuration)
            {
                fadeTime += Time.deltaTime;
                canvasGroup.alpha = EaseOutQuad(fadeTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            
            float staggerDelay = 0.08f;
            foreach (var kvp in primaryStats)
            {
                kvp.Value.PlayEntranceAnimation();
                yield return new WaitForSeconds(staggerDelay);
            }
            
            foreach (var kvp in secondaryStats)
            {
                kvp.Value.PlayEntranceAnimation();
                yield return new WaitForSeconds(staggerDelay * 0.5f);
            }
            
            isAnimatingEntrance = false;
            
            UpdateStats();
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        #endregion
    }
    
    #region Helper Components
    
    public class CircularStatIndicator : MonoBehaviour
    {
        private string statLabel;
        private Color statColor;
        private int maxValue;
        private int currentValue;
        private bool isPercentage;
        
        private Image backgroundRing;
        private Image fillRing;
        private Text valueText;
        private Text labelText;
        private CanvasGroup canvasGroup;
        
        private Coroutine fillAnimation;
        
        private static readonly Color RingBackground = new Color(1f, 1f, 1f, 0.08f);
        
        public void Initialize(string label, Color color, int max, bool percentage)
        {
            statLabel = label;
            statColor = color;
            maxValue = max;
            isPercentage = percentage;
            currentValue = 0;
            
            BuildUI();
        }
        
        private void BuildUI()
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = gameObject.AddComponent<RectTransform>();
            }
            
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            
            GameObject bgRingObj = new GameObject("BackgroundRing");
            bgRingObj.transform.SetParent(transform, false);
            
            RectTransform bgRect = bgRingObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.7f);
            bgRect.anchorMax = new Vector2(0.5f, 0.7f);
            bgRect.sizeDelta = new Vector2(70f, 70f);
            bgRect.anchoredPosition = Vector2.zero;
            
            backgroundRing = bgRingObj.AddComponent<Image>();
            backgroundRing.color = RingBackground;
            backgroundRing.type = Image.Type.Filled;
            backgroundRing.fillMethod = Image.FillMethod.Radial360;
            backgroundRing.fillOrigin = (int)Image.Origin360.Top;
            backgroundRing.fillClockwise = true;
            backgroundRing.fillAmount = 1f;
            backgroundRing.sprite = CreateCircleSprite();
            
            GameObject fillRingObj = new GameObject("FillRing");
            fillRingObj.transform.SetParent(transform, false);
            
            RectTransform fillRect = fillRingObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.5f, 0.7f);
            fillRect.anchorMax = new Vector2(0.5f, 0.7f);
            fillRect.sizeDelta = new Vector2(70f, 70f);
            fillRect.anchoredPosition = Vector2.zero;
            
            fillRing = fillRingObj.AddComponent<Image>();
            fillRing.color = statColor;
            fillRing.type = Image.Type.Filled;
            fillRing.fillMethod = Image.FillMethod.Radial360;
            fillRing.fillOrigin = (int)Image.Origin360.Top;
            fillRing.fillClockwise = true;
            fillRing.fillAmount = 0f;
            fillRing.sprite = CreateCircleSprite();
            
            GameObject innerCircle = new GameObject("InnerCircle");
            innerCircle.transform.SetParent(transform, false);
            
            RectTransform innerRect = innerCircle.AddComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.5f, 0.7f);
            innerRect.anchorMax = new Vector2(0.5f, 0.7f);
            innerRect.sizeDelta = new Vector2(54f, 54f);
            innerRect.anchoredPosition = Vector2.zero;
            
            Image innerImage = innerCircle.AddComponent<Image>();
            innerImage.color = new Color(0.118f, 0.094f, 0.086f, 1f);
            innerImage.sprite = CreateCircleSprite();
            
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(transform, false);
            
            RectTransform valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.5f, 0.7f);
            valueRect.anchorMax = new Vector2(0.5f, 0.7f);
            valueRect.sizeDelta = new Vector2(50f, 24f);
            valueRect.anchoredPosition = Vector2.zero;
            
            valueText = valueObj.AddComponent<Text>();
            valueText.text = "0";
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 18;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = Color.white;
            valueText.alignment = TextAnchor.MiddleCenter;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(transform, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(100f, 20f);
            labelRect.anchoredPosition = new Vector2(0f, 8f);
            
            labelText = labelObj.AddComponent<Text>();
            labelText.text = statLabel;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 10;
            labelText.fontStyle = FontStyle.Bold;
            labelText.color = new Color(0.7f, 0.65f, 0.6f, 1f);
            labelText.alignment = TextAnchor.MiddleCenter;
        }
        
        private Sprite CreateCircleSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            
            float center = size / 2f;
            float radius = size / 2f - 1f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist <= radius)
                    {
                        float alpha = Mathf.Clamp01((radius - dist) * 2f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            tex.Apply();
            
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
        
        public void SetValue(int value, bool animate)
        {
            int clampedValue = Mathf.Clamp(value, 0, maxValue);
            
            if (animate && gameObject.activeInHierarchy)
            {
                if (fillAnimation != null)
                {
                    StopCoroutine(fillAnimation);
                }
                fillAnimation = StartCoroutine(AnimateToValue(clampedValue));
            }
            else
            {
                currentValue = clampedValue;
                UpdateVisuals();
            }
        }
        
        public void SetMaxValue(int max)
        {
            maxValue = Mathf.Max(1, max);
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            float normalizedValue = (float)currentValue / maxValue;
            
            if (fillRing != null)
            {
                fillRing.fillAmount = normalizedValue;
            }
            
            if (valueText != null)
            {
                valueText.text = currentValue.ToString();
            }
            
            UpdateStatLevelColor(normalizedValue);
        }
        
        private void UpdateStatLevelColor(float normalized)
        {
            Color targetColor;
            
            if (normalized <= 0.25f)
            {
                targetColor = new Color(0.898f, 0.345f, 0.329f, 1f);
            }
            else if (normalized <= 0.5f)
            {
                targetColor = new Color(0.945f, 0.769f, 0.059f, 1f);
            }
            else if (normalized <= 0.75f)
            {
                targetColor = new Color(0.486f, 0.702f, 0.478f, 1f);
            }
            else
            {
                targetColor = new Color(0.396f, 0.612f, 0.851f, 1f);
            }
            
            if (fillRing != null)
            {
                fillRing.color = Color.Lerp(statColor, targetColor, 0.3f);
            }
        }
        
        private IEnumerator AnimateToValue(int targetValue)
        {
            int startValue = currentValue;
            float startFill = fillRing != null ? fillRing.fillAmount : 0f;
            float targetFill = (float)targetValue / maxValue;
            
            float duration = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                
                currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, t));
                
                if (fillRing != null)
                {
                    fillRing.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                }
                
                if (valueText != null)
                {
                    valueText.text = currentValue.ToString();
                }
                
                yield return null;
            }
            
            currentValue = targetValue;
            UpdateVisuals();
        }
        
        public void PlayEntranceAnimation()
        {
            StartCoroutine(EntranceAnimation());
        }
        
        private IEnumerator EntranceAnimation()
        {
            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.7f;
            
            float duration = 0.4f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutBack(elapsed / duration);
                
                canvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
                transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.7f, Vector3.one, t);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
    
    public class LinearStatBar : MonoBehaviour
    {
        private string statLabel;
        private Color statColor;
        private int maxValue;
        private int currentValue;
        
        private Image fillBar;
        private Text valueText;
        private Text labelText;
        private RectTransform fillRect;
        private CanvasGroup canvasGroup;
        
        private Coroutine fillAnimation;
        
        public void Initialize(string label, Color color, int max)
        {
            statLabel = label;
            statColor = color;
            maxValue = max;
            currentValue = 0;
            
            BuildUI();
        }
        
        private void BuildUI()
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = gameObject.AddComponent<RectTransform>();
            }
            
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            
            GameObject labelRow = new GameObject("LabelRow");
            labelRow.transform.SetParent(transform, false);
            
            RectTransform labelRowRect = labelRow.AddComponent<RectTransform>();
            labelRowRect.anchorMin = new Vector2(0f, 0.6f);
            labelRowRect.anchorMax = new Vector2(1f, 1f);
            labelRowRect.offsetMin = Vector2.zero;
            labelRowRect.offsetMax = Vector2.zero;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(labelRow.transform, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.7f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            labelText = labelObj.AddComponent<Text>();
            labelText.text = statLabel;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 12;
            labelText.fontStyle = FontStyle.Normal;
            labelText.color = new Color(0.976f, 0.961f, 0.937f, 1f);
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(labelRow.transform, false);
            
            RectTransform valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.7f, 0f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            
            valueText = valueObj.AddComponent<Text>();
            valueText.text = "0";
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 12;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = statColor;
            valueText.alignment = TextAnchor.MiddleRight;
            
            GameObject trackObj = new GameObject("Track");
            trackObj.transform.SetParent(transform, false);
            
            RectTransform trackRect = trackObj.AddComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0.4f);
            trackRect.offsetMin = Vector2.zero;
            trackRect.offsetMax = Vector2.zero;
            
            Image trackImage = trackObj.AddComponent<Image>();
            trackImage.color = new Color(1f, 1f, 1f, 0.08f);
            
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(trackObj.transform, false);
            
            fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = new Vector2(0f, 1f);
            fillRect.offsetMax = new Vector2(0f, -1f);
            fillRect.sizeDelta = new Vector2(0f, 0f);
            
            fillBar = fillObj.AddComponent<Image>();
            fillBar.color = statColor;
        }
        
        public void SetValue(int value, bool animate)
        {
            int clampedValue = Mathf.Clamp(value, 0, maxValue);
            
            if (animate && gameObject.activeInHierarchy)
            {
                if (fillAnimation != null)
                {
                    StopCoroutine(fillAnimation);
                }
                fillAnimation = StartCoroutine(AnimateToValue(clampedValue));
            }
            else
            {
                currentValue = clampedValue;
                UpdateVisuals();
            }
        }
        
        private void UpdateVisuals()
        {
            if (valueText != null)
            {
                valueText.text = currentValue.ToString();
            }
            
            if (fillRect != null && fillRect.parent != null)
            {
                RectTransform trackRect = fillRect.parent.GetComponent<RectTransform>();
                float normalized = (float)currentValue / maxValue;
                float targetWidth = trackRect.rect.width * normalized;
                fillRect.sizeDelta = new Vector2(targetWidth, fillRect.sizeDelta.y);
            }
            
            UpdateBarColor();
        }
        
        private void UpdateBarColor()
        {
            float normalized = (float)currentValue / maxValue;
            Color targetColor;
            
            if (normalized <= 0.25f)
            {
                targetColor = new Color(0.898f, 0.345f, 0.329f, 1f);
            }
            else if (normalized <= 0.5f)
            {
                targetColor = new Color(0.945f, 0.769f, 0.059f, 1f);
            }
            else if (normalized <= 0.75f)
            {
                targetColor = new Color(0.486f, 0.702f, 0.478f, 1f);
            }
            else
            {
                targetColor = new Color(0.396f, 0.612f, 0.851f, 1f);
            }
            
            if (fillBar != null)
            {
                fillBar.color = Color.Lerp(statColor, targetColor, 0.4f);
            }
        }
        
        private IEnumerator AnimateToValue(int targetValue)
        {
            int startValue = currentValue;
            
            RectTransform trackRect = fillRect?.parent?.GetComponent<RectTransform>();
            if (trackRect == null) yield break;
            
            float startWidth = fillRect.sizeDelta.x;
            float targetWidth = trackRect.rect.width * ((float)targetValue / maxValue);
            
            float duration = 0.4f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                
                currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, t));
                fillRect.sizeDelta = new Vector2(Mathf.Lerp(startWidth, targetWidth, t), fillRect.sizeDelta.y);
                
                if (valueText != null)
                {
                    valueText.text = currentValue.ToString();
                }
                
                yield return null;
            }
            
            currentValue = targetValue;
            UpdateVisuals();
        }
        
        public void PlayEntranceAnimation()
        {
            StartCoroutine(EntranceAnimation());
        }
        
        private IEnumerator EntranceAnimation()
        {
            canvasGroup.alpha = 0f;
            
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                canvasGroup.alpha = t;
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    }
    
    public class FloatingChangeIndicator : MonoBehaviour
    {
        private Text changeText;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        
        private static readonly Color PositiveColor = new Color(0.486f, 0.702f, 0.478f, 1f);
        private static readonly Color NegativeColor = new Color(0.898f, 0.345f, 0.329f, 1f);
        
        public void Initialize()
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60f, 30f);
            
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            
            changeText = gameObject.AddComponent<Text>();
            changeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            changeText.fontSize = 16;
            changeText.fontStyle = FontStyle.Bold;
            changeText.alignment = TextAnchor.MiddleCenter;
        }
        
        public void Show(Transform target, int change)
        {
            gameObject.SetActive(true);
            
            rectTransform.position = target.position + Vector3.up * 40f;
            
            bool isPositive = change > 0;
            changeText.text = isPositive ? $"+{change}" : change.ToString();
            changeText.color = isPositive ? PositiveColor : NegativeColor;
            
            StartCoroutine(FloatAndFade());
        }
        
        private IEnumerator FloatAndFade()
        {
            Vector3 startPos = rectTransform.position;
            Vector3 endPos = startPos + Vector3.up * 50f;
            
            float duration = 1.2f;
            float elapsed = 0f;
            
            while (elapsed < 0.15f)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / 0.15f;
                yield return null;
            }
            canvasGroup.alpha = 1f;
            
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                rectTransform.position = Vector3.Lerp(startPos, endPos, EaseOutQuad(t));
                
                if (t > 0.6f)
                {
                    canvasGroup.alpha = 1f - ((t - 0.6f) / 0.4f);
                }
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    }
    
    #endregion
}
