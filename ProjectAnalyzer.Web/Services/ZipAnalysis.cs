using System.IO.Compression;
using ProjectAnalyzer.Web.Data;
using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services.Interfaces;

namespace ProjectAnalyzer.Web.Services;

public class ZipAnalysisService(IConfiguration config, AnalysisRepository repo, List<IFileAnalyzer> analyzers)
{
    public async Task<AnalysisResult> RunAsync(IFormFile zipFile)
    {
        var excluded = config.GetSection("ExcludedPatterns").Get<string[]>() ?? [];
        var enabledRules = await repo.GetAllRulesAsync();

        var result = new AnalysisResult
        {
            BranchUrl = $"ZIP upload: {zipFile.FileName}",
            RunAt = DateTime.UtcNow
        };

        // Copy upload into memory so we can read it as a ZIP
        using var memStream = new MemoryStream();
        await zipFile.CopyToAsync(memStream);
        memStream.Position = 0;

        await using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            // Skip folder entries — they have no extension
            if (string.IsNullOrEmpty(entry.Name)) continue;

            var filePath = entry.FullName;
            var ext = Path.GetExtension(filePath).ToLower();

            if (excluded.Any(p => filePath.EndsWith(p.TrimStart('*')))) continue;

            var analyzer = analyzers.FirstOrDefault(a =>
                a.SupportedExtensions.Contains(ext) && a.CanAnalyze(filePath));
            if (analyzer == null) continue;

            using var reader = new StreamReader(entry.Open());
            var content = await reader.ReadToEndAsync();

            result.Violations.AddRange(analyzer.Analyze(filePath, content, enabledRules));
        }

        await repo.SaveResultAsync(result);
        return result;
    }
}