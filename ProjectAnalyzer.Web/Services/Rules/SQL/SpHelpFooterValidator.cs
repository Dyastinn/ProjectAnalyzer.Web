using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.SQL;

public class SpHelpFooterValidator : IRuleValidator
{
    public string RuleId => "SQL004";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();

        if (!content.TrimEnd().EndsWith("SP_HELP", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add(new Violation
            {
                RuleId = RuleId,
                Severity = Severity.Warning,
                FilePath = filePath,
                LineNumber = 1,
                Message = "Missing SP_HELP at end of file."
            });
        }

        return violations;
    }
}