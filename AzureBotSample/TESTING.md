# Testing the Azure Bot Service Sample

## Running the Bot Locally

1. **Start the application**:
   ```bash
   cd AzureBotSample
   dotnet run
   ```

2. **The application will start on port 5000 (HTTP) or 5001 (HTTPS)**
   - Bot endpoint: `http://localhost:5000/api/messages`
   - Health endpoint: `http://localhost:5000/health`
   - Swagger UI: `http://localhost:5000/swagger` (development only)

## Testing with Bot Framework Emulator

1. **Download Bot Framework Emulator**:
   - Get it from: https://github.com/Microsoft/BotFramework-Emulator/releases
   - Install and run the emulator

2. **Connect to your local bot**:
   - Open Bot Framework Emulator
   - Click "Open Bot"
   - Enter Bot URL: `http://localhost:5000/api/messages`
   - Leave Microsoft App ID and Microsoft App Password empty for local testing
   - Click "Connect"

3. **Test the bot**:
   - Type any message (e.g., "Hello")
   - The bot should echo back: "Echo: Hello"
   - When you first connect, you should see a welcome message

## Example Conversation

```
Bot: Hello and welcome! I'm an Echo Bot. Send me a message and I'll echo it back to you.
You: Hello Bot!
Bot: Echo: Hello Bot!
You: How are you today?
Bot: Echo: How are you today?
```

## Troubleshooting

### Common Issues

1. **Port already in use**:
   - Change the port in `appsettings.json` or `launchSettings.json`
   - Or stop other applications using port 5000

2. **Emulator connection issues**:
   - Ensure the bot is running (`dotnet run`)
   - Check the endpoint URL matches: `http://localhost:5000/api/messages`
   - For local testing, leave App ID and Password empty

3. **Build errors**:
   - Run `dotnet restore` to ensure all packages are installed
   - Check that .NET 9.0 SDK is installed

### Health Check

Test if the bot is running by visiting: `http://localhost:5000/health`

You should see:
```json
{
  "status": "healthy",
  "timestamp": "2025-01-17T..."
}
```

## Next Steps for Azure Deployment

1. **Register your bot in Azure**:
   - Create an Azure Bot Service resource
   - Get the Microsoft App ID and App Password

2. **Update configuration**:
   - Add the credentials to `appsettings.json` or Azure App Service configuration

3. **Deploy to Azure App Service**:
   - Publish the application to Azure App Service
   - Configure the endpoint in Azure Bot Service
