using Amazon.SQS;
using Amazon.SQS.Model;
using DistributedShortener.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DistributedShortener.Api;

[ApiController]
[Route("sqs")]
public class SqsController : ControllerBase
{
    private readonly ILogger<SqsController> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly SqsSettings _sqsSettings;

    public SqsController(ILogger<SqsController> logger, IAmazonSQS sqs, IOptions<SqsSettings> sqsSettings)
    {
        _logger = logger;
        _sqs = sqs;
        _sqsSettings = sqsSettings.Value;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] string message)
    {
        var queueUrl = await GetQueueUrl();

        var response = await _sqs.SendMessageAsync(new SendMessageRequest
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

        var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 5
        });

        return Ok(response.Messages.Select(m => new { m.MessageId, m.Body }));
    }

    private async Task<string> GetQueueUrl()
    {
        var response = await _sqs.GetQueueUrlAsync(_sqsSettings.QueueName);
        return response.QueueUrl;
    }
}