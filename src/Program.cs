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
        options.AddAuthentication("kusto", new OAuthSettings()
        {
            ConnectionName = config.OAUTH_CONNECTION_NAME,
            Title = "Sign In to Azure Kusto",
            Text = "Please sign in to access Azure Kusto services.",
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

    // Build the application. Only configure authentication if an OAuth connection is present.
    var appBuilder = new ApplicationBuilder<AppState>()
        .WithStorage(storage)
        .WithTurnStateFactory(() => new AppState());

    if (!string.IsNullOrEmpty(config.OAUTH_CONNECTION_NAME))
    {
        var buildLogger = sp.GetRequiredService<ILogger<Program>>();
        buildLogger.LogInformation("Configuring OAuth authentication for connection '{ConnectionName}'", config.OAUTH_CONNECTION_NAME);
        try
        {
            appBuilder = appBuilder.WithAuthentication(adapter, options);
        }
        catch (Exception ex)
        {
            buildLogger.LogError(ex, "Failed to configure authentication – the bot will continue without OAuth features.");
        }
    }

    Application<AppState> app = appBuilder.Build();

    // Listen for user to say "/reset" and then delete conversation state
    app.OnMessage("/reset", async (turnContext, turnState, cancellationToken) =>
    {
        turnState.DeleteConversationState();
        await turnContext.SendActivityAsync("Ok I've deleted the current conversation state", cancellationToken: cancellationToken);
    });

    // Listen for ANY message to be received. MUST BE AFTER ANY OTHER MESSAGE HANDLERS
    app.OnActivity(ActivityTypes.Message, async (turnContext, turnState, cancellationToken) =>
    {
        int count = turnState.Conversation.MessageCount;

        // Increment message count
        turnState.Conversation.MessageCount = ++count;

        // Check if user is authenticated for Kusto access
        string? accessToken = null;
        var mainLogger = sp.GetRequiredService<ILogger<Program>>();
        
        if (!string.IsNullOrEmpty(config.OAUTH_CONNECTION_NAME))
        {
            // Use Teams AI framework authentication
            if (turnState.Temp.AuthTokens.ContainsKey("kusto"))
            {
                accessToken = turnState.Temp.AuthTokens["kusto"];
                mainLogger.LogInformation("Found OAuth token for Kusto access (length: {Length})", accessToken.Length);
            }
            else
            {
                // User is not authenticated, Teams AI will handle the sign-in flow automatically
                mainLogger.LogInformation("User not authenticated, OAuth sign-in will be prompted automatically");
                await turnContext.SendActivityAsync($"[{count}] You need to sign in to access Azure Kusto. Please complete the authentication process.", cancellationToken: cancellationToken);
                return; // Exit early, Teams AI will handle authentication flow
            }
        }
        else
        {
            // OAuth not configured and no fallback token is provided. Inform the user and stop processing.
            mainLogger.LogWarning("OAuth not configured and no access token available. Request cannot be authenticated.");
            await turnContext.SendActivityAsync($"[{count}] Authentication is not configured; unable to process secure query.", cancellationToken: cancellationToken);
            return;
        }

        // Send immediate acknowledgment to avoid bot timeout
        await turnContext.SendActivityAsync($"[{count}] Processing your query, this may take up to 30 seconds...", cancellationToken: cancellationToken);

        try
        {
            mainLogger.LogInformation("Starting HTTP request to kusto service for message: {Message}", turnContext.Activity.Text);

            // Create HTTP client to call the Azure service with explicit timeout
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(20); // Reduced to 20 seconds to stay under bot timeout
            
            mainLogger.LogInformation("HttpClient timeout set to 20 seconds");

            // Add Authorization header only if we have a token
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            }

            // Create explicit timeout cancellation token
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(18)); // 18 seconds to be safe
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

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

            mainLogger.LogInformation("Making POST request to kusto service");
            var startTime = DateTime.UtcNow;

            // Make POST request to the kusto query recommender service
            var response = await httpClient.PostAsync("http://aksdri.azurewebsites.net/api/kusto-query-recommender/suggest_agents", jsonContent, combinedCts.Token);

            var elapsed = DateTime.UtcNow - startTime;
            mainLogger.LogInformation("HTTP request completed in {ElapsedMs}ms with status {StatusCode}", elapsed.TotalMilliseconds, response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(combinedCts.Token);
                mainLogger.LogInformation("Successfully received response of length {Length}", responseContent.Length);
                
                try
                {
                    // Parse the JSON response to extract the clean agent output
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
                    var root = jsonDoc.RootElement;
                    
                    if (root.TryGetProperty("status", out var status) && status.GetString() == "success" &&
                        root.TryGetProperty("agent_output", out var agentOutput))
                    {
                        var cleanOutput = agentOutput.GetString();
                        await turnContext.SendActivityAsync($"[{count}] {cleanOutput}", cancellationToken: CancellationToken.None);
                    }
                    else
                    {
                        // Fallback to raw response if parsing fails
                        await turnContext.SendActivityAsync($"[{count}] Query suggestion: {responseContent}", cancellationToken: CancellationToken.None);
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    mainLogger.LogWarning(ex, "Failed to parse JSON response, sending raw response");
                    await turnContext.SendActivityAsync($"[{count}] Query suggestion: {responseContent}", cancellationToken: CancellationToken.None);
                }
            }
            else
            {
                mainLogger.LogWarning("HTTP request failed with status {StatusCode}", response.StatusCode);
                await turnContext.SendActivityAsync($"[{count}] Failed to get query suggestion. Status: {response.StatusCode}", cancellationToken: CancellationToken.None);
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            mainLogger.LogError(ex, "Request timed out after 18 seconds");
            await turnContext.SendActivityAsync($"[{count}] Request timed out after 18 seconds. The service may be busy, please try again.", cancellationToken: CancellationToken.None);
        }
        catch (TaskCanceledException ex)
        {
            mainLogger.LogError(ex, "Request was cancelled - this might be due to timeout");
            await turnContext.SendActivityAsync($"[{count}] Request was cancelled (likely timeout). Please try again.", cancellationToken: CancellationToken.None);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("ResponseEnded") || ex.Message.Contains("prematurely"))
        {
            mainLogger.LogError(ex, "Remote service closed connection prematurely");
            await turnContext.SendActivityAsync($"[{count}] The kusto service ended the response prematurely. This may indicate the query is too complex or the service is overloaded. Please try a simpler query.", cancellationToken: CancellationToken.None);
        }
        catch (HttpRequestException ex)
        {
            mainLogger.LogError(ex, "HTTP request failed");
            await turnContext.SendActivityAsync($"[{count}] Network error: {ex.Message}", cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            mainLogger.LogError(ex, "Error calling kusto query recommender service");
            await turnContext.SendActivityAsync($"[{count}] Error: Failed to get query suggestion - {ex.Message}", cancellationToken: CancellationToken.None);
        }
    });

    // Configure authentication event handlers only if OAuth is configured
    if (!string.IsNullOrEmpty(config.OAUTH_CONNECTION_NAME))
    {
        var authLogger = sp.GetRequiredService<ILogger<Program>>();
        try
        {
            var kustoAuth = app.Authentication.Get("kusto");

            kustoAuth.OnUserSignInSuccess(async (turnContext, state) =>
            {
                await turnContext.SendActivityAsync("Successfully signed in to Azure Kusto!");
                if (state.Temp.AuthTokens.ContainsKey("kusto"))
                {
                    await turnContext.SendActivityAsync($"Token received (length: {state.Temp.AuthTokens["kusto"].Length})");
                }
                await turnContext.SendActivityAsync($"You can now use authenticated features. Original message: {turnContext.Activity.Text}");
            });

            kustoAuth.OnUserSignInFailure(async (turnContext, state, ex) =>
            {
                await turnContext.SendActivityAsync("Failed to sign in to Azure Kusto");
                await turnContext.SendActivityAsync($"Error: {ex.Message}");
                authLogger.LogError(ex, "OAuth sign-in failed for user {UserId}", turnContext.Activity.From.Id);
            });
        }
        catch (Exception ex)
        {
            authLogger.LogError(ex, "Failed to attach authentication event handlers – continuing without OAuth.");
        }
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