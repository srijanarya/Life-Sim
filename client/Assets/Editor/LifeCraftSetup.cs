using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Main setup orchestrator for LifeCraft game.
/// Provides one-click setup to generate all scenes, prefabs, and configure iOS build.
/// </summary>
public class LifeCraftSetup : EditorWindow
{
    private static string[] requiredFolders = new string[]
    {
        "Assets/Scenes",
        "Assets/Prefabs",
        "Assets/Prefabs/UI",
        "Assets/Resources"
    };

    [MenuItem("Tools/LifeCraft/Setup Game %#l", false, 0)]
    public static void SetupGame()
    {
        if (EditorUtility.DisplayDialog("LifeCraft Setup",
            "This will create all scenes, prefabs, and configure the project for iOS.\n\nProceed?",
            "Setup", "Cancel"))
        {
            RunFullSetup();
        }
    }

    [MenuItem("Tools/LifeCraft/Quick Setup (Scenes Only)", false, 1)]
    public static void QuickSetup()
    {
        CreateFolderStructure();
        SceneGenerator.GenerateAllScenes();
        EditorUtility.DisplayDialog("Quick Setup Complete",
            "Scenes have been created!\n\nOpen Scenes/MainMenu to start.",
            "OK");
    }

    [MenuItem("Tools/LifeCraft/Configure iOS Build", false, 20)]
    public static void ConfigureIOS()
    {
        BuildConfigurator.ConfigureForIOS();
        EditorUtility.DisplayDialog("iOS Configuration",
            "iOS build settings have been configured.\n\nGo to File > Build Settings to build.",
            "OK");
    }

    [MenuItem("Tools/LifeCraft/Open Documentation", false, 100)]
    public static void OpenDocs()
    {
        Application.OpenURL("file://" + Path.GetFullPath("../QUICK_START.md"));
    }

    private static void RunFullSetup()
    {
        EditorUtility.DisplayProgressBar("LifeCraft Setup", "Creating folder structure...", 0.1f);
        CreateFolderStructure();

        EditorUtility.DisplayProgressBar("LifeCraft Setup", "Generating prefabs...", 0.3f);
        PrefabGenerator.GenerateAllPrefabs();

        EditorUtility.DisplayProgressBar("LifeCraft Setup", "Generating scenes...", 0.5f);
        SceneGenerator.GenerateAllScenes();

        EditorUtility.DisplayProgressBar("LifeCraft Setup", "Configuring build settings...", 0.8f);
        BuildConfigurator.ConfigureForIOS();

        EditorUtility.DisplayProgressBar("LifeCraft Setup", "Finalizing...", 0.95f);
        AssetDatabase.Refresh();

        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("Setup Complete!",
            "LifeCraft has been set up successfully!\n\n" +
            "Next steps:\n" +
            "1. Open Scenes/MainMenu\n" +
            "2. Press Play to test\n" +
            "3. File > Build Settings > iOS to build\n\n" +
            "Make sure the backend server is running!",
            "Let's Go!");

        // Open the main menu scene
        string mainMenuPath = "Assets/Scenes/MainMenu.unity";
        if (File.Exists(mainMenuPath))
        {
            EditorSceneManager.OpenScene(mainMenuPath);
        }
    }

    private static void CreateFolderStructure()
    {
        foreach (string folder in requiredFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string[] parts = folder.Split('/');
                string currentPath = parts[0];

                for (int i = 1; i < parts.Length; i++)
                {
                    string newPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = newPath;
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("[LifeCraft] Folder structure created");
    }

    // Validation methods
    [MenuItem("Tools/LifeCraft/Setup Game %#l", true)]
    private static bool ValidateSetup()
    {
        // Always allow setup
        return true;
    }
}
