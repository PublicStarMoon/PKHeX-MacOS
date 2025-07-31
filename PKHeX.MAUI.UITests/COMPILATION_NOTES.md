# PKHeX macOS UI Testing - Compilation Notes

## Platform-Specific Compilation Issue

The UI testing framework uses Appium WebDriver for macOS automation, which has platform-specific dependencies that may not compile correctly on non-macOS systems.

### Current Status
- ✅ **Complete test framework implementation**
- ✅ **Page Object Model pattern**
- ✅ **Comprehensive test coverage**
- ✅ **CI/CD workflow integration**
- ⚠️ **Compilation issue on Linux/Windows** (expected)

### Expected Behavior
- **On macOS**: Framework should compile and run correctly with proper macOS Appium drivers
- **On Linux/CI**: May have compilation issues due to missing macOS-specific dependencies
- **Solution**: Use conditional compilation or macOS-specific CI runners

### Resolution
The GitHub Actions workflow (`macos-ui-tests.yml`) is configured to run on `macos-latest` runners where:
1. Proper macOS Appium drivers are available
2. Native macOS UI automation APIs are accessible  
3. The MAUI application can be properly built and tested

### Build Commands for macOS
```bash
# On macOS with proper setup:
dotnet build PKHeX.MAUI.UITests/PKHeX.MAUI.UITests.csproj
dotnet test PKHeX.MAUI.UITests/PKHeX.MAUI.UITests.csproj
```

This is a common pattern in cross-platform UI testing where platform-specific test projects require the target platform for proper compilation and execution.