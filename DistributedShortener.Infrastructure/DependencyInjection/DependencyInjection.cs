using Amazon.SQS;
using DistributedShortener.Application.Abstractions;
using DistributedShortener.Infrastructure.Configurations;
using DistributedShortener.Infrastructure.Persistence;
using DistributedShortener.Infrastructure.Repositories;
using DistributedShortener.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DistributedShortener.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        // General DIs
        services.AddScoped<ILinkRepository, LinkRepository>();


        // POSTGRES
        
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            );

        // SQS
        
        // Sqs puede estar configurado con LocalStack (Docker) y en PROD leemos de settings
        // que tendran la URL real de SQS en Amazon.
        services.Configure<SqsSettings>(configuration.GetSection(SqsSettings.SectionName));

        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var sqsSettings = configuration.GetSection(SqsSettings.SectionName).Get<SqsSettings>()!;

            var config = new AmazonSQSConfig
            {
                ServiceURL = sqsSettings.ServiceUrl,
                AuthenticationRegion = sqsSettings.Region,
            };

            return new AmazonSQSClient(config);
        });

        
        // REDIS
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddScoped<ICacheService, CacheService>();
        
        return services;
    }
}