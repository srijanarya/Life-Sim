#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using LifeCraft.UI;
using LifeCraft.UI.Components;

namespace LifeCraft.Editor
{
    public static class MainMenuBuilder
    {
        private static readonly Color BackgroundDark = new Color(0.118f, 0.094f, 0.086f, 1f);
        private static readonly Color PrimaryAccent = new Color(0.796f, 0.431f, 0.353f, 1f);
        private static readonly Color SecondaryAccent = new Color(0.6f, 0.667f, 0.596f, 1f);
        private static readonly Color TextLight = new Color(0.976f, 0.961f, 0.937f, 1f);
        private static readonly Color TextMuted = new Color(0.7f, 0.65f, 0.6f, 1f);
        
        [MenuItem("LifeCraft/Create Main Menu UI", false, 100)]
        public static void CreateMainMenuUI()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            
            GameObject existingCanvas = GameObject.Find("MainMenuCanvas");
            if (existingCanvas != null)
            {
                if (!EditorUtility.DisplayDialog("Replace Existing UI?", 
                    "A MainMenuCanvas already exists. Replace it?", "Replace", "Cancel"))
                {
                    return;
                }
                Undo.DestroyObjectImmediate(existingCanvas);
            }
            
            GameObject canvasObj = CreateCanvas();
            CreateBackground(canvasObj.transform);
            GameObject safeArea = CreateSafeArea(canvasObj.transform);
            RectTransform logoTransform = CreateLogo(safeArea.transform);
            RectTransform buttonContainer = CreateButtonContainer(safeArea.transform, out Button[] buttons);
            RectTransform particleContainer = CreateParticleContainer(canvasObj.transform);
            CreateVersionText(safeArea.transform);
            
            MainMenuPanel mainMenuPanel = canvasObj.AddComponent<MainMenuPanel>();
            AssignSerializedFields(mainMenuPanel, canvasObj, logoTransform, buttonContainer, particleContainer, buttons);
            
            CreateEventSystem();
            
            Selection.activeGameObject = canvasObj;
            EditorGUIUtility.PingObject(canvasObj);
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            Debug.Log("Main Menu UI created successfully! Assign the MainMenuCanvas to UIManager.MainMenuPanel.");
        }
        
        [MenuItem("LifeCraft/Create Scene Transition Manager", false, 101)]
        public static void CreateSceneTransitionManager()
        {
            SceneTransitionManager existing = Object.FindAnyObjectByType<SceneTransitionManager>();
            if (existing != null)
            {
                Debug.Log("SceneTransitionManager already exists in the scene.");
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            
            GameObject transitionObj = new GameObject("SceneTransitionManager");
            transitionObj.AddComponent<SceneTransitionManager>();
            
            Undo.RegisterCreatedObjectUndo(transitionObj, "Create Scene Transition Manager");
            Selection.activeGameObject = transitionObj;
            
            Debug.Log("SceneTransitionManager created successfully!");
        }
        
        private static GameObject CreateCanvas()
        {
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Main Menu Canvas");
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            
            return canvasObj;
        }
        
        private static void CreateBackground(Transform parent)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(parent, false);
            
            RectTransform rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            AnimatedBackground animBg = bgObj.AddComponent<AnimatedBackground>();
            
            CreateGradientOverlay(bgObj.transform);
            CreateDecorativeElements(bgObj.transform);
        }
        
        private static void CreateGradientOverlay(Transform parent)
        {
            GameObject gradientObj = new GameObject("GradientOverlay");
            gradientObj.transform.SetParent(parent, false);
            
            RectTransform rect = gradientObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.4f);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image gradient = gradientObj.AddComponent<Image>();
            gradient.color = new Color(PrimaryAccent.r, PrimaryAccent.g, PrimaryAccent.b, 0.06f);
            gradient.raycastTarget = false;
        }
        
        private static void CreateDecorativeElements(Transform parent)
        {
            CreateDecorativeCircle(parent, "Circle1", new Vector2(-0.15f, 0.85f), 500f, PrimaryAccent, 0.04f);
            CreateDecorativeCircle(parent, "Circle2", new Vector2(1.1f, 0.55f), 350f, SecondaryAccent, 0.03f);
            CreateDecorativeCircle(parent, "Circle3", new Vector2(0.6f, -0.05f), 700f, PrimaryAccent, 0.025f);
        }
        
        private static void CreateDecorativeCircle(Transform parent, string name, Vector2 anchor, float size, Color color, float alpha)
        {
            GameObject circleObj = new GameObject(name);
            circleObj.transform.SetParent(parent, false);
            
            RectTransform rect = circleObj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
            
            Image circle = circleObj.AddComponent<Image>();
            circle.color = new Color(color.r, color.g, color.b, alpha);
            circle.raycastTarget = false;
        }
        
        private static RectTransform CreateParticleContainer(Transform parent)
        {
            GameObject particleObj = new GameObject("ParticleContainer");
            particleObj.transform.SetParent(parent, false);
            particleObj.transform.SetSiblingIndex(1);
            
            RectTransform rect = particleObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            return rect;
        }
        
        private static GameObject CreateSafeArea(Transform parent)
        {
            GameObject safeAreaObj = new GameObject("SafeArea");
            safeAreaObj.transform.SetParent(parent, false);
            
            RectTransform rect = safeAreaObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            safeAreaObj.AddComponent<SafeAreaHandler>();
            
            return safeAreaObj;
        }
        
        private static RectTransform CreateLogo(Transform parent)
        {
            GameObject logoContainer = new GameObject("LogoContainer");
            logoContainer.transform.SetParent(parent, false);
            
            RectTransform containerRect = logoContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.62f);
            containerRect.anchorMax = new Vector2(0.5f, 0.62f);
            containerRect.sizeDelta = new Vector2(600f, 180f);
            containerRect.anchoredPosition = Vector2.zero;
            
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(logoContainer.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.6f);
            titleRect.anchorMax = new Vector2(0.5f, 0.6f);
            titleRect.sizeDelta = new Vector2(500f, 80f);
            titleRect.anchoredPosition = Vector2.zero;
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "LIFECRAFT";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 64;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = TextLight;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;
            
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(logoContainer.transform, false);
            
            RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0.2f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0.2f);
            subtitleRect.sizeDelta = new Vector2(400f, 40f);
            subtitleRect.anchoredPosition = Vector2.zero;
            
            Text subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.text = "Your Story Awaits";
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subtitleText.fontSize = 22;
            subtitleText.fontStyle = FontStyle.Italic;
            subtitleText.color = TextMuted;
            subtitleText.alignment = TextAnchor.MiddleCenter;
            
            return containerRect;
        }
        
        private static RectTransform CreateButtonContainer(Transform parent, out Button[] buttons)
        {
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(parent, false);
            
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.22f);
            containerRect.anchorMax = new Vector2(0.5f, 0.52f);
            containerRect.sizeDelta = new Vector2(380f, 0f);
            containerRect.anchoredPosition = Vector2.zero;
            
            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(20, 20, 0, 0);
            
            buttons = new Button[4];
            buttons[0] = CreateMenuButton(buttonContainer.transform, "NewGameButton", "NEW GAME", ButtonStyleType.Primary, 60f);
            buttons[1] = CreateMenuButton(buttonContainer.transform, "ContinueButton", "CONTINUE", ButtonStyleType.Secondary, 52f);
            buttons[2] = CreateMenuButton(buttonContainer.transform, "SettingsButton", "SETTINGS", ButtonStyleType.Tertiary, 48f);
            buttons[3] = CreateMenuButton(buttonContainer.transform, "QuitButton", "QUIT", ButtonStyleType.Tertiary, 48f);
            
            return containerRect;
        }
        
        private enum ButtonStyleType { Primary, Secondary, Tertiary }
        
        private static Button CreateMenuButton(Transform parent, string name, string text, ButtonStyleType style, float height)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300f, height);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            
            Color buttonColor;
            Color textColor;
            
            switch (style)
            {
                case ButtonStyleType.Primary:
                    buttonColor = PrimaryAccent;
                    textColor = TextLight;
                    break;
                case ButtonStyleType.Secondary:
                    buttonColor = new Color(TextLight.r, TextLight.g, TextLight.b, 0.12f);
                    textColor = TextLight;
                    break;
                case ButtonStyleType.Tertiary:
                default:
                    buttonColor = Color.clear;
                    textColor = TextMuted;
                    break;
            }
            
            buttonBg.color = buttonColor;
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonBg;
            
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = style == ButtonStyleType.Primary ? 22 : 18;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.color = textColor;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.minHeight = 44f;
            
            return button;
        }
        
        private static void CreateVersionText(Transform parent)
        {
            GameObject versionObj = new GameObject("VersionText");
            versionObj.transform.SetParent(parent, false);
            
            RectTransform rect = versionObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(200f, 30f);
            rect.anchoredPosition = new Vector2(0f, 50f);
            
            Text versionText = versionObj.AddComponent<Text>();
            versionText.text = $"v{Application.version}";
            versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            versionText.fontSize = 12;
            versionText.color = new Color(TextMuted.r, TextMuted.g, TextMuted.b, 0.4f);
            versionText.alignment = TextAnchor.MiddleCenter;
        }
        
        private static void CreateEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }
            
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create Event System");
        }
        
        private static void AssignSerializedFields(MainMenuPanel panel, GameObject canvas, 
            RectTransform logoTransform, RectTransform buttonContainer, RectTransform particleContainer, Button[] buttons)
        {
            SerializedObject serializedPanel = new SerializedObject(panel);
            
            serializedPanel.FindProperty("newGameButton").objectReferenceValue = buttons[0];
            serializedPanel.FindProperty("continueButton").objectReferenceValue = buttons[1];
            serializedPanel.FindProperty("settingsButton").objectReferenceValue = buttons[2];
            serializedPanel.FindProperty("quitButton").objectReferenceValue = buttons[3];
            
            serializedPanel.FindProperty("panelCanvasGroup").objectReferenceValue = canvas.GetComponent<CanvasGroup>();
            serializedPanel.FindProperty("logoTransform").objectReferenceValue = logoTransform;
            serializedPanel.FindProperty("buttonContainer").objectReferenceValue = buttonContainer;
            serializedPanel.FindProperty("particleContainer").objectReferenceValue = particleContainer;
            
            serializedPanel.ApplyModifiedProperties();
        }
    }
}
#endif
