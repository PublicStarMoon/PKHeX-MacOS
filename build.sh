#!/bin/bash

# PKHeX macOS - Unified Build Script
# Usage: ./build.sh [debug|release]
# Default: debug

set -e  # Exit on any error

# Parse command line arguments
CONFIGURATION="${1:-debug}"

# Normalize configuration name (convert to lowercase)
CONFIGURATION=$(echo "$CONFIGURATION" | tr '[:upper:]' '[:lower:]')

case "$CONFIGURATION" in
    "debug"|"d")
        CONFIG="Debug"
        ;;
    "release"|"rel"|"r")
        CONFIG="Release"
        ;;
    *)
        echo "Error: Invalid configuration '${1:-debug}'"
        echo "Usage: $0 [debug|release]"
        echo "  debug    - Build in Debug configuration (default)"
        echo "  release  - Build in Release configuration"
        exit 1
        ;;
esac

echo "🔨 Building PKHeX macOS in ${CONFIG} configuration..."
echo "=================================================="

# Check if we're in the right directory
if [ ! -f "PKHeX.sln" ]; then
    echo "Error: PKHeX.sln not found. Please run this script from the PKHeX-MacOS root directory."
    exit 1
fi

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed or not in PATH."
    echo "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Display .NET version
echo "Using .NET version:"
dotnet --version
echo ""

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean PKHeX.MAUI/PKHeX.MAUI.csproj --configuration $CONFIG --verbosity quiet

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore PKHeX.MAUI/PKHeX.MAUI.csproj --verbosity quiet

# Build the project
echo "🔨 Building PKHeX.MAUI in ${CONFIG} configuration..."
dotnet build PKHeX.MAUI/PKHeX.MAUI.csproj --configuration $CONFIG --no-restore -r maccatalyst-arm64

# Check build result
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Build completed successfully!"
    echo "Configuration: $CONFIG"
    echo "Runtime: maccatalyst-arm64"
    echo "Output: PKHeX.MAUI/bin/$CONFIG/net8.0-maccatalyst/maccatalyst-arm64/"
    echo ""
    echo "To run the application, use:"
    echo "  ./run.sh $CONFIGURATION"
else
    echo ""
    echo "❌ Build failed!"
    exit 1
fi
