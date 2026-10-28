using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wildlands.Editor
{
    public static class ProjectTools
    {
        [MenuItem("Wildlands/Prepare Android project")]
        public static void Prepare()
        {
            PlayerSettings.companyName = "Wildlands Studio";
            PlayerSettings.productName = "Wildlands Hunt";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.wildlands.hunt");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            // Legacy input is deliberately used in this first prototype for keyboard + UGUI.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null) { input.intValue = 0; settings.ApplyModifiedProperties(); }
            // Persist the pipeline so build-time shader stripping sees the URP configuration.
            const string pipelinePath = "Assets/_Game/MobilePipeline.asset";
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, "Assets/_Game/MobileRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            for (int level = 0; level < QualitySettings.names.Length; level++)
            {
                int previous = QualitySettings.GetQualityLevel();
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.renderPipeline = pipeline;
                QualitySettings.SetQualityLevel(previous, false);
            }
            const string path = "Assets/_Game/Scenes/HuntingGround.unity";
            if (!System.IO.File.Exists(path))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, path);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Android ARM64 configured. Open HuntingGround and press Play. Android Build Support is required for APK builds.");
        }
        [MenuItem("Wildlands/Build development APK")]
        public static void BuildAndroid()
        {
            Prepare();
            System.IO.Directory.CreateDirectory("Builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Game/Scenes/HuntingGround.unity" },
                locationPathName = "Builds/Wildlands-dev.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Android build failed: " + report.summary.result);
        }
    }
}
