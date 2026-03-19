using Amazon.SQS;
using DistributedShortener.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedShortener.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
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