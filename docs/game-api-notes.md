# PEAK 2.4.b API notes

Inspected local build `3e62ee214` at `D:\SteamLibrary\steamapps\common\PEAK`.

## Verified entry points

- `Interaction.instance.currentHovered : IInteractible` identifies the current crosshair target.
- `Item.itemID : ushort` is used as the locale-independent MVP identity.
- `Item.UIData` exposes name, icon, and primary/secondary/scroll prompts.
- `Item.totalUses` and `ItemInstanceData` expose instance resources through `DataEntryKey.ItemUses`, `Fuel`, and `UseRemainingPercentage`.
- `Item.cooking : ItemCooking` exposes `canBeCooked` and `timesCookedLocal`.
- `Character.localCharacter.refs.afflictions` exposes `GetCurrentStatus` and `GetStatusCap`.
- `Action_ModifyStatus : ItemAction` exposes `statusType`, `changeAmount`, and inherited `OnConsumed`.

## Preview policy

The MVP treats only `Action_ModifyStatus` components with `OnConsumed == true` as exact direct effects. It groups changes by status, adds them to the current player value, and clamps to the game's current status cap.

Afflictions, delayed actions, cooking transformations, random effects, and custom scripted effects are deliberately not simulated yet. The UI falls back to prompts/resources instead of guessing.

## Compatibility boundary

All direct PEAK access is isolated to detection, providers, and state capture. If a future update renames members, those adapters can be disabled while the UI and pure preview model continue to load.
