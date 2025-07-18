# Teams Integration Deployment Guide

This guide will help you deploy your Azure Bot Service to Microsoft Teams.

## Prerequisites

1. **Azure CLI** - Install from [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Azure Developer CLI (azd)** - Install from [here](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)
3. **Azure Subscription** - Active Azure subscription
4. **Microsoft 365 Admin Access** - To configure Teams app (or Developer tenant)

## Step 1: Register Your Bot Application

### Option A: Using Azure Portal (Recommended for Teams)

1. Go to [Azure App Registrations](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps)
2. Click **"New registration"**
3. Fill in the details:
   - **Name**: `Azure Bot Sample for Teams`
   - **Supported account types**: `Accounts in any organizational directory (Any Azure AD directory - Multitenant)`
   - **Redirect URI**: Leave blank for now
4. Click **"Register"**
5. Copy the **Application (client) ID** - this is your `BOT_APP_ID`
6. Go to **"Certificates & secrets"**
7. Click **"New client secret"**
8. Add description: `Bot App Secret`
9. Copy the secret **Value** - this is your `BOT_APP_PASSWORD`

### Option B: Using Azure CLI

```bash
# Create app registration
az ad app create --display-name "Azure Bot Sample for Teams" --available-to-other-tenants true

# Get the app ID
APP_ID=$(az ad app list --display-name "Azure Bot Sample for Teams" --query '[0].appId' -o tsv)

# Create client secret
az ad app credential reset --id $APP_ID --years 2
```

## Step 2: Set Environment Variables

Create a `.env` file in your project root:

```bash
# Azure Environment
AZURE_ENV_NAME="your-bot-env"
AZURE_LOCATION="eastus"
AZURE_SUBSCRIPTION_ID="your-subscription-id"

# Bot Configuration
BOT_APP_ID="your-app-id-from-step-1"
BOT_APP_PASSWORD="your-app-password-from-step-1"
BOT_APP_TENANT_ID="your-tenant-id"
BOT_APP_TYPE="MultiTenant"

# Optional Customization
BOT_DISPLAY_NAME="Azure Bot Sample for Teams"
BOT_DESCRIPTION="Echo bot with Teams integration"
APP_SERVICE_PLAN_SKU="F1"
```

## Step 3: Deploy to Azure

1. **Initialize AZD environment**:
   ```bash
   cd /Users/chokevin/dev/cb/AzureBotSample
   azd init
   ```

2. **Login to Azure**:
   ```bash
   azd auth login
   ```

3. **Deploy the infrastructure and application**:
   ```bash
   azd up
   ```

   This will:
   - Create all Azure resources (App Service, Bot Service, Application Insights, etc.)
   - Deploy your bot application
   - Configure the Bot Service with Teams channel

## Step 4: Test Your Bot

### Web Chat Test
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your Bot Service resource
3. Click on **"Test in Web Chat"**
4. Send a message like "Hello" - you should see "Echo: Hello"

### Direct Bot Framework Test
1. Download [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases)
2. Connect to: `https://your-bot-app-service.azurewebsites.net/api/messages`
3. Use your BOT_APP_ID and BOT_APP_PASSWORD for authentication

## Step 5: Create Teams App Package

1. **Update the Teams manifest**:
   - Edit `teams-manifest/manifest.json`
   - Replace `YOUR_BOT_APP_ID_HERE` with your actual Bot App ID
   - Replace `YOUR_BOT_DOMAIN_HERE` with your App Service domain

2. **Create app icons** (required for Teams):
   - `icon-color.png` - 192x192 pixels
   - `icon-outline.png` - 32x32 pixels (white outline on transparent background)

3. **Create the app package**:
   ```bash
   cd teams-manifest
   zip -r bot-app.zip manifest.json icon-color.png icon-outline.png
   ```

## Step 6: Install in Microsoft Teams

### Option A: Upload Custom App (Developer/Test Environment)

1. Open Microsoft Teams
2. Go to **Apps** in the left sidebar
3. Click **"Manage your apps"**
4. Click **"Upload an app"** → **"Upload a custom app"**
5. Select your `bot-app.zip` file
6. Click **"Add"** to install the bot

### Option B: Submit to App Store (Production)

1. Go to [Partner Center](https://partner.microsoft.com/dashboard/microsoftteams/overview)
2. Submit your app package for review
3. Follow Microsoft Teams app submission guidelines

## Step 7: Test in Teams

1. In Teams, find your bot in the Apps section
2. Click **"Add"** to start a conversation
3. Send a message like "Hello Bot!"
4. You should receive "Echo: Hello Bot!"

## Step 8: Add to Team Channels (Optional)

1. Go to a Teams team
2. Click **"Apps"** tab
3. Search for your bot
4. Click **"Add"** to add it to the team
5. Team members can now @mention the bot

## Troubleshooting

### Common Issues:

1. **Bot not responding in Teams**:
   - Check App Service logs in Azure Portal
   - Verify BOT_APP_ID and BOT_APP_PASSWORD are correct
   - Ensure the bot endpoint is accessible: `https://your-app.azurewebsites.net/api/messages`

2. **Teams app installation fails**:
   - Verify manifest.json has correct Bot App ID
   - Check that icons are the correct size
   - Ensure all required fields in manifest are filled

3. **Authentication errors**:
   - Verify the app registration has correct redirect URIs
   - Check that the client secret hasn't expired

### Useful Commands:

```bash
# Check deployment status
azd show

# View application logs
azd logs

# Re-deploy just the application
azd deploy

# Clean up resources
azd down
```

## Security Best Practices

1. **Use Azure Key Vault** - Bot secrets are automatically stored in Key Vault
2. **Enable HTTPS Only** - App Service is configured for HTTPS only
3. **Use Managed Identity** - App Service uses managed identity for secure access
4. **Monitor with Application Insights** - Full telemetry and monitoring enabled

## Next Steps

1. **Customize the bot logic** in `EchoBot.cs`
2. **Add more Teams features**:
   - Adaptive Cards
   - Messaging Extensions
   - Task Modules
   - Meeting Extensions
3. **Set up CI/CD** with GitHub Actions
4. **Add authentication** for user-specific features
5. **Implement conversation state** for multi-turn conversations

## Useful Links

- [Azure Bot Service Documentation](https://docs.microsoft.com/en-us/azure/bot-service/)
- [Microsoft Teams Bot Documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/what-are-bots)
- [Bot Framework SDK](https://github.com/Microsoft/botframework-sdk)
- [Teams App Manifest Schema](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema)
