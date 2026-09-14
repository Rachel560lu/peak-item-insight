using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PeakItemInsight.Core;

// English templates are stable catalog keys. Embedded files keep installation
// dependency-free and allow new languages without changing effect logic.
internal static class LanguageCatalog
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalogs = Load();
    internal static string Resolve(string? configured, string gameLanguage)
    {
        var language = string.IsNullOrEmpty(configured) || configured == "Auto" ? gameLanguage : configured;
        switch (language)
        {
            case "Chinese": case "SimplifiedChinese": case "TraditionalChinese": return "Chinese";
            case "Turkish": return "Turkish";
            case "Spanish": case "SpanishSpain": case "SpanishLatam": return "Spanish";
            default: return "English";
        }
    }
    internal static string Text(string language, string english, string? chinese = null)
    {
        if (language == "Chinese") return chinese ?? english;
        return Catalogs.TryGetValue(language, out var catalog) && catalog.TryGetValue(english, out var text) ? text : english;
    }
    internal static string Format(string language, FormattableString chinese, FormattableString english)
    {
        if (language == "Chinese") return chinese.ToString(CultureInfo.CurrentCulture);
        return string.Format(CultureInfo.CurrentCulture, Text(language, english.Format), english.GetArguments());
    }
    private static Dictionary<string, Dictionary<string, string>> Load()
    {
        var catalogs = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var assembly = typeof(LanguageCatalog).Assembly;
        foreach (var language in new[] { "Turkish", "Spanish" })
        {
            var catalog = new Dictionary<string, string>(StringComparer.Ordinal);
            using (var stream = assembly.GetManifestResourceStream("PeakItemInsight.Localization." + language + ".tsv"))
            {
                if (stream != null)
                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                        var split = line.IndexOf('\t');
                        if (split <= 0 || split == line.Length - 1) continue;
                        catalog.Add(line.Substring(0, split), line.Substring(split + 1));
                    }
                }
            }
            catalogs.Add(language, catalog);
        }
        return catalogs;
    }
}
