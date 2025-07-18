# Teams App Package - OAuth Echo Bot

## 📱 **Ready-to-Upload Teams App Package**

**File:** `OAuth-Echo-Bot-Teams-App.zip`

This is a complete Microsoft Teams app package that you can upload directly to Teams to test your OAuth-enabled bot.

## 🚀 **How to Upload to Teams**

### Option 1: Teams Admin Center (Recommended)
1. Go to [Teams Admin Center](https://admin.teams.microsoft.com)
2. Navigate to **Teams apps** → **Manage apps**
3. Click **Upload new app**
4. Select `OAuth-Echo-Bot-Teams-App.zip`
5. Click **Upload**

### Option 2: Teams Desktop/Web App
1. Open Microsoft Teams
2. Go to **Apps** in the left sidebar
3. Click **Upload a custom app** (bottom left)
4. Select **Upload for [your organization]**
5. Choose `OAuth-Echo-Bot-Teams-App.zip`
6. Click **Add**

### Option 3: Teams Developer Portal
1. Go to [Teams Developer Portal](https://dev.teams.microsoft.com)
2. Click **Apps** → **Import app**
3. Upload `OAuth-Echo-Bot-Teams-App.zip`
4. Configure and publish

## 📋 **Before Uploading - Prerequisites**

### 1. **Bot Must Be Deployed**
Your bot needs to be running and accessible at a public HTTPS endpoint:
- Deploy to Azure App Service
- Configure the messaging endpoint in Azure Bot Service
- Ensure the endpoint responds to bot framework messages

### 2. **OAuth Configuration Required**
Configure OAuth in Azure Bot Service:
- Connection name: `BotTeamsAuthADv2`
- Azure AD v2 provider
- Required scopes: `https://graph.microsoft.com/User.Read`

### 3. **Bot Registration Settings**
In Azure Bot Service, ensure:
- **Bot handle**: Any unique name
- **Messaging endpoint**: `https://your-bot-url.azurewebsites.net/api/messages`
- **Microsoft App ID**: `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`
- **Channels**: Microsoft Teams enabled

## 🎯 **What's in the Package**

- **`manifest.json`** - Teams app configuration
- **`icon-color.png`** - Color icon (192x192)
- **`icon-outline.png`** - Outline icon (32x32)

## 🔧 **App Configuration**

The manifest is pre-configured with:
- **Bot ID**: `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`
- **Scopes**: Personal, Team, Group chat
- **OAuth permissions**: Identity, messageTeamMembers
- **Valid domains**: `*.azurewebsites.net`, `token.botframework.com`

## 🧪 **Testing the Bot**

Once uploaded and added to Teams:

1. **Find the bot**:
   - Search for "OAuth Echo Bot" in Teams
   - Or go to Apps → Built for your org

2. **Start a conversation**:
   - Click **Add** to install
   - Click **Open** to start chatting

3. **Test OAuth features**:
   ```
   You: login
   Bot: [OAuth login prompt]
   
   You: profile
   Bot: [Shows your Microsoft profile]
   
   You: Hello
   Bot: 🔐 Authenticated Echo (Hello [Your Name]!): Hello
   ```

## ⚠️ **Troubleshooting**

### Upload Issues
- **"App validation failed"**: Check manifest.json syntax
- **"Invalid bot ID"**: Verify bot registration in Azure
- **"Domain not allowed"**: Ensure your bot domain is in validDomains

### Bot Not Responding
- **Check bot deployment**: Ensure bot is running and accessible
- **Verify messaging endpoint**: Test `/api/messages` endpoint
- **Check Azure Bot Service**: Verify channels and settings

### OAuth Not Working
- **Verify OAuth connection**: Check `BotTeamsAuthADv2` setting
- **Check permissions**: Ensure Azure AD app has Graph permissions
- **Test authentication**: Try login command and check logs

## 📱 **Next Steps**

1. **Upload the package** to Teams
2. **Test basic functionality** (echo messages)
3. **Test OAuth features** (login, profile, logout)
4. **Customize the experience** as needed

The app is ready for immediate testing in Teams! 🎉
