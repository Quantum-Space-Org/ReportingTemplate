using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Quantum.Core;

namespace Quantum.ReportingTemplate;

public static class ReportingTemplateMiddleware
{
    private static bool _migrated;
    private static ReportingTemplateDbContext _reportingTemplateDbContext;

    public static async Task UseStoreReportingTemplate(this IApplicationBuilder app
        , StoreReportingTemplateConfig config)
    {
        _reportingTemplateDbContext = new ReportingTemplateDbContext(config.Options);

        IReportTemplateService reportTemplate = new ReportTemplateService(_reportingTemplateDbContext);
        await Migrate(config);

        app.Use(async (context, next) =>
        {
            if (context.Request.Method.ToUpper() == "GET" &&
                context.Request.Path.Value.ToLower().EndsWith(config.ToLowerApiBaseAddress()))
            {
                await GetAll(config, context, reportTemplate);
            }
            else if (context.Request.Method.ToUpper() == "POST" && context.Request.Path.Value
                         .ToLower()
                         .StartsWith("/" + config.ToLowerApiBaseAddress()))
            {
                await Store(config, context, reportTemplate);
            }
            else if (context.Request.Method.ToUpper() == "DELETE"
                     && context.Request.Path.Value.ToLower()
                    .StartsWith("/" + config.ToLowerApiBaseAddress()))
            {
                await Delete(config, context, reportTemplate);
            }
            else
            {
                await next();
            }
        });
    }

    private static async Task Delete(StoreReportingTemplateConfig config, HttpContext context,
        IReportTemplateService reportTemplateService)
    {
        var routeData = context.Request.Path;
        var lastIndexOf = routeData.Value.LastIndexOf("/") + 1;

        var substring = routeData.Value[lastIndexOf..];

        var id = long.Parse(substring);

        await reportTemplateService.Delete(config.Application, id);

        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
    }


    private static async Task Store(StoreReportingTemplateConfig storeReportingTemplateConfig, HttpContext context,
        IReportTemplateService reportTemplateService)
    {
        var storeTemplate = await StoreTemplate();

        storeTemplate.Application = storeReportingTemplateConfig.Application;

        await reportTemplateService.Store(storeTemplate);

        context.Response.StatusCode = 200;

        context.Response.ContentType = "application/json";

        async Task<ReportTemplate> StoreTemplate()
        {
            var jsonString = string.Empty;

            using (var inputStream = new StreamReader(context.Request.Body))
                jsonString = await inputStream.ReadToEndAsync();

            var storeTemplate = jsonString.Deserialize<ReportTemplate>();
            return storeTemplate;
        }
    }


    private static async Task GetAll(StoreReportingTemplateConfig config, HttpContext context,
        IReportTemplateService reportTemplate)
    {
        context.Response.StatusCode = 200;

        context.Response.ContentType = "application/json";

        var reportName = context.Request.Query["reportName"];

        var storeTemplateViewModels = await reportTemplate.GetAll(config.Application, reportName);

        var jsonString = JsonConvert.SerializeObject(storeTemplateViewModels
                                                                                , new JsonSerializerSettings
                                                                                {
                                                                                    ContractResolver = new DefaultContractResolver
                                                                                    {
                                                                                        NamingStrategy = new CamelCaseNamingStrategy()
                                                                                    },
                                                                                    Formatting = Formatting.Indented
                                                                                });

        await context.Response.WriteAsync(jsonString, Encoding.UTF8);
    }

    private static async Task Migrate(StoreReportingTemplateConfig reportTemplate)
    {
        if (reportTemplate.Migrate is false)
            return;

        if (_migrated is true)
            return;

        await _reportingTemplateDbContext.Database.EnsureCreatedAsync();
        await _reportingTemplateDbContext.Database.MigrateAsync();
        _migrated = true;
    }

}