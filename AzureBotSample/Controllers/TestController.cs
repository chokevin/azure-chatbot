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
            var text = !string.IsNullOrEmpty(message.Message) ? message.Message : message.Text;
            return Ok(new { echo = $"Echo: {text}", originalMessage = text });
        }
    }

    public class TestMessage
    {
        public string Message { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
