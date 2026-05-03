using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.JS;

public class NamingRuleValidator : IRuleValidator
{
    public string RuleId => "N001";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith("//")) continue;

            string? name = null;

            // function declaration
            if (line.StartsWith("function "))
            {
                name = line.Split(' ')[1].Split('(')[0];
            }

            // const fn = () =>
            if (line.Contains("=>") && line.Contains("="))
            {
                name = line.Split('=')[0].Replace("const", "")
                    .Replace("let", "")
                    .Replace("var", "")
                    .Trim();
            }

            if (!string.IsNullOrEmpty(name) && !name.StartsWith("fn"))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Warning,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = $"Function '{name}' must start with 'fn'."
                });
            }
        }

        return violations;
    }
}