using UnityEngine;
using UnityEngine.UI;

namespace LifeCraft.UI
{
    public class MainMenuSetup : MonoBehaviour
    {
        [Header("Theme Colors")]
        public Color backgroundDark = new Color(0.118f, 0.094f, 0.086f, 1f);
        public Color backgroundMid = new Color(0.188f, 0.153f, 0.133f, 1f);
        public Color primaryAccent = new Color(0.796f, 0.431f, 0.353f, 1f);
        public Color secondaryAccent = new Color(0.6f, 0.667f, 0.596f, 1f);
        public Color textLight = new Color(0.976f, 0.961f, 0.937f, 1f);
        public Color textMuted = new Color(0.7f, 0.65f, 0.6f, 1f);
        
        [ContextMenu("Generate Main Menu UI")]
        public void GenerateMainMenuUI()
        {
            Transform existingCanvas = transform.Find("MainMenuCanvas");
            if (existingCanvas != null)
            {
                DestroyImmediate(existingCanvas.gameObject);
            }
            
            GameObject canvasObj = CreateCanvas();
            CreateBackground(canvasObj.transform);
            CreateParticleContainer(canvasObj.transform);
            GameObject safeArea = CreateSafeArea(canvasObj.transform);
            CreateLogo(safeArea.transform);
            CreateButtonContainer(safeArea.transform);
            CreateVersionText(safeArea.transform);
            
            Debug.Log("Main Menu UI generated successfully!");
        }
        
        private GameObject CreateCanvas()
        {
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            canvasObj.transform.SetParent(transform);
            
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
        
        private void CreateBackground(Transform parent)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(parent, false);
            
            RectTransform rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = backgroundDark;
            bgImage.raycastTarget = false;
            
            CreateGradientOverlay(bgObj.transform);
            CreateDecorative(bgObj.transform);
        }
        
        private void CreateGradientOverlay(Transform parent)
        {
            GameObject gradientObj = new GameObject("GradientOverlay");
            gradientObj.transform.SetParent(parent, false);
            
            RectTransform rect = gradientObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.3f);
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image gradient = gradientObj.AddComponent<Image>();
            gradient.color = new Color(primaryAccent.r, primaryAccent.g, primaryAccent.b, 0.08f);
            gradient.raycastTarget = false;
        }
        
        private void CreateDecorative(Transform parent)
        {
            CreateDecorativeCircle(parent, new Vector2(-0.2f, 0.8f), 600f, primaryAccent, 0.05f);
            CreateDecorativeCircle(parent, new Vector2(1.1f, 0.6f), 400f, secondaryAccent, 0.04f);
            CreateDecorativeCircle(parent, new Vector2(0.5f, -0.1f), 800f, primaryAccent, 0.03f);
        }
        
        private void CreateDecorativeCircle(Transform parent, Vector2 anchor, float size, Color color, float alpha)
        {
            GameObject circleObj = new GameObject($"DecorativeCircle_{anchor}");
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
        
        private void CreateParticleContainer(Transform parent)
        {
            GameObject particleObj = new GameObject("ParticleContainer");
            particleObj.transform.SetParent(parent, false);
            
            RectTransform rect = particleObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        
        private GameObject CreateSafeArea(Transform parent)
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
        
        private void CreateLogo(Transform parent)
        {
            GameObject logoContainer = new GameObject("LogoContainer");
            logoContainer.transform.SetParent(parent, false);
            
            RectTransform containerRect = logoContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.65f);
            containerRect.anchorMax = new Vector2(0.5f, 0.65f);
            containerRect.sizeDelta = new Vector2(600f, 200f);
            containerRect.anchoredPosition = Vector2.zero;
            
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(logoContainer.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "LIFECRAFT";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 72;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = textLight;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            ContentSizeFitter fitter = titleObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(logoContainer.transform, false);
            
            RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0.5f, 0f);
            subtitleRect.anchorMax = new Vector2(0.5f, 0f);
            subtitleRect.sizeDelta = new Vector2(400f, 40f);
            subtitleRect.anchoredPosition = new Vector2(0f, -20f);
            
            Text subtitleText = subtitleObj.AddComponent<Text>();
            subtitleText.text = "Your Story Awaits";
            subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            subtitleText.fontSize = 24;
            subtitleText.fontStyle = FontStyle.Italic;
            subtitleText.color = textMuted;
            subtitleText.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreateButtonContainer(Transform parent)
        {
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(parent, false);
            
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.25f);
            containerRect.anchorMax = new Vector2(0.5f, 0.55f);
            containerRect.sizeDelta = new Vector2(400f, 0f);
            containerRect.anchoredPosition = Vector2.zero;
            
            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(40, 40, 0, 0);
            
            CreateMenuButton(buttonContainer.transform, "NewGameButton", "NEW GAME", ButtonType.Primary);
            CreateMenuButton(buttonContainer.transform, "ContinueButton", "CONTINUE", ButtonType.Secondary);
            CreateMenuButton(buttonContainer.transform, "SettingsButton", "SETTINGS", ButtonType.Tertiary);
            CreateMenuButton(buttonContainer.transform, "QuitButton", "QUIT", ButtonType.Tertiary);
        }
        
        private enum ButtonType { Primary, Secondary, Tertiary }
        
        private void CreateMenuButton(Transform parent, string name, string text, ButtonType type)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, type == ButtonType.Primary ? 64f : 56f);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            
            switch (type)
            {
                case ButtonType.Primary:
                    buttonBg.color = primaryAccent;
                    break;
                case ButtonType.Secondary:
                    buttonBg.color = new Color(textLight.r, textLight.g, textLight.b, 0.15f);
                    break;
                case ButtonType.Tertiary:
                    buttonBg.color = Color.clear;
                    break;
            }
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonBg;
            
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
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
            buttonText.fontSize = type == ButtonType.Primary ? 24 : 20;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            switch (type)
            {
                case ButtonType.Primary:
                    buttonText.color = textLight;
                    break;
                case ButtonType.Secondary:
                case ButtonType.Tertiary:
                    buttonText.color = textLight;
                    break;
            }
            
            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = rect.sizeDelta.y;
            layoutElement.minHeight = 44f;
        }
        
        private void CreateVersionText(Transform parent)
        {
            GameObject versionObj = new GameObject("VersionText");
            versionObj.transform.SetParent(parent, false);
            
            RectTransform rect = versionObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(200f, 30f);
            rect.anchoredPosition = new Vector2(0f, 40f);
            
            Text versionText = versionObj.AddComponent<Text>();
            versionText.text = "v0.1.0";
            versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            versionText.fontSize = 14;
            versionText.color = new Color(textMuted.r, textMuted.g, textMuted.b, 0.5f);
            versionText.alignment = TextAnchor.MiddleCenter;
        }
    }
}
