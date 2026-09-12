using System;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
using PeakItemInsight.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.Diagnostics;

// Actual production selection + paired renderer, supplied synthetic UI/data only.
internal static class RecoveryRoutingSmokeTest
{
    public static StatusRecoveryOverlays Run(MinimalPreviewPanel panel)
    {
        var healthy = Make("PairHealth", panel.transform, 60, 500, Color.green);
        var hunger = Make("PairHunger", panel.transform, 580, 100, Color.yellow);
        var injury = Make("PairInjury", panel.transform, 700, 100, new Color(1, .35f, 0));
        var pair = new StatusRecoveryOverlays();
        var food = new ItemPreview { Name = "Synthetic food", ItemId = 1 };
        food.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .05f, .03f));
        var bandage = new ItemPreview { Name = "Synthetic bandage", ItemId = 2 };
        bandage.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Injury, .2f, .15f));
        try
        {
            void Route(int? hovered, int? held, bool busy = false)
            {
                var key = PreviewTargetSelection.Select(hovered, held, busy);
                if (!key.IsValid) { pair.Hide(); panel.Hide(); return; }
                var preview = key.InstanceId == 1 ? food : key.InstanceId == 2 ? bandage : new ItemPreview { Name = "Synthetic tool" };
                preview.Source = key.Source; preview.TargetInstanceId = key.InstanceId;
                pair.Render(preview, healthy, hunger, injury, .7f);
                panel.Show(preview);
                Canvas.ForceUpdateCanvases();
                Require(preview.Source == key.Source && panel.LastTargetName.Contains(preview.Name) && !panel.LastTargetName.Contains(Labels.Source(key.Source)), "panel target agrees without source clutter");
            }
            Route(1, 2); Require(pair.Hunger.Visible && !pair.Injury.Visible, "hover food outranks held bandage");
            Route(null, 2); Require(!pair.Hunger.Visible && pair.Injury.Visible && Near(pair.Injury.Width, 25), "held injury quarter");
            Require(pair.Injury.OverlayRect!.parent == injury.transform, "injury bound to own badge");
            Route(3, 2); Require(!pair.Hunger.Visible && !pair.Injury.Visible, "hover tool clears held healing");
            Route(null, 2, true); Require(!pair.Injury.Visible && !panel.IsVisible, "busy held hides");
            Route(1, 2, true); Require(pair.Hunger.Visible, "unrelated hover during use");
            Route(null, 2); Require(pair.Injury.Visible, "cancel/finish restores held preview");
            Route(null, null); Require(!pair.Hunger.Visible && !pair.Injury.Visible && !panel.IsVisible, "empty hides all");
            bandage.Statuses.Clear(); bandage.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Injury, .05f, 0));
            Route(null, 2); Require(Near(pair.Injury.Width, 100), "full injury heal");
            bandage.Statuses.Clear(); bandage.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Injury, 0, 0));
            Route(null, 2); Require(!pair.Injury.Visible, "zero injury");
            bandage.Statuses.Clear(); bandage.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Injury, .05f, .1f));
            Route(null, 2); Require(!pair.Injury.Visible, "harm never green");
            bandage.Statuses.Clear(); bandage.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Injury, .2f, .15f));
            bandage.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .05f, .03f));
            Route(null, 2); Require(Near(pair.Hunger.Width, 40) && Near(pair.Injury.Width, 25), "two effects independent");
            pair.Render(bandage, healthy, hunger, injury, 0);
            Require(Near(pair.Injury.Opacity, 0) && Near(pair.Hunger.Opacity, 0), "native colour phase");
            pair.Render(bandage, healthy, hunger, injury, .7f);
            Require(Near(pair.Injury.Opacity, 1) && Near(pair.Hunger.Opacity, 1), "green phase synchronized");
            pair.Render(bandage, healthy, null, injury, .7f);
            Require(!pair.Hunger.Visible && pair.Injury.Visible, "missing hunger does not block injury");
            pair.Hide(); Require(!pair.Hunger.Visible && !pair.Injury.Visible, "pair hide");
            pair.Dispose(); Route(null, 2); Require(pair.Hunger.Visible && pair.Injury.Visible, "pair rebind");
            Require(hunger.color == Color.yellow && Near(injury.color.g, .35f) && Near(injury.rectTransform.rect.width, 100), "native untouched");
            SessionTrace.Write("SMOKE_RECOVERY_ROUTING_PASS", "hover-held-empty+busy+source-label+full-partial-zero-harm+independent-badges+phases+hide-rebind; synthetic=true");
            return pair;
        }
        catch { pair.Dispose(); throw; }
    }
    private static bool Near(float a, float b) => Math.Abs(a - b) < .01f;
    private static void Require(bool value, string name) { if (!value) throw new InvalidOperationException("Recovery routing: " + name); }
    private static Image Make(string name, Transform parent, float x, float width, Color colour)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        var rect = image.rectTransform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, 210); rect.sizeDelta = new Vector2(width, 24);
        image.color = colour; image.raycastTarget = false; return image;
    }
}
