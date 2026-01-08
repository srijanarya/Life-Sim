using UnityEngine;
using UnityEditor;

/// <summary>
/// Configures Unity project for iOS build.
/// Sets up player settings, icons, and build configuration.
/// </summary>
public class BuildConfigurator
{
    public static void ConfigureForIOS()
    {
        ConfigurePlayerSettings();
        ConfigureBuildSettings();
        ConfigureQualitySettings();

        Debug.Log("[LifeCraft] iOS build configuration complete!");
    }

    private static void ConfigurePlayerSettings()
    {
        // Company and product info
        PlayerSettings.companyName = "LifeCraft Games";
        PlayerSettings.productName = "LifeCraft";

        // iOS specific settings
        PlayerSettings.iOS.applicationDisplayName = "LifeCraft";
        PlayerSettings.iOS.buildNumber = "1";

        // Bundle identifier (change this to your own)
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.lifecraft.lifesimulator");

        // iOS version requirements
        PlayerSettings.iOS.targetOSVersionString = "14.0";

        // Device orientation
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        // Status bar
        PlayerSettings.statusBarHidden = false;
        PlayerSettings.iOS.statusBarStyle = iOSStatusBarStyle.LightContent;

        // Splash screen
        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.SplashScreen.backgroundColor = new Color(0.059f, 0.090f, 0.165f);

        // Resolution and presentation
        PlayerSettings.runInBackground = false;
        PlayerSettings.iOS.requiresFullScreen = true;

        // Scripting
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64

        // API compatibility
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard_2_1);

        // Other iOS settings
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.hideHomeButton = false;
        PlayerSettings.iOS.allowHTTPDownload = true; // Allow HTTP for localhost dev

        Debug.Log("[LifeCraft] Player settings configured for iOS");
    }

    private static void ConfigureBuildSettings()
    {
        // Switch to iOS build target if not already
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            Debug.Log("[LifeCraft] Switched build target to iOS");
        }

        // Development build settings for testing
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;
        EditorUserBuildSettings.connectProfiler = false;

        // Build options
        EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Debug;
    }

    private static void ConfigureQualitySettings()
    {
        // Set quality level for mobile
        QualitySettings.SetQualityLevel(2, true); // Medium quality

        // Optimize for mobile
        QualitySettings.vSyncCount = 1; // 60fps cap
        QualitySettings.antiAliasing = 2; // 2x MSAA
        QualitySettings.shadows = ShadowQuality.Disable; // No shadows for 2D game
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.billboardsFaceCameraPosition = false;

        Debug.Log("[LifeCraft] Quality settings configured for mobile");
    }

    // Menu items for individual configurations
    [MenuItem("Tools/LifeCraft/Build/Switch to iOS", false, 21)]
    public static void SwitchToIOS()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            Debug.Log("[LifeCraft] Switched to iOS build target");
        }
        else
        {
            Debug.Log("[LifeCraft] Already targeting iOS");
        }
    }

    [MenuItem("Tools/LifeCraft/Build/Switch to Android", false, 22)]
    public static void SwitchToAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            ConfigureAndroid();
            Debug.Log("[LifeCraft] Switched to Android build target");
        }
        else
        {
            Debug.Log("[LifeCraft] Already targeting Android");
        }
    }

    private static void ConfigureAndroid()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.lifecraft.lifesimulator");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
    }

    [MenuItem("Tools/LifeCraft/Build/Open Build Settings", false, 30)]
    public static void OpenBuildSettings()
    {
        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }

    [MenuItem("Tools/LifeCraft/Build/Build iOS (Debug)", false, 40)]
    public static void BuildIOSDebug()
    {
        string path = EditorUtility.SaveFolderPanel("Choose Build Location", "", "LifeCraft-iOS");
        if (string.IsNullOrEmpty(path)) return;

        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetEnabledScenes();
        buildOptions.locationPathName = path;
        buildOptions.target = BuildTarget.iOS;
        buildOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging;

        BuildPipeline.BuildPlayer(buildOptions);
    }

    [MenuItem("Tools/LifeCraft/Build/Build iOS (Release)", false, 41)]
    public static void BuildIOSRelease()
    {
        string path = EditorUtility.SaveFolderPanel("Choose Build Location", "", "LifeCraft-iOS");
        if (string.IsNullOrEmpty(path)) return;

        // Switch to release
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig.Release;

        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetEnabledScenes();
        buildOptions.locationPathName = path;
        buildOptions.target = BuildTarget.iOS;
        buildOptions.options = BuildOptions.None;

        BuildPipeline.BuildPlayer(buildOptions);
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var enabledScenes = new System.Collections.Generic.List<string>();

        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                enabledScenes.Add(scene.path);
            }
        }

        return enabledScenes.ToArray();
    }
}
