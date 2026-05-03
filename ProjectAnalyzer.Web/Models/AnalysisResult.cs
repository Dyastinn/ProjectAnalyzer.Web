namespace ProjectAnalyzer.Web.Models;

public class AnalysisResult
{
    public int Id { get; set; }
    public string BranchUrl {get; set;}
    public DateTime RunAt { get; set; } = DateTime.UtcNow;
    public List<Violation> Violations { get; set; } = new();
}

