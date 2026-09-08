# Changelog

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
