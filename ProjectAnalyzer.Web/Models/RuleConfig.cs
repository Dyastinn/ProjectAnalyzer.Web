namespace ProjectAnalyzer.Web.Models;

public class RuleConfig
{
    public string RuleId { get; set; } = "";      // primary key e.g. "SQL001"
    public string Description { get; set; } = "";
    public string Severity { get; set; } = "";
    public bool IsEnabled { get; set; }
}