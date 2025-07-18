using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Net.Http;
using System.Text.Json;

namespace EchoBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _connectionName;

    public MainDialog(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        : base(nameof(MainDialog))
    {
        _httpClientFactory = httpClientFactory;
        _connectionName = configuration["ConnectionName"] ?? "BotTeamsAuthADv2";

        AddDialog(new OAuthPrompt(
            nameof(OAuthPrompt),
            new OAuthPromptSettings
            {
                ConnectionName = _connectionName,
                Text = "Please Sign In",
                Title = "Sign In",
                Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
            }));

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            PromptStepAsync,
            LoginStepAsync,
        }));

        // The initial child Dialog to run.
        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var command = stepContext.Context.Activity.Text?.ToLowerInvariant();
        
        return command switch
        {
            "login" => await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken),
            "logout" => await HandleLogoutAsync(stepContext, cancellationToken),
            "profile" => await HandleProfileAsync(stepContext, cancellationToken),
            _ => await HandleEchoAsync(stepContext, cancellationToken)
        };
    }

    private async Task<DialogTurnResult> HandleLogoutAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        try
        {
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();
            var userId = stepContext.Context.Activity.From.Id;
            var channelId = stepContext.Context.Activity.ChannelId;

            if (userTokenClient != null)
            {
                await userTokenClient.SignOutUserAsync(userId, _connectionName, channelId, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("✅ You have been signed out."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("❌ Unable to sign out. Please try again."), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"❌ Error during sign out: {ex.Message}"), cancellationToken);
        }
        
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

    private async Task<DialogTurnResult> HandleProfileAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        try
        {
            var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();
            if (userTokenClient == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("❌ Authentication not available."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            var userId = stepContext.Context.Activity.From.Id;
            var channelId = stepContext.Context.Activity.ChannelId;
            var tokenResponse = await userTokenClient.GetUserTokenAsync(userId, _connectionName, channelId, null, cancellationToken);
            
            if (tokenResponse?.Token == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("🔐 You need to login first. Type 'login' to authenticate."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            var userInfo = await GetUserInfoAsync(tokenResponse.Token);
            var message = userInfo != null 
                ? $@"👤 **Your Profile Information:**
- **Name:** {userInfo.DisplayName ?? "Not available"}
- **Email:** {userInfo.Mail ?? userInfo.UserPrincipalName ?? "Not available"}
- **User ID:** {userInfo.Id ?? "Not available"}"
                : "❌ Sorry, I couldn't retrieve your profile information.";
                
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }
        catch (Exception ex)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"❌ Error retrieving profile: {ex.Message}"), cancellationToken);
        }
        
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

    private async Task<DialogTurnResult> HandleEchoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var messageText = stepContext.Context.Activity.Text;
        
        try
        {
            var tokenResponse = await GetUserTokenAsync(stepContext);
            var replyText = tokenResponse?.Token != null 
                ? await CreateAuthenticatedEchoAsync(tokenResponse.Token, messageText)
                : $"📢 **Echo**: {messageText}\n\n💡 *Tip: Type 'login' to authenticate for enhanced features!*";
                
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
        }
        catch (Exception)
        {
            // Fallback to basic echo on any error
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"📢 **Echo**: {messageText}"), cancellationToken);
        }
        
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

    private async Task<TokenResponse?> GetUserTokenAsync(WaterfallStepContext stepContext)
    {
        var userTokenClient = stepContext.Context.TurnState.Get<UserTokenClient>();
        if (userTokenClient == null) return null;

        var userId = stepContext.Context.Activity.From.Id;
        var channelId = stepContext.Context.Activity.ChannelId;
        return await userTokenClient.GetUserTokenAsync(userId, _connectionName, channelId, null, default);
    }

    private async Task<string> CreateAuthenticatedEchoAsync(string token, string messageText)
    {
        var userInfo = await GetUserInfoAsync(token);
        var userName = userInfo?.DisplayName ?? "there";
        return $"🔐 **Authenticated Echo** (Hello {userName}!): {messageText}";
    }

    private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var tokenResponse = (TokenResponse)stepContext.Result;
        if (tokenResponse?.Token == null)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("❌ **Login was not successful.** Please try again by typing 'login'."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        // Get user information and send welcome message
        var userInfo = await GetUserInfoAsync(tokenResponse.Token);
        var welcomeMessage = userInfo != null 
            ? $"🎉 **Welcome {userInfo.DisplayName}!** You are now signed in.\n\n✅ Authentication successful! **Available commands:**\n- Type **'profile'** to see your profile\n- Type **'logout'** to sign out\n- Send any message for an authenticated echo"
            : "🎉 **You are now signed in!** I can access your Microsoft Graph data.\n\n**Available commands:**\n- Type **'profile'** to see your profile\n- Type **'logout'** to sign out\n- Send any message for enhanced features";
            
        await stepContext.Context.SendActivityAsync(MessageFactory.Text(welcomeMessage), cancellationToken);
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

    private async Task<UserInfo?> GetUserInfoAsync(string accessToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting user info: {ex.Message}");
            return null;
        }
    }
}

public class UserInfo
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Mail { get; set; }
    public string? UserPrincipalName { get; set; }
}
