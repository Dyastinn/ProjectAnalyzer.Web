using Microsoft.EntityFrameworkCore;
using ProjectAnalyzer.Web.Models;

namespace ProjectAnalyzer.Web.Data;

public class AnalysisRepository(AppDbContext db)
{
    public async Task SaveResultAsync(AnalysisResult result)
    {
        db.AnalysisResults.Add(result);
        await db.SaveChangesAsync();
    }

    public async Task<List<AnalysisResult>> GetRecentResultsAsync(int count = 10)
        => await db.AnalysisResults
            .OrderByDescending(r => r.RunAt)
            .Take(count)
            .ToListAsync();

    public async Task<AnalysisResult?> GetResultByIdAsync(int id)
        => await db.AnalysisResults
            .Include(r => r.Violations)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<RuleConfig>> GetAllRulesAsync()
        => await db.RuleConfigs.ToListAsync();

    public async Task UpdateRuleAsync(RuleConfig rule)
    {
        db.RuleConfigs.Update(rule);
        await db.SaveChangesAsync();
    }
}