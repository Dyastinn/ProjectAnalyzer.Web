using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.Shared;

public class DeadCodeValidator : IRuleValidator
{
    public string RuleId => "CQ001";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        int blockSize = 0;
        int threshold = 10;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            bool isComment =
                line.StartsWith("//") ||
                line.StartsWith("'") ||
                line.StartsWith("--") ||
                line.StartsWith("/*") ||
                line.StartsWith("*");

            if (isComment)
            {
                blockSize++;
            }
            else
            {
                if (blockSize >= threshold)
                {
                    violations.Add(new Violation
                    {
                        RuleId = RuleId,
                        Severity = Severity.Warning,
                        FilePath = filePath,
                        LineNumber = i - blockSize + 1,
                        Message = $"Large commented-out block ({blockSize} lines)."
                    });
                }

                blockSize = 0;
            }
        }

        return violations;
    }
}