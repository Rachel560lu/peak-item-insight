# Turkish and Spanish localization

Mod labels, effects, tool instructions and settings use embedded UTF-8 catalogs:
src/Localization/Turkish.tsv and src/Localization/Spanish.tsv.
Each non-comment line is an English source template, a tab, and its translation.
English source templates are stable keys; keep them unchanged or update both catalogs.
Dynamic strings use composite placeholders such as {0:0.#}. Never translate or
remove placeholder indices or number formats. Runtime interpolation occurs after
translation, so durations remain correct.

Native item names, descriptions and interaction prompts reuse PEAK localization.
Auto follows Turkish and both Spanish variants. Explicit Spanish uses SpanishSpain;
Auto preserves SpanishLatam native text. Other unsupported mod languages fall back
to English, while native text continues to follow the game. Existing Chinese and
English config values remain valid. Language changes invalidate cached previews.

No translation service, runtime network call, or extra library is required.
Catalogs ship inside the DLL. Missing entries fall back to English. Settings labels
can shrink to 16 points to accommodate longer translations, and the UI checks the
native font fallback chain for the new languages' accented letters.

## Verification

Run scripts/verify.ps1 for the production build and offline checks covering
language routing, catalogs, fallback, numeric templates, and unknown risk.
The in-game settings glyph test now covers all four languages. The minimal UI
font/geometry test also exercises all four languages with localized resource labels.

Before release, run the in-game smoke tests and visually inspect both new languages:
pause entry, settings, clear/present/unknown poison rows, temporary buffs, Piton
and Remedy Fungus instructions, hover/held previews and accented characters.
Have native speakers review terminology. Offline checks cannot establish font
availability or actual screen layout inside PEAK.

Local verification: production build passed with zero warnings/errors; offline suite passed all checks. Source audit confirms 79 production templates in each catalog, with matching placeholder indices and formats. In-game checks have been expanded but have not been run for this change.
