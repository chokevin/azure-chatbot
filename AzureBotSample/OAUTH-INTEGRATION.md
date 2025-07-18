# OAuth Integration Guide

This bot has been enhanced with Microsoft OAuth authentication capabilities, allowing users to login with their Microsoft credentials and access their profile information through Microsoft Graph API.

## Features

### 🔐 Authentication Commands
- **`login`** - Initiates OAuth flow with Microsoft credentials
- **`logout`** - Signs out the authenticated user
- **`profile`** - Displays user profile information (requires authentication)

### 📢 Enhanced Echo Functionality
- **Unauthenticated users**: Regular echo with login prompt
- **Authenticated users**: Enhanced echo with personalized greeting using their display name

## Implementation Details

### Key Components

1. **`OAuthEchoBot.cs`** - Main bot class that handles OAuth-enabled conversations
2. **`MainDialog.cs`** - Dialog that manages the OAuth flow and user interactions
3. **`UserTokenClient`** - Modern approach for token management (replaces deprecated IUserTokenProvider)
4. **Microsoft Graph API integration** - Fetches user profile information

### OAuth Flow

1. User types `login`
2. Bot presents OAuth prompt (redirects to Microsoft login)
3. User authenticates with Microsoft credentials
4. Bot receives access token
5. Bot can now access Microsoft Graph on behalf of the user

### Security Features

- Uses Azure Managed Identity for bot authentication
- Secure token storage and management
- Proper error handling and fallback mechanisms
- No hardcoded credentials

## Setup Requirements

### 1. Azure Bot Registration
Your bot is already configured with:
- **App ID**: `4bc2fae2-04dc-4b23-a9b7-e6b6422995ee`
- **Tenant ID**: `72f988bf-86f1-41af-91ab-2d7cd011db47`
- **App Type**: User-Assigned Managed Identity

### 2. OAuth Connection Configuration

You'll need to configure an OAuth connection setting in Azure Bot Service:

1. Go to Azure Portal → Your Bot Resource
2. Navigate to **Configuration** → **OAuth Connection Settings**
3. Click **Add Setting** and configure:
   - **Name**: `BotTeamsAuthADv2` (matches the ConnectionName in appsettings.json)
   - **Service Provider**: Azure Active Directory v2
   - **Client ID**: Your Azure AD application client ID
   - **Client Secret**: Your Azure AD application client secret
   - **Tenant ID**: `72f988bf-86f1-41af-91ab-2d7cd011db47`
   - **Scopes**: `https://graph.microsoft.com/User.Read`

### 3. Azure AD App Registration

Ensure your Azure AD app registration has:
- **Redirect URIs** configured for bot OAuth
- **API permissions** for Microsoft Graph (User.Read)
- **Client secret** generated and configured in OAuth connection setting

## Testing the OAuth Integration

### Local Testing
1. Run the bot locally: `dotnet run`
2. Use Bot Framework Emulator or Teams to test
3. Try these commands:
   - Type `login` to authenticate
   - Type `profile` to see user information
   - Type `logout` to sign out
   - Send any other message for echo functionality

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

#### Profile Command:
```
User: profile
Bot: 👤 Your Profile Information:
     - Name: John Doe
     - Email: john.doe@microsoft.com
     - User ID: 12345678-1234-5678-9abc-123456789012
```

## Troubleshooting

### Common Issues

1. **OAuth prompt not appearing**
   - Verify OAuth connection setting is configured correctly
   - Check that the connection name matches `BotTeamsAuthADv2`

2. **Authentication fails**
   - Verify Azure AD app registration redirect URIs
   - Check client secret is valid and configured
   - Ensure proper scopes are configured

3. **Profile information not loading**
   - Verify Microsoft Graph permissions are granted
   - Check that User.Read scope is included
   - Review network connectivity to graph.microsoft.com

### Debug Logging

The bot includes comprehensive logging for OAuth operations:
- Authentication attempts
- Token retrieval
- Graph API calls
- Error conditions

Check the application logs for detailed debugging information.

## Security Considerations

- **Token Security**: Tokens are securely managed by Azure Bot Service
- **Scope Limitations**: Only requests minimal required permissions (User.Read)
- **Error Handling**: Graceful fallback when authentication fails
- **Managed Identity**: Uses Azure Managed Identity for enhanced security

## Microsoft Graph Integration

The bot demonstrates basic Microsoft Graph usage:
- **User Profile**: Fetches display name, email, and user ID
- **Extensible**: Can be easily extended for additional Graph API calls
- **Secure**: Uses proper authentication headers and error handling

## Future Enhancements

Consider these additional features:
- Calendar integration
- Email access
- File access (OneDrive)
- Teams-specific APIs
- Group membership information

---

*This OAuth integration follows Azure Bot Framework best practices and Microsoft Graph API guidelines for secure, scalable authentication.*
