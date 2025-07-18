# OAuth-Enabled Bot - Teams Deployment Package

## 📦 Package Contents

This deployment package contains an OAuth-enabled Azure Bot with Microsoft Teams integration. The bot supports Microsoft authentication and provides personalized responses.

### Package Files
- **`azure-echo-bot-oauth-teams.zip`** - Complete source code package
- **`azure-echo-bot-oauth-publish.zip`** - Compiled binaries ready for deployment

## 🚀 Quick Deployment to Azure

### Option 1: Using Azure Developer CLI (Recommended)

1. **Extract the source package**:
   ```bash
   unzip azure-echo-bot-oauth-teams.zip
   cd AzureBotSample
   ```

2. **Deploy with azd**:
   ```bash
   azd auth login
   azd init
   azd up
   ```

### Option 2: Manual Azure App Service Deployment

1. **Create Azure App Service**:
   - Go to Azure Portal
   - Create new App Service (Windows, .NET 9.0)
   - Note the App Service name and URL

2. **Deploy the binaries**:
   - Extract `azure-echo-bot-oauth-publish.zip`
   - Upload to your App Service via FTP, GitHub Actions, or ZIP deployment

3. **Configure App Settings**:
   ```json
   {
     "MicrosoftAppType": "UserAssignedMSI",
     "MicrosoftAppId": "4bc2fae2-04dc-4b23-a9b7-e6b6422995ee",
     "MicrosoftAppTenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
     "ConnectionName": "BotTeamsAuthADv2"
   }
   ```

## 🔐 OAuth Configuration Required

After deployment, you **must** configure OAuth settings in Azure Bot Service:

### 1. Create OAuth Connection Setting

1. Go to **Azure Portal** → Your Bot Resource
2. Navigate to **Configuration** → **OAuth Connection Settings**
3. Click **Add Setting**
4. Configure:
   - **Name**: `BotTeamsAuthADv2`
   - **Service Provider**: Azure Active Directory v2
   - **Client ID**: [Your Azure AD App Client ID]
   - **Client Secret**: [Your Azure AD App Client Secret]
   - **Tenant ID**: `72f988bf-86f1-41af-91ab-2d7cd011db47`
   - **Scopes**: `https://graph.microsoft.com/User.Read`

### 2. Azure AD App Registration

Ensure your Azure AD app has:
- **Redirect URIs** configured for bot OAuth
- **API Permissions**: Microsoft Graph User.Read (granted)
- **Client Secret** generated and valid

## 📱 Teams Integration

### Teams Manifest Configuration

The package includes a Teams manifest at `teams-manifest/manifest.json`. Update these values:

```json
{
  "bots": [
    {
      "botId": "4bc2fae2-04dc-4b23-a9b7-e6b6422995ee",
      "supportsFiles": false,
      "isNotificationOnly": false,
      "scopes": ["personal", "team", "groupchat"]
    }
  ],
  "webApplicationInfo": {
    "id": "4bc2fae2-04dc-4b23-a9b7-e6b6422995ee",
    "resource": "https://graph.microsoft.com/"
  }
}
```

### Deploy to Teams

1. **Create Teams App Package**:
   - Zip the contents of `teams-manifest/` folder
   - Include `manifest.json` and any icons

2. **Upload to Teams**:
   - Teams Admin Center → Manage Apps → Upload Custom App
   - Or Teams App Studio/Developer Portal

## 🧪 Testing OAuth Features

### Available Commands

Once deployed and configured:

- **`login`** - Initiates Microsoft OAuth authentication
- **`logout`** - Signs out the authenticated user  
- **`profile`** - Shows user profile information (requires login)
- **Any other message** - Enhanced echo with personalized greeting

### Expected Behavior

#### Before Authentication:
```
User: Hello
Bot: 📢 Echo: Hello
     💡 Tip: Type 'login' to authenticate for enhanced features!
```

#### After Authentication:
```
User: Hello  
Bot: 🔐 Authenticated Echo (Hello John Doe!): Hello
```

#### Profile Information:
```
User: profile
Bot: 👤 Your Profile Information:
     - Name: John Doe
     - Email: john.doe@microsoft.com
     - User ID: 12345678-1234-5678-9abc-123456789012
```

## 🔧 Configuration Files

### Core Configuration
- **`appsettings.json`** - Production settings with Managed Identity
- **`appsettings.Development.json`** - Development overrides

### Infrastructure  
- **`infra/main.bicep`** - Azure infrastructure template
- **`infra/main.parameters.json`** - Infrastructure parameters

## 📋 Troubleshooting

### Common Issues

1. **OAuth not working**:
   - Verify OAuth connection setting name matches `BotTeamsAuthADv2`
   - Check Azure AD app permissions and client secret
   - Ensure bot endpoint is HTTPS

2. **Teams integration fails**:
   - Verify bot ID in Teams manifest matches your registration
   - Check messaging endpoint in bot registration
   - Ensure bot is published and channels are configured

3. **Profile information not loading**:
   - Verify Microsoft Graph permissions are granted
   - Check User.Read scope in OAuth configuration
   - Review bot logs for Graph API errors

### Debug Information

The bot includes comprehensive logging. Check Application Insights or App Service logs for:
- OAuth token acquisition attempts
- Microsoft Graph API calls
- Authentication flow errors

## 📚 Documentation

- **`OAUTH-INTEGRATION.md`** - Detailed OAuth setup guide
- **`TEAMS-DEPLOYMENT.md`** - Teams-specific deployment instructions  
- **`README.md`** - Project overview and quick start

## 🛡️ Security Notes

- Uses Azure Managed Identity for enhanced security
- OAuth tokens are securely managed by Azure Bot Service
- Minimal permissions requested (User.Read only)
- No hardcoded credentials in source code

---

**Ready to deploy!** Follow the OAuth configuration steps after deployment to enable authentication features.
