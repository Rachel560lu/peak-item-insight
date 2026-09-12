using PeakItemInsight.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

int passed = 0, failed = 0;
void Check(string name, Action action)
{
    try { action(); passed++; Console.WriteLine($"PASS {name}"); }
    catch (Exception e) { failed++; Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
void Expect(bool value) { if (!value) throw new Exception("Assertion failed"); }
void Near(float actual, float expected) => Expect(Math.Abs(actual - expected) < 0.00001f);

Check("source modes independently choose hover or held", () => {
    Expect(PreviewTargetSelection.Select(1, 2, false, PreviewMode.Hover).InstanceId == 1);
    Expect(PreviewTargetSelection.Select(1, 2, false, PreviewMode.Held).InstanceId == 2);
    Expect(PreviewTargetSelection.Select(1, 2, false, PreviewMode.Both).InstanceId == 1);
    Expect(!PreviewTargetSelection.Select(1, 2, false, PreviewMode.Off).IsValid);
});
Check("source mode absent targets never fall back to disabled source", () => {
    Expect(!PreviewTargetSelection.Select(null, 2, false, PreviewMode.Hover).IsValid);
    Expect(!PreviewTargetSelection.Select(1, null, false, PreviewMode.Held).IsValid);
    Expect(PreviewTargetSelection.Select(null, 2, false, PreviewMode.Both).Source == PreviewSource.Held);
});
Check("source mode busy held item suppressed including hovered same instance", () => {
    foreach (var mode in Enum.GetValues<PreviewMode>()) Expect(!PreviewTargetSelection.Select(2, 2, true, mode).IsValid);
    Expect(PreviewTargetSelection.Select(1, 2, true, PreviewMode.Hover).InstanceId == 1);
});
Check("text and stamina modes cover all 16 independent combinations", () => {
    foreach (var text in Enum.GetValues<PreviewMode>()) foreach (var bar in Enum.GetValues<PreviewMode>()) {
        var a = PreviewTargetSelection.Select(10, 20, false, text);
        var b = PreviewTargetSelection.Select(10, 20, false, bar);
        Expect(a.InstanceId == (text == PreviewMode.Off ? 0 : text == PreviewMode.Held ? 20 : 10));
        Expect(b.InstanceId == (bar == PreviewMode.Off ? 0 : bar == PreviewMode.Held ? 20 : 10));
    }
});

Check("hunger .40 -> .15 after food -.25", () => Near(PreviewMath.Apply(.4f, -.25f, 1), .15f));
Check("food clamps at zero", () => Near(PreviewMath.Apply(.1f, -.25f, 1), 0));
Check("healthy food produces no false gain", () => Near(PreviewMath.Apply(0, -.25f, 1), 0));
Check("poison .10 -> .30", () => Near(PreviewMath.Apply(.1f, .2f, 1), .3f));
Check("status cap respected", () => Near(PreviewMath.Apply(.9f, .3f, 1), 1));
Check("native caps can exceed one", () => Near(PreviewMath.Apply(.9f, .3f, 2), 1.2f));
Check("gain from total .6 by -.2", () => Near(PreviewMath.ProjectedCapacity(.6f, -.2f), .6f));
Check("mixed cancelling effects retain endpoint", () => Near(PreviewMath.ProjectedCapacity(.4f, -.2f + .2f), .6f));
Check("overfull total must recover hidden overflow first", () => Near(PreviewMath.ProjectedCapacity(1.3f, -.2f), 0));
Check("overfull crossing one", () => Near(PreviewMath.ProjectedCapacity(1.1f, -.2f), .1f));
Check("capacity lower and upper clamp", () => { Near(PreviewMath.Capacity(2), 0); Near(PreviewMath.Capacity(0), 1); });
Check("NaN rejected", () => { bool caught = false; try { PreviewMath.Apply(float.NaN, 0, 1); } catch (ArgumentOutOfRangeException) { caught = true; } Expect(caught); });
Check("hover delay and leave", () => {
    var gate = new HoverGate(); Expect(!gate.Advance(1, 0, .12f)); Expect(gate.Changed);
    Expect(!gate.Advance(1, .1f, .12f)); Expect(!gate.Changed);
    Expect(gate.Advance(1, .13f, .12f)); Expect(!gate.Advance(null, .14f, .12f)); Expect(gate.Changed);
});
Check("A to B invalidates displayed A immediately", () => {
    var gate = new HoverGate(); gate.Advance(1, 0, .12f); Expect(gate.Advance(1, 1, .12f));
    Expect(!gate.Advance(2, 1.1f, .12f)); Expect(gate.Changed); Expect(gate.Advance(2, 1.3f, .12f));
});
Check("two FakeItems of same prefab still reset delay by world ID", () => {
    var gate = new HoverGate(); gate.Advance(100, 0, .12f); gate.Advance(100, 1, .12f);
    Expect(!gate.Advance(101, 1.01f, .12f)); Expect(gate.Changed);
});
Check("disable or scene change resets timer", () => {
    var gate = new HoverGate(); gate.Advance(1, 0, .12f); gate.Reset();
    Expect(!gate.Advance(1, 5, .12f)); Expect(gate.Advance(1, 5.2f, .12f));
});
Check("zero delay enters immediately", () => Expect(new HoverGate().Advance(1, 0, 0)));
var session = new[] { "T pid=42 START version=0.1.6 path=test", "T pid=42 FIRST_FRAME frame=1", "T pid=42 HEARTBEAT frame=1", "T pid=42 HEARTBEAT frame=400", "T pid=42 SMOKE_UI_PASS ok" };
Check("session evidence: complete same PID", () => Expect(SessionEvidence.IsReady(session, 42, "0.1.6")));
Check("session evidence: Steam replacement PID rejected", () => Expect(!SessionEvidence.IsReady(session, 43, "0.1.6")));
Check("session evidence: stale version rejected", () => Expect(!SessionEvidence.IsReady(session, 42, "0.1.5")));
Check("session evidence: startup-only rejected", () => Expect(!SessionEvidence.IsReady(session.Take(1).ToArray(), 42, "0.1.6")));
Check("session evidence: destroyed runtime rejected", () => Expect(!SessionEvidence.IsReady(session.Append("T pid=42 PLUGIN_DESTROY reason=quit").ToArray(), 42, "0.1.6")));
Check("session evidence: errors rejected", () => Expect(!SessionEvidence.IsReady(session.Append("T pid=42 ERROR exception=test").ToArray(), 42, "0.1.6")));
Check("randomized projection invariants (10000 samples)", () => {
    var random = new Random(3527290);
    for (var i = 0; i < 10000; i++) {
        float sum = (float)random.NextDouble() * 2, amount = (float)random.NextDouble();
        float before = PreviewMath.Capacity(sum), heal = PreviewMath.ProjectedCapacity(sum, -amount), harm = PreviewMath.ProjectedCapacity(sum, amount);
        Expect(heal >= before && harm <= before && heal <= 1 && harm >= 0);
    }
});

var gamePath = @"D:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed\Assembly-CSharp.dll";
Check("target: empty hands and no hover hides", () => Expect(!PreviewTargetSelection.Select(null, null, false).IsValid));
Check("target: held item over floor", () => Expect(PreviewTargetSelection.Select(null, 12, false).Equals(new PreviewTargetKey(PreviewSource.Held, 12))));
Check("target: hovered tool/unknown outranks held food", () => Expect(PreviewTargetSelection.Select(8, 12, false).Equals(new PreviewTargetKey(PreviewSource.Hover, 8))));
Check("target: using held item hides over floor", () => Expect(!PreviewTargetSelection.Select(null, 12, true).IsValid));
Check("target: independent hover survives held use", () => Expect(PreviewTargetSelection.Select(8, 12, true).Source == PreviewSource.Hover));
Check("target: cannot bypass busy through same-item hover", () => Expect(!PreviewTargetSelection.Select(12, 12, true).IsValid));
Check("target: cancellation restores held source", () => { Expect(!PreviewTargetSelection.Select(null, 12, true).IsValid); Expect(PreviewTargetSelection.Select(null, 12, false).IsValid); });
Check("target: switching same type but distinct instances invalidates", () => {
    var gate = new PreviewTargetGate(); var a = new PreviewTargetKey(PreviewSource.Held, 12);
    Expect(!gate.Advance(a, 0, .12f)); Expect(gate.Advance(a, 1, .12f));
    Expect(!gate.Advance(new PreviewTargetKey(PreviewSource.Held, 13), 2, .12f)); Expect(gate.Changed);
});
Check("target: source is part of identity even with same ID", () => {
    var gate = new PreviewTargetGate(); gate.Advance(new PreviewTargetKey(PreviewSource.Hover, 12), 0, 0);
    Expect(!gate.Advance(new PreviewTargetKey(PreviewSource.Held, 12), 1, .12f)); Expect(gate.Changed);
});
Check("target: hover leave falls back then empty clears", () => {
    var gate = new PreviewTargetGate(); gate.Advance(PreviewTargetSelection.Select(8, 12, false), 0, 0);
    Expect(gate.Advance(PreviewTargetSelection.Select(null, 12, false), 1, 0)); Expect(gate.Changed);
    Expect(!gate.Advance(PreviewTargetSelection.Select(null, null, false), 2, 0)); Expect(gate.Changed);
});
Check("target: stable held target does not reset timer", () => {
    var gate = new PreviewTargetGate(); var key = new PreviewTargetKey(PreviewSource.Held, 12);
    gate.Advance(key, 0, .12f); Expect(gate.Advance(key, 1, .12f)); Expect(!gate.Changed);
    Expect(gate.Advance(key, 2, .12f)); Expect(!gate.Changed);
    gate.Reset(); Expect(!gate.Advance(key, 3, .12f));
});
Check("injury pulse: 20 to 15 covers quarter", () => Near(HungerPulseMath.Fraction(.2f, .15f), .25f));
Check("injury pulse: full and zero treatment", () => { Near(HungerPulseMath.Fraction(.05f, 0), 1); Near(HungerPulseMath.Fraction(.05f, .05f), 0); });
Check("hunger pulse: full recovery", () => Near(HungerPulseMath.Fraction(.05f, 0), 1));
Check("hunger pulse: excess food clamped by production prediction", () => Near(HungerPulseMath.Fraction(.05f, PreviewMath.Apply(.05f, -.2f, 1)), 1));
Check("hunger pulse: partial 2 of 5 percent", () => Near(HungerPulseMath.Fraction(.05f, .03f), .4f));
Check("hunger pulse: no recovery or worsening stays original", () => { Near(HungerPulseMath.Fraction(.05f, .05f), 0); Near(HungerPulseMath.Fraction(.05f, .1f), 0); });
Check("hunger pulse: zero hunger never paints", () => Near(HungerPulseMath.Fraction(0, 0), 0));
Check("hunger pulse: invalid inputs fail closed", () => { Near(HungerPulseMath.Fraction(float.NaN, 0), 0); Near(HungerPulseMath.Fraction(.05f, -1), 0); Near(HungerPulseMath.Fraction(float.PositiveInfinity, 0), 0); });
Check("hunger pulse: yellow green yellow cycle", () => { Near(HungerPulseMath.Alpha(0), 0); Near(HungerPulseMath.Alpha(.7f), 1); Near(HungerPulseMath.Alpha(1.4f), 0); });
Check("hunger pulse: minimum badge width uses proportional coverage", () => Near(12 * HungerPulseMath.Fraction(.05f, .03f), 4.8f));
Check("hunger pulse: 10000 bounded fractions and opacities", () => {
    var random = new Random(18);
    for (var i = 0; i < 10000; i++) {
        var before = (float)random.NextDouble(); var after = (float)random.NextDouble();
        var fraction = HungerPulseMath.Fraction(before, after); var alpha = HungerPulseMath.Alpha(i * .031f);
        Expect(fraction >= 0 && fraction <= 1 && alpha >= 0 && alpha <= 1);
        if (after >= before) Near(fraction, 0);
    }
});
using var game = ModuleDefinition.ReadModule(gamePath);
Check("installed API: hunger badge can bind by status and rect", () => {
    var type = game.Types.Single(t => t.Name == "BarAffliction");
    Expect(type.Fields.Any(f => f.Name == "afflictionType"));
    Expect(type.Fields.Any(f => f.Name == "rtf" && f.FieldType.Name == "RectTransform"));
    var bar = game.Types.Single(t => t.Name == "StaminaBar");
    Expect(bar.Fields.Any(f => f.Name == "afflictions"));
    Expect(bar.Fields.Any(f => f.Name == "staminaBar"));
});
Check("installed API: interaction target and FakeItem prefab", () => {
    Expect(game.Types.Single(t => t.Name == "Interaction").Fields.Any(f => f.Name == "currentHovered"));
    Expect(game.Types.Single(t => t.Name == "FakeItem").Fields.Any(f => f.Name == "realItemPrefab" && f.FieldType.Name == "Item"));
});
Check("installed API: remaining uses is optional value", () => {
    var type = game.Types.Single(t => t.Name == "OptionableIntItemData");
    Expect(type.Fields.Any(f => f.Name == "HasData")); Expect(type.Fields.Any(f => f.Name == "Value"));
});
Check("installed API: LateUpdate actually contains currentHovered writes", () => {
    var method = game.Types.Single(t => t.Name == "Interaction").Methods.Single(m => m.Name == "LateUpdate");
    Expect(method.Body.Instructions.Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "currentHovered"));
});
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var dll = Path.Combine(root, "bin/Release/netstandard2.1/PeakItemInsight.dll");
Check("compiled live HUD routes through recovery pair, not old rectangles", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var hud = mod.Types.Single(t => t.Name == "HudGhostOverlay");
    var late = hud.Methods.Single(m => m.Name == "LateUpdate");
    Expect(late.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "RenderRecoveries"));
    var render = hud.Methods.Single(m => m.Name == "RenderRecoveries");
    Expect(render.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "StatusRecoveryOverlays" && m.Name == "Render"));
    Expect(!render.Body.Instructions.Any(i => i.Operand is MethodReference m && (m.Name == "RenderSegment" || m.Name == "RenderProbe")));
});
Check("compiled hide path clears the new overlay", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var hud = mod.Types.Single(t => t.Name == "HudGhostOverlay");
    Expect(hud.Methods.Single(m => m.Name == "Hide").Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "HideSegment"));
    Expect(hud.Methods.Single(m => m.Name == "HideSegment").Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "StatusRecoveryOverlays" && m.Name == "Hide"));
});
Check("compiled runtime selects source before prediction", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var tick = mod.Types.Single(t => t.Name == "RuntimeController").Methods.Single(m => m.Name == "Tick");
    var calls = tick.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToList();
    var select = calls.FindIndex(m => m.DeclaringType.Name == "PreviewTargetSelection" && m.Name == "Select");
    var build = calls.FindIndex(m => m.DeclaringType.Name == "PreviewOrchestrator" && m.Name == "Build");
    Expect(select >= 0 && build > select);
    Expect(calls.Any(m => m.DeclaringType.Name == "HeldResolver" && m.Name == "Resolve"));
});
Check("compiled production preview does not call gameplay mutations", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var forbidden = new HashSet<string> { "RunAction", "AddStatus", "SubtractStatus", "SetStatus", "Consume", "RPC", "RPCA", "AddExtraStamina", "SetData", "Interact", "Interact_CastFinished", "AdjustStatus", "AddAffliction", "ClearPoisonAfflictions", "RunRandomEffect", "GetItemName", "GetData" };
    var violations = mod.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
        .SelectMany(m => m.Body.Instructions.Select(i => (method: m, instruction: i)))
        .Where(x => x.instruction.Operand is MethodReference r && forbidden.Contains(r.Name)).ToArray();
    if (violations.Length > 0) throw new Exception(string.Join(", ", violations.Select(v => v.method.FullName)));
});
Check("risk: capped poison is still poisonous", () => Expect(RiskEvidence.Assess(true, true, false) == RiskLevel.Present));
Check("risk: partial harmful facts outrank unknown", () => Expect(RiskEvidence.Assess(true, false, true) == RiskLevel.Present));
Check("risk: absent requires complete instance evidence", () => {
    Expect(RiskEvidence.Assess(false, true, false) == RiskLevel.Absent);
    Expect(RiskEvidence.Assess(false, false, false) == RiskLevel.Unknown);
    Expect(RiskEvidence.Assess(false, true, true) == RiskLevel.Unknown);
});
Check("layout: new poison reserves its native ordered slot", () => {
    var r = StatusGhostLayout.Build(1000, new[] { 100f, 0f, 50f }, new[] { 0f, 100f, 0f });
    Near(r[0].CurrentStart, 750); Near(r[1].AddedStart, 850); Near(r[2].CurrentStart, 950);
});
Check("layout: increased existing spores only draws increment", () => {
    var r = StatusGhostLayout.Build(1000, new[] { 100f }, new[] { 50f });
    Near(r[0].CurrentStart, 900); Near(r[0].AddedStart, 850); Near(r[0].AddedWidth, 50);
});
Check("layout: no gain keeps actual minimum badge width", () => {
    var r = StatusGhostLayout.Build(500, new[] { 12f, 30f }, new[] { 0f, 0f });
    Near(r[0].CurrentStart, 458); Near(r[0].CurrentWidth, 12);
});
Check("layout: 10000 ordered non-overlapping spans", () => {
    var random = new Random(200);
    for (var i = 0; i < 10000; i++) {
        var c = Enumerable.Range(0, 8).Select(_ => (float)random.NextDouble() * 100).ToArray();
        var a = Enumerable.Range(0, 8).Select(_ => (float)random.NextDouble() * 50).ToArray();
        var r = StatusGhostLayout.Build(1000, c, a);
        for (var j = 0; j < r.Length; j++) {
            Expect(Math.Abs(r[j].AddedStart + r[j].AddedWidth - r[j].CurrentStart) < .001f);
            if (j + 1 < r.Length) Expect(Math.Abs(r[j].CurrentStart + r[j].CurrentWidth - r[j + 1].AddedStart) < .001f);
        }
    }
});
Check("compiled HUD dispatches additions and restores owned groups", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var hud = mod.Types.Single(t => t.Name == "HudGhostOverlay");
    Expect(hud.Methods.Single(m => m.Name == "RenderRecoveries").Body.Instructions.Any(i =>
        i.Operand is MethodReference m && m.DeclaringType.Name == "StatusIncreaseOverlay" && m.Name == "Render"));
    Expect(hud.Methods.Single(m => m.Name == "HideSegment").Body.Instructions.Any(i =>
        i.Operand is MethodReference m && m.DeclaringType.Name == "StatusIncreaseOverlay" && m.Name == "Hide"));
});
Check("installed mushroom case 8 remains immediate 25 spores", () => {
    var type = game.GetTypes().Single(t => t.FullName == "Action_RandomMushroomEffect/<RunRandomEffect>d__9");
    var method = type.Methods.Single(m => m.Name == "MoveNext");
    var switches = method.Body.Instructions.Where(i => i.OpCode == OpCodes.Switch).ToArray();
    var entry = ((Instruction[])switches[1].Operand)[8];
    var block = method.Body.Instructions.SkipWhile(i => i != entry).Take(12).ToArray();
    Expect(block.Any(i => i.OpCode == OpCodes.Ldc_I4_S && Convert.ToInt32(i.Operand) == 10));
    Expect(block.Any(i => i.OpCode == OpCodes.Ldc_R4 && Math.Abs((float)i.Operand - .25f) < .00001f));
    Expect(block.Any(i => i.Operand is MethodReference m && m.Name == "AdjustStatus"));
});
Check("installed cosmetic animation action is side-effect-free", () => {
    var method = game.Types.Single(t => t.Name == "Action_PlayAnimation").Methods.Single(m => m.Name == "RunAction");
    Expect(method.Body.Instructions.All(i => i.OpCode == OpCodes.Ret || i.OpCode == OpCodes.Nop));
});
Check("consume: direct single-use fruit needs no charge data", () => Expect(ConsumptionRule.FiresConsumed(true, false, false, 0)));
Check("consume: direct action ignores positive charge counter", () => Expect(ConsumptionRule.FiresConsumed(true, false, true, 4)));
Check("consume: depleted item is unusable", () => Expect(!ConsumptionRule.FiresConsumed(true, false, true, 0)));
Check("consume: final charged use only", () => {
    Expect(ConsumptionRule.FiresConsumed(false, true, true, 1));
    Expect(!ConsumptionRule.FiresConsumed(false, true, true, 2));
    Expect(!ConsumptionRule.FiresConsumed(false, true, true, -1));
    Expect(!ConsumptionRule.FiresConsumed(false, true, false, 0));
});
Check("timeline: poison uses active duration not delay", () => Near(EffectProjection.Project(0, 2, new[] { new ProjectedEffect(.025f, 8, 3) }), .2f));
Check("timeline: existing poison and cap", () => Near(EffectProjection.Project(.95f, 1, new[] { new ProjectedEffect(.025f, 8, 3) }), 1));
Check("timeline: healing before delayed harm clamps first", () => Near(EffectProjection.Project(.1f, 1, new[] { new ProjectedEffect(-.5f), new ProjectedEffect(.3f, 0, 10) }), .3f));
Check("timeline: staggered opposite rates", () => Near(EffectProjection.Project(0, 1, new[] { new ProjectedEffect(-.1f, 5), new ProjectedEffect(.1f, 5, 2) }), .2f));
Check("timeline: zero and locked", () => {
    Near(EffectProjection.Project(.3f, 1, Array.Empty<ProjectedEffect>()), .3f);
    Near(EffectProjection.Project(.3f, 1, new[] { new ProjectedEffect(.2f, 8) }, true), .3f);
});
Check("compiled reader uses consumption predicate", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var method = mod.Types.Single(t=>t.Name=="ItemEffectReader").Methods.Single(m=>m.Name=="Read");
    Expect(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="ConsumptionRule"));
    Expect(!method.Body.Instructions.Any(i=>i.Operand is FieldReference f && f.Name=="totalUses"));
});
foreach (var poison in AuditedPoisonCases.All)
{
    Check("audited poison timeline: " + poison.Name, () => {
        var effect = new[] { new ProjectedEffect(poison.Rate, poison.Duration, poison.Delay) };
        Near(EffectProjection.Project(0, 1, effect), poison.Total);
        Near(EffectProjection.Project(.2f, 1, effect), .2f + poison.Total);
        Near(EffectProjection.Project(.95f, 1, effect), 1);
        Near(EffectProjection.Project(.2f, 1, effect, true), .2f);
        var spans = StatusGhostLayout.Build(700, new[] { 0f }, new[] { poison.Total * 700 });
        Near(spans[0].AddedWidth, poison.Total * 700);
    });
}
Check("compiled production and asset tests share status projector", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    foreach (var typeName in new[] { "DirectStatusProvider", "OptimizationSmokeTest" })
        Expect(mod.Types.Single(t => t.Name == typeName).Methods.Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions).Any(i => i.Operand is MethodReference m &&
                m.DeclaringType.Name == "StatusProjector" && m.Name == "Populate"));
});
Check("mixed HUD: hunger 5 to zero and poison zero to 10", () => {
    var r = StatusGhostLayout.BuildProjected(1000, new[] { 50f, 0f }, new[] {
        StatusGhostLayout.ProjectWidth(50, .05f, 0, 1000, 12),
        StatusGhostLayout.ProjectWidth(0, 0, .1f, 1000, 12) });
    Near(r[0].CurrentWidth, 0); Near(r[0].AddedWidth, 0);
    Near(r[1].AddedWidth, 100); Near(r[1].AddedStart, 900);
    Near(PreviewMath.Capacity(0 + .1f), .9f);
});
Check("mixed HUD: partial hunger recovery retains exact after value", () => {
    var r = StatusGhostLayout.BuildProjected(1000, new[] { 150f, 0f }, new[] { 100f, 100f });
    Near(r[0].CurrentWidth, 100); Near(r[0].CurrentStart, 800); Near(r[1].AddedWidth, 100);
});
Check("mixed HUD: existing poison and unaffected injury retained", () => {
    var r = StatusGhostLayout.BuildProjected(1000, new[] { 100f, 50f, 200f }, new[] { 100f, 0f, 300f });
    Near(r[0].CurrentWidth, 100); Near(r[1].CurrentWidth, 0);
    Near(r[2].CurrentWidth, 200); Near(r[2].AddedWidth, 100);
});
Check("native badge width: threshold, minimum and stretch padding", () => {
    Near(StatusGhostLayout.BadgeWidth(.01f, 600, 30, 5), 0);
    Near(StatusGhostLayout.BadgeWidth(.02f, 600, 30, 5), 35);
    Near(StatusGhostLayout.BadgeWidth(.1f, 600, 30, 5), 65);
    Near(StatusGhostLayout.ProjectWidth(37, .02f, .02f, 600, 30, 5), 37);
});
Check("mixed HUD: equal net changes still remove hunger and add poison", () => {
    var r = StatusGhostLayout.BuildProjected(1000, new[] { 50f, 0f }, new[] { 0f, 50f });
    Near(r[0].CurrentWidth, 0); Near(r[1].AddedWidth, 50);
});
Check("mixed HUD: randomized after widths preserved without overlap", () => {
    var rng = new Random(26);
    for (var i = 0; i < 10000; i++) {
        var before = Enumerable.Range(0, 8).Select(_ => (float)rng.NextDouble() * 100).ToArray();
        var after = Enumerable.Range(0, 8).Select(_ => (float)rng.NextDouble() * 100).ToArray();
        var r = StatusGhostLayout.BuildProjected(1000, before, after);
        for (var j = 0; j < r.Length; j++) {
            Expect(Math.Abs(r[j].CurrentWidth + r[j].AddedWidth - after[j]) < .001f);
            if (j + 1 < r.Length) Expect(Math.Abs(r[j].CurrentStart + r[j].CurrentWidth - r[j + 1].AddedStart) < .001f);
        }
    }
});
Check("compiled extra renderer uses native visuals not RoundedCard", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var extra = mod.Types.Single(t => t.Name == "ExtraStaminaOverlay");
    Expect(extra.Methods.Where(m => m.HasBody).SelectMany(m => m.Body.Instructions).Any(i =>
        i.Operand is MethodReference m && m.DeclaringType.Name == "NativeVisualCopy" && m.Name == "Create"));
    Expect(!extra.Fields.Any(f => f.FieldType.Name == "RoundedCard"));
});
Check("minimal: reference arrows preserve signed endpoints without timing", () => {
    var p = new ItemPreview { IsFood = true, HasExtraStamina = true, ExtraAfter = .1f };
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .05f, 0));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 0, .1f));
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.05f));
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .025f, 4, 2));
    var rows = MinimalRows.Build(p, true);
    Expect(rows[0].Direction == -1 && rows[0].DisplayText == "5" && rows[0].Text == "-5");
    Expect(rows[1].Direction == 1 && rows[1].DisplayText == "10");
    Expect(rows.Last().Direction == 1 && rows.Last().DisplayText == "10");
    Expect(new MinimalRow("清除").Direction == 0);
});
Check("minimal: food potency retained while HUD recovery is capped", () => {
    var p = new ItemPreview { IsFood = true, PoisonRisk = RiskLevel.Absent, SporeRisk = RiskLevel.Absent };
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.15f));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .05f, 0));
    var rows = MinimalRows.Build(p, true);
    Expect(rows.Count == 1 && rows[0].Text == "-15"); Near(p.Statuses[0].Before, .05f);
});
Check("minimal: timed poison and hunger retained with no duplicate danger", () => {
    var p = new ItemPreview { IsFood = true, PoisonRisk = RiskLevel.Present, SporeRisk = RiskLevel.Absent };
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .05f, 0));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 0, .1f));
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.05f));
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .025f, 4, 2));
    var zh = MinimalRows.Build(p, true); var en = MinimalRows.Build(p, false);
    Expect(zh.Count == 2 && zh[0].Text == "-5" && zh[1].Text == "+10");
    Expect(en[1].Text == "+10");
});
Check("minimal: delayed harm follows clamped recovery, not raw cancellation", () => {
    var p = new ItemPreview();
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, -.5f));
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, .5f, delay: 10));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Drowsy, 0, .5f));
    var rows = MinimalRows.Build(p, false);
    Expect(rows.Count == 1 && rows[0].Text == "+50");
});
Check("minimal: capped poison stays visible", () => {
    var p = new ItemPreview { IsFood = true, PoisonRisk = RiskLevel.Present, SporeRisk = RiskLevel.Absent };
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .1f));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 1, 1));
    Expect(MinimalRows.Build(p, false).Single().Text == "+10");
});
Check("minimal: unknown risk and known spores remain explicit", () => {
    var p = new ItemPreview { IsFood = true, PoisonRisk = RiskLevel.Unknown, SporeRisk = RiskLevel.Present };
    var rows = MinimalRows.Build(p, false);
    Expect(rows.Count == 2 && rows[0].Text == "?" && rows[1].Text == "Present");
});
Check("minimal: bonus uses actual gain and handles full capacity", () => {
    var p = new ItemPreview { HasExtraStamina = true, ExtraBefore = .9f, ExtraAfter = 1 };
    Expect(MinimalRows.Build(p, false).Single().Text == "+10");
    p.ExtraBefore = 1; Expect(MinimalRows.Build(p, false).Single().Text == "+0");
});
Check("minimal: clear and structured uses survive without cooking/debug clutter", () => {
    var p = new ItemPreview { DebugId = "secret", Description = "long description" };
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Injury, 0, clears: true));
    p.Resources.Add(new ResourceLine("次数", "3 / 4", ResourceKind.Uses));
    p.Resources.Add(new ResourceLine("烹饪", "x2", ResourceKind.Cooked));
    p.Diagnostics.Add("unsupported");
    var rows = MinimalRows.Build(p, true); Expect(rows.Count == 2 && rows[0].Text == "清除" && rows[1].Text == "3 / 4 次数");
});
Check("minimal: no empty view and special instructions preserved", () => {
    var p = new ItemPreview(); Expect(MinimalRows.Build(p, false).Count == 0);
    p.CompactUse = "Drop/throw: stay in cloud"; Expect(MinimalRows.Build(p, false).Single().Text == p.CompactUse);
});
Check("minimal: infinite stamina short label without duplicated duration", () => {
    var p = new ItemPreview { InfiniteStamina = true, SummarizeEffects = true, CompactInfinity = "Temporary" };
    var rows = MinimalRows.Build(p, false); Expect(rows.Count == 1 && rows[0].Lightning && rows[0].Text == "Temporary ∞");
});
Check("minimal-only: legacy Detailed setting cannot restore removed card", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    Expect(!mod.Types.Any(t => t.Name == "PreviewPanel" || t.Name == "RoundedCard"));
    var plugin = mod.Types.Single(t => t.Name == "Plugin");
    var strings = plugin.Methods.Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
        .Where(i => i.OpCode == OpCodes.Ldstr).Select(i => (string)i.Operand).ToArray();
    Expect(!strings.Intersect(new[] { "DisplayMode", "PanelScale", "OffsetX", "OffsetY", "ShowDetails", "BackgroundOpacity" }).Any());
    var runtime = mod.Types.Single(t => t.Name == "RuntimeController");
    Expect(runtime.Methods.Single(m => m.Name == "Tick").Body.Instructions.Any(i => i.Operand is MethodReference m &&
        m.DeclaringType.Name == "MinimalPreviewPanel" && m.Name == "Show"));
});
Check("minimal native: explicit font chain instead of scene font lottery", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var view = mod.Types.Single(t => t.Name == "MinimalPreviewPanel");
    var body = view.Methods.Single(m => m.Name == "AdoptFont").Body.Instructions;
    Expect(body.Any(i => i.Operand is FieldReference f && f.DeclaringType.Name == "FontFallbackSwapper" && f.Name == "mainBaseFont"));
    Expect(!body.Any(i => i.Operand is MethodReference m && m.Name == "FindObjectsOfTypeAll"));
    Expect(body.Any(i => i.Operand is MethodReference m && m.Name == "HasCharacter" && m.Parameters.Count == 3));
});
Check("preview anchor: holstered selected slot does not own hovered item", () => {
    Expect(PreviewAnchor.Select(PreviewSource.Hover, 0, false, true, true, false) == 1);
});
Check("preview anchor: all occupancy patterns ignore highlight and temporary held UI", () => {
    for (var mask = 0; mask < 8; mask++)
    for (var selected = -1; selected < 4; selected++)
    foreach (var temporary in new[] { false, true })
    {
        var expected = Enumerable.Range(0, 3).FirstOrDefault(i => (mask & (1 << i)) == 0, 3);
        Expect(PreviewAnchor.Select(PreviewSource.Hover, selected < 0 ? null : selected,
            (mask & 1) == 0, (mask & 2) == 0, (mask & 4) == 0, temporary) == expected);
    }
});
Check("preview anchor: held and hover transitions restore their own anchors", () => {
    Expect(PreviewAnchor.Select(PreviewSource.Held, 2, true, false, false, false) == 2);
    Expect(PreviewAnchor.Select(PreviewSource.Hover, 2, true, false, false, false) == 0);
    Expect(PreviewAnchor.Select(PreviewSource.Hover, 2, false, false, false, false) == 3);
    Expect(PreviewAnchor.Select(PreviewSource.Hover, 2, false, true, false, false) == 1);
    Expect(PreviewAnchor.Select(PreviewSource.Held, 2, false, true, false, false) == 2);
    Expect(PreviewAnchor.Select(PreviewSource.Held, null, false, false, false, true) == 4);
    Expect(PreviewAnchor.Select(PreviewSource.None, 0, true, true, true, false) == -1);
});
Check("minimal native: selected inventory index, not decorative icon", () => {
    using var mod = ModuleDefinition.ReadModule(dll);
    var view = mod.Types.Single(t => t.Name == "MinimalPreviewPanel");
    var body = view.Methods.Single(m => m.Name == "ResolveAnchor").Body.Instructions;
    Expect(body.Any(i => i.Operand is FieldReference f && f.Name == "currentSelectedSlot"));
    Expect(body.Any(i => i.Operand is MethodReference m && m.Name == "get_Value"));
    Expect(body.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "PreviewAnchor" && m.Name == "Select"));
    Expect(body.Any(i => i.Operand is FieldReference f && f.Name == "backpack" && f.DeclaringType.Name == "GUIManager"));
    Expect(!view.Methods.Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
        .Any(i => i.Operand is FieldReference f && f.Name == "selectedSlotIcon"));
});
foreach (var before in new[] { 0f, .1f, .25f, .5f })
Check($"timed summary: energy drink from {before} uses HUD endpoint without mutation", () => {
    var p = new ItemPreview();
    var type = CharacterAfflictions.STATUSTYPE.Drowsy;
    p.Effects.AddRange(new[] { new EffectFact(type, -1), new EffectFact(type, -.5f),
        new EffectFact(type, -1, 8), new EffectFact(type, .25f, 0, 8) });
    var after = EffectProjection.Project(before, 1, p.Effects.Select(e => new ProjectedEffect(e.Amount, e.Duration, e.Delay)));
    p.Statuses.Add(new StatusDelta(type, before, after));
    var facts = p.Effects.ToArray(); var snapshot = p.Statuses.ToArray();
    foreach (var chinese in new[] { true, false }) {
        var rows = MinimalRows.Build(p, chinese);
        Expect(rows.Count == (before == .25f ? 0 : 1));
        if (rows.Count > 0) { Near(float.Parse(rows[0].Text, System.Globalization.CultureInfo.InvariantCulture), (.25f - before) * 100); Expect(rows[0].Direction == Math.Sign(.25f - before)); }
    }
    Expect(p.Effects.SequenceEqual(facts) && p.Statuses.SequenceEqual(snapshot));
});
Check("timed summary: heat pack displays actual cold recovery not 360", () => {
    var p = new ItemPreview(); var type = CharacterAfflictions.STATUSTYPE.Cold;
    p.Effects.Add(new EffectFact(type, -.06f, 60));
    p.Statuses.Add(new StatusDelta(type, .4f, 0));
    Expect(MinimalRows.Build(p, true).Single().Text == "-40");
});
Check("timed summary: poison and hunger remain separate, cap respected", () => {
    var p = new ItemPreview { IsFood = true, PoisonRisk = RiskLevel.Present, SporeRisk = RiskLevel.Absent };
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.05f));
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .025f, 4, 2));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .02f, 0));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, .98f, 1));
    var rows = MinimalRows.Build(p, true);
    Expect(rows.Count == 2 && rows[0].Text == "-2" && rows[1].Text == "+2");
});
Check("timed summary: missing snapshot never fabricates cumulative gain", () => {
    var p = new ItemPreview(); p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Cold, -.06f, 60));
    Expect(MinimalRows.Build(p, true).Count == 0);
});
Check("timed summary: capped poison remains labelled, never false safe", () => {
    var p = new ItemPreview { IsFood = true, PoisonRisk = RiskLevel.Present, SporeRisk = RiskLevel.Absent };
    p.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, .025f, 4));
    p.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, 1, 1));
    var row = MinimalRows.Build(p, true).Single();
    Expect(row.Text == "有" && row.Status == CharacterAfflictions.STATUSTYPE.Poison && row.Direction == 0);
});
Check("timed summary: zero bonus hidden, range infinity labelled once", () => {
    var p = new ItemPreview { SummarizeEffects = true, HasExtraStamina = true, ExtraBefore = 1, ExtraAfter = 1,
        InfiniteStamina = true, CompactInfinity = "范围内" };
    var row = MinimalRows.Build(p, true).Single(); Expect(row.Lightning && row.Text == "范围内 ∞");
});
Console.WriteLine($"RESULT passed={passed} failed={failed}");
Environment.ExitCode = failed > 0 ? 1 : 0;
