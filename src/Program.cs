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
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp => sp.GetService<TeamsAdapter>()!);
builder.Services.AddSingleton<BotAdapter>(sp => sp.GetService<TeamsAdapter>()!);

// Create the storage to persist turn state
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot>(sp =>
{
    IStorage storage = sp.GetService<IStorage>()!;
    TeamsAdapter adapter = sp.GetService<TeamsAdapter>()!;
    var config = sp.GetService<IOptions<ConfigOptions>>()?.Value ?? new ConfigOptions();

    AuthenticationOptions<AppState> options = new();
    options.AddAuthentication("entra", new OAuthSettings()
    {
        ConnectionName = config.OAUTH_CONNECTION_NAME,
        Title = "Sign In",
        Text = "Please sign in to use the bot.",
        EndOnInvalidMessage = true,
        EnableSso = true,
    }
    );

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

    // Listen for user to say "/signout" - not needed without OAuth
    app.OnMessage("/signout", async (context, state, cancellationToken) =>
    {
        await context.SendActivityAsync("No authentication is configured for this bot");
    });

    // Listen for ANY message to be received. MUST BE AFTER ANY OTHER MESSAGE HANDLERS
    app.OnActivity(ActivityTypes.Message, async (turnContext, turnState, cancellationToken) =>
    {
        int count = turnState.Conversation.MessageCount;
        
        // Increment message count
        turnState.Conversation.MessageCount = ++count;

        // Echo the user's message with count
        await turnContext.SendActivityAsync($"[{count}] You said: {turnContext.Activity.Text}", cancellationToken: cancellationToken);
    });

    app.Authentication.Get("entra").OnUserSignInSuccess(async (context, state) =>
    {
        // Successfully logged in
        await context.SendActivityAsync("Successfully logged in");
        await context.SendActivityAsync($"Token string length: {state.Temp.AuthTokens["entra"].Length}");
        await context.SendActivityAsync($"This is what you said before the AuthFlow started: {context.Activity.Text}");
    });

    app.Authentication.Get("entra").OnUserSignInFailure(async (context, state, ex) =>
    {
        // Failed to login
        await context.SendActivityAsync("Failed to login");
        await context.SendActivityAsync($"Error message: {ex.Message}");
    });

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
