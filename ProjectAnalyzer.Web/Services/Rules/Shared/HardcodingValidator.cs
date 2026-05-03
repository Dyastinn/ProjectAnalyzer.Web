using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.Shared;

public class HardcodingValidator : IRuleValidator
{
    public string RuleId => "HC001";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Contains("// noqa: HC001") || line.Contains("-- noqa: HC001")) continue;

            // Connection strings
            if (line.Contains("Server=") || line.Contains("Data Source="))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Error,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = "Hardcoded connection string."
                });
            }

            // Credentials
            if (line.Contains("Password=") || line.Contains("User ID="))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Error,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = "Hardcoded credentials."
                });
            }

            // File paths
            if (line.Contains("C:\\") || line.Contains("/var/") || line.Contains("/home/"))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Warning,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = "Hardcoded file path."
                });
            }

            // Magic numbers (basic heuristic)
            if (System.Text.RegularExpressions.Regex.IsMatch(line, @"\b\d{2,}\b") &&
                !line.Contains("const") &&
                !line.Contains("enum"))
            {
                violations.Add(new Violation
                {
                    RuleId = RuleId,
                    Severity = Severity.Warning,
                    FilePath = filePath,
                    LineNumber = i + 1,
                    Message = "Possible magic number."
                });
            }
        }

        return violations;
    }
}