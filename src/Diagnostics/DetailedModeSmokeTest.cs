using System;
using System.Linq;
using PeakItemInsight.Core;
using PeakItemInsight.UI;
using TMPro;
using UnityEngine;

namespace PeakItemInsight.Diagnostics;

internal static class DetailedModeSmokeTest
{
    internal static void Run(MinimalPreviewPanel view)
    {
        var language = PresentationOptions.Language!.Value;
        var style = PresentationOptions.DescriptionStyle!.Value;
        var scale = PresentationOptions.MinimalScale!.Value;
        var opacity = PresentationOptions.DetailedBackgroundOpacity!.Value;
        var texture = new Texture2D(8,16);
        var font = FontFallbackSwapper.instance.mainBaseFont;
        var material = font.material;
        try
        {
            var p = new ItemPreview { Name = "能量饮料 / Energy drink", IsFood = true,
                PoisonRisk = RiskLevel.Absent, SporeRisk = RiskLevel.Absent };
            p.Icon = texture;
            p.Resources.Add(new ResourceLine("Cooked","x2",ResourceKind.Cooked));
            p.Buffs.Add(new BuffFact(BuffKind.Speed, 8));
            p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, -1));
            p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, -.5f));
            p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, -1, 8));
            p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, .25f, delay: 8, trigger: EffectTrigger.EffectEnd));
            p.CompactNotes.Add("Speed boost");
            foreach (var lang in new[] { "Chinese", "English", "Turkish", "Spanish" })
            foreach (var zoom in new[] { .5f, 1f, 2f })
            {
                PresentationOptions.Language.Value = lang;
                PresentationOptions.MinimalScale.Value = zoom;
                PresentationOptions.DescriptionStyle.Value = DescriptionStyle.Detailed;
                view.Show(p); Canvas.ForceUpdateCanvases();
                var layouts = view.LayoutPasses;
                for (var repeat = 0; repeat < 100; repeat++) view.Show(p);
                Require(view.LayoutPasses == layouts, "unchanged detailed card does not rebuild layout");
                p.Name += "!";
                view.Show(p);
                Require(view.LayoutPasses == layouts + 1, "changed title rebuilds layout");
                Require(view.HasBackground && view.VisibleRows == 5, "title, buff, combined clearing, delayed penalty and cooking");
                var clearing = DetailedRows.Build(p, lang).Single(r => r.Text.Contains(
                    lang == "Chinese" ? "随后" : lang == "English" ? "keep clearing" : lang == "Turkish" ? "devam" : "sigue"));
                var valueText = view.GetComponentsInChildren<TextMeshProUGUI>().Single(t => t.text == clearing.Text);
                Require(valueText != null && !view.BodyText.Contains("-100"), "prose clearing replaces duplicate numeric rows without icon fallback duplication");
                var itemIcon = view.GetComponentsInChildren<UnityEngine.UI.RawImage>().Single();
                Require(itemIcon.texture == texture, "native item texture next to title");
                Require(Mathf.Abs(itemIcon.rectTransform.rect.width/itemIcon.rectTransform.rect.height-.5f) < .001f, "item icon aspect ratio preserved");
                PresentationOptions.DetailedBackgroundOpacity.Value = .35f;
                view.Show(p);
                var background = view.GetComponentInChildren<RoundedCard>();
                Require(Mathf.Abs(background.color.a-.35f) < .001f, "background opacity setting applied");
                Require(itemIcon.color.a == 1, "item icon remains opaque");
                Require(view.GetComponentsInChildren<PreviewSymbol>().Any(s => s.Kind == PreviewSymbolKind.Clock), "mesh timing icon");
                foreach (var text in view.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    text.ForceMeshUpdate();
                    Require(text.font == font && text.rectTransform.rect.height > 0, "native font and positive layout");
                    foreach (var c in text.textInfo.characterInfo.Take(text.textInfo.characterCount))
                    {
                        var requested = text.text[c.index];
                        if (!char.IsWhiteSpace(requested))
                            Require(c.textElement != null && c.textElement.unicode == requested, "exact glyph");
                    }
                }
                var rect = (RectTransform)view.transform.Find("InventoryDescription");
                Require(rect.rect.height * rect.localScale.y <= ((RectTransform)view.transform).rect.height - 32, "all rows fit screen");

                PresentationOptions.DescriptionStyle.Value = DescriptionStyle.Minimal;
                view.Show(p);
                Require(!view.HasBackground && view.IsVisible, "minimal switch removes background");
                Require(!view.GetComponentsInChildren<PreviewSymbol>().Any(), "minimal switch removes timing and buff symbols");
                Require(!view.GetComponentsInChildren<UnityEngine.UI.RawImage>().Any(), "minimal switch removes title icon");
                Require(!view.BodyText.Contains("Cooked"), "minimal switch omits cooking row");
                view.Hide(); Require(!view.IsVisible, "hide clears card");
            }
            Require(font.material == material, "shared font material unchanged");
            PresentationOptions.DescriptionStyle.Value = DescriptionStyle.Detailed;
            view.Show(new ItemPreview()); Require(!view.IsVisible, "no empty placeholders");
            SessionTrace.Write("SMOKE_DETAILED_PASS", "readable effect prose; combined immediate+continuous clearing with distinct after-effect penalty; original background; exact glyphs in 4 languages; cached layout; timing icons; .5/1/2 scales; hide and switch cleanup");
        }
        finally
        {
            PresentationOptions.Language.Value = language;
            PresentationOptions.DescriptionStyle.Value = style;
            PresentationOptions.MinimalScale.Value = scale;
            PresentationOptions.DetailedBackgroundOpacity!.Value = opacity;
            UnityEngine.Object.Destroy(texture);
            view.Hide();
        }
    }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException("Detailed: " + message); }
}
