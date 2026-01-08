using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

/// <summary>
/// Generates UI prefabs for the game.
/// Creates reusable UI components like buttons, panels, stat bars, etc.
/// </summary>
public class PrefabGenerator
{
    private static string prefabsPath = "Assets/Prefabs/UI";

    public static void GenerateAllPrefabs()
    {
        // Ensure directory exists
        if (!Directory.Exists(prefabsPath))
        {
            Directory.CreateDirectory(prefabsPath);
            AssetDatabase.Refresh();
        }

        GenerateButtonPrefab();
        GenerateStatBarPrefab();
        GenerateEventCardPrefab();
        GenerateDecisionButtonPrefab();
        GenerateToastPrefab();

        AssetDatabase.Refresh();
        Debug.Log("[LifeCraft] All prefabs generated successfully!");
    }

    private static void GenerateButtonPrefab()
    {
        var buttonGO = new GameObject("GameButton");

        var rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.39f, 0.4f, 0.95f);

        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;

        // Add color transition
        var colors = button.colors;
        colors.normalColor = new Color(0.39f, 0.4f, 0.95f);
        colors.highlightedColor = new Color(0.5f, 0.51f, 0.98f);
        colors.pressedColor = new Color(0.31f, 0.32f, 0.76f);
        button.colors = colors;

        // Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var text = textGO.AddComponent<Text>();
        text.text = "Button";
        text.fontSize = 22;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Save prefab
        string path = prefabsPath + "/GameButton.prefab";
        PrefabUtility.SaveAsPrefabAsset(buttonGO, path);
        Object.DestroyImmediate(buttonGO);

        Debug.Log("[LifeCraft] Created prefab: GameButton");
    }

    private static void GenerateStatBarPrefab()
    {
        var statBar = new GameObject("StatBar");

        var rect = statBar.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 30);

        // Icon placeholder
        var icon = new GameObject("Icon");
        icon.transform.SetParent(statBar.transform);
        var iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.sizeDelta = new Vector2(24, 24);
        iconRect.anchoredPosition = new Vector2(0, 0);
        var iconImage = icon.AddComponent<Image>();
        iconImage.color = Color.white;

        // Background
        var bg = new GameObject("Background");
        bg.transform.SetParent(statBar.transform);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(1, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.offsetMin = new Vector2(30, -5);
        bgRect.offsetMax = new Vector2(-40, 5);
        var bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.25f, 0.34f);

        // Fill
        var fill = new GameObject("Fill");
        fill.transform.SetParent(bg.transform);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.75f, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.94f, 0.27f, 0.27f);

        // Value text
        var value = new GameObject("Value");
        value.transform.SetParent(statBar.transform);
        var valueRect = value.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(1, 0.5f);
        valueRect.anchorMax = new Vector2(1, 0.5f);
        valueRect.pivot = new Vector2(1, 0.5f);
        valueRect.sizeDelta = new Vector2(35, 20);
        valueRect.anchoredPosition = new Vector2(0, 0);
        var valueText = value.AddComponent<Text>();
        valueText.text = "75";
        valueText.fontSize = 16;
        valueText.alignment = TextAnchor.MiddleRight;
        valueText.color = Color.white;
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Save prefab
        string path = prefabsPath + "/StatBar.prefab";
        PrefabUtility.SaveAsPrefabAsset(statBar, path);
        Object.DestroyImmediate(statBar);

        Debug.Log("[LifeCraft] Created prefab: StatBar");
    }

    private static void GenerateEventCardPrefab()
    {
        var card = new GameObject("EventCard");

        var rect = card.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(340, 400);

        var image = card.AddComponent<Image>();
        image.color = new Color(0.118f, 0.161f, 0.231f);

        // Header
        var header = new GameObject("Header");
        header.transform.SetParent(card.transform);
        var headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 50);
        headerRect.anchoredPosition = Vector2.zero;

        // Type badge
        var badge = new GameObject("TypeBadge");
        badge.transform.SetParent(header.transform);
        var badgeRect = badge.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0, 0.5f);
        badgeRect.anchorMax = new Vector2(0, 0.5f);
        badgeRect.pivot = new Vector2(0, 0.5f);
        badgeRect.sizeDelta = new Vector2(100, 28);
        badgeRect.anchoredPosition = new Vector2(15, 0);
        var badgeImage = badge.AddComponent<Image>();
        badgeImage.color = new Color(0.39f, 0.4f, 0.95f);

        var badgeText = new GameObject("Text");
        badgeText.transform.SetParent(badge.transform);
        var badgeTextRect = badgeText.AddComponent<RectTransform>();
        badgeTextRect.anchorMin = Vector2.zero;
        badgeTextRect.anchorMax = Vector2.one;
        badgeTextRect.offsetMin = Vector2.zero;
        badgeTextRect.offsetMax = Vector2.zero;
        var bText = badgeText.AddComponent<Text>();
        bText.text = "CAREER";
        bText.fontSize = 12;
        bText.alignment = TextAnchor.MiddleCenter;
        bText.color = Color.white;
        bText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Rarity
        var rarity = new GameObject("Rarity");
        rarity.transform.SetParent(header.transform);
        var rarityRect = rarity.AddComponent<RectTransform>();
        rarityRect.anchorMin = new Vector2(1, 0.5f);
        rarityRect.anchorMax = new Vector2(1, 0.5f);
        rarityRect.pivot = new Vector2(1, 0.5f);
        rarityRect.sizeDelta = new Vector2(80, 20);
        rarityRect.anchoredPosition = new Vector2(-15, 0);
        var rarityText = rarity.AddComponent<Text>();
        rarityText.text = "Common";
        rarityText.fontSize = 14;
        rarityText.alignment = TextAnchor.MiddleRight;
        rarityText.color = new Color(0.58f, 0.64f, 0.72f);
        rarityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Title
        var title = new GameObject("Title");
        title.transform.SetParent(card.transform);
        var titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(300, 40);
        titleRect.anchoredPosition = new Vector2(0, -60);
        var titleText = title.AddComponent<Text>();
        titleText.text = "Event Title";
        titleText.fontSize = 26;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Description
        var desc = new GameObject("Description");
        desc.transform.SetParent(card.transform);
        var descRect = desc.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 1);
        descRect.anchorMax = new Vector2(0.5f, 1);
        descRect.pivot = new Vector2(0.5f, 1);
        descRect.sizeDelta = new Vector2(300, 100);
        descRect.anchoredPosition = new Vector2(0, -110);
        var descText = desc.AddComponent<Text>();
        descText.text = "Event description goes here. This explains what's happening in your life.";
        descText.fontSize = 16;
        descText.alignment = TextAnchor.UpperCenter;
        descText.color = new Color(0.58f, 0.64f, 0.72f);
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Decisions container
        var decisions = new GameObject("Decisions");
        decisions.transform.SetParent(card.transform);
        var decisionsRect = decisions.AddComponent<RectTransform>();
        decisionsRect.anchorMin = new Vector2(0, 0);
        decisionsRect.anchorMax = new Vector2(1, 0);
        decisionsRect.pivot = new Vector2(0.5f, 0);
        decisionsRect.sizeDelta = new Vector2(0, 180);
        decisionsRect.anchoredPosition = new Vector2(0, 20);
        var vlg = decisions.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.LowerCenter;
        vlg.padding = new RectOffset(20, 20, 10, 10);

        // Save prefab
        string path = prefabsPath + "/EventCard.prefab";
        PrefabUtility.SaveAsPrefabAsset(card, path);
        Object.DestroyImmediate(card);

        Debug.Log("[LifeCraft] Created prefab: EventCard");
    }

    private static void GenerateDecisionButtonPrefab()
    {
        var button = new GameObject("DecisionButton");

        var rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 50);

        var image = button.AddComponent<Image>();
        image.color = new Color(0.2f, 0.26f, 0.34f);

        var btn = button.AddComponent<Button>();
        btn.targetGraphic = image;

        var colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.26f, 0.34f);
        colors.highlightedColor = new Color(0.25f, 0.31f, 0.4f);
        colors.pressedColor = new Color(0.15f, 0.2f, 0.28f);
        btn.colors = colors;

        // Text
        var text = new GameObject("Text");
        text.transform.SetParent(button.transform);
        var textRect = text.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 0);
        textRect.offsetMax = new Vector2(-15, 0);
        var textComp = text.AddComponent<Text>();
        textComp.text = "Make this choice";
        textComp.fontSize = 18;
        textComp.alignment = TextAnchor.MiddleLeft;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Layout element for proper sizing in layout groups
        var le = button.AddComponent<LayoutElement>();
        le.preferredWidth = 280;
        le.preferredHeight = 50;

        // Save prefab
        string path = prefabsPath + "/DecisionButton.prefab";
        PrefabUtility.SaveAsPrefabAsset(button, path);
        Object.DestroyImmediate(button);

        Debug.Log("[LifeCraft] Created prefab: DecisionButton");
    }

    private static void GenerateToastPrefab()
    {
        var toast = new GameObject("Toast");

        var rect = toast.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 60);

        var image = toast.AddComponent<Image>();
        image.color = new Color(0.118f, 0.161f, 0.231f);

        // Accent bar (left side)
        var accent = new GameObject("Accent");
        accent.transform.SetParent(toast.transform);
        var accentRect = accent.AddComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0, 0);
        accentRect.anchorMax = new Vector2(0, 1);
        accentRect.pivot = new Vector2(0, 0.5f);
        accentRect.sizeDelta = new Vector2(4, 0);
        accentRect.anchoredPosition = Vector2.zero;
        var accentImage = accent.AddComponent<Image>();
        accentImage.color = new Color(0.06f, 0.73f, 0.51f); // Success green

        // Message
        var message = new GameObject("Message");
        message.transform.SetParent(toast.transform);
        var msgRect = message.AddComponent<RectTransform>();
        msgRect.anchorMin = Vector2.zero;
        msgRect.anchorMax = Vector2.one;
        msgRect.offsetMin = new Vector2(20, 10);
        msgRect.offsetMax = new Vector2(-20, -10);
        var msgText = message.AddComponent<Text>();
        msgText.text = "Toast message";
        msgText.fontSize = 18;
        msgText.alignment = TextAnchor.MiddleLeft;
        msgText.color = Color.white;
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Canvas group for fade animations
        toast.AddComponent<CanvasGroup>();

        // Save prefab
        string path = prefabsPath + "/Toast.prefab";
        PrefabUtility.SaveAsPrefabAsset(toast, path);
        Object.DestroyImmediate(toast);

        Debug.Log("[LifeCraft] Created prefab: Toast");
    }
}
