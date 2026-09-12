using System;
using System.Linq;
using PeakItemInsight.Core;
using PeakItemInsight.UI;
using TMPro;
using UnityEngine;

namespace PeakItemInsight.Diagnostics;

internal static class MinimalModeSmokeTest
{
    internal static void Run(MinimalPreviewPanel active)
    {
        var view = MinimalPreviewPanel.Create();
        var language = PresentationOptions.Language!.Value;
        var scale = PresentationOptions.MinimalScale!.Value;
        try
        {
            var p = new ItemPreview { Name = "有毒食物 / Poisonous food", IsFood = true, PoisonRisk = RiskLevel.Present, SporeRisk = RiskLevel.Absent };
            p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.05f));
            p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .025f, 4, 2));
            foreach (var lang in new[] { "Chinese", "English" })
            foreach (var source in new[] { PreviewSource.Held, PreviewSource.Hover })
            foreach (var zoom in new[] { .5f, 1f, 2f })
            {
                PresentationOptions.Language.Value = lang; PresentationOptions.MinimalScale.Value = zoom; p.Source = source;
                p.Resources.Clear();
                p.Resources.Add(new ResourceLine(lang == "Chinese" ? "剩余次数" : "uses", "4 / 4", ResourceKind.Uses));
                p.Resources.Add(new ResourceLine(lang == "Chinese" ? "剩余" : "remaining", "100%", ResourceKind.Remaining));
                active.Hide(); view.Show(p); Canvas.ForceUpdateCanvases();
                Require(view.IsVisible && !active.IsVisible, "only active preview visible");
                Require(view.VisibleRows == (source == PreviewSource.Hover ? 5 : 4), "hover identity, effects and resource glyphs");
                var texts = view.GetComponentsInChildren<TextMeshProUGUI>();
                Require(texts.Any(t => t.text.StartsWith("5")) && texts.Any(t => t.text.StartsWith("10")), "mixed values visible");
                var arrows = view.GetComponentsInChildren<PreviewDeltaArrow>();
                Require(arrows.Length == 2 && arrows.Any(a => Mathf.Abs(a.transform.localEulerAngles.z - 180) < .1f)
                    && arrows.Any(a => Mathf.Abs(a.transform.localEulerAngles.z) < .1f), "independent up and down arrows");
                foreach (var text in texts)
                {
                    text.ForceMeshUpdate();
                    Require(text.rectTransform.rect.height > 0 && text.textInfo.characterCount > 0, "TMP generates row geometry");
                    Require(text.font == FontFallbackSwapper.instance.mainBaseFont, "native main font, not default sans");
                    foreach (var character in text.textInfo.characterInfo.Take(text.textInfo.characterCount))
                    {
                        var requested = text.text[character.index];
                        if (!char.IsWhiteSpace(requested))
                            Require(character.textElement != null && character.textElement.unicode == requested,
                                $"no substituted glyph: U+{(int)requested:X4} in {text.text}");
                    }
                }
                view.Hide(); active.Show(p);
                Require(!view.IsVisible && active.IsVisible, "hide and rebind clears old view");
            }
            view.Show(new ItemPreview()); Require(!view.IsVisible, "empty view hidden");
            foreach (var dimensions in new[] { new Vector2(1920, 1080), new Vector2(1920, 1200), new Vector2(2560, 1080) })
            {
                var area = new Rect(-dimensions / 2, dimensions);
                var anchor = new Vector2(area.xMax - 240, area.yMin + 210);
                var position = MinimalPreviewPanel.ClampToScreen(area, anchor, new Vector2(220, 240));
                Require(position == anchor, "effects stay centered on inventory at all aspect ratios");
                var clamped = MinimalPreviewPanel.ClampToScreen(area, new Vector2(area.xMax, area.yMax), new Vector2(220, 240));
                Require(clamped.x + 110 <= area.xMax - 16 && clamped.y + 240 <= area.yMax - 16, "screen edge containment");
            }
            SessionTrace.Write("SMOKE_MINIMAL_NATIVE_PASS", $"font={FontFallbackSwapper.instance.mainBaseFont.name}; exact Chinese/English glyphs; delta arrows; 16:9/16:10/21:9 anchor clamp");
            SessionTrace.Write("SMOKE_MINIMAL_PASS", "Chinese+English; hover+held; scales .5/1/2; mixed timed poison; hide/rebind; TMP geometry; empty view. Native in-game anchoring needs visual acceptance.");
        }
        finally
        {
            PresentationOptions.Language.Value = language; PresentationOptions.MinimalScale.Value = scale;
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException("Minimal: " + message); }
}
