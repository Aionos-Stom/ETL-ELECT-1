namespace ProcesoETL.Core.Configuration;

public class DataSourceSettings
{
    public string CsvPath { get; set; } = string.Empty;
    public CsvFileSettings CsvFiles { get; set; } = new();
    public string ApiBaseUrl { get; set; } = string.Empty;
    public ApiEndpointSettings ApiEndpoints { get; set; } = new();
    public string ApiKey { get; set; } = string.Empty;
}

public class CsvFileSettings
{
    public string Customers { get; set; } = string.Empty;
    public string Products { get; set; } = string.Empty;
    public string Orders { get; set; } = string.Empty;
    public string OrderDetails { get; set; } = string.Empty;
}

public class ApiEndpointSettings
{
    public string Comments { get; set; } = string.Empty;
    public string Reviews { get; set; } = string.Empty;
}

public class ETLSettings
{
    public int RunIntervalMinutes { get; set; } = 60;
    public bool EnableParallelProcessing { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
    public string StagingPath { get; set; } = "staging";
}
