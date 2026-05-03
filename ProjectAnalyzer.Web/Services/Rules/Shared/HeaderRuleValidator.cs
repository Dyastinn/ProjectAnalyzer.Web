using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services.Rules.Shared;

public class HeaderRuleValidator : IRuleValidator
{
    public string RuleId => "H001";

    public List<Violation> Validate(string content, string filePath)
    {
        var violations = new List<Violation>();

        var lines = content.Split('\n')
            .Take(50) // limit to header region
            .Select(l => l.Trim())
            .ToList();

        bool hasAuthor = false;
        bool hasDescription = false;
        bool hasDate = false;
        bool hasChangeNumber = false;

        foreach (var line in lines)
        {
            var upper = line.ToUpperInvariant();

            if (!hasAuthor && upper.Contains("AUTHOR"))
                hasAuthor = true;

            if (!hasDescription && upper.Contains("DESCRIPTION"))
                hasDescription = true;

            if (!hasDate && upper.Contains("DATE"))
                hasDate = true;

            if (!hasChangeNumber && upper.Contains("CHANGE NUMBER"))
                hasChangeNumber = true;
        }

        if (!hasAuthor)
            violations.Add(Create(filePath, "Missing Author in header."));

        if (!hasDescription)
            violations.Add(Create(filePath, "Missing Description in header."));

        if (!hasDate)
            violations.Add(Create(filePath, "Missing Date in header."));

        if (!hasChangeNumber)
            violations.Add(Create(filePath, "Missing Change Number in header."));

        return violations;
    }

    private Violation Create(string filePath, string message)
    {
        return new Violation
        {
            RuleId = RuleId,
            Severity = Severity.Warning,
            FilePath = filePath,
            LineNumber = 1,
            Message = message
        };
    }
}