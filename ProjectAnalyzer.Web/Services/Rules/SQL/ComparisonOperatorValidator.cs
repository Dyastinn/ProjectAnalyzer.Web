using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.SQL;

public class ComparisonOperatorValidator : IRuleValidator
{
    public string RuleId => "SQL002";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Contains("-- noqa: SQL002")) continue;

            if (line.Contains("<>"))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Error,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = "Use != instead of <>."
                });
            }
        }

        return violations;
    }
}