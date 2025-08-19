#!/bin/bash

# Legacy script - redirects to unified run script in Release mode
# This script is kept for backward compatibility

echo "⚠️  This script is deprecated. Please use the unified run script instead:"
echo "   ./run.sh release"
echo ""
echo "Redirecting to unified script..."
echo ""

# Redirect to the new unified script
./run.sh release
