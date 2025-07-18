using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace AzureBotSample.Controllers;

/// <summary>
/// Bot Controller that handles incoming HTTP requests from the Bot Framework Connector service.
/// This controller is responsible for receiving activities and passing them to the bot adapter.
/// </summary>
[Route("api/messages")]
[ApiController]
public class BotController : ControllerBase
{
    private readonly CloudAdapter _adapter;
    private readonly IBot _bot;
    private readonly ILogger<BotController> _logger;

    /// <summary>
    /// Initializes a new instance of the BotController.
    /// </summary>
    /// <param name="adapter">The cloud adapter</param>
    /// <param name="bot">The bot implementation</param>
    /// <param name="logger">Logger for debugging and monitoring</param>
    public BotController(CloudAdapter adapter, IBot bot, ILogger<BotController> logger)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes incoming activities from the Bot Framework Connector service.
    /// This endpoint receives all bot activities including messages, member additions, etc.
    /// </summary>
    /// <returns>HTTP 200 OK on successful processing</returns>
    [HttpPost]
    public async Task PostAsync()
    {
        try
        {
            _logger.LogInformation("Processing incoming bot activity");
            
            // Delegate the processing of the HTTP POST to the adapter
            // The adapter will invoke the bot's message handlers
            await _adapter.ProcessAsync(Request, Response, _bot);
            
            _logger.LogInformation("Successfully processed bot activity");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bot activity");
            
            // Return 500 status code for unhandled exceptions
            // The Bot Framework will retry failed requests
            Response.StatusCode = 500;
        }
    }
}
