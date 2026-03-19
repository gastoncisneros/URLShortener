using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedShortener.Infrastructure.DependencyInjection;

public static class DependencyInjection
{    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localstack:4566",
                AuthenticationRegion = "us-east-1",
            };
            
            return new AmazonSQSClient(config);
        });
        
        return services;
    }
}