using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Quantum.ReportingTemplate;

public class ReportTemplateService : IReportTemplateService
{
    private readonly ReportingTemplateDbContext _dbContext;

    public ReportTemplateService(ReportingTemplateDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Store(ReportTemplate reportTemplate)
    {
        await GuardAgainstDuplication(reportTemplate);
        await _dbContext.ReportTemplates.AddAsync(reportTemplate);
        await _dbContext.SaveChangesAsync();
    }

    private async Task GuardAgainstDuplication(ReportTemplate reportTemplate)
    {
        var template = await _dbContext.ReportTemplates
            .FirstOrDefaultAsync(a => a.Application == reportTemplate.Application
                                      && a.ReportName == reportTemplate.ReportName
                                      && a.Name == reportTemplate.ReportName);

        if (template is not null)
            throw new StoreTemplateNameIsDuplicatedException();
    }

    public async Task<List<StoreTemplateViewModel>> GetAll(string application, string reportName)
    {
        return await _dbContext.ReportTemplates
            .Where(a => a.Application == application && a.ReportName == reportName)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new StoreTemplateViewModel
            {
                Name = a.Name,
                ReportName = a.ReportName,
                Tags = a.Tags,
                Application = a.Application,
                Payload = a.Payload,
                Id = a.Id,
            })

            .ToListAsync();
    }

    public async Task Delete(string application, long id)
    {
        var reportTemplate = await Get(id);
        if (reportTemplate is not null)
        {
            _dbContext.ReportTemplates.Remove(reportTemplate);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<StoreTemplateViewModel> Get(string accounting, long id)
    {
        var a = await Get(id);
        if (a is null)
            return null;

        return new StoreTemplateViewModel
        {
            Name = a.Name,
            ReportName = a.ReportName,
            Tags = a.Tags,
            Application = a.Application,
            Payload = a.Payload,
            Id = a.Id,
        };
    }

    private async Task<ReportTemplate> Get(long id)
    {
        return await _dbContext.ReportTemplates.FindAsync(id);
    }
}

public class StoreTemplateNameIsDuplicatedException : Exception
{
}