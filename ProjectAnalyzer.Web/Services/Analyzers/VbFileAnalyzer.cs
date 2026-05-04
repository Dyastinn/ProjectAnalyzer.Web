using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;
using ProjectAnalyzer.Web.Services.Rules.Shared;
using ProjectAnalyzer.Web.Services.Rules.VB;

namespace ProjectAnalyzer.Web.Services.Analyzers;

public class VbFileAnalyzer : IFileAnalyzer
{
    private readonly List<IRuleValidator> _validators =
    [
        new HeaderRuleValidator(),
        new NamingRuleValidator(),
        new VariableNamingValidator(),
        new HardcodingValidator(),
        new DeadCodeValidator(),
        new DuplicateLogicValidator(),
        new NestingDepthValidator()
    ];

    public IReadOnlyCollection<string> SupportedExtensions { get; } = [".vb"];

    public bool CanAnalyze(string filePath)
        => !filePath.EndsWith(".Designer.vb", StringComparison.OrdinalIgnoreCase);

    public List<Violation> Analyze(string filePath, string content, List<RuleConfig> enabledRules)
    {
        var violations = new List<Violation>();
        foreach (var validator in _validators)
        {
            var config = enabledRules.FirstOrDefault(r => r.RuleId == validator.RuleId);
            if (config == null || !config.IsEnabled) continue;
            violations.AddRange(validator.Validate(content, filePath));
        }
        return violations;
    }
}