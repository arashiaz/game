using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wildlands
{
    public sealed class PerformanceSettings : MonoBehaviour
    {
        public enum Detail { Low, Medium, High }
        public Detail Quality { get; private set; }
        public int FrameCap { get; private set; }
        public bool Automatic { get; private set; }
        public float MeasuredFps { get; private set; }
        private float sampleTime;
        private int samples;
        private float warmup = 8;
        private int slowWindows;
        private float renderScale;
        private const string Key = "wildlands.settings.v1";

        [System.Serializable]
        private class Saved { public int quality = 1; public int cap = 30; public bool automatic = true; }

        private void Awake()
        {
            var saved = new Saved();
            try { if (PlayerPrefs.HasKey(Key)) saved = JsonUtility.FromJson<Saved>(PlayerPrefs.GetString(Key)) ?? saved; }
            catch (System.ArgumentException) { /* Ignore malformed local preferences. */ }
            Automatic = saved.automatic;
            FrameCap = saved.cap == 60 ? 60 : 30;
            Quality = (Detail)Mathf.Clamp(saved.quality, 0, 2);
            if (Automatic) Quality = Recommend();
            Apply();
        }

        private static Detail Recommend()
        {
            // Conservative starting heuristic, not a device benchmark or RAM-extension estimate.
            int memory = SystemInfo.systemMemorySize;
            int gpu = SystemInfo.graphicsMemorySize;
            if (memory >= 8000 && gpu >= 2000 && SystemInfo.graphicsShaderLevel >= 45) return Detail.High;
            return memory >= 4000 ? Detail.Medium : Detail.Low;
        }

        public void Configure(Detail quality, int cap, bool automatic)
        {
            Automatic = automatic;
            Quality = automatic ? Recommend() : quality;
            FrameCap = cap == 60 ? 60 : 30;
            warmup = 8;
            slowWindows = 0;
            sampleTime = 0;
            samples = 0;
            Apply();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(new Saved { quality = (int)Quality, cap = FrameCap, automatic = Automatic }));
            PlayerPrefs.Save();
        }

        private void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = FrameCap;
            QualitySettings.shadows = Quality == Detail.Low ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.shadowDistance = Quality == Detail.High ? 65 : 35;
            QualitySettings.shadowResolution = Quality == Detail.High ? ShadowResolution.High : ShadowResolution.Low;
            QualitySettings.lodBias = Quality == Detail.High ? 1.5f : 0.8f;
            renderScale = Quality == Detail.High ? 1f : Quality == Detail.Medium ? 0.85f : 0.7f;
            SetScale();
            // URP controls its own shadow distance and MSAA.
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
            {
                urp.shadowDistance = Quality == Detail.Low ? 0 : Quality == Detail.High ? 65 : 35;
                urp.msaaSampleCount = Quality == Detail.High ? 4 : 2;
            }
        }

        private void SetScale()
        {
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
                urp.renderScale = renderScale;
        }

        private void Update()
        {
            if (Time.timeScale == 0) { sampleTime = 0; samples = 0; return; }
            if (warmup > 0) { warmup -= Time.unscaledDeltaTime; return; }
            sampleTime += Time.unscaledDeltaTime;
            samples++;
            if (sampleTime < 4) return;
            MeasuredFps = samples / sampleTime;
            sampleTime = 0;
            samples = 0;
            if (!Automatic) return;
            slowWindows = MeasuredFps < FrameCap * 0.8f ? slowWindows + 1 : 0;
            // Hysteresis: require 12 seconds of sustained low FPS, never oscillate upward.
            if (slowWindows >= 3 && renderScale > 0.6f)
            {
                renderScale = Mathf.Max(0.6f, renderScale - 0.1f);
                SetScale();
                slowWindows = 0;
            }
        }
    }
}
