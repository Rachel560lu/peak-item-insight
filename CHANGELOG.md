# Changelog

## 0.2.5 — 2026-09-12 / UI fixes and refreshed demos

- Size bonus-stamina previews from the visible native fill, not the HUD container or a hidden placeholder. Add a stable thin track; preserve game-owned UI.
- Localize action keys and add concise Chinese/English Scout Cannon and Passport descriptions.
- Explain normal piton reuse versus breakable rusty map pitons; add Remedy Fungus drop/throw, group-healing and stay-in-cloud instructions in Chinese/English.
- Keep unsupported-effect and prefab diagnostics in test logs, not player cards. Poison/spore safety checks remain unchanged.
- Add Unity regressions for oversized hidden templates, nonzero pivots, existing bonus rows, both target sources and real loaded bilingual tool cards.
- Refresh the bilingual gameplay demos: energy drink, poisonous mushroom, poisonous berry, bandage, then the existing safe mushroom and item card.

## 0.2.4 — 2026-09-11 / player-page and cover update

- Replace the Thunderstore package cover with the new Item Insight artwork.
- Streamline both READMEs around download, gameplay demos, usage and common settings.
- Preserve the same 0.2.2 gameplay DLL; this package changes presentation only.

## 0.2.3 — 2026-09-11 / documentation-only package

- Add the three gameplay/card GIFs to Thunderstore and link public GitHub source and issues.
- Preserve the tested 0.2.2 DLL without rebuilding or changing gameplay; runtime log version remains 0.2.2.

## 0.2.2 — 2026-09-11 / first Thunderstore release

- Published under MIT as Rachel560lu-PeakItemInsight-0.2.2 with bilingual player documentation. User-confirmed gameplay demos cover poison, safe-food recovery and item cards; broader compatibility remains unverified.

- Harden new poison/status previews against inactive native slots with zero height or empty filled images, without changing the native slot or player state.
- Share the production status projector with loaded-asset regression tests. Require all six audited toxic fruit/mushroom IDs, rates, delays and durations to match; test hover/held source labels, zero/existing/capped poison, pulse phases and clearing.
- Keep audited values as test expectations, not runtime item-ID overrides, so disabled/cooked effects are not reintroduced.
- Add rate-limited poison HUD diagnostics including projected delta, active geometry and opacity. Real in-level raycast/equip acceptance remains separate from synthetic tests.

## 0.2.1 — development / not released

- Fix direct-consumption `OnConsumed` effects being incorrectly gated by a one-use counter. Charge depletion remains a separate last-use condition.
- Include timed poison/cold/drowsiness in an item-only cumulative timeline, with delay, duration, ordering and caps. Card labels these as estimates; existing buffs, natural recovery, environment and quantized game ticks are not simulated.
- Remove source labels, uncooked/no rows and generic food explanation. Keep independent poison/spore risk, signed effects and useful charges; timed rows show cumulative amounts.
- Extend gain/recovery routing beyond poison/spores, including petrify; add extra-stamina and infinite-stamina visual cues. Gain copies pulse on the same 1.4-second cycle; native state is restored on hide.
- Add actual loaded-food `OnConsumed` poison regression tests and a coverage inventory. Unknown actions, random area effects, thorns, target-dependent relics and special characters prevent an all-item coverage claim.
- 74 offline checks pass; title-screen synthetic/native-shaped UI tests are separate from pending real-world acceptance. Nothing published or pushed.

## 0.2.0 — development / not released

- Add separate raw effect facts, instantaneous projections and poison/spore risk assessment. Caps, cancellation and prefab-only data cannot falsely establish safety.
- Parse direct status actions, immediate status afflictions, timed poison and the audited PEAK 2.4.b level-specific mushroom mapping. Timed effects are labeled but excluded from exact immediate HUD ranges.
- Add native-style poison/spore increase segments and icons, independent recovery pulses, and mixed recovery/harm rendering. Owned image copies preserve native sprite style; a scoped visibility override restores native badges on hide/dispose.
- Redesign cards with game fonts, icons, type/risk hierarchy, signed effect lines, rounded translucent background and border. Remove default mini-bars; add game-following Chinese/English, details and animation options.
- Retain hover priority and held fallback. Periodically refresh conditional data and presentation changes even if the coarse state hash is unchanged.
- Add risk/layout/API tests, Unity synthetic coverage, loaded-asset inspection and bilingual screenshots. Full world-item/consumption and cross-resolution acceptance remains pending; do not promote old 0.1.9 release evidence to this build.

## 0.1.9

- Add current-hand preview with explicit Hover/Held source labels. A hovered item always wins, including tools/unknown items; otherwise fall back to the currently equipped instance, not inventory contents.
- Suppress a held item's preview during primary/secondary use, casting or consumption; reselect after cancellation/completion. Cache identity includes source/instance, and scene/player/HUD changes reset the target gate.
- Add Injury recovery pulses on the native injury badge using the accepted hunger renderer. Hunger/injury remain independent, share animation timing, and both clear when no target remains.
- Add production selection tests and a Unity routing/rendering smoke test for hover/held/empty, use suppression, source labels, full/partial/zero recovery, simultaneous statuses, and cleanup.
- Prediction coverage is unchanged: unknown, timed and conditional effects are still incomplete. Real held-item and injury-badge acceptance remains required.

## 0.1.8

- Replace live net-capacity/extra-stamina rectangles with a hunger-recovery pulse inside the native hunger badge. Other effects remain textual in this first iteration.
- Full recovery covers the whole badge; partial recovery covers a proportional slice toward the healthy side; zero/worsening hunger does not pulse. Smooth yellow/healthy-green cycle lasts 1.4 seconds.
- Own clipped UI child follows native bounds/scale without changing native colour, width, player state, or item actions. Looking away, disabling preview, and native HUD inactivity hide the child.
- Add pure math/API/compiled-routing checks and a Unity title-screen smoke test for the production hunger renderer, with optional automatic exit.
- Recognize zero-thread/zero-handle terminated process records in the local test launcher; never ignore unknown/live process metadata.

Earlier entries below describe historical builds, not the current lifecycle implementation.

## 0.1.3

- Drive hover processing from a postfix on `Interaction.LateUpdate`, immediately after PEAK finalizes `currentHovered`.
- Avoid unreliable plugin/runtime `Update` scheduling under the current hidden-manager Unity 6 setup.

## 0.1.2

- Move frame-driven hover and UI work off the hidden BepInEx plugin host and onto a dedicated persistent runtime object.
- Log runtime initialization and preview construction so silent lifecycle failures are observable.

## 0.1.1

- Resolve PEAK's world-loot `FakeItem` objects through their real item prefabs.
- Preview primary-use status actions beyond `OnConsumed`.
- Render status changes as a translucent segment on PEAK's stamina HUD.
- Reuse PEAK's own TMP font for contextual tool cards.
- Add low-volume hover target diagnostics for compatibility testing.

## 0.1.0 MVP

- Detect hovered PEAK items without picking them up.
- Preview direct consumed status changes against the local player's current state.
- Render compact before/after status bars and danger-cap warnings.
- Show remaining uses, fuel, cooking state, and native interaction prompts when available.
- Add a dedicated piton usage hint.
- Add caching keyed by item instance, resources, cooking state, and player status.
- Keep all behavior client-side and read-only.
