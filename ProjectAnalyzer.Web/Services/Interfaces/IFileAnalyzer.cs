using ProjectAnalyzer.Web.Models;

namespace ProjectAnalyzer.Web.Services.Interfaces;

public interface IFileAnalyzer
{
    IReadOnlyCollection<string> SupportedExtensions { get; }   // e.g. [".sql", ".vb"]     
    bool CanAnalyze(string filePath);        // false for *.Designer.vb
    List<Violation> Analyze(string filePath, string fileContent, List<RuleConfig> enabledRules);
}
