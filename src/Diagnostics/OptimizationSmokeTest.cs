using System;
using System.Collections.Generic;
using System.Linq;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
using PeakItemInsight.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PeakItemInsight.Diagnostics;

internal static class OptimizationSmokeTest
{
    internal static StatusIncreaseOverlay Run(MinimalPreviewPanel panel)
    {
        // Disabled UI-only game components provide native-shaped templates; never
        // create a Character, Item or run any gameplay callbacks.
        var host = new GameObject("SyntheticNativeHud", typeof(RectTransform));
        host.SetActive(false); host.transform.SetParent(panel.transform, false);
        var bar = host.AddComponent<StaminaBar>(); bar.enabled = false;
        var full = host.GetComponent<RectTransform>();
        full.anchorMin = full.anchorMax = full.pivot = Vector2.zero;
        full.anchoredPosition = new Vector2(80, 320); full.sizeDelta = new Vector2(700, 26);
        bar.fullBar = full; bar.minAfflictionWidth = 12;
        var healthy = Image("Healthy", host.transform, 0, 560, Color.green);
        bar.staminaBar = healthy.rectTransform;
        var hunger = Badge(host.transform, CharacterAfflictions.STATUSTYPE.Hunger, 560, 100, Color.yellow);
        var poison = Badge(host.transform, CharacterAfflictions.STATUSTYPE.Poison, 660, 0, new Color(.6f,.2f,.8f));
        var spores = Badge(host.transform, CharacterAfflictions.STATUSTYPE.Spores, 660, 40, new Color(.9f,.3f,.6f));
        bar.afflictions = new[] { hunger, poison, spores };
        host.SetActive(true); poison.gameObject.SetActive(false);
        var priorGroup = spores.gameObject.AddComponent<CanvasGroup>(); priorGroup.alpha = .9f;
        var mixed = new ItemPreview { Name = "Mixed food / 混合效果", Category = "CONSUMABLE", IsFood = true, CompleteEffects = true };
        mixed.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .2f, .1f));
        mixed.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 0, .1f));
        mixed.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Spores, .05f, .1f));
        mixed.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.1f));
        mixed.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .1f));
        mixed.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Spores, .05f));
        ItemEffectReader.AssessRisks(mixed);
        var overlay = new StatusIncreaseOverlay();
        try
        {
            Require(overlay.Render(bar, mixed, healthy, .7f), "render mixed");
            Canvas.ForceUpdateCanvases();
            Require(overlay.Visible && overlay.AddedCount == 2, "new poison plus existing spore gain");
            Require(Math.Abs(overlay.ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE.Hunger) - 70) < .01f,
                "mixed peak shrinks hunger to its after value, not original 100px");
            Require(Math.Abs(overlay.AddedOpacity - 1) < .001f, "harm peak phase");
            overlay.Render(bar, mixed, healthy, 0);
            Require(Math.Abs(overlay.AddedOpacity) < .001f, "harm original phase");
            Require(Math.Abs(priorGroup.alpha - .9f) < .001f, "original phase keeps native row visible and positioned");
            overlay.Render(bar, mixed, healthy, 1.4f);
            Require(Math.Abs(overlay.AddedOpacity) < .001f, "harm returns to original phase");
            Require(!poison.gameObject.activeSelf && hunger.rtf.rect.width == 100, "native activity and widths unchanged");
            overlay.Hide();
            Require(Math.Abs(priorGroup.alpha - .9f) < .001f, "existing CanvasGroup alpha restored");
            Require(!overlay.Visible && spores.GetComponents<CanvasGroup>().All(g => g == priorGroup || g.alpha == 1), "native visibility restored");
            var empty = new ItemPreview();
            Require(!overlay.Render(bar, empty, healthy, 0), "zero changes hides additions");
            overlay.Dispose();
            Require(overlay.Render(bar, mixed, healthy, .7f), "dispose/rebind");
            // Exercise the SAME live renderer for every enum value, not a whitelist.
            foreach (var type in StatusTypes.Previewable)
            {
                overlay.Dispose(); poison.afflictionType = type;
                var one = new ItemPreview(); one.Statuses.Add(new StatusDelta(type, 0, .1f));
                Require(overlay.Render(bar, one, healthy, .7f) && overlay.AddedCount > 0, "status gain route: " + type);
                overlay.Hide(); Require(!overlay.Visible, "status hide: " + type);
            }
            overlay.Dispose(); poison.afflictionType = CharacterAfflictions.STATUSTYPE.Poison;
            // Reproduce the reported fruit: five hunger removed while ten poison
            // is added. Assert actual rendered widths at BOTH pulse endpoints.
            foreach (var source in new[] { PreviewSource.Hover, PreviewSource.Held })
            {
                var fruit = new ItemPreview { Source = source };
                fruit.StatusBefore[CharacterAfflictions.STATUSTYPE.Hunger] = .05f;
                fruit.StatusBefore[CharacterAfflictions.STATUSTYPE.Poison] = 0;
                fruit.StatusBefore[CharacterAfflictions.STATUSTYPE.Spores] = .05f;
                fruit.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .05f, 0));
                fruit.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 0, .1f));
                overlay.Render(bar, fruit, healthy, .7f); Canvas.ForceUpdateCanvases();
                Require(overlay.ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE.Hunger) == 0,
                    "mixed fruit peak removes yellow hunger: " + source);
                Require(Math.Abs(overlay.ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE.Poison) - 70) < .01f &&
                    overlay.VisibleAddedWidth > 69 && overlay.HealthyWidth > 580,
                    "mixed fruit peak has poison geometry AND green recovered area: " + source);
                fruit.Statuses[0] = new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .15f, .1f);
                fruit.StatusBefore[CharacterAfflictions.STATUSTYPE.Hunger] = .15f;
                overlay.Render(bar, fruit, healthy, .7f);
                Require(Math.Abs(overlay.ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE.Hunger) - 70) < .01f,
                    "mixed fruit partial recovery retains only ten hunger: " + source);
                overlay.Render(bar, fruit, healthy, 0);
                Require(overlay.AddedOpacity == 0 && hunger.GetComponent<CanvasGroup>().alpha == 1,
                    "original pulse phase restores native hunger: " + source);
                overlay.Hide(); Require(hunger.rtf.rect.width == 100, "native hunger width untouched");
            }
            SessionTrace.Write("SMOKE_MIXED_HUD_PASS", "hover+held; full+partial hunger recovery; simultaneous poison; real rendered widths; green fill; original phase+cleanup");
            overlay.Dispose();
            // Previously untested native state: zero poison => inactive slot,
            // collapsed layout and empty Filled image. Assert actual geometry,
            // not only a counter incremented from the requested status delta.
            var poisonImage = poison.GetComponent<Image>();
            poisonImage.type = UnityEngine.UI.Image.Type.Filled; poisonImage.fillAmount = 0;
            poison.rtf.sizeDelta = Vector2.zero;
            var toxic = new ItemPreview(); toxic.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 0, .1f));
            Require(overlay.Render(bar, toxic, healthy, .7f) && overlay.VisibleAddedWidth > 60,
                "hidden empty zero-height poison slot produces visible geometry");
            Require(!poison.gameObject.activeSelf && poison.rtf.rect.height == 0 && poisonImage.fillAmount == 0,
                "fallback must not mutate native poison slot");
            overlay.Hide(); Require(overlay.VisibleAddedWidth == 0, "visible geometry clears");
            overlay.Dispose();
            poison.rtf.sizeDelta = new Vector2(0, 26); poisonImage.type = UnityEngine.UI.Image.Type.Simple;
            poisonImage.fillAmount = 1;
            overlay.Render(bar, mixed, healthy, .7f);
            var wrapper = new GameObject("ExtraWrapper", typeof(RectTransform)).GetComponent<RectTransform>();
            wrapper.SetParent(host.transform, false); wrapper.anchorMin = wrapper.anchorMax = wrapper.pivot = Vector2.zero;
            wrapper.anchoredPosition = new Vector2(0, -50); wrapper.sizeDelta = Vector2.zero;
            var extraTemplate = Image("ExtraTemplate", wrapper, 0, 0, Color.green);
            var outline = Image("NativeOutline", wrapper, -6, 20, Color.white);
            var icon = Image("NativeLightning", wrapper, -35, 20, Color.green);
            var shadow = new GameObject("OriginalRoundedShadow", typeof(RectTransform));
            shadow.SetActive(false); shadow.transform.SetParent(outline.transform, false);
            var shadowRect = shadow.GetComponent<RectTransform>();
            shadowRect.anchorMin = Vector2.zero; shadowRect.anchorMax = Vector2.one;
            shadowRect.sizeDelta = new Vector2(-4, 0);
            shadow.AddComponent<UnityEngine.UI.ProceduralImage.ProceduralImage>();
            var corners = shadow.AddComponent<UniformModifier>();
            JsonUtility.FromJsonOverwrite("{\"radius\":7}", corners);
            shadow.SetActive(true);
            bar.extraBar = wrapper; bar.extraBarOutline = outline.rectTransform; bar.extraStaminaIcon = icon;
            bar.extraBarStamina = extraTemplate.rectTransform; extraTemplate.gameObject.SetActive(false);
            var extraOverlay = new ExtraStaminaOverlay();
            try
            {
                full.sizeDelta = new Vector2(700, 180);
                full.pivot = new Vector2(.5f, .5f);
                full.localScale = new Vector3(1.2f, .8f, 1);
                extraTemplate.rectTransform.sizeDelta = new Vector2(0, 26);
                foreach (var source in new[] { PreviewSource.Hover, PreviewSource.Held })
                {
                    var bonus = new ItemPreview { Source = source, HasExtraStamina = true, ExtraBefore = 0, ExtraAfter = .2f };
                    extraOverlay.Render(bar, bonus, healthy, .7f);
                    Require(extraOverlay.ExtraVisible && !extraTemplate.gameObject.activeSelf, "extra gain with inactive native template: " + source);
                    Require(Math.Abs(extraOverlay.ExtraBounds.height - 26) < .01f && Math.Abs(extraOverlay.ExtraBounds.width - 140) < .01f,
                        "bonus uses visible fill height, not tall container or stale hidden template: " + source);
                    var mainTop = full.InverseTransformPoint(healthy.rectTransform.TransformPoint(new Vector3(0, healthy.rectTransform.rect.yMax)));
                    var mainBottom = full.InverseTransformPoint(healthy.rectTransform.TransformPoint(new Vector3(0, healthy.rectTransform.rect.yMin)));
                    Require(extraOverlay.RowBounds.yMin > mainTop.y && extraOverlay.TrackVisible && extraOverlay.IconVisible,
                        "zero real bonus places complete frame and lightning ABOVE main bar: " + source);
                    Require(wrapper.sizeDelta == Vector2.zero && outline.enabled && icon.enabled,
                        "upper preview leaves collapsed native wrapper and graphics unchanged");
                    extraTemplate.rectTransform.sizeDelta = new Vector2(70, 26);
                    extraTemplate.rectTransform.anchoredPosition = new Vector2(12, 0);
                    extraTemplate.gameObject.SetActive(true);
                    bonus.ExtraBefore = .1f; bonus.ExtraAfter = .4f;
                    extraOverlay.Render(bar, bonus, healthy, .7f);
                    var nativeLeft = full.InverseTransformPoint(extraTemplate.rectTransform.TransformPoint(new Vector3(0, 13)));
                    Require(Math.Abs(extraOverlay.ExtraBounds.xMin - nativeLeft.x - 70) < .01f &&
                        Math.Abs(extraOverlay.ExtraBounds.center.y - nativeLeft.y) < .01f, "existing bonus row alignment: " + source);
                    Require(extraTemplate.rectTransform.sizeDelta == new Vector2(70, 26), "native bonus geometry unchanged");
                    Require(extraOverlay.RowBounds.yMax < mainBottom.y && !outline.enabled && !icon.enabled,
                        "existing real bonus switches below with one visible native-style row");
                    wrapper.anchoredPosition = Vector2.zero;
                    extraOverlay.Render(bar, bonus, healthy, .7f);
                    Require(extraOverlay.RowBounds.yMax < mainBottom.y && wrapper.anchoredPosition == Vector2.zero,
                        "overlapping native anchor is cleared by preview offset without moving native row");
                    wrapper.anchoredPosition = new Vector2(0, -50);
                    bonus.ExtraBefore = 0;
                    extraOverlay.Render(bar, bonus, healthy, .7f);
                    Require(extraOverlay.RowBounds.yMin > mainTop.y && outline.enabled && icon.enabled,
                        "bonus depletion returns above and restores native graphics while wrapper remains visible");
                    extraOverlay.Render(bar, bonus, healthy, .7f);
                    Require(extraOverlay.RowBounds.yMin > mainTop.y && extraOverlay.RowBounds.yMin < mainTop.y + 15,
                        "repeated upper render does not accumulate vertical offset");
                    extraTemplate.gameObject.SetActive(false); extraTemplate.rectTransform.sizeDelta = new Vector2(0, 26);
                    bonus.ExtraBefore = bonus.ExtraAfter;
                    extraOverlay.Render(bar, bonus, healthy, .7f); Require(!extraOverlay.ExtraVisible, "extra cap: " + source);
                    Require(outline.enabled && icon.enabled && extraTemplate.enabled, "native graphic flags restored on cap");
                    bonus.InfiniteStamina = true; extraOverlay.Render(bar, bonus, healthy, .7f);
                    Require(extraOverlay.InfiniteVisible, "infinite stamina: " + source);
                    extraOverlay.Hide(); Require(!extraOverlay.InfiniteVisible && !extraOverlay.ExtraVisible && !extraOverlay.TrackVisible, "extra hide");
                }
            }
            finally { extraOverlay.Dispose(); full.sizeDelta = new Vector2(700, 26); full.pivot = Vector2.zero; full.localScale = Vector3.one; }
            // Actual serialized native row: settled wrapper 45px tall, Back
            // stretched vertically with sizeDelta.y=-12, fill x=46, outline x=40.
            wrapper.sizeDelta = Vector2.zero; wrapper.gameObject.SetActive(false);
            extraTemplate.rectTransform.anchorMin = Vector2.zero;
            extraTemplate.rectTransform.anchorMax = new Vector2(0, 1);
            extraTemplate.rectTransform.sizeDelta = new Vector2(0, -12);
            extraTemplate.rectTransform.anchoredPosition = new Vector2(46, 0);
            outline.rectTransform.anchorMin = Vector2.zero;
            outline.rectTransform.anchorMax = new Vector2(0, 1);
            outline.rectTransform.sizeDelta = new Vector2(20, 0);
            outline.rectTransform.anchoredPosition = new Vector2(40, 0);
            var nativeLayout = new ExtraStaminaOverlay();
            try
            {
                nativeLayout.Render(bar, new ItemPreview { HasExtraStamina = true, ExtraAfter = .2f }, healthy, .7f);
                Canvas.ForceUpdateCanvases();
                Require(nativeLayout.ExtraVisible && nativeLayout.IconVisible && nativeLayout.TrackVisible &&
                    Math.Abs(nativeLayout.ExtraBounds.height - 33) < .01f && Math.Abs(nativeLayout.ExtraBounds.width - 140) < .01f,
                    "serialized native anchors produce 33px visible fill despite negative sizeDelta height");
                var copiedCorners = nativeLayout.OutlineImage!.GetComponentInChildren<UniformModifier>(true);
                Require(copiedCorners != null && JsonUtility.ToJson(copiedCorners) == JsonUtility.ToJson(corners),
                    "native ProceduralImage UniformModifier serialized radius preserved");
                nativeLayout.Hide(); Require(outline.enabled && icon.enabled && !wrapper.gameObject.activeSelf,
                    "original collapsed wrapper and graphic flags restored");
            }
            finally { nativeLayout.Dispose(); }
            SessionTrace.Write("SMOKE_NATIVE_BONUS_LAYOUT_PASS", "game level3 verified: native wrapper45, Back vertical stretch -12=>33px, x46, outline x40; UniformModifier radius preserved; no gameplay controllers cloned");
            SessionTrace.Write("SMOKE_BONUS_GEOMETRY_PASS", "zero bonus above; existing bonus below; overlap clearance; depletion restores graphics; no position drift; native outline+icon; gainWidth=140; scaled parent+nonzero pivot+hover/held+cap+hide; synthetic HUD");
            CheckNativeBonus(healthy, bar, host.transform);
            var bounded = new ItemPreview { CompleteEffects = true, IsFood = true };
            bounded.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .2f));
            bounded.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 2, 2));
            ItemEffectReader.AssessRisks(bounded);
            Require(bounded.PoisonRisk == RiskLevel.Present, "capped toxin still risky");
            Require(bounded.SporeRisk == RiskLevel.Absent, "spores independent");
            bounded.PrefabOnly = true; ItemEffectReader.AssessRisks(bounded);
            Require(bounded.SporeRisk == RiskLevel.Unknown, "prefab cannot establish safety");
            var text = string.Join("\n", MinimalRows.Build(mixed, Labels.Chinese).Select(r => r.Text));
            Require(text.Contains("+10") && text.Contains("-10"), "signed facts retained");
            Require(!text.Contains("→"), "no debug-style projections by default");
            var mushroom = new ItemPreview { CompleteEffects = true };
            MushroomEffectReader.ApplyKnown(8, mushroom); ItemEffectReader.AssessRisks(mushroom);
            Require(mushroom.SporeRisk == RiskLevel.Present && mushroom.Effects[0].Amount == .25f, "mapped spore mushroom");
            var cure = new ItemPreview { CompleteEffects = true };
            MushroomEffectReader.ApplyKnown(4, cure); ItemEffectReader.AssessRisks(cure);
            Require(cure.Effects.Count == 4 && cure.SporeRisk == RiskLevel.Absent, "mapped cure mushroom");
            var unknown = new ItemPreview { CompleteEffects = true };
            MushroomEffectReader.ApplyKnown(100, unknown); ItemEffectReader.AssessRisks(unknown);
            Require(unknown.SporeRisk == RiskLevel.Unknown, "unknown mapping fails closed");
            SessionTrace.Write("SMOKE_OPTIMIZATION_PASS", "all enum gain routes+harm pulse phases+extra/infinite hover/held+inactive templates+mixed+hide+rebind+ownership+risk+signed-card; synthetic=true");

            // Read actual loaded assets as data only: stronger than hand-authored facts,
            // but does not prove the in-level hover mapping or consumption result.
            var assets = Resources.FindObjectsOfTypeAll<Item>().Where(i => i != null && i.UIData != null).ToArray();
            CheckToolCards(assets);
            var checkedCount = 0;
            var poisonChecked = 0;
            var audited = new HashSet<ushort>();
            var descriptions = 0;
            foreach (var item in assets)
            {
                var preview = new ItemPreview { Name = item.UIData.itemName, Icon = item.UIData.icon, PrefabOnly = true };
                ItemEffectReader.Read(item, preview);
                foreach (var expected in AuditedPoisonCases.All.Where(c => c.Id == item.itemID))
                {
                    Require(item.UIData.itemName == expected.Name, "audited item identity: " + expected.Id);
                    var toxins = preview.Effects.Where(e => e.Type == CharacterAfflictions.STATUSTYPE.Poison && e.Amount > 0).ToArray();
                    Require(toxins.Length == 1, "one audited poison effect: " + expected.Name);
                    var toxin = toxins[0];
                    Require(Math.Abs(toxin.Amount - expected.Rate) < .00001f &&
                        Math.Abs(toxin.Duration - expected.Duration) < .00001f &&
                        Math.Abs(toxin.Delay - expected.Delay) < .00001f,
                        "audited rate/duration/delay: " + expected.Name);
                    foreach (var source in new[] { PreviewSource.Hover, PreviewSource.Held })
                    foreach (var before in new[] { 0f, .2f, .95f })
                    {
                        preview.Source = source;
                        StatusProjector.Populate(preview, t => t == CharacterAfflictions.STATUSTYPE.Poison ? before : .3f, t => 1f, false);
                        var projected = preview.Statuses.Single(s => s.Type == CharacterAfflictions.STATUSTYPE.Poison);
                        Require(Math.Abs(projected.After - Math.Min(1, before + expected.Total)) < .00001f,
                            "production projection matches independent audit: " + expected.Name);
                        Require(overlay.Render(bar, preview, healthy, .7f) && overlay.VisibleAddedWidth > 1,
                            "audited poison visible on " + source + ": " + expected.Name);
                        overlay.Render(bar, preview, healthy, 0);
                        Require(overlay.VisibleAddedWidth == 0, "audited original phase");
                        overlay.Hide(); Require(!overlay.Visible, "audited target leave");
                    }
                    audited.Add(expected.Id);
                    SessionTrace.Write("AUDITED_POISON_PASS", $"id={expected.Id} name={expected.Name} total={expected.Total} delay={expected.Delay} duration={expected.Duration} sources=Hover,Held states=zero,existing,cap; loaded assets and synthetic HUD, not live raycast");
                }
                SessionTrace.Write("ASSET_COVERAGE", $"name={preview.Name} id={item.itemID} facts={preview.Effects.Count} extra={preview.HasExtraStamina} infinite={preview.InfiniteStamina} unsupported={string.Join(",", preview.Unsupported.Distinct())} timed={string.Join(",", preview.Effects.Where(e=>e.Timed).Select(e=>$"{e.Type}:{e.Amount}x{e.Duration}+delay{e.Delay}"))}");
                var direct = item.GetComponentsInChildren<ItemAction>(true).Any(a => ItemEffectReader.IsActive(a, item) && (a is Action_Consume || a is Action_ConsumeAndSpawn) && (a.OnPressed || a.OnCastFinished));
                foreach (var poisonAction in item.GetComponentsInChildren<Action_InflictPoison>(true).Where(a => ItemEffectReader.IsActive(a, item) && a.OnConsumed && direct && a.inflictionTime > 0 && a.poisonPerSecond > 0))
                {
                    poisonChecked++;
                    Require(preview.PoisonRisk == RiskLevel.Present, "real consumed poison recognised: " + preview.Name);
                    Require(preview.Effects.Any(e => e.Type == CharacterAfflictions.STATUSTYPE.Poison && Math.Abs(e.Duration - poisonAction.inflictionTime) < .001f && Math.Abs(e.Amount - poisonAction.poisonPerSecond) < .001f), "real poison rate/duration: " + preview.Name);
                    foreach (var source in new[] { PreviewSource.Hover, PreviewSource.Held })
                    {
                        preview.Source = source;
                        var after = EffectProjection.Project(0, 2, preview.Effects.Where(e => e.Type == CharacterAfflictions.STATUSTYPE.Poison).Select(e => new ProjectedEffect(e.Amount, e.Duration, e.Delay)));
                        Require(after > 0, "real timed poison projects on " + source);
                        preview.Statuses.Clear();
                        preview.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 0, after));
                        Require(overlay.Render(bar, preview, healthy, .7f) && overlay.AddedCount > 0 && overlay.AddedOpacity > .99f,
                            "real poison facts reach production renderer on " + source);
                        overlay.Hide();
                        Require(!overlay.Visible, "real poison leave clears on " + source);
                    }
                }
                if (!string.IsNullOrWhiteSpace(Labels.ItemDescription(item))) descriptions++;
                if (!preview.IsFood) continue;
                checkedCount++;
                SessionTrace.Write("ASSET_EFFECT_READ", $"name={preview.Name} id={item.itemID} facts={preview.Effects.Count} poison={preview.PoisonRisk} spores={preview.SporeRisk} actions={string.Join(",", item.GetComponentsInChildren<ItemAction>(true).Select(a => a.GetType().Name).Distinct())}");
            }
            Require(checkedCount > 0, "real loaded food resources were inspected");
            Require(poisonChecked > 0 && assets.Any(i => i.itemID == 3), "real toxic fruit regression fixture present");
            Require(audited.Count == AuditedPoisonCases.All.Length, "all six audited poison resources must be loaded and pass");
            SessionTrace.Write("ASSET_EFFECT_SUMMARY", $"foods={checkedCount} loadedItems={assets.Length} consumedPoisonPassed={poisonChecked} descriptions={descriptions}; read-only assets, not world acceptance");
            var shown = assets.FirstOrDefault(i => (i.itemTags & Item.ItemTags.Mushroom) != 0);
            mixed.Icon = shown?.UIData.icon;
            mixed.Name = Labels.Text("混合效果 · 界面自检", "Mixed effects · UI self-test");
            mixed.Category = Labels.Text("消耗品", "CONSUMABLE");
            mixed.Description = Labels.Text("合成数据；展示恢复与负面占用，不代表该物品实际效果。",
                "Synthetic values demonstrate recovery and harm, not this item's real effects.");
            mixed.Resources.Add(new ResourceLine(Labels.Uses, "3 / 4"));
            overlay.Render(bar, mixed, healthy, .7f);
            panel.Show(mixed);
            return overlay;
        }
        catch { overlay.Dispose(); throw; }
    }
    private static void CheckToolCards(Item[] assets)
    {
        var previous = PresentationOptions.Language!.Value;
        try
        {
            foreach (var language in new[] { "Chinese", "English", "Auto" })
            {
                PresentationOptions.Language.Value = language;
                var chinese = Labels.Chinese;
                foreach (var name in new[] { "Scout Cannon", "Passport" })
                {
                    var item = assets.FirstOrDefault(i => i.UIData.itemName == name);
                    Require(item != null, "real tool fixture loaded: " + name);
                    var preview = new ItemPreview();
                    ItemEffectReader.Read(item!, preview);
                    new PromptProvider().Populate(item!, preview);
                    var body = preview.Description;
                    Require(!body.Contains(Labels.Partial), "internal partial diagnostic excluded: " + name);
                    Require(body.Contains(chinese ? (name == "Passport" ? "打开护照" : "调整角度") :
                        (name == "Passport" ? "Open your passport" : "adjust its angle")), "localized tool description: " + language + " / " + name);
                    if (chinese) Require(!body.Contains("open") && !body.Contains("place") && !body.Contains("change angle"), "no raw action keys");
                    if (name == "Passport") Require(preview.Diagnostics.Contains(Labels.Partial), "unsupported passport diagnostic retained outside card");
                }
                foreach (var key in new[] { "open", "place", "change angle" })
                    Require(!string.IsNullOrWhiteSpace(Labels.InteractPrompt(key)) &&
                        (!chinese || !Labels.InteractPrompt(key).Equals(key, StringComparison.OrdinalIgnoreCase)), "localized action lookup: " + key);
                foreach (var name in new[] { "Piton", "Remedy Fungus" })
                {
                    var item = assets.FirstOrDefault(i => i.UIData.itemName == name);
                    Require(item != null, "real beginner tool fixture loaded: " + name);
                    // Exercise the production orchestration order: a late native
                    // description must not replace these instructions with a key.
                    using var log = new BepInEx.Logging.ManualLogSource("ToolCardSmoke");
                    var orchestrator = new PreviewOrchestrator(new IItemPreviewProvider[]
                    { new InstanceResourceProvider(), new PitonProvider(), new PromptProvider() }, log);
                    foreach (var source in new[] { PreviewSource.Hover, PreviewSource.Held })
                    {
                        var preview = orchestrator.Build(item!, false, true); preview.Source = source;
                        var body = preview.Description;
                        if (name == "Piton")
                        {
                            Require(new PitonProvider().CanHandle(item!), "piton matched by native item identity");
                            Require(body.Contains(chinese ? "没有固定使用次数" : "no rest-use limit") &&
                                body.Contains(chinese ? "锈蚀岩钉会断裂" : "rusty pitons"), "normal and rusty pitons distinguished");
                            var spike = item!.GetComponentInChildren<ClimbingSpikeComponent>(true);
                            Require(spike != null && spike.hammeredVersionPrefab != null, "actual piton hammered prefab present");
                            Require(spike!.hammeredVersionPrefab!.GetComponentInChildren<ClimbHandle>(true) != null &&
                                spike.hammeredVersionPrefab.GetComponentInChildren<ShittyPiton>(true) == null,
                                "deployable normal piton has handhold but not timed breaking component");
                        }
                        else
                        {
                            Require(body.Contains(chinese ? "丢下或投掷" : "Drop or throw") &&
                                body.Contains(chinese ? "自己和队友" : "nearby teammates") &&
                                body.Contains(chinese ? "毒素与孢子" : "poison and spores") &&
                                body.Contains(chinese ? "留在云内" : "Stay in the cloud"), "remedy cloud use and targets described");
                            var cloud = item!.GetComponentInChildren<ShelfShroom>(true);
                            Require(cloud != null && cloud.breakOnCollision && cloud.instantiateOnBreak != null, "actual remedy collision-to-cloud component present; components=" +
                                string.Join(",", item.GetComponentsInChildren<MonoBehaviour>(true).Where(c => c != null).Select(c => c.GetType().Name)));
                            SessionTrace.Write("REMEDY_CLOUD_ASSET", "prefab=" + cloud!.instantiateOnBreak!.name + " minImpactSpeed=" + cloud.minBreakVelocity);
                            Require(preview.Statuses.Count == 0, "area cloud not misrepresented as unconditional self-use prediction");
                        }
                        Require(!body.Contains(Labels.Partial), "beginner descriptions have no internal diagnostics");
                    }
                }
                var uncertain = new ItemPreview { IsFood = true, CompleteEffects = false };
                ItemEffectReader.AssessRisks(uncertain);
                Require(uncertain.PoisonRisk == RiskLevel.Unknown && uncertain.SporeRisk == RiskLevel.Unknown,
                    "removing diagnostic does not falsely mark unknown food safe");
            }
            Require(MinimalRows.Build(new ItemPreview(), Labels.Chinese).Count == 0, "empty tool card does not add no-preview filler");
            SessionTrace.Write("SMOKE_TOOL_CARDS_PASS", "loaded Scout Cannon+Passport; Chinese+English+Auto; localized prompts; diagnostic hidden but retained; risk preserved; no empty-card filler");
            SessionTrace.Write("SMOKE_BEGINNER_TOOLS_PASS", "loaded Piton+Remedy Fungus; native deployment/cloud components; Chinese+English+Auto; hover/held; production description order");
        }
        finally { PresentationOptions.Language.Value = previous; }
    }
    internal static void ShowEnglish(MinimalPreviewPanel panel)
    {
        var previous = PresentationOptions.Language!.Value;
        try
        {
            PresentationOptions.Language.Value = "English";
            var actual = Resources.FindObjectsOfTypeAll<Item>().FirstOrDefault(i => i.UIData != null && i.UIData.itemName == "Energy Drink");
            var preview = new ItemPreview { Name = actual != null ? Labels.ItemName(actual) : "Energy Drink", Icon = actual?.UIData.icon,
                IsFood = true, Category = "CONSUMABLE", Source = PreviewSource.Held,
                Description = "Synthetic preview fixture: pulse shows estimated cumulative poison." };
            preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .02f, 5, 3));
            ItemEffectReader.AssessRisks(preview);
            panel.Show(preview);
            Require(!panel.LastTargetName.Contains("Held") && preview.PoisonRisk == RiskLevel.Present && panel.BodyText.Contains("+10") && panel.BodyText.Contains("/ 5s") && panel.BodyText.Contains("after 3s"), "English minimal semantics");
            SessionTrace.Write("SMOKE_ENGLISH_PASS", "explicit language restored after formatting");
        }
        finally { PresentationOptions.Language.Value = previous; }
    }
    private static void CheckNativeBonus(Image healthy, StaminaBar fixture, Transform parent)
    {
        var native = Resources.FindObjectsOfTypeAll<StaminaBar>().FirstOrDefault(b => b != fixture && b.extraBar != null &&
            b.extraBarStamina != null && b.extraBarOutline != null && b.extraStaminaIcon != null &&
            !b.transform.name.StartsWith("Synthetic"));
        if (native == null)
        {
            SessionTrace.Write("SMOKE_NATIVE_BONUS_ASSETS_PENDING", "Title does not load original in-level HUD; serialized game hierarchy audited offline; native layout+modifier tests passed; original sprite binding/visual appearance needs in-level acceptance");
            return;
        }
        var root = NativeVisualCopy.Create(native!.extraBar, parent,
            native.petrifyAffliction != null ? native.petrifyAffliction.rtf : null);
        root.name = "SyntheticOriginalExtraHud";
        var previousRoot = fixture.extraBar; var previousFill = fixture.extraBarStamina;
        var previousOutline = fixture.extraBarOutline; var previousIcon = fixture.extraStaminaIcon;
        var overlay = new ExtraStaminaOverlay();
        try
        {
            fixture.extraBar = root;
            fixture.extraBarStamina = NativeVisualCopy.Find(native.extraBar, root, native.extraBarStamina)!;
            fixture.extraBarOutline = NativeVisualCopy.Find(native.extraBar, root, native.extraBarOutline)!;
            fixture.extraStaminaIcon = NativeVisualCopy.Find(native.extraBar, root, native.extraStaminaIcon.rectTransform)!.GetComponent<Image>();
            root.gameObject.SetActive(false); root.sizeDelta = Vector2.zero;
            var nativeFill = HudGhostOverlay.NativeFill(native.extraBarStamina, null);
            var nativeOutline = native.extraBarOutline.GetComponentInChildren<Image>(true);
            Require(nativeFill != null && nativeOutline != null, "original extra HUD fill+outline images exist");
            overlay.Render(fixture, new ItemPreview { HasExtraStamina = true, ExtraBefore = 0, ExtraAfter = .2f }, healthy, .7f);
            Canvas.ForceUpdateCanvases();
            Require(overlay.ExtraVisible && overlay.IconVisible && overlay.TrackVisible && overlay.ExtraBounds.height > 1,
                "original extra visual hierarchy renders despite collapsed source");
            Require(overlay.IconImage!.sprite == native.extraStaminaIcon.sprite &&
                overlay.IconImage.GetType() == native.extraStaminaIcon.GetType() &&
                overlay.OutlineImage!.sprite == nativeOutline!.sprite && overlay.OutlineImage.material == nativeOutline.material,
                "original lightning+outline assets and component types preserved");
            Require(root.sizeDelta == Vector2.zero && !root.gameObject.activeSelf, "original fixture never enabled or resized");
            Require(overlay.ExtraBounds.width > 130 && overlay.ExtraBounds.width < 150,
                "original extra bar gain is exactly twenty percent of synthetic 700px scale");
            overlay.Hide(); Require(!overlay.ExtraVisible && !overlay.IconVisible, "native-style extra cleanup");
            SessionTrace.Write("SMOKE_NATIVE_BONUS_ASSETS_PASS", $"fill={nativeFill!.GetType().Name}:{nativeFill.sprite?.name} outline={nativeOutline!.GetType().Name}:{nativeOutline.sprite?.name} icon={native.extraStaminaIcon.sprite?.name}; loaded original visuals, inactive wrapper, owned clone only");
        }
        finally
        {
            overlay.Dispose(); Object.Destroy(root.gameObject);
            fixture.extraBar = previousRoot; fixture.extraBarStamina = previousFill;
            fixture.extraBarOutline = previousOutline; fixture.extraStaminaIcon = previousIcon;
        }
    }

    private static void Require(bool condition, string label)
    { if (!condition) throw new InvalidOperationException("Optimization smoke: " + label); }
    private static BarAffliction Badge(Transform parent, CharacterAfflictions.STATUSTYPE type, float x, float width, Color colour)
    {
        var image = Image(type.ToString(), parent, x, width, colour);
        var go = image.gameObject; go.SetActive(false);
        var native = go.AddComponent<BarAffliction>(); native.enabled = false;
        native.rtf = image.rectTransform; native.afflictionType = type;
        native.icon = Image("Icon", go.transform, 0, 20, colour);
        native.icon.rectTransform.anchoredPosition = new Vector2(0, 30);
        go.SetActive(true); return native;
    }
    private static Image Image(string name, Transform parent, float x, float width, Color colour)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        var r = image.rectTransform; r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = Vector2.zero; r.anchoredPosition = new Vector2(x, 0); r.sizeDelta = new Vector2(width, 26);
        image.color = colour; image.raycastTarget = false; return image;
    }
}
