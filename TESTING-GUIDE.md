# Azure Echo Bot - Testing Guide

This guide provides step-by-step instructions for testing your Azure Echo Bot with User-Assigned Managed Service Identity (MSI) authentication.

## 🚀 Bot Configuration Summary

- **Bot Service Name**: `bot-service-opkp6mndkn5xg`
- **App Service**: `bot-app-opkp6mndkn5xg`
- **Authentication Type**: UserAssignedMSI
- **Bot ID**: `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`
- **Endpoint**: `https://bot-app-opkp6mndkn5xg.azurewebsites.net/api/messages`

## 📋 Testing Methods

### 1. Azure Portal Web Chat Testing

**Steps:**
1. Open [Azure Portal](https://portal.azure.com)
2. Navigate to **Resource Groups** → **rg-kevin-test**
3. Click on **bot-service-opkp6mndkn5xg** (Bot Service)
4. In the left sidebar, click **"Test in Web Chat"**
5. Type a message like "Hello" in the chat
6. **Expected Result**: Bot responds with "Echo: Hello"

**Troubleshooting:**
- If you see "Unauthorized" errors, verify that both the Bot Service and App Service are using the same MSI client ID
- Check that the bot endpoint is responding: `https://bot-app-opkp6mndkn5xg.azurewebsites.net/health`

### 2. Microsoft Teams Testing

**Upload the Bot to Teams:**
1. Open Microsoft Teams
2. Go to **Apps** → **Manage your apps**
3. Click **"Upload an app"** → **"Upload a custom app"**
4. Select the file: `azure-echo-bot-msi-teams.zip`
5. Click **"Add"** to install the bot

**Test the Bot:**
1. **Personal Chat**: Start a 1:1 conversation with the bot
2. **Team Chat**: Add the bot to a team and mention it with `@Azure Echo Bot (MSI)`
3. **Group Chat**: Add the bot to a group conversation

**Test Messages:**
- Send: "Hello"
- Expected: "Echo: Hello"
- Send: "How are you?"
- Expected: "Echo: How are you?"

### 3. Bot Framework Emulator Testing (Local Development)

**Prerequisites:**
- Download [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases)

**Steps:**
1. Open Bot Framework Emulator
2. Click **"Open Bot"**
3. Enter the endpoint: `https://bot-app-opkp6mndkn5xg.azurewebsites.net/api/messages`
4. Leave **Microsoft App ID** and **Microsoft App Password** empty (for MSI)
5. Click **"Connect"**
6. Send test messages

**Note**: Direct testing may show authentication errors, which is expected for MSI bots without proper tokens.

## 🔍 Health Check Testing

Verify the bot application is running:

```bash
curl https://bot-app-opkp6mndkn5xg.azurewebsites.net/health
```

**Expected Response**: HTTP 200 OK

## 🐛 Troubleshooting Guide

### Common Issues and Solutions

#### 1. "Unauthorized" Errors in Web Chat
**Cause**: Mismatch between Bot Service and App Service authentication configuration
**Solution**: 
- Verify Bot Service msaAppType is "UserAssignedMSI"
- Check App Service environment variables:
  - `MicrosoftAppId` = `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`
  - `AZURE_CLIENT_ID` = `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`
  - `MicrosoftAppType` = `UserAssignedMSI`

#### 2. Bot Not Responding in Teams
**Cause**: Teams app manifest configuration or bot registration issues
**Solution**:
- Ensure the bot ID in Teams manifest matches the MSI client ID
- Verify the bot endpoint is accessible
- Check that Microsoft Teams channel is enabled in Bot Service

#### 3. Bot Service Not Found
**Cause**: Bot service was not created or was deleted
**Solution**: Redeploy using `azd up` to recreate all resources

#### 4. 500 Internal Server Error
**Cause**: Application startup issues or missing dependencies
**Solution**: 
- Check App Service logs in Azure Portal
- Verify Managed Identity is properly attached to App Service
- Ensure Application Insights is configured correctly

## 🔧 Verification Commands

Check current configuration:

```bash
# Verify Bot Service configuration
az bot show --name bot-service-opkp6mndkn5xg --resource-group rg-kevin-test --query "properties.{msaAppType:msaAppType,msaAppId:msaAppId,endpoint:endpoint}"

# Check App Service environment variables
az webapp config appsettings list --name bot-app-opkp6mndkn5xg --resource-group rg-kevin-test --query "[?name=='MicrosoftAppId' || name=='AZURE_CLIENT_ID' || name=='MicrosoftAppType']"

# Test health endpoint
curl https://bot-app-opkp6mndkn5xg.azurewebsites.net/health
```

## 📊 Expected Test Results

| Test Method | Test Input | Expected Output |
|-------------|------------|-----------------|
| Azure Portal Web Chat | "Hello" | "Echo: Hello" |
| Teams Personal Chat | "Test message" | "Echo: Test message" |
| Teams Team Chat | "@Azure Echo Bot (MSI) Hi" | "Echo: Hi" |
| Health Check | GET /health | HTTP 200 OK |

## 🎯 Success Criteria

✅ **Bot responds correctly in Azure Portal Web Chat**  
✅ **Bot can be uploaded to Microsoft Teams successfully**  
✅ **Bot responds to messages in Teams personal and team chats**  
✅ **Health endpoint returns 200 OK**  
✅ **No authentication errors in logs**  

## 📞 Support

If you encounter issues:

1. **Check Azure Portal**: Monitor App Service and Bot Service logs
2. **Verify Configuration**: Ensure all IDs and endpoints match
3. **Test Health Endpoint**: Confirm the application is running
4. **Review Teams Manifest**: Validate bot ID and permissions

---

**Last Updated**: July 17, 2025  
**Bot Version**: 1.0.1  
**Authentication**: User-Assigned Managed Service Identity
