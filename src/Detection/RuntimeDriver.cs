using UnityEngine;
using UnityEngine.SceneManagement;
using PeakItemInsight.Diagnostics;

namespace PeakItemInsight.Detection;

internal sealed class RuntimeDriver : MonoBehaviour
{
    private float _nextHeartbeat;
    private float _heartbeatTime;
    private int _heartbeatFrame;
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
            var now = Time.realtimeSinceStartup;
            var fps = (Time.frameCount - _heartbeatFrame) / Mathf.Max(.001f, now - _heartbeatTime);
            _heartbeatTime = now; _heartbeatFrame = Time.frameCount;
            var character = Character.localCharacter;
            var gui = GUIManager.instance;
            SessionTrace.Write("HEARTBEAT", $"frame={Time.frameCount} scene={SceneManager.GetActiveScene().name} runtime={Plugin.Runtime != null} fps={fps:0.0} localPlayer={Player.localPlayer != null} userIdReady={(character != null && character.data.userID != 0)} slotBlocked={(character != null && character.input.itemSwitchBlocked)} modal={(gui != null && gui.windowBlockingInput)}");
        }
        Plugin.Runtime?.Tick();
        Plugin.StaminaRuntime?.Tick();
        WorldPreviewTrace.Tick();
        _smokeTest.Tick();
    }
}
