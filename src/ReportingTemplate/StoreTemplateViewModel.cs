namespace Quantum.ReportingTemplate;

public class StoreTemplateViewModel
{
    public long Id { get; set; } = Snowflake.SnowflakeIdGenerator.New().ToLong();
    public string Application { get; set; }
    public string Name { get; set; }
    public string ReportName { get; set; }
    public string Payload { get; set; }
    public string[] Tags { get; set; }
}