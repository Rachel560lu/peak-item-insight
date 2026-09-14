# Readable detailed descriptions

Detailed mode now uses short action phrases next to native status icons. Continuous changes say what changes each second, with a clock and a duration phrase. Immediate changes say "Immediately"; delayed or end-of-effect changes start with the trigger. Missing effects, durations and side effects remain omitted.

Examples from the audited local PEAK assets:
- Big Lollipop: gain infinite stamina for 8 seconds; timer starts when climbing; immediately reduce hunger by 5; after the effect ends, increase drowsiness by 3.3 each second for 10 seconds.
- Energy Drink: gain a speed boost for 8 seconds; immediately reduce heat by 30; immediately clear drowsiness, then keep clearing it for 8 seconds; after the effect ends, increase drowsiness by 25.
- Heat Pack: reduce cold by 6 each second for 60 seconds.

Repeated immediate recoveries with matching trigger timing are combined. Opposite signs, delayed changes and rates remain distinct. Drowsiness full clearing is described as clearing; other statuses retain their quantities unless explicitly marked Clears because their native caps may exceed 1. A cumulative timed recovery alone is never called clearing.

The formatter changes presentation only; underlying EffectFact values and HUD projection remain unchanged. Existing row pooling, layout caching, 4 Hz prediction budget, item icon, cooking count, background transparency and independent stamina settings are preserved. Status names are part of the sentence, so a missing native icon no longer appends a duplicate status name.

Chinese, English, Turkish and Spanish use the existing localization catalog. Duration and trigger phrasing are translated along with immediate/rate/clear templates.

Verification: production builds without warnings; 138 checks pass, including energy-drink combination, lollipop trigger/rate, heat-pack duration, separate opposite signs and recovery that must not be mislabeled as clearing. Native UI probe additionally checks long prose and exact glyphs in 4 languages at 3 scales, repeated identical layout reuse, absence of duplicate fallback names and style-switch cleanup. Native runtime passed on final PID 43084 after Steam replaced initial PID 42520: SMOKE_DETAILED_PASS, SMOKE_SETTINGS_PASS, SMOKE_SETTINGS_CLICK_PASS and SMOKE_UI_PASS. The trace confirms MVID 944886b0-fe61-4f56-954d-7d57f54ccb36 loaded from the workspace isolation. In-world visual acceptance remains separate.
