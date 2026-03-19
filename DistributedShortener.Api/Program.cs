using Amazon.SQS;
using Amazon.SQS.Model;
using DistributedShortener.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();

var app = builder.Build();

// Create queue on app start
using (var scope = app.Services.CreateScope())
{
    var sqsClient = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
    var queuesResponse = await sqsClient.ListQueuesAsync(new ListQueuesRequest());
    
    var existingQueue = queuesResponse.QueueUrls.FirstOrDefault(url => url.EndsWith("my-queue"));

    if (existingQueue == null)
    {
        await sqsClient.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = "my-queue",
        });
    }
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.Run();
