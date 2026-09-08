# PEAK Item Insight — In-game verification plan

Target build: PEAK 2.4.b. Test profile: `PeakItemInsight-Test`.

Historical checklist. Follow `verification-plan.md` and `verification-results.md` first. Do not request another user playtest until their prerequisites pass. Startup logs alone do not establish that the final Steam-launched process loaded the plugin.

## Safety gate

- PEAK is closed before replacing the DLL.
- The game-directory `BepInEx` junction resolves only to the test profile.
- Test profile contains BepInEx and `PeakItemInsight.dll` only.
- Existing quicksave hash is recorded before the run.
- Use a new offline run; do not continue the existing save.

## Gate 1 — Loader and frame hook

Pass only when the fresh `LogOutput.log` contains all of:

1. `Loading [PEAK Item Insight 0.1.6]` plus matching final process ID in independent diagnostics.
2. `Runtime controller initialized.`
3. `FIRST_FRAME` and multiple `HEARTBEAT` entries in the same final process; the current driver does not depend on Harmony.
4. Looking from empty space to an item produces a `Hover target:` transition.

Any missing line blocks later visual testing.

## Gate 2 — Target resolution

Test each target for at least one second, then move the crosshair to empty terrain.

| Case | Expected log | Expected UI |
|---|---|---|
| Empty terrain | `Hover target: null; resolved item: none` | Everything hidden |
| Uncollected food in luggage | `Hover target: FakeItem; resolved item: <id>` | Status preview or explicit no-change state |
| Uncollected piton/tool | `Hover target: FakeItem; resolved item: <id>` | Context card |
| Dropped real item, if PEAK keeps it instantiated | `Hover target: Item; resolved item: <id>` | Same result as its FakeItem form |
| Luggage/campfire/player | Non-item type; `resolved item: none` | No item card |

Pass requires FakeItem and Item forms of the same item to produce equivalent previews.

## Gate 3 — Status calculation

Use food in two controlled player states.

### 3A. Non-zero status

Accumulate a visible amount of Hunger, then hover a food that removes Hunger.

- Log reports `statuses:1` or more.
- Ghost segment is green and extends from the current maximum stamina endpoint to the projected endpoint.
- Segment size matches the reported before/after change within normal HUD rounding.
- Moving off the item hides the segment immediately.

### 3B. Fully healthy / clamped result

At zero Hunger, hover the same food.

- The mod must not silently look broken.
- Expected final UX: explicit `无变化` / wasted-effect indication.
- No green or red segment is drawn when projected and current values are identical.

### 3C. Harmful food

Hover an item with a positive Poison/Injury/etc. delta.

- Ghost segment is red and represents the projected loss of maximum stamina.
- No real character status changes before consuming the item.

## Gate 4 — Tool information cards

| Item | Required content |
|---|---|
| Piton | Placement/rest explanation and available uses when discoverable |
| Multi-use item | Remaining uses |
| Fuel item | Remaining fuel |
| Cookable item | Cooked state |
| Unknown non-status item | At minimum a localized item name and available controls; never a blank panel |

The card appears after the configured hover delay, uses a readable PEAK font, stays on screen, and disappears when the target changes.

## Gate 5 — HUD lifecycle

Repeat one food preview across:

1. Airport
2. New offline level
3. Pause/unpause
4. Inventory pickup/drop
5. Scene transition, if reachable without using an existing save

The overlay must rebind if `GUIManager.bar` is recreated. No duplicate ghost segments or cards may remain.

## Gate 6 — Non-mutation and cleanup

- Hovering never changes status values, item uses, fuel, cooked amount, or network state.
- `Player.log` and `LogOutput.log` contain no `PeakItemInsight` exception.
- Exit normally.
- Remove the temporary game-directory junction.
- Existing quicksave hash equals the pre-test hash.

## Evidence to capture

- Fresh loader log excerpt.
- One `FakeItem` resolution line and one preview-count line.
- Screenshots for beneficial, harmful, no-change, and piton cases.
- Final exception scan and quicksave hash comparison.
