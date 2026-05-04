using ProjectAnalyzer.Web.Data;
using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services;

public class AnalysisService(IConfiguration config, AnalysisRepository repo, List<IFileAnalyzer> analyzers)
{
    public async Task<AnalysisResult> RunAsync(string devOpsUrl)
    {
        var pat = config["DevOps:Pat"];
        var token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Authorization", $"Basic {token}");
        http.DefaultRequestHeaders.Add("Accept", "application/json");

        // Step A: Get the list of changed files from the branch diff
        var diffUrl = BuildDiffUrl(devOpsUrl);
        var diffJson = await http.GetStringAsync(diffUrl);
        var changedFiles = ParseChangedFiles(diffJson);

        // Step B: Load which rules are enabled
        var enabledRules = await repo.GetAllRulesAsync();
        var excluded = config.GetSection("ExcludedPatterns").Get<string[]>() ?? [];

        var result = new AnalysisResult
        {
            BranchUrl = devOpsUrl,
            RunAt = DateTime.UtcNow
        };

        // Step C: For each changed file, fetch its content and analyze
        foreach (var filePath in changedFiles)
        {
            if (excluded.Any(p => filePath.EndsWith(p.TrimStart('*')))) continue;

            var ext = Path.GetExtension(filePath).ToLower();
            var analyzer = analyzers.FirstOrDefault(a =>
                a.SupportedExtensions.Contains(ext) && a.CanAnalyze(filePath));
            if (analyzer == null) continue;

            var contentUrl = BuildContentUrl(devOpsUrl, filePath);
            var content = await http.GetStringAsync(contentUrl);

            result.Violations.AddRange(analyzer.Analyze(filePath, content, enabledRules));
        }

        await repo.SaveResultAsync(result);
        return result;
    }

    // --- Fill these in when you have your DevOps URL to test with ---
    // Come back to me and paste your URL — I'll write these for you
    private string BuildDiffUrl(string devOpsUrl) => "";
    private List<string> ParseChangedFiles(string json) => new();
    private string BuildContentUrl(string devOpsUrl, string filePath) => "";
}