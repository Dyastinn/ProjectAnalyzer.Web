namespace ProjectAnalyzer.Web.Models;

public class Violation
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string RuleId { get; set; } = string.Empty;
    public Severity Severity { get; set; }    
    public int LineNumber { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuppressed { get; set; }        // true if line has -- noqa: RULEID
    public int AnalysisResultId { get; set; }
}