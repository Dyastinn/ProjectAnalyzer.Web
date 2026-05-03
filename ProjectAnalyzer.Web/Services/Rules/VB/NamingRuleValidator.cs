using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.VB;

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

            if (line.StartsWith("'")) continue;

            if (line.StartsWith("Function ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Sub ", StringComparison.OrdinalIgnoreCase))
            {
                var name = line.Split(' ')[1].Split('(')[0];

                if (!name.StartsWith("fn"))
                {
                    violations.Add(Create(i, filePath, name));
                }
            }
        }

        return violations;
    }

    private Violation Create(int i, string filePath, string name)
    {
        return new Violation
        {
            RuleId = RuleId,
            Severity = Severity.Warning,
            FilePath = filePath,
            LineNumber = i + 1,
            Message = $"Function '{name}' must start with 'fn'."
        };
    }
}