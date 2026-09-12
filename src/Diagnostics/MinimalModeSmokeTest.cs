using System;
using System.Linq;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
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
            StatusProjector.Populate(p, type => type == CharacterAfflictions.STATUSTYPE.Hunger ? .05f : 0, _ => 1, false);
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
            CheckTimedItems(view);
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

    private static void CheckTimedItems(MinimalPreviewPanel view)
    {
        var checkedCount = 0;
        var names = new System.Collections.Generic.HashSet<string>();
        foreach (var lang in new[] { "Chinese", "English" })
        {
            PresentationOptions.Language!.Value = lang;
            for (var effect = 0; effect < 10; effect++)
            {
                var mapped = new ItemPreview();
                MushroomEffectReader.ApplyKnown(effect, mapped);
                StatusProjector.Populate(mapped, _ => .3f, _ => 1, false);
                var body = string.Join("|", MinimalRows.Build(mapped, lang == "Chinese").Select(r => r.Text));
                Require(body.Length > 0 && !body.Contains("秒") && !body.Contains("after "), "mapped mushroom retains concise effect");
                Require(mapped.InfiniteStamina == false, "presentation label must not enable HUD infinite stamina");
            }
            foreach (var item in Resources.FindObjectsOfTypeAll<Item>().Where(i => i != null && i.UIData != null))
            {
                var preview = new ItemPreview { Name = item.UIData.itemName, PrefabOnly = true };
                ItemEffectReader.Read(item, preview);
                if (!preview.SummarizeEffects && !preview.Effects.Any(e => e.Timed)) continue;
                names.Add(preview.Name);
                var facts = preview.Effects.ToArray();
                foreach (var before in new[] { 0f, .1f, .25f, .5f, 1f })
                foreach (var locked in new[] { false, true })
                foreach (var source in new[] { PreviewSource.Hover, PreviewSource.Held })
                {
                    preview.Source = source;
                    StatusProjector.Populate(preview, _ => before, _ => 1, locked);
                    var snapshot = preview.Statuses.ToArray();
                    var rows = MinimalRows.Build(preview, lang == "Chinese");
                    var numeric = rows.Where(r => r.Status.HasValue && r.Direction != 0).ToArray();
                    var changed = snapshot.Where(s => Math.Round((s.After - s.Before) * 100, 1) != 0).ToArray();
                    Require(numeric.Length == changed.Length, "one row per changed status: " + preview.Name);
                    foreach (var status in changed)
                    {
                        var row = numeric.Single(r => r.Status == status.Type);
                        Require(Math.Abs(float.Parse(row.Text, System.Globalization.CultureInfo.InvariantCulture) -
                            Math.Round((status.After - status.Before) * 100, 1)) < .001, "row equals HUD endpoint");
                    }
                    Require(!rows.Any(r => r.Text.Contains("秒") || r.Text.Contains("after ") || r.Text.Contains(" / ")),
                        "no rate, duration or delay in timed effect summary: " + preview.Name);
                    Require(preview.Effects.SequenceEqual(facts) && preview.Statuses.SequenceEqual(snapshot), "formatter is read-only");
                    checkedCount++;
                }
                if (preview.Name == "Energy Drink")
                {
                    StatusProjector.Populate(preview, _ => .5f, _ => 1, false);
                    view.Show(preview); Canvas.ForceUpdateCanvases();
                    Require(view.BodyText.Contains("25") && view.BodyText.Contains(lang == "Chinese" ? "加速" : "Speed boost"), "energy drink native view has final drowsy delta and short buff");
                    SessionTrace.Write("SMOKE_TIMED_ENERGY", $"language={lang} rows={view.BodyText.Replace('\n', '|')}");
                }
            }
        }
        Require(names.Contains("Energy Drink") && names.Contains("Heat Pack") && names.Contains("Big Lollipop") && names.Contains("Green Crispberry"), "real timed fixtures loaded");
        SessionTrace.Write("SMOKE_TIMED_SUMMARY_PASS", $"cases={checkedCount}; items={string.Join(",", names)}; both languages/sources, caps, locked, HUD endpoints unchanged");
    }

    private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException("Minimal: " + message); }
}
