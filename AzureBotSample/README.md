# Azure Bot Service Sample - Echo Bot with Teams Integration

This is a sample Azure Bot Service application built with .NET 9.0, following the [Azure Bot Service Quickstart guide](https://learn.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-create-bot?view=azure-bot-service-4.0&tabs=csharp%2Cvs), **now enhanced with Microsoft Teams integration**.

## Features

- **Echo Bot Implementation**: Echoes back any message sent to it
- **Welcome Message**: Greets new users when they join the conversation
- **Microsoft Teams Integration**: Ready-to-deploy Teams bot with channel configuration
- **Azure Infrastructure**: Complete Bicep templates for Azure deployment
- **Error Handling**: Comprehensive error handling with logging and user feedback
- **Health Checks**: Built-in health monitoring endpoint
- **Security**: Following Azure security best practices with Key Vault and Managed Identity
- **Logging**: Structured logging for debugging and monitoring

## Project Structure

```
AzureBotSample/
├── Controllers/
│   └── BotController.cs          # HTTP endpoint for bot messages
├── infra/                        # Azure Infrastructure (Bicep templates)
│   ├── main.bicep               # Main infrastructure template
│   └── main.parameters.json     # Template parameters
├── teams-manifest/               # Microsoft Teams app manifest
│   └── manifest.json            # Teams app configuration
├── EchoBot.cs                    # Main bot logic
├── AdapterWithErrorHandler.cs   # Error handling for bot adapter
├── Program.cs                    # Application startup and configuration
├── azure.yaml                   # Azure Developer CLI configuration
├── appsettings.json             # Production configuration
├── appsettings.Development.json # Development configuration
├── AzureBotSample.csproj        # Project file with dependencies
├── TEAMS-DEPLOYMENT.md          # Teams integration deployment guide
└── TESTING.md                   # Local testing guide
```

## Quick Start - Teams Integration

### Deploy to Azure and Connect to Teams

1. **Register your bot** (get App ID and Password):
   ```bash
   # See TEAMS-DEPLOYMENT.md for detailed instructions
   ```

2. **Deploy to Azure**:
   ```bash
   azd auth login
   azd init
   azd up
   ```

3. **Configure Teams app** using the manifest in `teams-manifest/`

4. **Install in Teams** and start chatting!

For detailed instructions, see [TEAMS-DEPLOYMENT.md](./TEAMS-DEPLOYMENT.md).

## Local Development

### Prerequisites

- .NET 9.0 SDK
- Azure subscription
- Bot Framework Emulator (for local testing)
- Azure Developer CLI (azd) for deployment

### Local Development Setup

1. **Clone and restore packages**:
   ```bash
   cd AzureBotSample
   dotnet restore
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Test with Bot Framework Emulator**:
   - Download [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases)
   - Connect to: `http://localhost:5000/api/messages`
   - Leave App ID and App Password empty for local testing

## Configuration

### Local Development
For local testing, no configuration is required. The bot will run without authentication.

### Azure Deployment
When deploying to Azure, configure these settings in your App Service:

```json
{
  "MicrosoftAppType": "MultiTenant",
  "MicrosoftAppId": "<your-app-id>",
  "MicrosoftAppPassword": "<your-app-password>",
  "MicrosoftAppTenantId": "<your-tenant-id>"
}
```

## Security Best Practices Implemented

- **Managed Identity Support**: Ready for Azure Managed Identity when deployed
- **Secure Configuration**: Sensitive settings externalized to configuration
- **Error Handling**: Comprehensive error handling without exposing sensitive information
- **HTTPS Enforcement**: HTTPS redirection enabled
- **Input Validation**: Bot Framework handles input validation
- **Health Monitoring**: Health check endpoint for monitoring

## Bot Framework Components

- **Microsoft.Bot.Builder**: Core Bot Framework functionality
- **Microsoft.Bot.Builder.Integration.AspNet.Core**: ASP.NET Core integration
- **Microsoft.Bot.Builder.Azure**: Azure-specific extensions

## API Endpoints

- `POST /api/messages` - Bot Framework message endpoint
- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation (development only)

## Deployment to Azure

1. **Create Azure Bot Service**:
   ```bash
   az bot create --resource-group myResourceGroup --name myBotName --kind webapp --subscription mySubscription
   ```

2. **Deploy to Azure App Service**:
   ```bash
   az webapp deployment source config-zip --resource-group myResourceGroup --name myAppService --src publish.zip
   ```

3. **Configure App Settings**:
   Set the Microsoft App credentials in the Azure portal under Configuration.

## Testing

### Local Testing
Use the Bot Framework Emulator to test locally:
1. Start the application with `dotnet run`
2. Open Bot Framework Emulator
3. Connect to `http://localhost:5000/api/messages`

### Azure Testing
After deployment, test in the Azure portal using the Web Chat test interface.

## Monitoring and Logging

The application includes:
- Structured logging with configurable levels
- Health checks for monitoring
- Error tracking and user-friendly error messages
- Bot Framework telemetry integration

## Next Steps

- Add Application Insights for advanced monitoring
- Implement conversation state and user state
- Add authentication and authorization
- Integrate with Azure Cognitive Services
- Add multi-turn conversation support

## References

- [Azure Bot Service Documentation](https://docs.microsoft.com/en-us/azure/bot-service/)
- [Bot Framework SDK](https://github.com/Microsoft/botframework-sdk)
- [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator)
