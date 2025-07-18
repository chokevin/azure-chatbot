using AzureBotSample;
using EchoBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder.Dialogs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Add bot framework services
// Configure authentication to use Managed Identity when available, fallback to configuration
var microsoftAppId = builder.Configuration["MicrosoftAppId"];
var microsoftAppPassword = builder.Configuration["MicrosoftAppPassword"];
var microsoftAppTenantId = builder.Configuration["MicrosoftAppTenantId"];
var microsoftAppType = builder.Configuration["MicrosoftAppType"] ?? "MultiTenant";
var azureClientId = builder.Configuration["AZURE_CLIENT_ID"];

if (microsoftAppType == "UserAssignedMSI" || string.IsNullOrEmpty(microsoftAppPassword))
{
    // Use Managed Identity authentication with Bot Framework's built-in support
    builder.Services.AddSingleton<BotFrameworkAuthentication>(serviceProvider =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Configuring Bot Framework to use User-Assigned Managed Identity authentication");
        logger.LogInformation($"App ID: {microsoftAppId}");
        logger.LogInformation($"Tenant ID: {microsoftAppTenantId}");
        logger.LogInformation($"Azure Client ID: {azureClientId}");
        
        // Create a configuration that tells Bot Framework to use Managed Identity
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MicrosoftAppType"] = "UserAssignedMSI",
                ["MicrosoftAppId"] = microsoftAppId,
                ["MicrosoftAppTenantId"] = microsoftAppTenantId,
                ["MicrosoftAppPassword"] = "", // Explicitly empty for managed identity
                ["AZURE_CLIENT_ID"] = azureClientId
            })
            .Build();
        
        return new ConfigurationBotFrameworkAuthentication(config, logger: logger);
    });
}
else
{
    // Use traditional configuration-based authentication
    builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
}

// Register the bot adapter with error handling
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Register dialog services
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<ConversationState>();

// Register dialogs
builder.Services.AddSingleton<MainDialog>();

// Register the bot implementation with OAuth and dialog support
builder.Services.AddTransient<IBot, OAuthEchoBot>();

// Add logging services for better debugging and monitoring
builder.Services.AddLogging();

// Add health checks for monitoring
builder.Services.AddHealthChecks();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}
else
{
    // Use HSTS in production for security
    app.UseHsts();
}

// Enable HTTPS redirection for security
app.UseHttpsRedirection();

// Map controllers for the bot endpoint
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();
