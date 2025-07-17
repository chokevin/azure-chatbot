using AzureBotSample;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add bot framework services
// Register authentication services
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Register the bot adapter with error handling
builder.Services.AddSingleton<CloudAdapter, AdapterWithErrorHandler>();

// Register the bot implementation
builder.Services.AddTransient<IBot, EchoBot>();

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
