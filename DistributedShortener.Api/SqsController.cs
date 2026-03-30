using Amazon.SQS;
using Amazon.SQS.Model;
using DistributedShortener.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedShortener.Api;

[ApiController]
[Route("sqs")]
public class SqsController(ILogger<SqsController> logger, IAmazonSQS sqs, IOptions<SqsSettings> sqsSettings)
    : ControllerBase
{
    private readonly ILogger<SqsController> _logger = logger;
    private readonly SqsSettings _sqsSettings = sqsSettings.Value;

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] string message)
    {
        var queueUrl = await GetQueueUrl();

        var response = await sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = message
        });

        return Ok(new { MessageId = response.MessageId });
    }

    [HttpGet("receive")]
    public async Task<IActionResult> ReceiveMessages()
    {
        var queueUrl = await GetQueueUrl();

        var response = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 5
        });

        return Ok(response);
    }

    private async Task<string> GetQueueUrl()
    {
        var response = await sqs.GetQueueUrlAsync(_sqsSettings.QueueName);
        return response.QueueUrl;
    }
}