#!/bin/bash

# Legacy script - redirects to unified run script in Release mode
# This script is kept for backward compatibility

echo "⚠️  This script is deprecated. Please use the unified run script instead:"
echo "   ./run.sh release"
echo ""
echo "Redirecting to unified script..."
echo ""

# Change back to root directory and redirect to the new unified script
cd "$(dirname "$0")"
./run.sh release
