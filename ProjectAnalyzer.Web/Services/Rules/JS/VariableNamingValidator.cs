using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.JS;

using System.Text.RegularExpressions;

public class VariableNamingValidator : IRuleValidator
{
    public string RuleId => "V001";

    private static readonly Regex JsVarRegex =
        new(@"^\s*(let|const|var)\s+([^=;]+)", RegexOptions.IgnoreCase);

    private static readonly Regex JsFunctionRegex =
        new(@"function\s+\w+\s*\((.*?)\)", RegexOptions.IgnoreCase);

    private static readonly Regex JsArrowRegex =
        new(@"\((.*?)\)\s*=>", RegexOptions.IgnoreCase);

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();
        var lines = content.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            var line = StripComments(rawLine);

            if (string.IsNullOrWhiteSpace(line)) continue;

            // --- Local (let/const/var) ---
            var varMatch = JsVarRegex.Match(line);
            if (varMatch.Success)
            {
                var vars = varMatch.Groups[2].Value.Split(',');

                foreach (var v in vars)
                {
                    var name = v.Split('=')[0].Trim();
                    ValidateName(name, "l", i, filePath, violations);
                }
            }

            // --- Function parameters ---
            var fnMatch = JsFunctionRegex.Match(line);
            if (fnMatch.Success)
            {
                var paramList = fnMatch.Groups[1].Value.Split(',');

                foreach (var p in paramList)
                {
                    var name = p.Trim();
                    ValidateName(name, "p", i, filePath, violations);
                }
            }

            // --- Arrow function parameters ---
            var arrowMatch = JsArrowRegex.Match(line);
            if (arrowMatch.Success)
            {
                var paramList = arrowMatch.Groups[1].Value.Split(',');

                foreach (var p in paramList)
                {
                    var name = p.Trim();
                    ValidateName(name, "p", i, filePath, violations);
                }
            }

            // --- Global (heuristic: no indentation) ---
            if (!rawLine.StartsWith(" ") && JsVarRegex.IsMatch(line))
            {
                var name = JsVarRegex.Match(line).Groups[2].Value
                    .Split('=')[0].Trim();

                ValidateName(name, "g", i, filePath, violations);
            }
        }

        return violations;
    }

    private static string StripComments(string line)
    {
        var idx = line.IndexOf("//");
        return idx >= 0 ? line.Substring(0, idx) : line;
    }

    private void ValidateName(string name, string expectedPrefix, int lineIndex,
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