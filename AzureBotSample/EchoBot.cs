using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace AzureBotSample;

/// <summary>
/// Simple Echo Bot implementation.
/// This bot echoes back any message sent to it.
/// </summary>
public class EchoBot : ActivityHandler
{
    /// <summary>
    /// Handles incoming messages by echoing them back.
    /// </summary>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var replyText = $"Echo: {turnContext.Activity.Text}";
        await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
    }

    /// <summary>
    /// Handles new members being added to the conversation.
    /// Sends a welcome message.
    /// </summary>
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        const string welcomeText = "Hello and welcome! I am a simple Echo Bot. Send me any message and I'll echo it back to you.";
        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
            }
        }
    }
}
