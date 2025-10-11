using System;
using Amazon;
using Amazon.DynamoDBv2;
using CreditPro.Application.Interfaces;
using CreditPro.Infrastructure.Configuration;
using CreditPro.Infrastructure.Persistence;
using CreditPro.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CreditPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DynamoDbOptions>(configuration.GetSection(DynamoDbOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexión 'Postgres' no está configurada.");
        }

        services.AddDbContext<CreditProDbContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<IAmazonDynamoDB>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DynamoDbOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.TableName))
            {
                throw new InvalidOperationException("El nombre de la tabla de DynamoDB no está configurado.");
            }

            var dynamoConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region)
            };

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                dynamoConfig.ServiceURL = options.ServiceUrl;
            }

            return new AmazonDynamoDBClient(dynamoConfig);
        });

        services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();

        return services;
    }
}
