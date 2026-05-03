using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.Shared;

public class NestingDepthValidator : IRuleValidator
{
    public string RuleId => "CQ003";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();

        var depth = 0;
        var maxDepth = 0;

        var lines = content.Split('\n');

        var isSql = filePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase);
        var isVb  = filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase);
        var isJs  = filePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase);

        for (var i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var line = raw.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(line)) continue;

            // --- JS / C-like ---
            if (isJs)
            {
                if (line.Contains("{")) depth++;
                if (line.Contains("}")) depth--;
            }

            // --- SQL ---
            if (isSql)
            {
                if (line.StartsWith("BEGIN")) depth++;
                if (line.StartsWith("END")) depth--;
            }

            // --- VB ---
            if (isVb)
            {
                if (line.StartsWith("IF ") ||
                    line.StartsWith("FOR ") ||
                    line.StartsWith("WHILE "))
                    depth++;

                if (line.StartsWith("END IF") ||
                    line.StartsWith("NEXT") ||
                    line.StartsWith("END WHILE"))
                    depth--;
            }

            if (depth < 0) depth = 0;
            if (depth > maxDepth) maxDepth = depth;
        }

        if (maxDepth > 3)
        {
            violations.Add(new Violation
            {
                RuleId = RuleId,
                Severity = Severity.Warning,
                FilePath = filePath,
                LineNumber = 1,
                Message = $"Nesting depth too high ({maxDepth})."
            });
        }

        return violations;
    }
}