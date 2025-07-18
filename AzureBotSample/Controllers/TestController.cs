using Microsoft.AspNetCore.Mvc;

namespace AzureBotSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Bot application is running correctly", timestamp = DateTime.UtcNow });
        }

        [HttpPost("echo")]
        public IActionResult Echo([FromBody] TestMessage message)
        {
            var text = message.Text ?? message.Message ?? string.Empty;
            return Ok(new { echo = $"Echo: {text}", originalMessage = text });
        }
    }

    public class TestMessage
    {
        public string? Text { get; set; }
        public string? Message { get; set; }
    }
}
