using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace AzureBotSample;

/// <summary>
/// Error handler for the bot adapter.
/// Implements comprehensive error handling and logging for bot operations.
/// </summary>
public class AdapterWithErrorHandler : CloudAdapter
{
    /// <summary>
    /// Initializes a new instance of the AdapterWithErrorHandler class.
    /// </summary>
    /// <param name="botFrameworkAuthentication">Bot Framework authentication</param>
    /// <param name="logger">Logger for error tracking</param>
    public AdapterWithErrorHandler(BotFrameworkAuthentication botFrameworkAuthentication, ILogger<AdapterWithErrorHandler> logger)
        : base(botFrameworkAuthentication, logger)
    {
        // Set up error handling with comprehensive logging and user feedback
        OnTurnError = async (turnContext, exception) =>
        {
            // Log the exception for debugging and monitoring
            logger.LogError(exception, "Exception caught in adapter");

            // Send a message to the user informing them of the error
            // This helps with user experience and debugging
            try
            {
                var errorMessageText = "The bot encountered an error or bug.";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);
            }
            catch (Exception sendException)
            {
                // If we can't send error messages, log this secondary failure
                logger.LogError(sendException, "Exception caught while sending error message to user");
            }

            // Send a trace activity for Bot Framework Emulator debugging
            // This is only visible in the emulator and helps with local development
            try
            {
                var traceActivity = Activity.CreateTraceActivity("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
                await turnContext.SendActivityAsync(traceActivity);
            }
            catch (Exception traceException)
            {
                logger.LogError(traceException, "Exception caught while sending trace activity");
            }
        };
    }
}
