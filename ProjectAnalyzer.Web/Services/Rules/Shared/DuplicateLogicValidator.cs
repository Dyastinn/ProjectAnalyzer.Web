using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.Shared;

public class DuplicateLogicValidator : IRuleValidator
{
    public string RuleId => "CQ002";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        const int window = 3;
        var seen = new Dictionary<string, int>();

        for (int i = 0; i <= lines.Length - window; i++)
        {
            var block = string.Join("\n",
                lines.Skip(i).Take(window)
                    .Select(l => Normalize(l)));

            if (block.Length < 20) continue;

            if (seen.ContainsKey(block))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Error,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = $"Duplicate logic block (first seen at line {seen[block]})."
                });
            }
            else
            {
                seen[block] = i + 1;
            }
        }

        return violations;
    }

    private string Normalize(string line)
    {
        return System.Text.RegularExpressions.Regex
            .Replace(line, @"\s+", "")
            .ToLowerInvariant();
    }
}