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
                active.Hide(); view.Show(p); Canvas.ForceUpdateCanvases();
                Require(view.IsVisible && !active.IsVisible, "only active preview visible");
                Require(view.VisibleRows == (source == PreviewSource.Hover ? 3 : 2), "hover identity and effects");
                var texts = view.GetComponentsInChildren<TextMeshProUGUI>();
                Require(texts.Any(t => t.text.Contains("-5")) && texts.Any(t => t.text.Contains("+10")), "mixed values visible");
                foreach (var text in texts)
                {
                    text.ForceMeshUpdate();
                    Require(text.rectTransform.rect.height > 0 && text.textInfo.characterCount > 0, "TMP generates row geometry");
                }
                view.Hide(); active.Show(p);
                Require(!view.IsVisible && active.IsVisible, "hide and rebind clears old view");
            }
            view.Show(new ItemPreview()); Require(!view.IsVisible, "empty view hidden");
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
