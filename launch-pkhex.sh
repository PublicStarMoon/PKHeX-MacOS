#!/bin/bash

# PKHeX for macOS Launch Script
# This script builds and launches PKHeX as a native macOS application

echo "🎮 PKHeX for macOS - Native App Launcher"
echo "========================================"

# Change to the MAUI project directory
cd "$(dirname "$0")/PKHeX.MAUI"

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK not found. Please install .NET 8.0 SDK first."
    echo "   Download from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check if MAUI workload is installed
if ! dotnet workload list | grep -q "maui"; then
    echo "❌ Error: MAUI workload not installed."
    echo "   Please run: sudo dotnet workload install maui"
    exit 1
fi

echo "🔨 Building PKHeX for macOS..."
dotnet build --configuration Release

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo "🚀 Launching PKHeX..."
    
    # Try to open the app bundle directly
    APP_PATH="bin/Release/net8.0-maccatalyst/maccatalyst-arm64/PKHeX.MAUI.app"
    if [ -d "$APP_PATH" ]; then
        open "$APP_PATH"
        echo "📱 PKHeX should now be running as a native macOS app!"
        echo ""
        echo "💡 Tip: You can also find the app at:"
        echo "   $(pwd)/$APP_PATH"
        echo ""
        echo "   You can drag this .app file to your Applications folder for easy access."
    else
        echo "⚠️  App bundle not found. Running with dotnet run instead..."
        dotnet run
    fi
else
    echo "❌ Build failed. Please check the error messages above."
    exit 1
fi
