using UnityEngine;
using UnityEngine.SceneManagement;
using PeakItemInsight.Diagnostics;

namespace PeakItemInsight.Detection;

internal sealed class RuntimeDriver : MonoBehaviour
{
    private float _nextHeartbeat;
    private bool _started;
    private readonly EngineSmokeTest _smokeTest = new EngineSmokeTest();
    private void Awake() => SessionTrace.Write("DRIVER_AWAKE");
    private void OnDestroy() => SessionTrace.Write("DRIVER_DESTROY");
    private void Update()
    {
        if (!_started) { _started = true; SessionTrace.Write("FIRST_FRAME", $"frame={Time.frameCount}"); }
        if (Time.unscaledTime >= _nextHeartbeat)
        {
            _nextHeartbeat = Time.unscaledTime + 5f;
            SessionTrace.Write("HEARTBEAT", $"frame={Time.frameCount} scene={SceneManager.GetActiveScene().name} runtime={Plugin.Runtime != null}");
        }
        Plugin.Runtime?.Tick();
        Plugin.StaminaRuntime?.Tick();
        WorldPreviewTrace.Tick();
        _smokeTest.Tick();
    }
}
