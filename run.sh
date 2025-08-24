#!/bin/bash

# PKHeX macOS - Unified Run Script
# Usage: ./run.sh [debug|release]
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
        echo "  debug    - Run Debug configuration (default)"
        echo "  release  - Run Release configuration"
        exit 1
        ;;
esac

echo "🚀 Running PKHeX macOS in ${CONFIG} configuration..."
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

# Display runtime information
echo "Using .NET version:"
dotnet --version
echo ""

echo "🎯 Starting PKHeX macOS..."
echo "Press Ctrl+C to stop the application"
echo ""

# Run the application
dotnet run --project PKHeX.MAUI/PKHeX.MAUI.csproj --configuration $CONFIG --framework net8.0-maccatalyst -r maccatalyst-arm64

# Check if the application started successfully
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Application exited normally."
else
    echo ""
    echo "❌ Application exited with an error."
    exit 1
fi
