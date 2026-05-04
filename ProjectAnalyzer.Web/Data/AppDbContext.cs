using Microsoft.EntityFrameworkCore;
using ProjectAnalyzer.Web.Models;

namespace ProjectAnalyzer.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AnalysisResult> AnalysisResults { get; set; }
    public DbSet<Violation> Violations { get; set; }
    public DbSet<RuleConfig> RuleConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // RuleId is a string primary key, not an auto-increment int
        modelBuilder.Entity<RuleConfig>().HasKey(r => r.RuleId);
    }
}