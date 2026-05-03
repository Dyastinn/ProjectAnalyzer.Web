using ProjectAnalyzer.Web.Models;

namespace ProjectAnalyzer.Web.Services.Interfaces;

public interface IRuleValidator
{
    string RuleId { get; }                   // "SQL001", "H001", etc.
    List<Violation> Validate(string content, string filePath);
}