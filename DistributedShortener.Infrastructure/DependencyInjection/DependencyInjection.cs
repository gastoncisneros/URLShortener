using Amazon.SQS;
using DistributedShortener.Application.Abstractions;
using DistributedShortener.Infrastructure.Configurations;
using DistributedShortener.Infrastructure.Persistence;
using DistributedShortener.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedShortener.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            );

        services.AddScoped<ILinkRepository, LinkRepository>();
        
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

        return services;
    }
}