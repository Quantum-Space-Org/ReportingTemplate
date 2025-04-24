using Microsoft.EntityFrameworkCore;

namespace Quantum.ReportingTemplate;

public class ReportingTemplateDbContext : DbContext
{
    public DbSet<ReportTemplate> ReportTemplates { get; set; }
    public ReportingTemplateDbContext(DbContextOptions<ReportingTemplateDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportTemplate>()
            .Property(a => a.Tags)
            .HasConversion(e => Newtonsoft.Json.JsonConvert.SerializeObject(e),
                e => string.IsNullOrWhiteSpace(e)
                                                            ? new string[] { }
                                                            : Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(e));

        base.OnModelCreating(modelBuilder);
    }
}