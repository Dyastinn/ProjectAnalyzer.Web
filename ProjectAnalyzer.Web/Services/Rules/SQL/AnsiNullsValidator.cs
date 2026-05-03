using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.SQL;

public class AnsiNullsValidator : IRuleValidator
{
    public string RuleId => "SQL003";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();

        if (!content.Contains("SET ANSI_NULLS ON", StringComparison.OrdinalIgnoreCase) ||
            !content.Contains("SET QUOTED_IDENTIFIER ON", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add(new Violation
            {
                RuleId = RuleId,
                Severity = Severity.Error,
                FilePath = filePath,
                LineNumber = 1,
                Message = "Missing SET ANSI_NULLS ON or SET QUOTED_IDENTIFIER ON."
            });
        }

        return violations;
    }
}