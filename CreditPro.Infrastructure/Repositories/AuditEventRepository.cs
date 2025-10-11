using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CreditPro.Application.Interfaces;
using CreditPro.Domain.Entities;
using CreditPro.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CreditPro.Infrastructure.Repositories;

public class AuditEventRepository : IAuditEventRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DynamoDbOptions _options;

    public AuditEventRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbOptions> options)
    {
        _dynamoDb = dynamoDb;
        _options = options.Value;
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["applicationId"] = new AttributeValue { S = auditEvent.ApplicationId.ToString() },
            ["timestamp"] = new AttributeValue { S = auditEvent.Timestamp.ToString("O", CultureInfo.InvariantCulture) },
            ["eventType"] = new AttributeValue { S = auditEvent.EventType },
            ["newState"] = new AttributeValue { S = auditEvent.NewState },
            ["details"] = new AttributeValue { S = auditEvent.Details.ToJsonString() }
        };

        var request = new PutItemRequest
        {
            TableName = _options.TableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<AuditEvent>> GetByApplicationIdAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _options.TableName,
            KeyConditionExpression = "applicationId = :appId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":appId"] = new AttributeValue { S = applicationId.ToString() }
            }
        };

        var response = await _dynamoDb.QueryAsync(request, cancellationToken).ConfigureAwait(false);

        return response.Items
            .Select(item => new AuditEvent(
                Guid.Parse(item["applicationId"].S),
                DateTime.Parse(item["timestamp"].S, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                item["eventType"].S,
                item["newState"].S,
                ParseDetails(item)))
            .ToList();
    }

    private static JsonObject ParseDetails(Dictionary<string, AttributeValue> item)
    {
        if (!item.TryGetValue("details", out var details) || string.IsNullOrWhiteSpace(details.S))
        {
            return new JsonObject();
        }

        var node = JsonNode.Parse(details.S);
        return node as JsonObject ?? new JsonObject();
    }
}
