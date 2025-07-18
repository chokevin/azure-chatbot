using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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
            DisplayTokenAsync,
        }));

        // The initial child Dialog to run.
        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var messageText = stepContext.Options?.ToString() ?? "Type 'login' to authenticate or anything else to echo.";
        var activity = stepContext.Context.Activity;
        
        if (activity.Text?.ToLowerInvariant() == "login")
        {
            // Prompt user to login
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }
        else if (activity.Text?.ToLowerInvariant() == "logout")
        {
            // Sign out the user
            var botAdapter = (BotFrameworkAdapter)stepContext.Context.Adapter;
            await botAdapter.SignOutUserAsync(stepContext.Context, _connectionName, null, cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        else
        {
            // Echo the user's message
            var replyText = $"Echo: {activity.Text}";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(replyText), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }

    private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Get the token from the previous step
        var tokenResponse = (TokenResponse)stepContext.Result;
        if (tokenResponse?.Token != null)
        {
            // Use the token to get user information
            var userInfo = await GetUserInfoAsync(tokenResponse.Token);
            
            var welcomeMessage = userInfo != null 
                ? $"Welcome {userInfo.DisplayName}! You are now signed in. I can now access your Microsoft Graph data."
                : "You are now signed in!";
                
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(welcomeMessage), cancellationToken);
            
            return await stepContext.NextAsync(tokenResponse, cancellationToken);
        }

        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful."), cancellationToken);
        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

    private async Task<DialogTurnResult> DisplayTokenAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var tokenResponse = (TokenResponse)stepContext.Result;
        if (tokenResponse?.Token != null)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("I now have access to your Microsoft Graph data. You can type 'logout' to sign out or ask me to access your information."), 
                cancellationToken);
        }

        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }

    private async Task<UserInfo?> GetUserInfoAsync(string accessToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            System.Diagnostics.Debug.WriteLine($"Error getting user info: {ex.Message}");
        }

        return null;
    }
}

public class UserInfo
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Mail { get; set; }
    public string? UserPrincipalName { get; set; }
}
