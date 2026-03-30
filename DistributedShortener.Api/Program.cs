using Amazon.SQS;
using Amazon.SQS.Model;
using Carter;
using DistributedShortener.Api.Exceptions;
using DistributedShortener.Application.DepencencyInjection;
using DistributedShortener.Infrastructure.Configurations;
using DistributedShortener.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration); // EF Core + Redis + SQS
builder.Services.AddApplication(builder.Configuration); //MediatR + FluentValidation + Behaviors
builder.Services.AddCarter();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// // Create queue on app start
// SqsSettings sqsSettings = builder.Configuration.GetSection(SqsSettings.SectionName).Get<SqsSettings>()!;
//
// using (var scope = app.Services.CreateScope())
// {
//     var sqsClient = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();
//     var queuesResponse = await sqsClient.ListQueuesAsync(new ListQueuesRequest());
//
//     var existingQueue = queuesResponse.QueueUrls?.FirstOrDefault(url => url.EndsWith(sqsSettings.QueueName));
//
//     if (existingQueue == null)
//     {
//         await sqsClient.CreateQueueAsync(new CreateQueueRequest
//         {
//             QueueName = sqsSettings.QueueName,
//         });
//     }
// }

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapCarter();
app.Run();
