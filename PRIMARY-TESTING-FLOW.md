# OAuth Bot - Primary Testing Flow

## 🎯 **Primary Deployment & Testing Workflow**

This document outlines the **single recommended workflow** for deploying and testing the OAuth-enabled bot in Microsoft Teams.

## 📋 **Prerequisites**

- Azure subscription with appropriate permissions
- Azure Developer CLI (`azd`) installed
- Microsoft Teams access (admin or sideloading permissions)
- Git repository cloned locally

## 🚀 **Step-by-Step Deployment Flow**

### **Step 1: Deploy to Azure using AZD**

1. **Navigate to the project directory**:
   ```bash
   cd AzureBotSample
   ```

2. **Authenticate with Azure**:
   ```bash
   azd auth login
   ```

3. **Initialize the project** (first time only):
   ```bash
   azd init
   ```
   - Accept the default environment name or provide your own
   - Select your Azure subscription
   - Choose your preferred Azure region

4. **Deploy the infrastructure and application**:
   ```bash
   azd up
   ```
   
   This command will:
   - ✅ Create Azure resources (App Service, Bot Service, etc.)
   - ✅ Build and deploy the OAuth-enabled bot
   - ✅ Configure the bot with Managed Identity
   - ✅ Set up the messaging endpoint automatically
   - ✅ Return the bot's public URL

5. **Note the deployment outputs**:
   - **Bot URL**: `https://[your-app-name].azurewebsites.net`
   - **Bot App ID**: `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`

### **Step 2: Configure OAuth Connection**

1. **Go to Azure Portal** → Search for your Bot Service resource

2. **Navigate to Configuration** → **OAuth Connection Settings**

3. **Add OAuth Setting**:
   - Click **"Add Setting"**
   - **Name**: `BotTeamsAuthADv2`
   - **Service Provider**: `Azure Active Directory v2`
   - **Client ID**: [Your Azure AD App Registration Client ID]
   - **Client Secret**: [Your Azure AD App Registration Client Secret]
   - **Tenant ID**: `72f988bf-86f1-41af-91ab-2d7cd011db47`
   - **Scopes**: `https://graph.microsoft.com/User.Read`

4. **Save the configuration**

### **Step 3: Create Teams App Package**

**Option A: Using the build script (Recommended)**:
```bash
./build-teams-app.sh
```

**Option B: Manual creation**:
```bash
cd AzureBotSample/teams-manifest
zip -r ../../OAuth-Echo-Bot-Teams-App.zip manifest.json icon-color.png icon-outline.png
```

Both methods create `OAuth-Echo-Bot-Teams-App.zip` in the root directory.

### **Step 4: Upload to Teams**

1. **Open Microsoft Teams**

2. **Navigate to Apps**:
   - Click **"Apps"** in the left sidebar
   - Click **"Upload a custom app"** (bottom left)
   - Select **"Upload for [your organization]"**

3. **Upload the package**:
   - Select `OAuth-Echo-Bot-Teams-App.zip`
   - Click **"Add"** to install the app

4. **Start testing**:
   - Search for **"OAuth Echo Bot"** in Teams
   - Click **"Open"** to start a conversation

## 🧪 **Testing the OAuth Features**

### **Test Commands**

Execute these commands in order to test all OAuth functionality:

1. **Basic Echo** (without authentication):
   ```
   You: Hello World
   Bot: 📢 Echo: Hello World
        💡 Tip: Type 'login' to authenticate for enhanced features!
   ```

2. **Login Flow**:
   ```
   You: login
   Bot: [OAuth login card/prompt appears]
   [Complete Microsoft authentication in browser]
   Bot: 🎉 Welcome [Your Name]! You are now signed in.
   ```

3. **Profile Information**:
   ```
   You: profile
   Bot: 👤 Your Profile Information:
        - Name: [Your Display Name]
        - Email: [Your Email]
        - User ID: [Your Microsoft User ID]
   ```

4. **Authenticated Echo**:
   ```
   You: Hello again
   Bot: 🔐 Authenticated Echo (Hello [Your Name]!): Hello again
   ```

5. **Logout**:
   ```
   You: logout
   Bot: ✅ You have been signed out.
   ```

## ⚡ **Quick Re-deployment Workflow**

For subsequent deployments after making code changes:

1. **Update and redeploy**:
   ```bash
   cd AzureBotSample
   azd up
   ```

2. **No need to recreate Teams package** - existing installation will automatically use the updated bot

## 🔧 **Configuration Files Used**

This workflow relies on these pre-configured files:

- **`azure.yaml`** - AZD configuration with service definitions
- **`infra/main.bicep`** - Azure infrastructure template
- **`infra/main.parameters.json`** - Infrastructure parameters
- **`teams-manifest/manifest.json`** - Teams app configuration
- **`appsettings.json`** - Bot configuration with OAuth settings
- **`build-teams-app.sh`** - Automated Teams app package builder

## ⚠️ **Important Notes**

### **Do NOT**
- ❌ Create additional zip files for deployment
- ❌ Use manual Azure portal deployments
- ❌ Modify the infrastructure manually
- ❌ Create multiple Teams app packages

### **Always**
- ✅ Use `azd up` for deployments
- ✅ Use the single Teams app package workflow
- ✅ Test OAuth functionality in this specific order
- ✅ Verify OAuth connection settings after deployment

## 🔍 **Troubleshooting**

### **If `azd up` fails**:
1. Check Azure subscription permissions
2. Verify `azd auth login` was successful
3. Ensure unique resource names

### **If OAuth doesn't work**:
1. Verify OAuth connection name is exactly `BotTeamsAuthADv2`
2. Check Azure AD app registration permissions
3. Ensure client secret is valid and not expired

### **If Teams upload fails**:
1. Verify the zip package contains all three files
2. Check Teams admin policies for custom app uploads
3. Ensure manifest.json has correct bot ID

---

**This is the ONLY recommended workflow for deployment and testing.** Following this process ensures consistent, reliable deployments and proper OAuth functionality testing.
