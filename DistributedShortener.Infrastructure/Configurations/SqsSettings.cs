namespace DistributedShortener.Infrastructure.Configurations;

public class SqsSettings
{
    public const string SectionName = "Sqs";

    public required string ServiceUrl { get; set; }
    public required string QueueName { get; set; }
    public required string Region { get; set; }
}