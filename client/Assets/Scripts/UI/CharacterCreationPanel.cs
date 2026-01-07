using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LifeCraft.Core;
using LifeCraft.Data;

namespace LifeCraft.UI
{
    public class CharacterCreationPanel : MonoBehaviour
    {
        private enum CreationStep { Identity = 0, Attributes = 1, Summary = 2 }
        
        private CreationStep currentStep = CreationStep.Identity;
        private const int TOTAL_STEPS = 3;
        
        private readonly Color charcoalDark = new Color(0.118f, 0.094f, 0.086f, 1f);
        private readonly Color charcoalMid = new Color(0.188f, 0.153f, 0.133f, 1f);
        private readonly Color creamLight = new Color(0.976f, 0.961f, 0.937f, 1f);
        private readonly Color terracotta = new Color(0.796f, 0.431f, 0.353f, 1f);
        private readonly Color sage = new Color(0.6f, 0.667f, 0.596f, 1f);
        private readonly Color warmBeige = new Color(0.867f, 0.788f, 0.694f, 1f);
        private readonly Color errorRed = new Color(0.898f, 0.345f, 0.329f, 1f);
        private readonly Color successGreen = new Color(0.486f, 0.702f, 0.478f, 1f);
        
        [Header("Container References")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private RectTransform stepIndicatorContainer;
        
        [Header("Step Containers")]
        [SerializeField] private RectTransform identityStepContainer;
        [SerializeField] private RectTransform attributesStepContainer;
        [SerializeField] private RectTransform summaryStepContainer;
        
        [Header("Identity Step")]
        [SerializeField] private InputField characterNameInput;
        [SerializeField] private Text nameValidationText;
        [SerializeField] private Image nameInputBorder;
        
        [Header("Attributes Step")]
        [SerializeField] private StatSliderUI intelligenceSlider;
        [SerializeField] private StatSliderUI charismaSlider;
        [SerializeField] private StatSliderUI physicalSlider;
        [SerializeField] private StatSliderUI creativitySlider;
        [SerializeField] private Text pointsRemainingText;
        [SerializeField] private Button resetStatsButton;
        
        [Header("Summary Step")]
        [SerializeField] private Text summaryNameText;
        [SerializeField] private RectTransform summaryStatsContainer;
        [SerializeField] private Image avatarPreviewImage;
        
        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Text nextButtonText;
        [SerializeField] private Image loadingSpinner;
        
        [Header("Animation")]
        [SerializeField] private float transitionDuration = 0.4f;
        [SerializeField] private float staggerDelay = 0.08f;
        
        private const int TOTAL_BONUS_POINTS = 20;
        private const int BASE_STAT_VALUE = 10;
        private const int MIN_STAT_VALUE = 10;
        private const int MAX_STAT_VALUE = 20;
        
        private string characterName = "";
        private int bonusIntelligence = 0;
        private int bonusCharisma = 0;
        private int bonusPhysical = 0;
        private int bonusCreativity = 0;
        
        private int PointsRemaining => TOTAL_BONUS_POINTS - (bonusIntelligence + bonusCharisma + bonusPhysical + bonusCreativity);
        
        private bool isTransitioning = false;
        private bool isCreatingGame = false;
        private Coroutine spinnerAnimation;
        private List<Image> stepIndicators = new List<Image>();
        private GameObject runtimeContainer;
        
        private void Awake()
        {
            InitializePanel();
        }
        
        private void OnEnable()
        {
            ResetToInitialState();
            StartCoroutine(PlayEntranceAnimation());
        }
        
        private void OnDisable()
        {
            if (spinnerAnimation != null)
            {
                StopCoroutine(spinnerAnimation);
            }
        }
        
        private void OnDestroy()
        {
            CleanupListeners();
            
            if (runtimeContainer != null)
            {
                Destroy(runtimeContainer);
            }
        }
        
        private void InitializePanel()
        {
            if (contentContainer == null)
            {
                BuildUIFromCode();
            }
            
            SetupListeners();
            SetupStepIndicators();
            InitializeStatSliders();
        }
        
        private void SetupListeners()
        {
            if (characterNameInput != null)
            {
                characterNameInput.onValueChanged.AddListener(OnNameChanged);
                characterNameInput.onEndEdit.AddListener(OnNameEndEdit);
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
            
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }
            
            if (resetStatsButton != null)
            {
                resetStatsButton.onClick.AddListener(OnResetStatsClicked);
            }
        }
        
        private void CleanupListeners()
        {
            if (characterNameInput != null)
            {
                characterNameInput.onValueChanged.RemoveListener(OnNameChanged);
                characterNameInput.onEndEdit.RemoveListener(OnNameEndEdit);
            }
            
            backButton?.onClick.RemoveAllListeners();
            nextButton?.onClick.RemoveAllListeners();
            resetStatsButton?.onClick.RemoveAllListeners();
        }
        
        private void SetupStepIndicators()
        {
            if (stepIndicatorContainer == null) return;
            
            stepIndicators.Clear();
            
            for (int i = 0; i < stepIndicatorContainer.childCount; i++)
            {
                Image indicator = stepIndicatorContainer.GetChild(i).GetComponent<Image>();
                if (indicator != null)
                {
                    stepIndicators.Add(indicator);
                }
            }
        }
        
        private void InitializeStatSliders()
        {
            if (intelligenceSlider != null) intelligenceSlider.OnValueChanged = (val) => OnStatChanged("intelligence", val);
            if (charismaSlider != null) charismaSlider.OnValueChanged = (val) => OnStatChanged("charisma", val);
            if (physicalSlider != null) physicalSlider.OnValueChanged = (val) => OnStatChanged("physical", val);
            if (creativitySlider != null) creativitySlider.OnValueChanged = (val) => OnStatChanged("creativity", val);
        }
        
        private void ResetToInitialState()
        {
            currentStep = CreationStep.Identity;
            characterName = "";
            bonusIntelligence = 0;
            bonusCharisma = 0;
            bonusPhysical = 0;
            bonusCreativity = 0;
            
            isTransitioning = false;
            isCreatingGame = false;
            
            if (characterNameInput != null)
            {
                characterNameInput.text = "";
            }
            
            ResetStatSliders();
            UpdateStepIndicators();
            ShowCurrentStep(false);
            UpdateNavigationButtons();
            HideValidationError();
            HideLoadingSpinner();
        }
        
        private void ResetStatSliders()
        {
            intelligenceSlider?.SetValue(BASE_STAT_VALUE, false);
            charismaSlider?.SetValue(BASE_STAT_VALUE, false);
            physicalSlider?.SetValue(BASE_STAT_VALUE, false);
            creativitySlider?.SetValue(BASE_STAT_VALUE, false);
            
            bonusIntelligence = 0;
            bonusCharisma = 0;
            bonusPhysical = 0;
            bonusCreativity = 0;
            
            UpdatePointsDisplay();
        }
        
        private void BuildUIFromCode()
        {
            runtimeContainer = new GameObject("CharacterCreationUI");
            runtimeContainer.transform.SetParent(transform, false);
            
            RectTransform containerRect = runtimeContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            panelCanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            CreateBackground(containerRect);
            CreateHeader(containerRect);
            contentContainer = CreateContentArea(containerRect);
            CreateIdentityStep();
            CreateAttributesStep();
            CreateSummaryStep();
            CreateNavigationFooter(containerRect);
        }
        
        private void CreateBackground(RectTransform parent)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(parent, false);
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = charcoalDark;
            bgImage.raycastTarget = true;
            
            CreateFloatingOrbs(bgRect);
        }
        
        private void CreateFloatingOrbs(RectTransform parent)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject orbObj = new GameObject($"Orb_{i}");
                orbObj.transform.SetParent(parent, false);
                
                RectTransform orbRect = orbObj.AddComponent<RectTransform>();
                float size = Random.Range(200f, 400f);
                orbRect.sizeDelta = new Vector2(size, size);
                
                float xPos = (i == 1) ? Screen.width * 0.3f : -Screen.width * 0.2f;
                float yPos = Random.Range(-200f, 200f);
                orbRect.anchoredPosition = new Vector2(xPos, yPos);
                
                Image orbImage = orbObj.AddComponent<Image>();
                Color orbColor = (i == 0) ? terracotta : sage;
                orbColor.a = Random.Range(0.02f, 0.06f);
                orbImage.color = orbColor;
                orbImage.raycastTarget = false;
                
                StartCoroutine(AnimateOrb(orbRect, i));
            }
        }
        
        private IEnumerator AnimateOrb(RectTransform orbRect, int index)
        {
            Vector2 startPos = orbRect.anchoredPosition;
            float speed = Random.Range(0.2f, 0.4f);
            float amplitude = Random.Range(30f, 60f);
            float phaseOffset = index * 2.1f;
            
            while (gameObject.activeInHierarchy)
            {
                float offsetX = Mathf.Sin(Time.time * speed + phaseOffset) * amplitude;
                float offsetY = Mathf.Cos(Time.time * speed * 0.7f + phaseOffset) * amplitude * 0.5f;
                orbRect.anchoredPosition = startPos + new Vector2(offsetX, offsetY);
                yield return null;
            }
        }
        
        private void CreateHeader(RectTransform parent)
        {
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(parent, false);
            
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -40f);
            headerRect.sizeDelta = new Vector2(0f, 120f);
            
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerRect, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, 0f);
            titleRect.sizeDelta = new Vector2(300f, 50f);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "CREATE YOUR LIFE";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyle.Normal;
            titleText.color = creamLight;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            CreateStepIndicator(headerRect);
        }
        
        private void CreateStepIndicator(RectTransform parent)
        {
            GameObject indicatorObj = new GameObject("StepIndicator");
            indicatorObj.transform.SetParent(parent, false);
            
            stepIndicatorContainer = indicatorObj.AddComponent<RectTransform>();
            stepIndicatorContainer.anchorMin = new Vector2(0.5f, 0f);
            stepIndicatorContainer.anchorMax = new Vector2(0.5f, 0f);
            stepIndicatorContainer.pivot = new Vector2(0.5f, 0f);
            stepIndicatorContainer.anchoredPosition = new Vector2(0f, 20f);
            stepIndicatorContainer.sizeDelta = new Vector2(100f, 20f);
            
            HorizontalLayoutGroup layout = indicatorObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            
            string[] stepLabels = { "Identity", "Attributes", "Summary" };
            for (int i = 0; i < TOTAL_STEPS; i++)
            {
                CreateStepDot(stepIndicatorContainer, i, stepLabels[i]);
            }
        }
        
        private void CreateStepDot(RectTransform parent, int index, string label)
        {
            GameObject dotContainer = new GameObject($"Step_{index}");
            dotContainer.transform.SetParent(parent, false);
            
            RectTransform containerRect = dotContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(12f, 12f);
            
            Image dotImage = dotContainer.AddComponent<Image>();
            dotImage.color = index == 0 ? terracotta : new Color(creamLight.r, creamLight.g, creamLight.b, 0.3f);
            dotImage.type = Image.Type.Simple;
            
            stepIndicators.Add(dotImage);
        }
        
        private RectTransform CreateContentArea(RectTransform parent)
        {
            GameObject contentObj = new GameObject("ContentArea");
            contentObj.transform.SetParent(parent, false);
            
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.offsetMin = new Vector2(24f, 100f);
            contentRect.offsetMax = new Vector2(-24f, -180f);
            
            return contentRect;
        }
        
        private void CreateIdentityStep()
        {
            GameObject stepObj = new GameObject("IdentityStep");
            stepObj.transform.SetParent(contentContainer, false);
            
            identityStepContainer = stepObj.AddComponent<RectTransform>();
            identityStepContainer.anchorMin = Vector2.zero;
            identityStepContainer.anchorMax = Vector2.one;
            identityStepContainer.offsetMin = Vector2.zero;
            identityStepContainer.offsetMax = Vector2.zero;
            
            stepObj.AddComponent<CanvasGroup>();
            
            CreateStepTitle(identityStepContainer, "Who are you?", "Choose a name that will carry through your life journey", -20f);
            CreateNameInputCard(identityStepContainer);
            CreateAvatarPreview(identityStepContainer);
        }
        
        private void CreateStepTitle(RectTransform parent, string title, string subtitle, float yOffset)
        {
            GameObject titleObj = new GameObject("StepTitle");
            titleObj.transform.SetParent(parent, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, yOffset);
            titleRect.sizeDelta = new Vector2(320f, 40f);
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = creamLight;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            GameObject subtitleObj = new GameObject("StepSubtitle");
            subtitleObj.transform.SetParent(parent, false);
            
            RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 1f);
            subtitleRect.anchorMax = new Vector2(0.5f, 1f);
            subtitleRect.pivot = new Vector2(0.5f, 1f);
            subtitleRect.anchoredPosition = new Vector2(0f, yOffset - 45f);
            subtitleRect.sizeDelta = new Vector2(280f, 50f);
            
            Text subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.text = subtitle;
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subtitleText.fontSize = 14;
            subtitleText.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.6f);
            subtitleText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateNameInputCard(RectTransform parent)
        {
            GameObject cardObj = new GameObject("NameInputCard");
            cardObj.transform.SetParent(parent, false);
            
            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(0f, 20f);
            cardRect.sizeDelta = new Vector2(300f, 70f);
            
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = charcoalMid;
            
            GameObject borderObj = new GameObject("InputBorder");
            borderObj.transform.SetParent(cardRect, false);
            
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-2f, -2f);
            borderRect.offsetMax = new Vector2(2f, 2f);
            borderRect.SetAsFirstSibling();
            
            nameInputBorder = borderObj.AddComponent<Image>();
            nameInputBorder.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.1f);
            nameInputBorder.raycastTarget = false;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(cardRect, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0f, 0f);
            labelRect.anchoredPosition = new Vector2(16f, 8f);
            labelRect.sizeDelta = new Vector2(0f, 20f);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "CHARACTER NAME";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 10;
            labelText.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.5f);
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject inputObj = new GameObject("NameInput");
            inputObj.transform.SetParent(cardRect, false);
            
            RectTransform inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0f, 0f);
            inputRect.anchorMax = new Vector2(1f, 0.7f);
            inputRect.offsetMin = new Vector2(16f, 8f);
            inputRect.offsetMax = new Vector2(-16f, 0f);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputRect, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text inputTextComponent = textObj.AddComponent<Text>();
            inputTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputTextComponent.fontSize = 22;
            inputTextComponent.color = creamLight;
            inputTextComponent.alignment = TextAnchor.MiddleLeft;
            inputTextComponent.supportRichText = false;
            
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputRect, false);
            
            RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            Text placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "Enter name...";
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.fontSize = 22;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.3f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            
            characterNameInput = inputObj.AddComponent<InputField>();
            characterNameInput.textComponent = inputTextComponent;
            characterNameInput.placeholder = placeholderText;
            characterNameInput.characterLimit = 20;
            characterNameInput.contentType = InputField.ContentType.Name;
            
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = Color.clear;
            characterNameInput.targetGraphic = inputBg;
            
            GameObject validationObj = new GameObject("ValidationText");
            validationObj.transform.SetParent(parent, false);
            
            RectTransform validationRect = validationObj.AddComponent<RectTransform>();
            validationRect.anchorMin = new Vector2(0.5f, 0.5f);
            validationRect.anchorMax = new Vector2(0.5f, 0.5f);
            validationRect.pivot = new Vector2(0.5f, 1f);
            validationRect.anchoredPosition = new Vector2(0f, -30f);
            validationRect.sizeDelta = new Vector2(300f, 20f);
            
            nameValidationText = validationObj.AddComponent<Text>();
            nameValidationText.text = "";
            nameValidationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameValidationText.fontSize = 12;
            nameValidationText.color = errorRed;
            nameValidationText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateAvatarPreview(RectTransform parent)
        {
            GameObject previewObj = new GameObject("AvatarPreview");
            previewObj.transform.SetParent(parent, false);
            
            RectTransform previewRect = previewObj.AddComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0f);
            previewRect.anchorMax = new Vector2(0.5f, 0f);
            previewRect.pivot = new Vector2(0.5f, 0f);
            previewRect.anchoredPosition = new Vector2(0f, 20f);
            previewRect.sizeDelta = new Vector2(120f, 120f);
            
            Image circleBg = previewObj.AddComponent<Image>();
            circleBg.color = charcoalMid;
            
            GameObject avatarObj = new GameObject("AvatarIcon");
            avatarObj.transform.SetParent(previewRect, false);
            
            RectTransform avatarRect = avatarObj.AddComponent<RectTransform>();
            avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
            avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
            avatarRect.sizeDelta = new Vector2(60f, 60f);
            
            avatarPreviewImage = avatarObj.AddComponent<Image>();
            avatarPreviewImage.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.3f);
            
            GameObject labelObj = new GameObject("AvatarLabel");
            labelObj.transform.SetParent(previewRect, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -10f);
            labelRect.sizeDelta = new Vector2(120f, 20f);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "Your Avatar";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 11;
            labelText.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.4f);
            labelText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateAttributesStep()
        {
            GameObject stepObj = new GameObject("AttributesStep");
            stepObj.transform.SetParent(contentContainer, false);
            
            attributesStepContainer = stepObj.AddComponent<RectTransform>();
            attributesStepContainer.anchorMin = Vector2.zero;
            attributesStepContainer.anchorMax = Vector2.one;
            attributesStepContainer.offsetMin = Vector2.zero;
            attributesStepContainer.offsetMax = Vector2.zero;
            
            CanvasGroup stepCG = stepObj.AddComponent<CanvasGroup>();
            stepCG.alpha = 0f;
            stepCG.blocksRaycasts = false;
            
            CreateStepTitle(attributesStepContainer, "Shape Your Potential", "Distribute 20 bonus points across your attributes", -20f);
            CreatePointsDisplay(attributesStepContainer);
            CreateStatSlidersContainer(attributesStepContainer);
            CreateResetButton(attributesStepContainer);
        }
        
        private void CreatePointsDisplay(RectTransform parent)
        {
            GameObject pointsObj = new GameObject("PointsRemaining");
            pointsObj.transform.SetParent(parent, false);
            
            RectTransform pointsRect = pointsObj.AddComponent<RectTransform>();
            pointsRect.anchorMin = new Vector2(0.5f, 1f);
            pointsRect.anchorMax = new Vector2(0.5f, 1f);
            pointsRect.pivot = new Vector2(0.5f, 1f);
            pointsRect.anchoredPosition = new Vector2(0f, -100f);
            pointsRect.sizeDelta = new Vector2(200f, 50f);
            
            Image bgImage = pointsObj.AddComponent<Image>();
            bgImage.color = new Color(terracotta.r, terracotta.g, terracotta.b, 0.15f);
            
            GameObject textObj = new GameObject("PointsText");
            textObj.transform.SetParent(pointsRect, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            pointsRemainingText = textObj.AddComponent<Text>();
            pointsRemainingText.text = "20 Points Remaining";
            pointsRemainingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pointsRemainingText.fontSize = 16;
            pointsRemainingText.fontStyle = FontStyle.Bold;
            pointsRemainingText.color = terracotta;
            pointsRemainingText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateStatSlidersContainer(RectTransform parent)
        {
            GameObject slidersObj = new GameObject("StatSliders");
            slidersObj.transform.SetParent(parent, false);
            
            RectTransform slidersRect = slidersObj.AddComponent<RectTransform>();
            slidersRect.anchorMin = new Vector2(0.5f, 0.5f);
            slidersRect.anchorMax = new Vector2(0.5f, 0.5f);
            slidersRect.pivot = new Vector2(0.5f, 0.5f);
            slidersRect.anchoredPosition = new Vector2(0f, -20f);
            slidersRect.sizeDelta = new Vector2(300f, 280f);
            
            VerticalLayoutGroup layout = slidersObj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(0, 0, 10, 10);
            
            intelligenceSlider = CreateStatSlider(slidersRect, "Intelligence", "Problem-solving & learning", new Color(0.4f, 0.6f, 0.9f));
            charismaSlider = CreateStatSlider(slidersRect, "Charisma", "Social influence & relationships", new Color(0.9f, 0.5f, 0.5f));
            physicalSlider = CreateStatSlider(slidersRect, "Physical", "Health & athleticism", new Color(0.5f, 0.8f, 0.5f));
            creativitySlider = CreateStatSlider(slidersRect, "Creativity", "Art, innovation & expression", new Color(0.8f, 0.6f, 0.9f));
        }
        
        private StatSliderUI CreateStatSlider(RectTransform parent, string statName, string description, Color statColor)
        {
            GameObject sliderObj = new GameObject($"Slider_{statName}");
            sliderObj.transform.SetParent(parent, false);
            
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(300f, 55f);
            
            LayoutElement layoutElement = sliderObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 55f;
            layoutElement.preferredHeight = 55f;
            
            StatSliderUI sliderUI = sliderObj.AddComponent<StatSliderUI>();
            sliderUI.Initialize(statName, description, statColor, BASE_STAT_VALUE, MIN_STAT_VALUE, MAX_STAT_VALUE, creamLight, charcoalMid);
            
            return sliderUI;
        }
        
        private void CreateResetButton(RectTransform parent)
        {
            GameObject resetObj = new GameObject("ResetButton");
            resetObj.transform.SetParent(parent, false);
            
            RectTransform resetRect = resetObj.AddComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(0.5f, 0f);
            resetRect.anchorMax = new Vector2(0.5f, 0f);
            resetRect.pivot = new Vector2(0.5f, 0f);
            resetRect.anchoredPosition = new Vector2(0f, 20f);
            resetRect.sizeDelta = new Vector2(140f, 40f);
            
            Image btnBg = resetObj.AddComponent<Image>();
            btnBg.color = Color.clear;
            
            resetStatsButton = resetObj.AddComponent<Button>();
            resetStatsButton.targetGraphic = btnBg;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(resetRect, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = "Reset Stats";
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 14;
            btnText.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.6f);
            btnText.alignment = TextAnchor.MiddleCenter;
            
            GameObject lineObj = new GameObject("Underline");
            lineObj.transform.SetParent(resetRect, false);
            
            RectTransform lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.15f, 0f);
            lineRect.anchorMax = new Vector2(0.85f, 0f);
            lineRect.pivot = new Vector2(0.5f, 0f);
            lineRect.anchoredPosition = new Vector2(0f, 5f);
            lineRect.sizeDelta = new Vector2(0f, 1f);
            
            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.3f);
        }
        
        private void CreateSummaryStep()
        {
            GameObject stepObj = new GameObject("SummaryStep");
            stepObj.transform.SetParent(contentContainer, false);
            
            summaryStepContainer = stepObj.AddComponent<RectTransform>();
            summaryStepContainer.anchorMin = Vector2.zero;
            summaryStepContainer.anchorMax = Vector2.one;
            summaryStepContainer.offsetMin = Vector2.zero;
            summaryStepContainer.offsetMax = Vector2.zero;
            
            CanvasGroup stepCG = stepObj.AddComponent<CanvasGroup>();
            stepCG.alpha = 0f;
            stepCG.blocksRaycasts = false;
            
            CreateStepTitle(summaryStepContainer, "Ready to Begin", "Review your character before starting your journey", -20f);
            CreateSummaryCard(summaryStepContainer);
        }
        
        private void CreateSummaryCard(RectTransform parent)
        {
            GameObject cardObj = new GameObject("SummaryCard");
            cardObj.transform.SetParent(parent, false);
            
            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(0f, -20f);
            cardRect.sizeDelta = new Vector2(300f, 320f);
            
            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = charcoalMid;
            
            GameObject nameObj = new GameObject("CharacterName");
            nameObj.transform.SetParent(cardRect, false);
            
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1f);
            nameRect.anchorMax = new Vector2(0.5f, 1f);
            nameRect.pivot = new Vector2(0.5f, 1f);
            nameRect.anchoredPosition = new Vector2(0f, -20f);
            nameRect.sizeDelta = new Vector2(280f, 40f);
            
            summaryNameText = nameObj.AddComponent<Text>();
            summaryNameText.text = "Your Character";
            summaryNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            summaryNameText.fontSize = 24;
            summaryNameText.fontStyle = FontStyle.Bold;
            summaryNameText.color = creamLight;
            summaryNameText.alignment = TextAnchor.MiddleCenter;
            
            CreateSummaryInfo(cardRect, "Starting Age", "18 years old", -70f);
            CreateSummaryInfo(cardRect, "Starting Wealth", "$0", -105f);
            CreateSummaryInfo(cardRect, "Health", "100%", -140f);
            
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(cardRect, false);
            
            RectTransform dividerRect = dividerObj.AddComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0.1f, 1f);
            dividerRect.anchorMax = new Vector2(0.9f, 1f);
            dividerRect.pivot = new Vector2(0.5f, 0.5f);
            dividerRect.anchoredPosition = new Vector2(0f, -165f);
            dividerRect.sizeDelta = new Vector2(0f, 1f);
            
            Image dividerImage = dividerObj.AddComponent<Image>();
            dividerImage.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.1f);
            
            summaryStatsContainer = CreateStatsSummaryContainer(cardRect);
        }
        
        private void CreateSummaryInfo(RectTransform parent, string label, string value, float yOffset)
        {
            GameObject infoObj = new GameObject($"Info_{label}");
            infoObj.transform.SetParent(parent, false);
            
            RectTransform infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0f, 1f);
            infoRect.anchorMax = new Vector2(1f, 1f);
            infoRect.pivot = new Vector2(0.5f, 1f);
            infoRect.anchoredPosition = new Vector2(0f, yOffset);
            infoRect.sizeDelta = new Vector2(0f, 28f);
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(infoRect, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.offsetMin = new Vector2(20f, 0f);
            labelRect.offsetMax = new Vector2(0f, 0f);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 13;
            labelText.color = new Color(creamLight.r, creamLight.g, creamLight.b, 0.6f);
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(infoRect, false);
            
            RectTransform valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.5f, 0f);
            valueRect.anchorMax = new Vector2(1f, 1f);
            valueRect.offsetMin = new Vector2(0f, 0f);
            valueRect.offsetMax = new Vector2(-20f, 0f);
            
            Text valueText = valueObj.AddComponent<Text>();
            valueText.text = value;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 13;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = creamLight;
            valueText.alignment = TextAnchor.MiddleRight;
        }
        
        private RectTransform CreateStatsSummaryContainer(RectTransform parent)
        {
            GameObject statsObj = new GameObject("StatsSummary");
            statsObj.transform.SetParent(parent, false);
            
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0f, 0f);
            statsRect.anchorMax = new Vector2(1f, 1f);
            statsRect.offsetMin = new Vector2(20f, 20f);
            statsRect.offsetMax = new Vector2(-20f, -175f);
            
            GridLayoutGroup grid = statsObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120f, 30f);
            grid.spacing = new Vector2(10f, 8f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            
            return statsRect;
        }
        
        private void CreateNavigationFooter(RectTransform parent)
        {
            GameObject footerObj = new GameObject("NavigationFooter");
            footerObj.transform.SetParent(parent, false);
            
            RectTransform footerRect = footerObj.AddComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0f, 0f);
            footerRect.anchorMax = new Vector2(1f, 0f);
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.anchoredPosition = new Vector2(0f, 30f);
            footerRect.sizeDelta = new Vector2(0f, 60f);
            
            backButton = CreateNavigationButton(footerRect, "Back", false);
            RectTransform backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 0.5f);
            backRect.anchorMax = new Vector2(0f, 0.5f);
            backRect.pivot = new Vector2(0f, 0.5f);
            backRect.anchoredPosition = new Vector2(24f, 0f);
            
            nextButton = CreateNavigationButton(footerRect, "Next", true);
            RectTransform nextRect = nextButton.GetComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(1f, 0.5f);
            nextRect.anchorMax = new Vector2(1f, 0.5f);
            nextRect.pivot = new Vector2(1f, 0.5f);
            nextRect.anchoredPosition = new Vector2(-24f, 0f);
            
            nextButtonText = nextButton.GetComponentInChildren<Text>();
            
            CreateLoadingSpinner(nextRect);
        }
        
        private Button CreateNavigationButton(RectTransform parent, string text, bool isPrimary)
        {
            GameObject btnObj = new GameObject($"{text}Button");
            btnObj.transform.SetParent(parent, false);
            
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(isPrimary ? 160f : 100f, 50f);
            
            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = isPrimary ? terracotta : Color.clear;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            
            AddButtonPressAnimation(btn);
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnRect, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text btnText = textObj.AddComponent<Text>();
            btnText.text = text;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.fontSize = 16;
            btnText.fontStyle = isPrimary ? FontStyle.Bold : FontStyle.Normal;
            btnText.color = isPrimary ? creamLight : new Color(creamLight.r, creamLight.g, creamLight.b, 0.7f);
            btnText.alignment = TextAnchor.MiddleCenter;
            
            return btn;
        }
        
        private void AddButtonPressAnimation(Button button)
        {
            ButtonPressEffect effect = button.gameObject.AddComponent<ButtonPressEffect>();
            effect.Initialize(0.95f, 0.1f);
        }
        
        private void CreateLoadingSpinner(RectTransform parent)
        {
            GameObject spinnerObj = new GameObject("LoadingSpinner");
            spinnerObj.transform.SetParent(parent, false);
            
            RectTransform spinnerRect = spinnerObj.AddComponent<RectTransform>();
            spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
            spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
            spinnerRect.sizeDelta = new Vector2(24f, 24f);
            
            loadingSpinner = spinnerObj.AddComponent<Image>();
            loadingSpinner.color = creamLight;
            loadingSpinner.gameObject.SetActive(false);
        }
        
        private void OnBackClicked()
        {
            if (isTransitioning || currentStep == CreationStep.Identity) return;
            
            TriggerHapticFeedback();
            NavigateToStep(currentStep - 1);
        }
        
        private void OnNextClicked()
        {
            if (isTransitioning || isCreatingGame) return;
            
            TriggerHapticFeedback();
            
            if (!ValidateCurrentStep())
            {
                return;
            }
            
            if (currentStep == CreationStep.Summary)
            {
                StartCoroutine(CreateCharacterAndStartGame());
            }
            else
            {
                NavigateToStep(currentStep + 1);
            }
        }
        
        private void NavigateToStep(CreationStep newStep)
        {
            if (isTransitioning) return;
            
            StartCoroutine(TransitionToStep(newStep));
        }
        
        private IEnumerator TransitionToStep(CreationStep newStep)
        {
            isTransitioning = true;
            
            RectTransform currentContainer = GetStepContainer(currentStep);
            RectTransform newContainer = GetStepContainer(newStep);
            
            bool goingForward = newStep > currentStep;
            
            yield return StartCoroutine(FadeStep(currentContainer, false, goingForward));
            
            currentStep = newStep;
            UpdateStepIndicators();
            UpdateNavigationButtons();
            
            if (currentStep == CreationStep.Summary)
            {
                UpdateSummaryDisplay();
            }
            
            yield return StartCoroutine(FadeStep(newContainer, true, goingForward));
            
            isTransitioning = false;
        }
        
        private IEnumerator FadeStep(RectTransform stepContainer, bool fadeIn, bool fromRight)
        {
            if (stepContainer == null) yield break;
            
            CanvasGroup cg = stepContainer.GetComponent<CanvasGroup>();
            if (cg == null) cg = stepContainer.gameObject.AddComponent<CanvasGroup>();
            
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            
            float slideDistance = 50f;
            float startX = fadeIn ? (fromRight ? slideDistance : -slideDistance) : 0f;
            float endX = fadeIn ? 0f : (fromRight ? -slideDistance : slideDistance);
            
            Vector2 startPos = stepContainer.anchoredPosition;
            startPos.x = startX;
            
            cg.alpha = startAlpha;
            cg.blocksRaycasts = fadeIn;
            stepContainer.anchoredPosition = startPos;
            stepContainer.gameObject.SetActive(true);
            
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / transitionDuration);
                
                cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                
                Vector2 pos = stepContainer.anchoredPosition;
                pos.x = Mathf.Lerp(startX, endX, t);
                stepContainer.anchoredPosition = pos;
                
                yield return null;
            }
            
            cg.alpha = endAlpha;
            cg.blocksRaycasts = fadeIn;
            
            Vector2 finalPos = stepContainer.anchoredPosition;
            finalPos.x = endX;
            stepContainer.anchoredPosition = finalPos;
            
            if (!fadeIn)
            {
                stepContainer.gameObject.SetActive(false);
            }
        }
        
        private RectTransform GetStepContainer(CreationStep step)
        {
            return step switch
            {
                CreationStep.Identity => identityStepContainer,
                CreationStep.Attributes => attributesStepContainer,
                CreationStep.Summary => summaryStepContainer,
                _ => identityStepContainer
            };
        }
        
        private void UpdateStepIndicators()
        {
            for (int i = 0; i < stepIndicators.Count; i++)
            {
                bool isActive = i <= (int)currentStep;
                bool isCurrent = i == (int)currentStep;
                
                Color targetColor = isCurrent ? terracotta : 
                    (isActive ? new Color(terracotta.r, terracotta.g, terracotta.b, 0.5f) : 
                    new Color(creamLight.r, creamLight.g, creamLight.b, 0.2f));
                
                StartCoroutine(AnimateColor(stepIndicators[i], targetColor, 0.2f));
            }
        }
        
        private void UpdateNavigationButtons()
        {
            if (backButton != null)
            {
                CanvasGroup backCG = backButton.GetComponent<CanvasGroup>();
                if (backCG == null) backCG = backButton.gameObject.AddComponent<CanvasGroup>();
                
                bool showBack = currentStep != CreationStep.Identity;
                StartCoroutine(FadeCanvasGroup(backCG, showBack ? 1f : 0f, 0.2f));
                backButton.interactable = showBack;
            }
            
            if (nextButtonText != null)
            {
                nextButtonText.text = currentStep == CreationStep.Summary ? "Begin Life" : "Next";
            }
            
            if (nextButton != null)
            {
                Image btnImage = nextButton.GetComponent<Image>();
                if (btnImage != null)
                {
                    Color targetColor = currentStep == CreationStep.Summary ? successGreen : terracotta;
                    StartCoroutine(AnimateColor(btnImage, targetColor, 0.3f));
                }
            }
        }
        
        private void ShowCurrentStep(bool animate)
        {
            if (identityStepContainer != null)
            {
                CanvasGroup cg = identityStepContainer.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = currentStep == CreationStep.Identity ? 1f : 0f;
                    cg.blocksRaycasts = currentStep == CreationStep.Identity;
                }
                identityStepContainer.gameObject.SetActive(currentStep == CreationStep.Identity);
            }
            
            if (attributesStepContainer != null)
            {
                CanvasGroup cg = attributesStepContainer.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = currentStep == CreationStep.Attributes ? 1f : 0f;
                    cg.blocksRaycasts = currentStep == CreationStep.Attributes;
                }
                attributesStepContainer.gameObject.SetActive(currentStep == CreationStep.Attributes);
            }
            
            if (summaryStepContainer != null)
            {
                CanvasGroup cg = summaryStepContainer.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = currentStep == CreationStep.Summary ? 1f : 0f;
                    cg.blocksRaycasts = currentStep == CreationStep.Summary;
                }
                summaryStepContainer.gameObject.SetActive(currentStep == CreationStep.Summary);
            }
        }
        
        private bool ValidateCurrentStep()
        {
            switch (currentStep)
            {
                case CreationStep.Identity:
                    return ValidateName();
                    
                case CreationStep.Attributes:
                    return ValidateStats();
                    
                case CreationStep.Summary:
                    return true;
                    
                default:
                    return true;
            }
        }
        
        private bool ValidateName()
        {
            characterName = characterNameInput?.text?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(characterName))
            {
                ShowValidationError("Please enter a name for your character");
                ShakeInput();
                return false;
            }
            
            if (characterName.Length < 2)
            {
                ShowValidationError("Name must be at least 2 characters");
                ShakeInput();
                return false;
            }
            
            if (characterName.Length > 20)
            {
                ShowValidationError("Name must be 20 characters or less");
                ShakeInput();
                return false;
            }
            
            HideValidationError();
            return true;
        }
        
        private bool ValidateStats()
        {
            if (PointsRemaining > 0)
            {
                UIManager.Instance?.ShowMessage("Points Remaining", $"You still have {PointsRemaining} points to distribute!", 2f);
                return false;
            }
            
            return true;
        }
        
        private void ShowValidationError(string message)
        {
            if (nameValidationText != null)
            {
                nameValidationText.text = message;
                StartCoroutine(FadeText(nameValidationText, 1f, 0.2f));
            }
            
            if (nameInputBorder != null)
            {
                StartCoroutine(AnimateColor(nameInputBorder, new Color(errorRed.r, errorRed.g, errorRed.b, 0.6f), 0.2f));
            }
        }
        
        private void HideValidationError()
        {
            if (nameValidationText != null)
            {
                StartCoroutine(FadeText(nameValidationText, 0f, 0.2f));
            }
            
            if (nameInputBorder != null)
            {
                StartCoroutine(AnimateColor(nameInputBorder, new Color(creamLight.r, creamLight.g, creamLight.b, 0.1f), 0.2f));
            }
        }
        
        private void ShakeInput()
        {
            if (characterNameInput != null)
            {
                StartCoroutine(ShakeTransform(characterNameInput.GetComponent<RectTransform>(), 8f, 0.4f));
            }
        }
        
        private void OnNameChanged(string newName)
        {
            characterName = newName;
            
            if (!string.IsNullOrEmpty(newName))
            {
                HideValidationError();
            }
        }
        
        private void OnNameEndEdit(string finalName)
        {
            characterName = finalName.Trim();
            
            if (characterNameInput != null)
            {
                characterNameInput.text = characterName;
            }
        }
        
        private void OnStatChanged(string statName, int newValue)
        {
            int bonus = newValue - BASE_STAT_VALUE;
            
            int otherBonuses = bonusIntelligence + bonusCharisma + bonusPhysical + bonusCreativity;
            
            switch (statName)
            {
                case "intelligence":
                    otherBonuses -= bonusIntelligence;
                    break;
                case "charisma":
                    otherBonuses -= bonusCharisma;
                    break;
                case "physical":
                    otherBonuses -= bonusPhysical;
                    break;
                case "creativity":
                    otherBonuses -= bonusCreativity;
                    break;
            }
            
            int newTotal = otherBonuses + bonus;
            
            if (newTotal > TOTAL_BONUS_POINTS)
            {
                bonus = TOTAL_BONUS_POINTS - otherBonuses;
                newValue = BASE_STAT_VALUE + bonus;
            }
            
            switch (statName)
            {
                case "intelligence":
                    bonusIntelligence = bonus;
                    intelligenceSlider?.SetValue(newValue, false);
                    break;
                case "charisma":
                    bonusCharisma = bonus;
                    charismaSlider?.SetValue(newValue, false);
                    break;
                case "physical":
                    bonusPhysical = bonus;
                    physicalSlider?.SetValue(newValue, false);
                    break;
                case "creativity":
                    bonusCreativity = bonus;
                    creativitySlider?.SetValue(newValue, false);
                    break;
            }
            
            UpdatePointsDisplay();
            UpdateSliderMaxValues();
        }
        
        private void UpdatePointsDisplay()
        {
            if (pointsRemainingText == null) return;
            
            int remaining = PointsRemaining;
            pointsRemainingText.text = remaining == 0 ? "All Points Allocated!" : $"{remaining} Points Remaining";
            
            Color targetColor = remaining == 0 ? successGreen : terracotta;
            StartCoroutine(AnimateTextColor(pointsRemainingText, targetColor, 0.2f));
        }
        
        private void UpdateSliderMaxValues()
        {
            int available = PointsRemaining;
            
            intelligenceSlider?.SetMaxValue(Mathf.Min(MAX_STAT_VALUE, BASE_STAT_VALUE + bonusIntelligence + available));
            charismaSlider?.SetMaxValue(Mathf.Min(MAX_STAT_VALUE, BASE_STAT_VALUE + bonusCharisma + available));
            physicalSlider?.SetMaxValue(Mathf.Min(MAX_STAT_VALUE, BASE_STAT_VALUE + bonusPhysical + available));
            creativitySlider?.SetMaxValue(Mathf.Min(MAX_STAT_VALUE, BASE_STAT_VALUE + bonusCreativity + available));
        }
        
        private void OnResetStatsClicked()
        {
            TriggerHapticFeedback();
            ResetStatSliders();
            UpdateSliderMaxValues();
        }
        
        private void UpdateSummaryDisplay()
        {
            if (summaryNameText != null)
            {
                summaryNameText.text = characterName;
            }
            
            if (summaryStatsContainer != null)
            {
                foreach (Transform child in summaryStatsContainer)
                {
                    Destroy(child.gameObject);
                }
                
                CreateStatSummaryItem(summaryStatsContainer, "Intelligence", BASE_STAT_VALUE + bonusIntelligence, new Color(0.4f, 0.6f, 0.9f));
                CreateStatSummaryItem(summaryStatsContainer, "Charisma", BASE_STAT_VALUE + bonusCharisma, new Color(0.9f, 0.5f, 0.5f));
                CreateStatSummaryItem(summaryStatsContainer, "Physical", BASE_STAT_VALUE + bonusPhysical, new Color(0.5f, 0.8f, 0.5f));
                CreateStatSummaryItem(summaryStatsContainer, "Creativity", BASE_STAT_VALUE + bonusCreativity, new Color(0.8f, 0.6f, 0.9f));
            }
        }
        
        private void CreateStatSummaryItem(RectTransform parent, string statName, int value, Color statColor)
        {
            GameObject itemObj = new GameObject($"Stat_{statName}");
            itemObj.transform.SetParent(parent, false);
            
            Image bgBar = itemObj.AddComponent<Image>();
            bgBar.color = new Color(statColor.r, statColor.g, statColor.b, 0.2f);
            
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(itemObj.transform, false);
            
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2((float)value / MAX_STAT_VALUE, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            Image fillBar = fillObj.AddComponent<Image>();
            fillBar.color = statColor;
            
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);
            
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(0.7f, 1f);
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = Vector2.zero;
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = statName;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 11;
            labelText.color = creamLight;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(itemObj.transform, false);
            
            RectTransform valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.7f, 0f);
            valueRect.anchorMax = Vector2.one;
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = new Vector2(-8f, 0f);
            
            Text valueText = valueObj.AddComponent<Text>();
            valueText.text = value.ToString();
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            valueText.fontSize = 12;
            valueText.fontStyle = FontStyle.Bold;
            valueText.color = creamLight;
            valueText.alignment = TextAnchor.MiddleRight;
        }
        
        private IEnumerator CreateCharacterAndStartGame()
        {
            isCreatingGame = true;
            ShowLoadingSpinner();
            
            if (nextButton != null) nextButton.interactable = false;
            if (backButton != null) backButton.interactable = false;
            
            var initialTraits = new CreateGameRequest
            {
                playerId = GameManager.Instance?.CurrentPlayer?.Id ?? "",
                initialTraits = new InitialTraits
                {
                    startingAge = 18,
                    characterName = characterName,
                    intelligence = BASE_STAT_VALUE + bonusIntelligence,
                    charisma = BASE_STAT_VALUE + bonusCharisma,
                    physical = BASE_STAT_VALUE + bonusPhysical,
                    creativity = BASE_STAT_VALUE + bonusCreativity,
                    health = 100,
                    wealth = 0
                }
            };
            
            bool success = false;
            string errorMessage = "";
            
            yield return ApiClient.Instance.Post<GameState>(
                "/api/game",
                initialTraits,
                (gameState) =>
                {
                    GameManager.Instance?.SetGameState(gameState);
                    PlayerPrefs.SetString("LastGameId", gameState.Id);
                    PlayerPrefs.Save();
                    success = true;
                },
                (error) =>
                {
                    errorMessage = error;
                    success = false;
                }
            );
            
            HideLoadingSpinner();
            
            if (success)
            {
                yield return StartCoroutine(PlayExitAnimation());
                UIManager.Instance?.ShowGamePanel();
            }
            else
            {
                Debug.LogError($"Failed to create game: {errorMessage}");
                UIManager.Instance?.ShowMessage("Error", "Failed to start your life. Please try again.", 3f);
                
                if (nextButton != null) nextButton.interactable = true;
                if (backButton != null) backButton.interactable = true;
            }
            
            isCreatingGame = false;
        }
        
        private void ShowLoadingSpinner()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.gameObject.SetActive(true);
                spinnerAnimation = StartCoroutine(AnimateSpinner());
            }
            
            if (nextButtonText != null)
            {
                nextButtonText.text = "";
            }
        }
        
        private void HideLoadingSpinner()
        {
            if (spinnerAnimation != null)
            {
                StopCoroutine(spinnerAnimation);
                spinnerAnimation = null;
            }
            
            if (loadingSpinner != null)
            {
                loadingSpinner.gameObject.SetActive(false);
            }
            
            if (nextButtonText != null)
            {
                nextButtonText.text = currentStep == CreationStep.Summary ? "Begin Life" : "Next";
            }
        }
        
        private IEnumerator AnimateSpinner()
        {
            if (loadingSpinner == null) yield break;
            
            RectTransform spinnerRect = loadingSpinner.GetComponent<RectTransform>();
            
            while (true)
            {
                spinnerRect.Rotate(0f, 0f, -360f * Time.deltaTime);
                yield return null;
            }
        }
        
        private IEnumerator PlayEntranceAnimation()
        {
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
            }
            
            yield return new WaitForSeconds(0.1f);
            
            yield return StartCoroutine(FadeCanvasGroup(panelCanvasGroup, 1f, 0.4f));
            
            if (identityStepContainer != null)
            {
                yield return StartCoroutine(StaggerRevealChildren(identityStepContainer, staggerDelay));
            }
        }
        
        private IEnumerator PlayExitAnimation()
        {
            yield return StartCoroutine(FadeCanvasGroup(panelCanvasGroup, 0f, 0.3f));
        }
        
        private IEnumerator StaggerRevealChildren(RectTransform container, float delay)
        {
            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                CanvasGroup childCG = child.GetComponent<CanvasGroup>();
                
                if (childCG == null)
                {
                    childCG = child.gameObject.AddComponent<CanvasGroup>();
                }
                
                childCG.alpha = 0f;
            }
            
            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                CanvasGroup childCG = child.GetComponent<CanvasGroup>();
                
                StartCoroutine(FadeCanvasGroup(childCG, 1f, 0.3f));
                yield return new WaitForSeconds(delay);
            }
        }
        
        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float target, float duration)
        {
            if (cg == null) yield break;
            
            float start = cg.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                cg.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }
            
            cg.alpha = target;
        }
        
        private IEnumerator AnimateColor(Image image, Color target, float duration)
        {
            if (image == null) yield break;
            
            Color start = image.color;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                image.color = Color.Lerp(start, target, t);
                yield return null;
            }
            
            image.color = target;
        }
        
        private IEnumerator AnimateTextColor(Text text, Color target, float duration)
        {
            if (text == null) yield break;
            
            Color start = text.color;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                text.color = Color.Lerp(start, target, t);
                yield return null;
            }
            
            text.color = target;
        }
        
        private IEnumerator FadeText(Text text, float targetAlpha, float duration)
        {
            if (text == null) yield break;
            
            Color start = text.color;
            Color target = new Color(start.r, start.g, start.b, targetAlpha);
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = EaseOutQuad(elapsed / duration);
                text.color = Color.Lerp(start, target, t);
                yield return null;
            }
            
            text.color = target;
        }
        
        private IEnumerator ShakeTransform(RectTransform target, float intensity, float duration)
        {
            if (target == null) yield break;
            
            Vector2 originalPos = target.anchoredPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float decay = 1f - (elapsed / duration);
                float offsetX = Random.Range(-1f, 1f) * intensity * decay;
                target.anchoredPosition = originalPos + new Vector2(offsetX, 0f);
                yield return null;
            }
            
            target.anchoredPosition = originalPos;
        }
        
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        private void TriggerHapticFeedback()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            #endif
        }
        
        [System.Serializable]
        private class CreateGameRequest
        {
            public string playerId;
            public InitialTraits initialTraits;
        }
        
        [System.Serializable]
        private class InitialTraits
        {
            public int startingAge;
            public string characterName;
            public int intelligence;
            public int charisma;
            public int physical;
            public int creativity;
            public int health;
            public int wealth;
        }
    }
    
    public class ButtonPressEffect : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        private float pressScale = 0.95f;
        private float duration = 0.1f;
        private RectTransform rectTransform;
        private Vector3 originalScale;
        private Coroutine currentAnimation;
        
        public void Initialize(float scale, float animDuration)
        {
            pressScale = scale;
            duration = animDuration;
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;
        }
        
        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            AnimateTo(originalScale * pressScale);
        }
        
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            AnimateTo(originalScale);
        }
        
        private void AnimateTo(Vector3 target)
        {
            if (currentAnimation != null) StopCoroutine(currentAnimation);
            currentAnimation = StartCoroutine(ScaleAnimation(target));
        }
        
        private IEnumerator ScaleAnimation(Vector3 target)
        {
            Vector3 start = rectTransform.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (1f - elapsed / duration) * (1f - elapsed / duration);
                rectTransform.localScale = Vector3.Lerp(start, target, t);
                yield return null;
            }
            
            rectTransform.localScale = target;
        }
    }
}
