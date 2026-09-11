# 0.2.5 local candidate: bonus preview and readable tool cards

## Report and cause

- `错误精力条外方块闪烁.mp4`: the Airplane Food/Granola Bar bonus-stamina preview used `max(fullBar.height, hiddenExtraFill.height)`. The outer HUD container and hidden/animated placeholder are not the visible stamina strip. The fallback also positioned an unframed fill above the outer container.
- Scout Cannon and Passport: `UIData` interaction fields contain localization keys (`place`, `change angle`, `open`). The provider displayed those keys directly.
- Unsupported-effect and prefab-only implementation diagnostics were mixed with player warnings and rendered as description text.

## Changes

- Derive bonus preview height/position from the visible healthy fill in the correct coordinate space. Ignore stale hidden template dimensions; use a valid visible native bonus row when present. A thin, stable enclosing track distinguishes bonus capacity from recovery. Only the gained portion pulses. Native objects and player status are not mutated.
- Resolve interaction keys through the game's selected-language tables. Add concise bilingual Cannon/Passport descriptions. Auto follows the game's language; explicit Chinese/English overrides still work.
- Separate internal diagnostics from player warnings. Remove empty-card “no previewable effect” filler. Keep poison/spore risk and useful limitations on timed estimates; no unsupported food is relabeled safe.

## Verification — 2026-09-11

- Release build: zero warnings/errors. `scripts/verify.ps1`: 81 passed, 0 failed.
- Final isolated engine run: PID 45644; runtime 0.2.5, normal exit 0.
- `SMOKE_BONUS_GEOMETRY_PASS`: outer height 180, inactive template height 200, actual fill height 26; projected gain width 140 and gap 8. Tests nonzero pivot, scaled parent, existing native bonus alignment, hover/held labels, unchanged native geometry and clearing.
- `SMOKE_TOOL_CARDS_PASS`: real loaded Scout Cannon and Passport assets, Chinese/English/Auto; localized actions, no partial diagnostic on cards, diagnostic still retained, unknown food risk preserved.
- Loaded-asset regressions: 194 items / 74 foods; all six audited poison cases pass. Hunger, injury, status gain, extra/infinite stamina and cleanup suites pass.
- Initial UI-fix candidate SHA256: `4CDF53F33D8FBC198CD2E3CF9C0528C3D52A2608DDF66A44176504888A1F60A2` (superseded by the beginner-tool description update below).
- Quicksave unchanged: SHA256 `306D8A3C08C8D1E6B7939E2229FE7B68E5D8C6FBE542D0CF2E004BDE88D5518D` before/after. Previous DLL, log and quicksave backed up under `backups/ui-fix-025/before-deploy/`.
- Only `PeakItemInsight-Test` updated. Public Thunderstore 0.2.4 package and release evidence are untouched. No push/publication performed.

## Remaining visual acceptance

The hidden engine run produced black screenshots; it verifies geometry/text assertions, not final in-world appearance. Do not describe it as completed visual acceptance.

1. Hover Airplane Food or Granola Bar with zero bonus stamina: a thin framed bonus preview sits directly above the main strip, with no distant flashing rectangle.
2. Hold the same item and look at the floor: same preview. Empty hands/look away or switch to a tool: it clears.
3. With existing bonus stamina, the preview aligns to its native row and pulses only the added amount.
4. In Chinese, Scout Cannon describes placing/aiming/launching in Chinese; Passport says “打开护照。” Neither card shows raw English action keys or internal partial-effect diagnostics.
5. Toxic food still shows poison/spore risk and the status preview; recovery previews remain intact.

This change does not claim to resolve every previously discussed mixed-status layout or special-item coverage limitation.

## Beginner-tool cards — 2026-09-12

- Normal, collectible Piton is item 18 / `ClimbingSpike`, with `ClimbingSpikeComponent.hammeredVersionPrefab`. It does not use the generic Constructable path. Its deployed prefab has a `ClimbHandle` and no `ShittyPiton` breaking component. Match the native item identity/component so the provider no longer falls back to the action key “设置”.
- Card explains placement/rest, one scout at a time, repeatable rest without a fixed use-count limit, and warns that naturally placed rusty pitons are different and can break. No fabricated remaining-durability number. Rusty map props remain outside the collectible-item scope.
- Remedy Fungus is item 90 / `HealingPuffShroom`. The loaded asset uses `ShelfShroom` with `breakOnCollision=true`, `minBreakVelocity=5`, and `instantiateOnBreak=HealingPuffShroomSpawn` (not CloudFungus). Card explains dropping/throwing, bursting on impact, healing self/nearby teammates, injury/poison/spore relief, and staying in the cloud for ongoing healing. This does not add an unconditional self-use HUD prediction for a spatial effect.
- Reference cross-checks: [Piton](https://peak.wiki.gg/wiki/Piton), [Remedy Fungus](https://peak.wiki.gg/wiki/Remedy_Fungus), [Equipment](https://peak.wiki.gg/wiki/Equipment). The wiki's Remedy totals/durations disagree between pages, so this descriptive update does not copy an unverified numeric total.
- Initial engine assertions caught incorrect test-fixture assumptions about Constructable/CloudFungus; the tests were corrected to the observed ClimbingSpikeComponent/ShelfShroom assets, retaining assertions on real deployed handholds and collision-spawned cloud resources.
- Engine run PID 59136 passed `SMOKE_BEGINNER_TOOLS_PASS` and exited 0. Tests exercise real loaded Piton/Remedy assets through production orchestration in Chinese, English and Auto, under both Hover/Held source labels. These are title-screen data/layout checks, not a new in-world visual acceptance.
- Latest compiled/isolated candidate SHA256: `F963AE469375E72CD88665931E650642E0F3A55A5DA43904CA184CB47593C5CC`. Offline checks remain 81/81. No release/push performed.
- Pre-update DLL/quicksave backup: `backups/tool-cards-025-20260912-001941/`.
