using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Quantum.ReportingTemplate.Tests;

public class Should
{
    [Fact]
    public async Task successfullySaved()
    {
        var myDbContext = await ReportingTemplateDbContext();

        IReportTemplateService service = new ReportTemplateService(myDbContext);

        var storeTemplate = new ReportTemplate
        {
            Application = "Accounting",
            Name = "گزارش",
            ReportName = "Aging",
            Payload = "payload",
            Tags = new[] { "report" }
        };

        await service.Store(storeTemplate);

        var results = await service.GetAll("Accounting", reportName: storeTemplate.ReportName);
        
        Assert.Equal(1, results.Count);
    }

    [Fact]
    public async Task checkDuplicateName()
    {
        var myDbContext = await ReportingTemplateDbContext();

        IReportTemplateService service = new ReportTemplateService(myDbContext);

        var storeTemplate = new ReportTemplate
        {
            Application = "Accounting",
            Name = "گزارش",
            ReportName = "Aging",
            Payload = "payload",
            Tags = new string[] { "report" }
        };

        await service.Store(storeTemplate);

        Func<Task> action = async () => await service.Store(storeTemplate);

        await Assert.ThrowsAsync<StoreTemplateNameIsDuplicatedException>(action);
    }

    [Fact]
    public async Task deletedSuccessfully()
    {
        var myDbContext = await ReportingTemplateDbContext();

        IReportTemplateService service = new ReportTemplateService(myDbContext);

        var storeTemplate = new ReportTemplate
        {
            Application = "Accounting",
            Name = "گزارش",
            ReportName = "Aging",
            Payload = "payload",
            Tags = new[] { "report" }
        };

        await service.Store(storeTemplate);

        await service.Delete("Accounting", storeTemplate.Id);
        var results = await service.GetAll("Accounting", reportName: storeTemplate.ReportName);

        Assert.Equal(0, results.Count);
    }

    [Fact]
    public async Task getById()
    {
        var myDbContext = await ReportingTemplateDbContext();

        IReportTemplateService service = new ReportTemplateService(myDbContext);

        var storeTemplate = new ReportTemplate
        {
            Application = "Accounting",
            Name = "گزارش",
            ReportName = "Aging",
            Payload = "payload",
            Tags = new string[] { "report" }
        };

        await service.Store(storeTemplate);

        var result = await service.Get("Accounting", storeTemplate.Id);

        Assert.NotNull(result);
    }


    private static async Task<ReportingTemplateDbContext> ReportingTemplateDbContext()
    {
        var sqliteConnection = new SqliteConnection("Filename=:memory:");
        sqliteConnection.Open();

        var options = new DbContextOptionsBuilder<ReportingTemplateDbContext>()
            .UseSqlite(sqliteConnection).Options;

        var myDbContext = new ReportingTemplateDbContext(options);
        await myDbContext.Database.EnsureCreatedAsync();
        await myDbContext.Database.MigrateAsync();
        return myDbContext;
    }

}