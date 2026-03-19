using Amazon.SQS;
using Amazon.SQS.Model;
using DistributedShortener.Infrastructure.Configuration;
using DistributedShortener.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Create queue on app start
var sqsSettings = builder.Configuration.GetSection(SqsSettings.SectionName).Get<SqsSettings>()!;

using (var scope = app.Services.CreateScope())
{
    var sqsClient = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
    var queuesResponse = await sqsClient.ListQueuesAsync(new ListQueuesRequest());

    var existingQueue = queuesResponse.QueueUrls?.FirstOrDefault(url => url.EndsWith(sqsSettings.QueueName));

    if (existingQueue == null)
    {
        await sqsClient.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = sqsSettings.QueueName,
        });
    }
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
