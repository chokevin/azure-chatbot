# OAuth-Enabled Azure Bot with Microsoft Teams Integration

This project demonstrates an OAuth-enabled Azure Bot Service that integrates with Microsoft Teams using .NET 9.0 and the Bot Framework SDK. Users can authenticate with their Microsoft credentials and access profile information.

## Project Structure

```
cb/
├── AzureBotSample/           # Main OAuth bot application
│   ├── Controllers/          # API controllers
│   ├── Dialogs/             # OAuth dialog and conversation flow
│   ├── infra/               # Azure infrastructure (Bicep)
│   ├── teams-manifest/       # Teams app manifest
│   ├── OAuthEchoBot.cs      # OAuth-enabled bot implementation
│   ├── MainDialog.cs        # OAuth dialog management
│   ├── Program.cs           # Application startup
│   └── azure.yaml           # Azure Developer CLI config
├── infra/                   # Infrastructure templates
├── scripts/                 # Deployment scripts
└── PRIMARY-TESTING-FLOW.md  # Main deployment & testing guide
```

## Features

- **🔐 OAuth Authentication**: Microsoft credential login with Azure AD
- **👤 Profile Integration**: Access user profile via Microsoft Graph API
- **📢 Enhanced Echo Bot**: Context-aware responses based on authentication status
- **🎯 Teams Integration**: Ready-to-deploy Teams bot with channel configuration
- **☁️ Azure Infrastructure**: Complete Bicep templates for deployment
- **🛡️ Security**: Managed Identity for authentication, secure token handling
- **❤️ Health Checks**: Built-in health monitoring endpoints

## 🚀 **Primary Deployment & Testing Workflow**

**⚡ Quick Start - Follow this single workflow for all deployments and testing:**

### **Step 1: Deploy with Azure Developer CLI**
```bash
cd AzureBotSample
azd auth login
azd init    # First time only
azd up      # Deploy everything
```

### **Step 2: Configure OAuth in Azure Portal**
1. Go to Azure Portal → Your Bot Resource → Configuration → OAuth Connection Settings
2. Add setting named `BotTeamsAuthADv2` with Azure AD v2 provider
3. Configure scopes: `https://graph.microsoft.com/User.Read`

### **Step 3: Create and Upload Teams App**
```bash
cd teams-manifest
zip -r ../OAuth-Echo-Bot-Teams-App.zip manifest.json icon-color.png icon-outline.png
```
Upload `OAuth-Echo-Bot-Teams-App.zip` to Microsoft Teams.

### **Step 4: Test OAuth Features**
- `login` → Microsoft authentication
- `profile` → View user profile  
- `logout` → Sign out
- Any message → Enhanced echo responses

**📋 For detailed step-by-step instructions, see [PRIMARY-TESTING-FLOW.md](PRIMARY-TESTING-FLOW.md)**

## OAuth Commands

- **`login`** - Authenticate with Microsoft credentials
- **`logout`** - Sign out from the bot
- **`profile`** - View your Microsoft profile information
- **Any other message** - Enhanced echo with personalized greeting (when authenticated)

## Documentation

- **[PRIMARY-TESTING-FLOW.md](PRIMARY-TESTING-FLOW.md)** - **Main deployment & testing workflow** ⭐
- **[OAUTH-INTEGRATION.md](docs/OAUTH-INTEGRATION.md)** - Detailed OAuth setup and troubleshooting
- **[TEAMS-APP-INSTRUCTIONS.md](docs/TEAMS-APP-INSTRUCTIONS.md)** - Teams app package upload guide

## Local Development (Optional)

For development and testing without deployment:

```bash
cd AzureBotSample
dotnet restore
dotnet run
# Test with Bot Framework Emulator at http://localhost:5209/api/messages
```

## Architecture

The solution includes:

- **App Service**: Hosts the .NET OAuth-enabled bot application
- **Bot Service**: Azure Bot Framework service with Teams channel and OAuth
- **Application Insights**: Telemetry and monitoring
- **Log Analytics**: Centralized logging
- **Managed Identity**: Secure Azure resource access (no hardcoded credentials)

## Security

The solution implements security best practices:
- Azure Managed Identity for Azure resource access
- OAuth 2.0 for user authentication
- Microsoft Graph API integration with minimal permissions (User.Read)
- HTTPS-only communication

## Troubleshooting

1. **Deployment Failures**: Check Azure portal for detailed error messages
2. **OAuth Not Working**: Verify OAuth connection setting is named `BotTeamsAuthADv2`
3. **Teams Integration**: Ensure bot endpoint is publicly accessible
4. **Authentication Issues**: Check Azure AD App Registration permissions

## Resources

- [Bot Framework Documentation](https://docs.microsoft.com/azure/bot-service/)
- [Microsoft Teams Bot Development](https://docs.microsoft.com/microsoftteams/platform/bots/what-are-bots)
- [Azure Bot Service](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction)
- [Microsoft Graph API](https://docs.microsoft.com/graph/)
- [Azure Developer CLI](https://docs.microsoft.com/azure/developer/azure-developer-cli/)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
