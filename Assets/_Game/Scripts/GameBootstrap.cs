using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wildlands
{
    // Entry point also works when Play is pressed in an empty scene.
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start()
        {
            if (Object.FindFirstObjectByType<HuntGame>() != null) return;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                var pipeline = UniversalRenderPipelineAsset.Create(renderer);
                GraphicsSettings.defaultRenderPipeline = pipeline;
                QualitySettings.renderPipeline = pipeline;
            }
            new GameObject("Wildlands").AddComponent<HuntGame>();
        }
    }
}
