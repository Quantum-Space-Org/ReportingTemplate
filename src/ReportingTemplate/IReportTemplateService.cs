using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quantum.ReportingTemplate;

public interface IReportTemplateService
{
    Task Store(ReportTemplate reportTemplate);
    Task<List<StoreTemplateViewModel>> GetAll(string application, string reportName);
    Task Delete(string application, long id);
    Task<StoreTemplateViewModel> Get(string accounting, long id);
}