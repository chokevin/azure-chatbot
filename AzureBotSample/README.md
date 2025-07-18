# OAuth-Enabled Azure Bot for Microsoft Teams

This Azure Bot Service application is built with .NET 9.0 and enhanced with **OAuth authentication** and **Microsoft Teams integration**. Users can authenticate with their Microsoft credentials and access profile information via Microsoft Graph API.

## Features

- **🔐 OAuth Authentication**: Microsoft credential login with Azure AD
- **📢 Enhanced Echo Bot**: Context-aware responses based on authentication status  
- **👤 Profile Integration**: Access user profile information via Microsoft Graph API
- **🎯 Microsoft Teams Integration**: Ready-to-deploy Teams bot with channel configuration
- **☁️ Azure Infrastructure**: Complete Bicep templates for Azure deployment
- **🛡️ Error Handling**: Comprehensive error handling with logging and user feedback
- **❤️ Health Checks**: Built-in health monitoring endpoint
- **🔒 Security**: Following Azure security best practices with Managed Identity

## OAuth Commands

- **`login`** - Authenticate with Microsoft credentials
- **`logout`** - Sign out from the bot  
- **`profile`** - View your Microsoft profile information
- **Any other message** - Enhanced echo with personalized greeting (when authenticated)

## Project Structure

```
AzureBotSample/
├── Controllers/
│   └── BotController.cs          # HTTP endpoint for bot messages
├── Dialogs/
│   └── MainDialog.cs            # OAuth dialog and conversation flow
├── infra/                        # Azure Infrastructure (Bicep templates)
│   ├── main.bicep               # Main infrastructure template
│   └── main.parameters.json     # Template parameters
├── teams-manifest/               # Microsoft Teams app manifest
│   └── manifest.json            # Teams app configuration
├── OAuthEchoBot.cs              # OAuth-enabled bot implementation
├── AdapterWithErrorHandler.cs   # Error handling for bot adapter
├── Program.cs                    # Application startup and configuration
├── azure.yaml                   # Azure Developer CLI configuration
├── appsettings.json             # Production configuration
└── AzureBotSample.csproj        # Project file with dependencies
```

## 🚀 **Primary Deployment & Testing Workflow**

**⚡ Use this single workflow for all deployments and testing:**

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

**📋 For detailed step-by-step instructions, see [PRIMARY-TESTING-FLOW.md](../PRIMARY-TESTING-FLOW.md)**

## 📚 **Documentation**

- **[PRIMARY-TESTING-FLOW.md](../PRIMARY-TESTING-FLOW.md)** - **Main deployment & testing workflow** ⭐
- **[OAUTH-INTEGRATION.md](../docs/OAUTH-INTEGRATION.md)** - Detailed OAuth setup and troubleshooting
- **[TEAMS-APP-INSTRUCTIONS.md](../docs/TEAMS-APP-INSTRUCTIONS.md)** - Teams app package upload guide

## 🔧 **Local Development** (Optional)

For development and testing without deployment:

```bash
dotnet restore
dotnet run
# Test with Bot Framework Emulator at http://localhost:5209/api/messages
```

## 🛡️ **Security Features**

- **Azure Managed Identity**: No hardcoded credentials
- **OAuth Token Management**: Secure Azure Bot Service token handling
- **Minimal Permissions**: Only User.Read Graph API scope
- **HTTPS Enforcement**: Production-ready security

## 📊 **Bot Framework Components**

- **Microsoft.Bot.Builder**: Core Bot Framework functionality
- **Microsoft.Bot.Builder.Dialogs**: OAuth and conversation management
- **Microsoft.Bot.Builder.Integration.AspNet.Core**: ASP.NET Core integration
- **Azure.Identity**: Managed Identity support

---

**🎯 Use the [PRIMARY-TESTING-FLOW.md](../PRIMARY-TESTING-FLOW.md) for all deployments and testing.**
