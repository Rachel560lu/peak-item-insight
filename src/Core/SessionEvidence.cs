using System;
using System.Linq;

namespace PeakItemInsight.Core;

internal static class SessionEvidence
{
    public static bool IsReady(string[] lines, int pid, string version)
    {
        var owned = lines.Where(line => line.Contains($" pid={pid} ")).ToArray();
        return owned.Any(l => l.Contains($" START version={version} "))
            && owned.Any(l => l.Contains(" FIRST_FRAME "))
            && owned.Count(l => l.Contains(" HEARTBEAT ")) >= 2
            && owned.Any(l => l.Contains(" SMOKE_UI_PASS "))
            && !owned.Any(l => l.Contains(" ERROR ") || l.Contains(" PLUGIN_DESTROY "));
    }
}
