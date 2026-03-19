using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;

namespace DistributedShortener.Api;

[ApiController]
[Route("sqs")]
public class SqsController : ControllerBase
{
    private readonly ILogger<SqsController> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly string _queueUrl;

    public SqsController(ILogger<SqsController> logger, IAmazonSQS sqs, string queueUrl)
    {
        _logger = logger;
        _sqs = sqs;
        _queueUrl = queueUrl;
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] string message)
    {
        var response = await _sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = message
        });
        
        return Ok(new { MessageId = response.MessageId });
    }

    [HttpGet("receive")]
    public async Task<IActionResult> ReceiveMessages()
    {
        var response = await _sqs.ReceiveMessageAsync(new 
            ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 5
            });
        
        return Ok(response.Messages.Select(m => new { m.MessageId, m.Body }));
    }
}