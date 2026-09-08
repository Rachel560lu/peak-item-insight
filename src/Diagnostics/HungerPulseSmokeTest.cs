using System;
using PeakItemInsight.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.Diagnostics;

// Runs inside Unity only with the existing opt-in title-screen smoke test.
// Exercises the actual production renderer, not a replacement/mock renderer.
internal static class HungerPulseSmokeTest
{
    public static HungerRecoveryOverlay Run(Transform canvas)
    {
        var hungry = Make("SyntheticHunger", canvas, new Vector2(580, 130), 100, Color.yellow);
        var healthy = Make("SyntheticHealth", canvas, new Vector2(60, 130), 500, Color.green);
        var nativeColour = hungry.color;
        var nativeSize = hungry.rectTransform.sizeDelta;
        var pulse = new HungerRecoveryOverlay();
        try
        {
            void Render(float before, float after, float elapsed)
            { pulse.Render(hungry, healthy, before, after, elapsed); Canvas.ForceUpdateCanvases(); }
            Render(.05f, 0, 0);
            Require(pulse.Visible && Near(pulse.Width, 100) && Near(pulse.Opacity, 0), "full width/yellow phase");
            Render(.05f, 0, .7f);
            Require(Near(pulse.Opacity, 1), "fully green phase");
            Render(.05f, 0, 1.4f);
            Require(Near(pulse.Opacity, 0), "return to yellow");
            Render(.05f, .03f, .7f);
            Require(Near(pulse.Width, 40), "partial 2/5 coverage");
            Require(pulse.ClipRect!.anchorMin.x == 0, "recovery starts toward healthy side");
            healthy.rectTransform.anchoredPosition = new Vector2(800, 130);
            Render(.05f, .03f, .7f);
            Require(Near(pulse.ClipRect.anchorMin.x, .6f), "mirrored healthy-side anchor");
            healthy.rectTransform.anchoredPosition = new Vector2(60, 130);
            hungry.rectTransform.anchoredPosition += new Vector2(7, 11);
            hungry.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1);
            Render(.05f, .03f, .7f);
            var a = new Vector3[4]; var b = new Vector3[4];
            hungry.rectTransform.GetWorldCorners(a); pulse.OverlayRect!.GetWorldCorners(b);
            for (var i = 0; i < 4; i++) Require(Vector3.Distance(a[i], b[i]) < .01f, "native bounds/scale tracking");
            Render(.05f, .05f, .7f); Require(!pulse.Visible, "zero effect hides");
            Render(0, 0, .7f); Require(!pulse.Visible, "zero hunger hides");
            Render(.05f, .06f, .7f); Require(!pulse.Visible, "worsening never shows green");
            Render(.05f, 0, .7f); pulse.Hide(); Require(!pulse.Visible, "leave hides immediately");
            healthy.gameObject.SetActive(false);
            Render(.05f, 0, .7f); Require(pulse.Visible, "empty/inactive healthy fill can still supply its colour");
            healthy.gameObject.SetActive(true);
            Render(.05f, 0, .7f);
            hungry.gameObject.SetActive(false);
            Require(!pulse.Visible, "inactive native HUD hides child");
            hungry.gameObject.SetActive(true);
            Require(hungry.color == nativeColour && hungry.rectTransform.sizeDelta == nativeSize, "native colour/size unchanged");
            pulse.Dispose(); Require(!pulse.Visible, "dispose hides immediately");
            Render(.05f, .03f, .7f); // leaves a partial sample for the smoke screenshot
            Require(Near(pulse.Width, 40), "rebind after disposal");
            SessionTrace.Write("SMOKE_HUNGER_PASS", "full+partial+zero+phases+side+scaled-bounds+hide+inactive+dispose+rebind+native-unchanged; synthetic=true");
            return pulse;
        }
        catch { pulse.Dispose(); throw; }
    }

    private static bool Near(float a, float b) => Math.Abs(a - b) < .01f;
    private static void Require(bool ok, string name)
    { if (!ok) throw new InvalidOperationException("Hunger pulse smoke failed: " + name); }
    private static Image Make(string name, Transform parent, Vector2 position, float width, Color colour)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var image = go.GetComponent<Image>();
        var rect = image.rectTransform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, 24);
        image.color = colour;
        image.raycastTarget = false;
        return image;
    }
}
