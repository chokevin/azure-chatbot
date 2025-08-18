// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Microsoft.Teams.AI;
using Quote.Agent;
using Quote.Agent.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure options
builder.Services.Configure<ConfigOptions>(builder.Configuration);

// Prepare Configuration for ConfigurationBotFrameworkAuthentication
builder.Configuration["MicrosoftAppType"] = "UserAssignedMSI";
builder.Configuration["MicrosoftAppId"] = "7a3c24f6-36bd-4b24-91fe-aefb3fdbf8ac";
builder.Configuration["MicrosoftAppTenantId"] = "72f988bf-86f1-41af-91ab-2d7cd011db47";
builder.Configuration["MicrosoftAppClientId"] = "7a3c24f6-36bd-4b24-91fe-aefb3fdbf8ac";

// Create the Federated Service Client Credentials to be used as the ServiceClientCredentials for the Bot Framework SDK.
builder.Services.AddSingleton<ServiceClientCredentialsFactory>(
    new FederatedServiceClientCredentialsFactory(
        builder.Configuration["MicrosoftAppId"],
        builder.Configuration["MicrosoftAppClientId"],
        builder.Configuration["MicrosoftAppTenantId"]));

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Cloud Adapter with error handling enabled.
// Note: some classes expect a BotAdapter and some expect a BotFrameworkHttpAdapter, so
// register the same adapter instance for all types.
builder.Services.AddSingleton<TeamsAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetService<TeamsAdapter>() !);
builder.Services.AddSingleton<BotAdapter>(sp => sp.GetService<TeamsAdapter>() !);

// Create the storage to persist turn state
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot>(sp =>
{
    IStorage storage = sp.GetService<IStorage>() !;
    TeamsAdapter adapter = sp.GetService<TeamsAdapter>() !;
    var config = sp.GetService<IOptions<ConfigOptions>>()?.Value ?? new ConfigOptions();

    AuthenticationOptions<AppState> options = new ();

    // Add authentication only if OAuth connection name is configured
    if (!string.IsNullOrEmpty(config.OAUTH_CONNECTION_NAME))
    {
        options.AddAuthentication("entra", new OAuthSettings()
        {
            ConnectionName = config.OAUTH_CONNECTION_NAME,
            Title = "Sign In to Microsoft Graph",
            Text = "Please sign in to access Microsoft Graph services.",
            EndOnInvalidMessage = true,
            EnableSso = true,
        });
    }
    else
    {
        // Log warning if OAuth is not configured
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("OAuth connection name is not configured. Authentication features will be disabled.");
    }

    // Create the application
    Application<AppState> app = new ApplicationBuilder<AppState>()
        .WithStorage(storage)
        .WithTurnStateFactory(() => new AppState())
        .WithAuthentication(adapter, options)
        .Build();

    // Listen for user to say "/reset" and then delete conversation state
    app.OnMessage("/reset", async (turnContext, turnState, cancellationToken) =>
    {
        turnState.DeleteConversationState();
        await turnContext.SendActivityAsync("Ok I've deleted the current conversation state", cancellationToken: cancellationToken);
    });

    // Listen for user to say "/signout"
    app.OnMessage("/signout", async (turnContext, state, cancellationToken) =>
    {
        if (!string.IsNullOrEmpty(config.OAUTH_CONNECTION_NAME))
        {
            await app.Authentication.SignOutUserAsync(turnContext, state, cancellationToken: cancellationToken);
            await turnContext.SendActivityAsync("You have been signed out successfully.");
        }
        else
        {
            await turnContext.SendActivityAsync("Authentication is not configured for this bot.");
        }
    });

    // Listen for ANY message to be received. MUST BE AFTER ANY OTHER MESSAGE HANDLERS
    app.OnActivity(ActivityTypes.Message, async (turnContext, turnState, cancellationToken) =>
    {
        int count = turnState.Conversation.MessageCount;

        // Increment message count
        turnState.Conversation.MessageCount = ++count;

        try
        {
            // Create HTTP client to call the Azure service
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Prepare the request payload
            var requestPayload = new
            {
                input_text = turnContext.Activity.Text
            };

            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestPayload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            // Make POST request to the kusto query recommender service
            var response = await httpClient.PostAsync("http://aksdri.azurewebsites.net/api/kusto-query-recommender/suggest_agents", jsonContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                await turnContext.SendActivityAsync($"[{count}] Query suggestion: {responseContent}", cancellationToken: cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync($"[{count}] Failed to get query suggestion. Status: {response.StatusCode}", cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error calling kusto query recommender service");
            await turnContext.SendActivityAsync($"[{count}] Error: Failed to get query suggestion - {ex.Message}", cancellationToken: cancellationToken);
        }
    });

    // Configure authentication event handlers only if OAuth is configured
    if (!string.IsNullOrEmpty(config.OAUTH_CONNECTION_NAME))
    {
        app.Authentication.Get("entra").OnUserSignInSuccess(async (turnContext, state) =>
        {
            // Successfully logged in
            await turnContext.SendActivityAsync("Successfully signed in to Microsoft Graph!");

            if (state.Temp.AuthTokens.ContainsKey("entra"))
            {
                await turnContext.SendActivityAsync($"Token received (length: {state.Temp.AuthTokens["entra"].Length})");
            }

            await turnContext.SendActivityAsync($"You can now use authenticated features. Original message: {turnContext.Activity.Text}");
        });

        app.Authentication.Get("entra").OnUserSignInFailure(async (turnContext, state, ex) =>
        {
            // Failed to login
            await turnContext.SendActivityAsync("Failed to sign in to Microsoft Graph");
            await turnContext.SendActivityAsync($"Error: {ex.Message}");

            // Log the full exception for debugging
            var logger = sp.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "OAuth sign-in failed for user {UserId}", turnContext.Activity.From.Id);
        });
    }

    return app;
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Quote Agent Bot starting up...");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    logger.LogInformation("Development mode: OpenAPI mapping enabled");
}

logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

logger.LogInformation("Quote Agent Bot started successfully");
app.Run();