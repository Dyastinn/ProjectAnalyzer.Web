using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;
using ProjectAnalyzer.Web.Services.Rules.Shared;
using ProjectAnalyzer.Web.Services.Rules.SQL;

namespace ProjectAnalyzer.Web.Services.Analyzers;

public class SqlFileAnalyzer : IFileAnalyzer
{
    private readonly List<IRuleValidator> _validators =
    [
        new AnsiNullsValidator(),
        new ComparisonOperatorValidator(),
        new SelectStarValidator(),
        new SpHelpFooterValidator(),
        
        new HeaderRuleValidator(),
        new HardcodingValidator(),
        new DeadCodeValidator(),
        new DuplicateLogicValidator(),
        new NestingDepthValidator()
    ];

    public IReadOnlyCollection<string> SupportedExtensions { get; } =
        [".sql"];

    public bool CanAnalyze(string filePath)
    {
        var lExt = Path.GetExtension(filePath);
        return SupportedExtensions.Contains(lExt, StringComparer.OrdinalIgnoreCase);
    }

    public List<Violation> Analyze(string filePath, string content, List<RuleConfig> enabledRules)
    {
        var lViolations = new List<Violation>();

        foreach (var lValidator in _validators)
        {
            var lConfig = enabledRules.FirstOrDefault(r => r.RuleId == lValidator.RuleId);
            if (lConfig is not { IsEnabled: true }) continue;

            lViolations.AddRange(lValidator.Validate(content, filePath));
        }

        return lViolations;
    }
}