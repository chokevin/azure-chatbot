# Azure Bot Service with Microsoft Teams Integration

This project demonstrates how to create an Azure Bot Service that integrates with Microsoft Teams using .NET 9.0 and the Bot Framework SDK.

## Project Structure

```
cb/
├── AzureBotSample/           # Main bot application
│   ├── Controllers/          # API controllers
│   ├── infra/               # Azure infrastructure (Bicep)
│   ├── teams-manifest/       # Teams app manifest
│   ├── EchoBot.cs           # Main bot implementation
│   ├── Program.cs           # Application startup
│   └── azure.yaml           # Azure Developer CLI config
├── infra/                   # Infrastructure templates (for deployment)
├── scripts/                 # Deployment scripts
└── README.md
```

## Features

- **Echo Bot**: Responds to user messages with the same text
- **Teams Integration**: Configured for Microsoft Teams channel
- **Azure Infrastructure**: Complete Bicep templates for deployment
- **Monitoring**: Application Insights and Log Analytics integration
- **Security**: Key Vault for secrets, Managed Identity for authentication
- **Health Checks**: Built-in health monitoring endpoints

## Quick Start

### 1. Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
- [Azure Developer CLI (azd)](https://docs.microsoft.com/azure/developer/azure-developer-cli/install-azd)
- Azure subscription with appropriate permissions

### 2. Create Bot Registration

Before deploying, you need to create an Azure AD App Registration for your bot. Follow the detailed guide in [BOT-REGISTRATION.md](BOT-REGISTRATION.md).

This will provide you with:
- Application (client) ID
- Client secret
- Instructions for setting environment variables

### 3. Set Environment Variables

After running the registration script, set the displayed environment variables:

**Bash/Zsh:**
```bash
export BOT_APP_ID='your-app-id'
export BOT_APP_PASSWORD='your-app-secret'
export BOT_APP_TYPE='MultiTenant'
export BOT_APP_TENANT_ID=''
```

**PowerShell:**
```powershell
$env:BOT_APP_ID = 'your-app-id'
$env:BOT_APP_PASSWORD = 'your-app-secret'
$env:BOT_APP_TYPE = 'MultiTenant'
$env:BOT_APP_TENANT_ID = ''
```

### 4. Deploy to Azure

```bash
# Initialize azd (first time only)
azd init

# Deploy infrastructure and application
azd up
```

### 5. Test Your Bot

After deployment, you can test your bot using:
- **Web Chat**: Available in Azure Portal under your Bot Service
- **Bot Framework Emulator**: For local testing
- **Microsoft Teams**: Follow the Teams deployment guide

## Microsoft Teams Integration

For detailed instructions on connecting your bot to Microsoft Teams, see [TEAMS-DEPLOYMENT.md](AzureBotSample/TEAMS-DEPLOYMENT.md).

## Local Development

### Running Locally

1. Navigate to the bot project:
   ```bash
   cd AzureBotSample
   ```

2. Set local environment variables:
   ```bash
   export MicrosoftAppType=MultiTenant
   export MicrosoftAppId=your-app-id
   export MicrosoftAppPassword=your-app-secret
   ```

3. Run the bot:
   ```bash
   dotnet run
   ```

4. The bot will be available at `http://localhost:5209`

### Testing with Bot Framework Emulator

1. Install [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases)
2. Connect to `http://localhost:5209/api/messages`
3. Use your App ID and Password for authentication

## Architecture

The solution includes:

- **App Service**: Hosts the .NET bot application
- **Bot Service**: Azure Bot Framework service with Teams channel
- **Application Insights**: Telemetry and monitoring
- **Log Analytics**: Centralized logging
- **Key Vault**: Secure secret storage
- **Managed Identity**: Secure Azure resource access

## Configuration

Key configuration files:

- `azure.yaml`: Azure Developer CLI configuration
- `infra/main.bicep`: Azure infrastructure template
- `infra/main.parameters.json`: Infrastructure parameters
- `teams-manifest/manifest.json`: Teams app manifest

## Monitoring

Access monitoring data through:
- **Application Insights**: Performance and usage analytics
- **Log Analytics**: Query logs and metrics
- **Health Checks**: `/health` endpoint for status monitoring

## Security

The solution implements security best practices:
- Managed Identity for Azure resource access
- Key Vault for secret management
- HTTPS-only communication
- Minimal required permissions

## Troubleshooting

1. **Deployment Failures**: Check Azure portal for detailed error messages
2. **Bot Not Responding**: Verify App ID and Password are correct
3. **Teams Integration**: Ensure bot endpoint is publicly accessible
4. **Authentication Issues**: Check App Registration permissions

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Resources

- [Bot Framework Documentation](https://docs.microsoft.com/azure/bot-service/)
- [Microsoft Teams Bot Development](https://docs.microsoft.com/microsoftteams/platform/bots/what-are-bots)
- [Azure Bot Service](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
