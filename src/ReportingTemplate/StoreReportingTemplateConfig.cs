using Microsoft.EntityFrameworkCore;

namespace Quantum.ReportingTemplate
{
        public class StoreReportingTemplateConfig
        {
            public DbContextOptions<ReportingTemplateDbContext> Options { get; set; }
            public string ApiUrlTemplate { get; set; } = "api";
            public string ApiResourcePath { get; set; } = "reportTemplate";
            public bool Migrate { get; set; } = true;
            public string Application { get; set; }

            public string ToLowerApiBaseAddress() 
                => (ApiUrlTemplate + "/" + ApiResourcePath).ToLower();
        }
}