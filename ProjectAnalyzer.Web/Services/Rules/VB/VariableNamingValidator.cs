using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.VB;

using System.Text.RegularExpressions;

public class VariableNamingValidator : IRuleValidator
{
    public string RuleId => "V001";

    private static readonly Regex DimRegex =
        new(@"^\s*Dim\s+(.+)$", RegexOptions.IgnoreCase);

    private static readonly Regex MethodSignatureRegex =
        new(@"\b(Sub|Function)\s+\w+\s*\((.*?)\)", RegexOptions.IgnoreCase);

    private static readonly Regex GlobalRegex =
        new(@"^\s*(Public|Private|Protected|Friend)\s+(\w+)", RegexOptions.IgnoreCase);

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            var line = StripComments(rawLine);

            if (string.IsNullOrWhiteSpace(line)) continue;

            // --- Local (Dim) ---
            var dimMatch = DimRegex.Match(line);
            if (dimMatch.Success)
            {
                var vars = dimMatch.Groups[1].Value.Split(',');

                foreach (var v in vars)
                {
                    var name = ExtractVarName(v);
                    Validate(name, "l", i, filePath, violations);
                }
            }

            // --- Parameters (implicit ByVal supported) ---
            var sigMatch = MethodSignatureRegex.Match(line);
            if (sigMatch.Success)
            {
                var paramList = sigMatch.Groups[2].Value.Split(',');

                foreach (var p in paramList)
                {
                    var cleaned = Regex.Replace(p, @"\b(ByVal|ByRef|Optional)\b",
                        "", RegexOptions.IgnoreCase).Trim();

                    var name = ExtractVarName(cleaned);
                    Validate(name, "p", i, filePath, violations);
                }
            }

            // --- Global / class-level ---
            var globalMatch = GlobalRegex.Match(line);
            if (globalMatch.Success && !line.Contains("Sub") && !line.Contains("Function"))
            {
                var name = globalMatch.Groups[2].Value;
                Validate(name, "g", i, filePath, violations);
            }
        }

        return violations;
    }

    private static string StripComments(string line)
    {
        var idx = line.IndexOf("'");
        return idx >= 0 ? line.Substring(0, idx) : line;
    }

    private static string ExtractVarName(string segment)
    {
        var parts = segment.Trim().Split(' ');
        return parts[0].Replace("()", "").Trim();
    }

    private void Validate(string name, string expectedPrefix, int lineIndex,
        string filePath, List<Violation> violations)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        if (!name.StartsWith(expectedPrefix))
        {
            violations.Add(Create(lineIndex, filePath,
                $"'{name}' must start with '{expectedPrefix}'."));
            return;
        }

        if (!IsCamelCase(name))
        {
            violations.Add(Create(lineIndex, filePath,
                $"'{name}' must be camelCase."));
        }
    }

    private static bool IsCamelCase(string name)
    {
        if (name.Length < 2) return false;

        return char.IsLower(name[0]) &&
               !name.Contains("_") &&
               name.Any(char.IsUpper);
    }

    private Violation Create(int lineIndex, string filePath, string message)
    {
        return new Violation
        {
            RuleId = RuleId,
            Severity = Severity.Warning,
            FilePath = filePath,
            LineNumber = lineIndex + 1,
            Message = message
        };
    }
}