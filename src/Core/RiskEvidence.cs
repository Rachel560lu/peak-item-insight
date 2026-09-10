namespace PeakItemInsight.Core;

internal enum RiskLevel { Unknown, Absent, Present }

internal static class RiskEvidence
{
    // Raw harmful facts outrank caps, recovery and incomplete coverage.
    public static RiskLevel Assess(bool harmfulFact, bool complete, bool prefabOnly) =>
        harmfulFact ? RiskLevel.Present : complete && !prefabOnly ? RiskLevel.Absent : RiskLevel.Unknown;
}
