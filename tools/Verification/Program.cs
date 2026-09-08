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
    var forbidden = new HashSet<string> { "RunAction", "AddStatus", "SubtractStatus", "SetStatus", "Consume", "RPC", "RPCA", "AddExtraStamina", "SetData", "Interact", "Interact_CastFinished" };
    var violations = mod.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
        .SelectMany(m => m.Body.Instructions.Select(i => (method: m, instruction: i)))
        .Where(x => x.instruction.Operand is MethodReference r && forbidden.Contains(r.Name)).ToArray();
    if (violations.Length > 0) throw new Exception(string.Join(", ", violations.Select(v => v.method.FullName)));
});
Console.WriteLine($"RESULT passed={passed} failed={failed}");
Environment.ExitCode = failed > 0 ? 1 : 0;
