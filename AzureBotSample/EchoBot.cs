using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AzureBotSample;

/// <summary>
/// Echo Bot implementation following Azure Bot Service quickstart guide.
/// This bot echoes back any message sent to it and provides a welcome message.
/// </summary>
public class EchoBot : ActivityHandler
{
    /// <summary>
    /// Handles incoming messages and echoes them back to the user.
    /// Implements core bot functionality as described in the Azure Bot Service documentation.
    /// </summary>
    /// <param name="turnContext">The turn context for this conversation turn</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        // Echo back the user's message with additional context
        var replyText = $"Echo: {turnContext.Activity.Text}";
        
        // Send the reply with proper error handling
        try
        {
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }
        catch (Exception ex)
        {
            // Log error in production scenarios
            Console.WriteLine($"Error sending message: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Handles new members being added to the conversation.
    /// Sends a welcome message when the bot is added to a conversation.
    /// </summary>
    /// <param name="membersAdded">List of members added to the conversation</param>
    /// <param name="turnContext">The turn context for this conversation turn</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        const string welcomeText = "Hello and welcome! I'm an Echo Bot. Send me a message and I'll echo it back to you.";

        foreach (var member in membersAdded)
        {
            // Don't send welcome message to the bot itself
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                try
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error in production scenarios
                    Console.WriteLine($"Error sending welcome message: {ex.Message}");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Handles unrecognized activity types.
    /// Provides fallback behavior for activities that aren't messages or member updates.
    /// </summary>
    /// <param name="turnContext">The turn context for this conversation turn</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected override async Task OnUnrecognizedActivityTypeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        // Log unrecognized activity for debugging
        Console.WriteLine($"Unrecognized activity type: {turnContext.Activity.Type}");
        
        // Optionally send a response for unrecognized activities
        await turnContext.SendActivityAsync(
            MessageFactory.Text("I received an activity I don't understand. Please send me a text message."), 
            cancellationToken);
    }
}
