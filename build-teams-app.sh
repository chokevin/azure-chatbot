#!/bin/bash

# OAuth Bot - Teams App Package Builder
# This script creates the Teams app package for OAuth Echo Bot

set -e

echo "🎯 Building Teams App Package for OAuth Echo Bot..."

# Navigate to teams-manifest directory
cd "$(dirname "$0")/AzureBotSample/teams-manifest"

# Verify required files exist
if [[ ! -f "manifest.json" ]]; then
    echo "❌ Error: manifest.json not found"
    exit 1
fi

if [[ ! -f "icon-color.png" ]]; then
    echo "❌ Error: icon-color.png not found"
    exit 1
fi

if [[ ! -f "icon-outline.png" ]]; then
    echo "❌ Error: icon-outline.png not found"
    exit 1
fi

# Remove existing package if it exists
rm -f ../OAuth-Echo-Bot-Teams-App.zip

# Create the Teams app package
echo "📦 Creating Teams app package..."
zip -r ../OAuth-Echo-Bot-Teams-App.zip manifest.json icon-color.png icon-outline.png

# Move to root directory
mv ../OAuth-Echo-Bot-Teams-App.zip ../../

# Verify the package
echo "✅ Teams app package created successfully!"
echo "📍 Location: OAuth-Echo-Bot-Teams-App.zip"
echo ""
echo "📋 Package contents:"
unzip -l ../../OAuth-Echo-Bot-Teams-App.zip

echo ""
echo "🚀 Ready to upload to Microsoft Teams!"
echo "   1. Open Microsoft Teams"
echo "   2. Go to Apps → Upload a custom app"
echo "   3. Select OAuth-Echo-Bot-Teams-App.zip"
echo "   4. Click Add to install"
