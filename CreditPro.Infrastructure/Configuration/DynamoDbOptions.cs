namespace CreditPro.Infrastructure.Configuration;

public class DynamoDbOptions
{
    public const string SectionName = "DynamoDb";
    public string TableName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string? ServiceUrl { get; set; }
}
