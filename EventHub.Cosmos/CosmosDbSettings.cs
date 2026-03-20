namespace EventHub.Cosmos;

public class CosmosDbSettings
{
    public const string SectionName = "CosmosDb";

    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = "EventHub";

    public string ContainerName { get; set; } = "Events";

    /// <summary>Partition key path (e.g. /Id for property Id).</summary>
    public string PartitionKeyPath { get; set; } = "/Id";

    /// <summary>When true, bypass TLS validation (local Linux emulator HTTPS only).</summary>
    public bool DisableServerCertificateValidation { get; set; }
}
