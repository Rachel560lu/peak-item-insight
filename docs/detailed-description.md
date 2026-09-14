# Optional detailed inventory description

Minimal remains the default. Esc → Item Insight settings → Description style switches between Minimal and Detailed; InventoryTextSource and StaminaPreviewSource remain independent.

The existing inventory panel has two layouts, sharing native font adoption, target selection, inventory anchors, screen clamping, pause/scene hiding and row pooling. Detailed restores the original RoundedCard mesh from 0.2.5, with a dark translucent fill, pale outline and subtle text shadow. Timing uses a mesh clock; buffs use native lightning or mesh shoe/shield icons.

Cards contain only available facts:
- Item name at the top.
- Plain-language effect descriptions with native status icons: immediate changes, per-second changes and explicit duration labels. Trigger delays and end-of-effect penalties are stated before the action. Redundant immediate drowsiness recovery and steady full clearing are combined without changing numeric facts.
- End-of-effect penalties remain separate from immediate recovery. Additional delay after the effect ends is preserved.
- Structured buff duration; climbing-triggered timers add a short condition.
- Relevant known resource quantities and necessary use conditions.
- Confirmed absent risks, absent side effects, missing durations, developer diagnostics and empty sections are omitted. Unknown food risks are explicitly labeled as unknown.

BuffFact, EffectTrigger and EndDelay are presentation metadata; existing numeric HUD projection inputs are unchanged. No gameplay actions are executed to obtain descriptions. Existing Turkish/Spanish localization changes are preserved and new labels use the same catalog.

Implementation verification: production and verification builds succeed with zero warnings/errors; 138 verifier checks pass, including omission, timeline, localization and real-item prose fixtures. An opt-in title-screen probe checks detailed/minimal switching, background cleanup, exact native glyphs, timing symbols and scales; its runtime result is recorded separately. Final in-world readability and positioning still require gameplay visual acceptance.

Old documentation screenshots show the previous two-row settings panel and should be replaced after gameplay visual acceptance.
