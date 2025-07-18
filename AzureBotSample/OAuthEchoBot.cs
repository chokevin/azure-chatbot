using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using EchoBot.Dialogs;

namespace AzureBotSample;

/// <summary>
/// OAuth-enabled Echo Bot that supports Microsoft authentication.
/// Users can type 'login' to authenticate with their Microsoft credentials,
/// 'logout' to sign out, or send any other message to get an echo response.
/// </summary>
public class OAuthEchoBot : ActivityHandler
{
    private readonly ConversationState _conversationState;
    private readonly UserState _userState;
    private readonly Dialog _dialog;
    private readonly ILogger<OAuthEchoBot> _logger;

    /// <summary>
    /// Initializes a new instance of the OAuthEchoBot.
    /// </summary>
    /// <param name="conversationState">The conversation state for managing dialog state</param>
    /// <param name="userState">The user state for storing user-specific data</param>
    /// <param name="dialog">The main dialog that handles OAuth and conversation flow</param>
    /// <param name="logger">Logger for debugging and monitoring</param>
    public OAuthEchoBot(ConversationState conversationState, UserState userState, MainDialog dialog, ILogger<OAuthEchoBot> logger)
    {
        _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        _userState = userState ?? throw new ArgumentNullException(nameof(userState));
        _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles incoming messages by running them through the dialog system.
    /// The dialog will handle OAuth authentication and message echoing.
    /// </summary>
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing message from user: {turnContext.Activity.From.Name ?? "Unknown"}");
        
        // Run the dialog with the new message activity
        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }

    /// <summary>
    /// Handles new members being added to the conversation.
    /// Sends a welcome message with authentication instructions.
    /// </summary>
    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        const string welcomeText = @"Hello and welcome! I am an OAuth-enabled Echo Bot. 

**Available Commands:**
- Type **'login'** to authenticate with your Microsoft credentials
- Type **'logout'** to sign out  
- Type **'profile'** to see your profile information (requires login)
- Send any other message to get an echo response

Try typing 'login' to get started with authentication!";

        foreach (var member in membersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                _logger.LogInformation($"Welcoming new member: {member.Name ?? member.Id}");
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Handles token response events from OAuth providers.
    /// This is called when the user completes the OAuth flow.
    /// </summary>
    protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received token response event");
        
        // Run the dialog to handle the token response
        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }

    /// <summary>
    /// Saves conversation and user state at the end of each turn.
    /// This ensures that authentication state and dialog progress are preserved.
    /// </summary>
    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes that might have occurred during the turn
        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        
        _logger.LogInformation("Turn completed and state saved");
    }
}
