using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Generates all game scenes programmatically.
/// Creates MainMenu, CharacterCreation, and Gameplay scenes with proper hierarchy.
/// </summary>
public class SceneGenerator
{
    private static Color bgColor = new Color(0.059f, 0.090f, 0.165f); // #0f172a

    public static void GenerateAllScenes()
    {
        GenerateMainMenuScene();
        GenerateCharacterCreationScene();
        GenerateGameplayScene();

        // Add scenes to build settings
        AddScenesToBuildSettings();

        Debug.Log("[LifeCraft] All scenes generated successfully!");
    }

    public static void GenerateMainMenuScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = bgColor;
        camera.orthographic = true;
        cameraGO.AddComponent<AudioListener>();
        cameraGO.tag = "MainCamera";

        // Event System
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // Canvas
        var canvasGO = CreateCanvas("MainCanvas");

        // Add managers
        var managersGO = new GameObject("--- MANAGERS ---");

        var gameManager = new GameObject("GameManager");
        gameManager.transform.SetParent(managersGO.transform);
        AddScriptIfExists(gameManager, "GameManager");

        var apiClient = new GameObject("ApiClient");
        apiClient.transform.SetParent(managersGO.transform);
        AddScriptIfExists(apiClient, "ApiClient");

        var uiManager = new GameObject("UIManager");
        uiManager.transform.SetParent(managersGO.transform);
        AddScriptIfExists(uiManager, "UIManager");

        // Main Menu Panel
        var mainMenuPanel = CreatePanel(canvasGO.transform, "MainMenuPanel");
        AddScriptIfExists(mainMenuPanel, "MainMenuPanel");

        // Title
        var titleGO = CreateText(mainMenuPanel.transform, "Title", "LifeCraft",
            new Vector2(0, 200), 72, TextAnchor.MiddleCenter);
        titleGO.GetComponent<Text>().color = new Color(0.39f, 0.4f, 0.95f);

        // Subtitle
        CreateText(mainMenuPanel.transform, "Subtitle", "Your Life, Your Choices",
            new Vector2(0, 130), 24, TextAnchor.MiddleCenter);

        // New Game Button
        var newGameBtn = CreateButton(mainMenuPanel.transform, "NewGameButton", "New Life",
            new Vector2(0, 20), new Vector2(280, 60));
        newGameBtn.GetComponent<Image>().color = new Color(0.39f, 0.4f, 0.95f);

        // Continue Button
        var continueBtn = CreateButton(mainMenuPanel.transform, "ContinueButton", "Continue",
            new Vector2(0, -50), new Vector2(280, 60));
        continueBtn.GetComponent<Image>().color = new Color(0.06f, 0.73f, 0.51f);

        // Settings Button
        CreateButton(mainMenuPanel.transform, "SettingsButton", "Settings",
            new Vector2(0, -120), new Vector2(280, 50));

        // Quit Button
        var quitBtn = CreateButton(mainMenuPanel.transform, "QuitButton", "Quit",
            new Vector2(0, -190), new Vector2(280, 50));
        quitBtn.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f);

        // Save scene
        string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[LifeCraft] MainMenu scene created: " + scenePath);
    }

    public static void GenerateCharacterCreationScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = bgColor;
        camera.orthographic = true;
        cameraGO.AddComponent<AudioListener>();
        cameraGO.tag = "MainCamera";

        // Event System
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // Canvas
        var canvasGO = CreateCanvas("CharacterCanvas");

        // Character Creation Panel
        var creationPanel = CreatePanel(canvasGO.transform, "CharacterCreationPanel");
        AddScriptIfExists(creationPanel, "CharacterCreationPanel");

        // Title
        CreateText(creationPanel.transform, "Title", "Create Your Character",
            new Vector2(0, 280), 36, TextAnchor.MiddleCenter);

        // Name Input
        CreateText(creationPanel.transform, "NameLabel", "Name:",
            new Vector2(-100, 200), 20, TextAnchor.MiddleLeft);
        CreateInputField(creationPanel.transform, "NameInput",
            new Vector2(50, 200), new Vector2(200, 40));

        // Age Input
        CreateText(creationPanel.transform, "AgeLabel", "Starting Age:",
            new Vector2(-100, 140), 20, TextAnchor.MiddleLeft);
        CreateInputField(creationPanel.transform, "AgeInput",
            new Vector2(50, 140), new Vector2(100, 40), "18");

        // Stats Label
        CreateText(creationPanel.transform, "StatsLabel", "Distribute 20 Bonus Points",
            new Vector2(0, 80), 24, TextAnchor.MiddleCenter);

        // Stat Sliders
        string[] stats = { "Health", "Intelligence", "Charisma", "Creativity", "Happiness" };
        float yPos = 20;

        foreach (string stat in stats)
        {
            CreateStatSlider(creationPanel.transform, stat, new Vector2(0, yPos));
            yPos -= 50;
        }

        // Buttons
        CreateButton(creationPanel.transform, "BackButton", "Back",
            new Vector2(-80, -280), new Vector2(140, 50));

        var startBtn = CreateButton(creationPanel.transform, "StartButton", "Start Life",
            new Vector2(80, -280), new Vector2(140, 50));
        startBtn.GetComponent<Image>().color = new Color(0.39f, 0.4f, 0.95f);

        // Save scene
        string scenePath = "Assets/Scenes/CharacterCreation.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[LifeCraft] CharacterCreation scene created: " + scenePath);
    }

    public static void GenerateGameplayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cameraGO = new GameObject("Main Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = bgColor;
        camera.orthographic = true;
        cameraGO.AddComponent<AudioListener>();
        cameraGO.tag = "MainCamera";

        // Event System
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // Canvas
        var canvasGO = CreateCanvas("GameplayCanvas");

        // Stats Panel (Top)
        var statsPanel = CreatePanel(canvasGO.transform, "StatsPanel", true);
        var statsRect = statsPanel.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 1);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.pivot = new Vector2(0.5f, 1);
        statsRect.sizeDelta = new Vector2(0, 120);
        statsRect.anchoredPosition = new Vector2(0, 0);
        AddScriptIfExists(statsPanel, "StatsPanel");

        // Character Name
        CreateText(statsPanel.transform, "CharacterName", "Character Name",
            new Vector2(0, -20), 28, TextAnchor.MiddleCenter);

        // Age/Time Info
        CreateText(statsPanel.transform, "TimeInfo", "Age: 18 | Year: 1 | Month: 1",
            new Vector2(0, -50), 18, TextAnchor.MiddleCenter);

        // Stat Bars Container
        var statBars = new GameObject("StatBars");
        statBars.transform.SetParent(statsPanel.transform);
        var statBarsRect = statBars.AddComponent<RectTransform>();
        statBarsRect.anchoredPosition = new Vector2(0, -85);

        CreateStatBar(statBars.transform, "Health", -120, new Color(0.94f, 0.27f, 0.27f));
        CreateStatBar(statBars.transform, "Happiness", 0, new Color(0.98f, 0.75f, 0.15f));
        CreateStatBar(statBars.transform, "Wealth", 120, new Color(0.06f, 0.73f, 0.51f));

        // Event Panel (Center)
        var eventPanel = CreatePanel(canvasGO.transform, "EventPanel");
        var eventRect = eventPanel.GetComponent<RectTransform>();
        eventRect.anchorMin = new Vector2(0.1f, 0.25f);
        eventRect.anchorMax = new Vector2(0.9f, 0.75f);
        eventRect.offsetMin = Vector2.zero;
        eventRect.offsetMax = Vector2.zero;
        AddScriptIfExists(eventPanel, "EventPopupPanel");

        // Event Type Badge
        var typeBadge = CreatePanel(eventPanel.transform, "TypeBadge");
        var badgeRect = typeBadge.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0, 1);
        badgeRect.anchorMax = new Vector2(0, 1);
        badgeRect.pivot = new Vector2(0, 1);
        badgeRect.sizeDelta = new Vector2(120, 30);
        badgeRect.anchoredPosition = new Vector2(20, -20);
        typeBadge.GetComponent<Image>().color = new Color(0.39f, 0.4f, 0.95f);

        CreateText(typeBadge.transform, "TypeText", "LIFE EVENT",
            Vector2.zero, 14, TextAnchor.MiddleCenter);

        // Event Title
        CreateText(eventPanel.transform, "EventTitle", "Welcome to Your Life!",
            new Vector2(0, 60), 28, TextAnchor.MiddleCenter);

        // Event Description
        var descText = CreateText(eventPanel.transform, "EventDescription",
            "Your journey begins. Every choice you make will shape your destiny.",
            new Vector2(0, 0), 18, TextAnchor.MiddleCenter);
        var descRect = descText.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(300, 80);

        // Decisions Container
        var decisions = new GameObject("DecisionsContainer");
        decisions.transform.SetParent(eventPanel.transform);
        var decisionsRect = decisions.AddComponent<RectTransform>();
        decisionsRect.anchoredPosition = new Vector2(0, -80);
        var vlg = decisions.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // Sample Decision Buttons
        CreateDecisionButton(decisions.transform, "Decision1", "Make a bold choice");
        CreateDecisionButton(decisions.transform, "Decision2", "Play it safe");

        // Bottom Actions
        var actionsPanel = CreatePanel(canvasGO.transform, "ActionsPanel", true);
        var actionsRect = actionsPanel.GetComponent<RectTransform>();
        actionsRect.anchorMin = new Vector2(0, 0);
        actionsRect.anchorMax = new Vector2(1, 0);
        actionsRect.pivot = new Vector2(0.5f, 0);
        actionsRect.sizeDelta = new Vector2(0, 80);
        actionsRect.anchoredPosition = new Vector2(0, 0);

        var hlg = actionsPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(20, 20, 10, 10);

        CreateButton(actionsPanel.transform, "AdvanceButton", "Next Month",
            Vector2.zero, new Vector2(140, 50));
        CreateButton(actionsPanel.transform, "StatsButton", "View Stats",
            Vector2.zero, new Vector2(120, 50));
        var quitBtn = CreateButton(actionsPanel.transform, "QuitButton", "Quit",
            Vector2.zero, new Vector2(100, 50));
        quitBtn.GetComponent<Image>().color = new Color(0.94f, 0.27f, 0.27f, 0.8f);

        // Save scene
        string scenePath = "Assets/Scenes/Gameplay.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[LifeCraft] Gameplay scene created: " + scenePath);
    }

    private static void AddScenesToBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/CharacterCreation.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Gameplay.unity", true)
        };

        EditorBuildSettings.scenes = scenes;
        Debug.Log("[LifeCraft] Scenes added to build settings");
    }

    // Helper Methods
    private static GameObject CreateCanvas(string name)
    {
        var canvasGO = new GameObject(name);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        return canvasGO;
    }

    private static GameObject CreatePanel(Transform parent, string name, bool transparent = false)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent);

        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = panel.AddComponent<Image>();
        image.color = transparent ? new Color(0, 0, 0, 0) : new Color(0.118f, 0.161f, 0.231f, 0.95f);

        return panel;
    }

    private static GameObject CreateText(Transform parent, string name, string text,
        Vector2 position, int fontSize, TextAnchor alignment)
    {
        var textGO = new GameObject(name);
        textGO.transform.SetParent(parent);

        var rect = textGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);

        var textComp = textGO.AddComponent<Text>();
        textComp.text = text;
        textComp.fontSize = fontSize;
        textComp.alignment = alignment;
        textComp.color = new Color(0.945f, 0.961f, 0.973f);
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return textGO;
    }

    private static GameObject CreateButton(Transform parent, string name, string text,
        Vector2 position, Vector2 size)
    {
        var buttonGO = new GameObject(name);
        buttonGO.transform.SetParent(parent);

        var rect = buttonGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.26f, 0.34f);

        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;

        // Button text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var textComp = textGO.AddComponent<Text>();
        textComp.text = text;
        textComp.fontSize = 22;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return buttonGO;
    }

    private static GameObject CreateInputField(Transform parent, string name,
        Vector2 position, Vector2 size, string defaultText = "")
    {
        var inputGO = new GameObject(name);
        inputGO.transform.SetParent(parent);

        var rect = inputGO.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = inputGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.34f);

        // Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        var textComp = textGO.AddComponent<Text>();
        textComp.fontSize = 18;
        textComp.alignment = TextAnchor.MiddleLeft;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.text = defaultText;

        // Placeholder
        var placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputGO.transform);
        var phRect = placeholderGO.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(10, 0);
        phRect.offsetMax = new Vector2(-10, 0);

        var phText = placeholderGO.AddComponent<Text>();
        phText.text = "Enter...";
        phText.fontSize = 18;
        phText.fontStyle = FontStyle.Italic;
        phText.alignment = TextAnchor.MiddleLeft;
        phText.color = new Color(0.5f, 0.5f, 0.5f);
        phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var inputField = inputGO.AddComponent<InputField>();
        inputField.textComponent = textComp;
        inputField.placeholder = phText;
        inputField.text = defaultText;

        return inputGO;
    }

    private static void CreateStatSlider(Transform parent, string statName, Vector2 position)
    {
        var container = new GameObject(statName + "Stat");
        container.transform.SetParent(parent);

        var rect = container.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 40);

        // Label
        CreateText(container.transform, "Label", statName + ":",
            new Vector2(-100, 0), 18, TextAnchor.MiddleLeft);

        // Value
        CreateText(container.transform, "Value", "50",
            new Vector2(120, 0), 18, TextAnchor.MiddleRight);

        // Slider background
        var sliderBg = new GameObject("SliderBG");
        sliderBg.transform.SetParent(container.transform);
        var bgRect = sliderBg.AddComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(20, 0);
        bgRect.sizeDelta = new Vector2(150, 10);
        var bgImage = sliderBg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.25f, 0.34f);

        // Note: Full slider implementation would need Slider component
    }

    private static void CreateStatBar(Transform parent, string name, float xPos, Color color)
    {
        var container = new GameObject(name + "Bar");
        container.transform.SetParent(parent);

        var rect = container.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(xPos, 0);
        rect.sizeDelta = new Vector2(80, 25);

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(container.transform);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.25f, 0.34f);

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(container.transform);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.5f, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = color;
    }

    private static void CreateDecisionButton(Transform parent, string name, string text)
    {
        var btn = CreateButton(parent, name, text, Vector2.zero, new Vector2(280, 50));
        btn.GetComponent<Image>().color = new Color(0.2f, 0.26f, 0.34f);

        var le = btn.AddComponent<LayoutElement>();
        le.preferredWidth = 280;
        le.preferredHeight = 50;
    }

    private static void AddScriptIfExists(GameObject go, string scriptName)
    {
        // Try to find and add the script
        string[] guids = AssetDatabase.FindAssets(scriptName + " t:MonoScript");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(scriptName + ".cs"))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null)
                {
                    System.Type type = script.GetClass();
                    if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
                    {
                        go.AddComponent(type);
                        return;
                    }
                }
            }
        }
        Debug.LogWarning($"[LifeCraft] Script not found: {scriptName}");
    }
}
