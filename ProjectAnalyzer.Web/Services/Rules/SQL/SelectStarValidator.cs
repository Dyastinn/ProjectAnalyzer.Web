using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.SQL;

public class SelectStarValidator : IRuleValidator
{
    public string RuleId => "SQL001";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Always check for suppression comment first
            if (line.Contains("-- noqa: SQL001")) continue;

            if (line.TrimStart().StartsWith("SELECT *", StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Error,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = "Avoid SELECT *. List your columns explicitly."
                });
            }
        }
        return violations;
    }
}