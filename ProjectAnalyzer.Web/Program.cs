using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectAnalyzer.Web.Data;
using ProjectAnalyzer.Web.Models;
using ProjectAnalyzer.Web.Services;
using ProjectAnalyzer.Web.Services.Analyzers;
using ProjectAnalyzer.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- DB: try Postgres first, fall back to MSSQL ---
var pgConn = builder.Configuration.GetConnectionString("Postgres");
var msConn = builder.Configuration.GetConnectionString("MsSql");
bool usePostgres = false;

if (!string.IsNullOrEmpty(pgConn))
{
    try
    {
        var lBuilder = new NpgsqlConnectionStringBuilder(pgConn);
        var lDbName = lBuilder.Database;

        lBuilder.Database = lDbName; // connect to default system DB

        using var conn = new NpgsqlConnection(lBuilder.ToString());
        conn.Open();

        usePostgres = true;
    }
    catch { }
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (usePostgres) options.UseNpgsql(pgConn);
    else             options.UseSqlServer(msConn);
});

// --- Allow large ZIP uploads (100 MB max) ---
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 100 * 1024 * 1024;
});

// --- Build the shared list of file analyzers ---
var analyzers = new List<IFileAnalyzer>
{
    new SqlFileAnalyzer(),
    new VbFileAnalyzer(),
    new JsFileAnalyzer()
};

// --- Register services ---
builder.Services.AddScoped<AnalysisRepository>();

builder.Services.AddScoped<AnalysisService>(sp =>
    new AnalysisService(
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<AnalysisRepository>(),
        analyzers
    ));

builder.Services.AddScoped<ZipAnalysisService>(sp =>
    new ZipAnalysisService(
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<AnalysisRepository>(),
        analyzers
    ));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Seed the rules table on first run ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // if (!db.RuleConfigs.Any())
    // {
    //     var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    //     foreach (var lRule in cfg.GetSection("RuleSettings").GetChildren())
    //     {
    //         if (!db.RuleConfigs.Any(r => r.RuleId == lRule.Key))
    //         {
    //             db.RuleConfigs.Add(new RuleConfig
    //             {
    //                 RuleId      = lRule.Key,
    //                 IsEnabled   = lRule.GetValue<bool>("Enabled"),
    //                 Severity    = lRule.GetValue<string>("Severity") ?? "WARNING",
    //                 Description = lRule.Key
    //             });
    //         }
    //     }
    //     db.SaveChanges();
    // }
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();