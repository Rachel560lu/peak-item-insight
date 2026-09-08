# Changelog

## 0.1.9 — local testing candidate

- Added injury recovery pulses on the recoverable portion of the native injury segment.
- Added held-item fallback when no item is hovered, with a Hover/Held source label.
- Pause the used item's preview during use and recalculate after use.
- Keep hunger and injury recovery pulses independent and synchronized.
- Added player-facing release documentation and package validation.

Automated and synthetic tests are not real-game acceptance. Injury/held acceptance and standard clean-profile startup validation remain open.

## 0.1.8 — local testing

- Replaced the separate hunger recovery block with an in-place pulse toward healthy green.
- Handle full, partial and zero hunger recovery; clear previews when the target is lost.
- Hunger recovery has player-confirmed in-game evidence.

## Earlier local prototypes

- Introduced hover detection, immediate-effect prediction and usage information.
- Iterated on runtime loading, diagnostics and HUD rendering.

These entries describe local development versions, not previous public Thunderstore releases.
