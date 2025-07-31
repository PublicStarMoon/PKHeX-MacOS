# PKHeX macOS UI Testing Framework

This document describes the automated UI testing framework for PKHeX macOS MAUI application, implementing comprehensive test coverage for critical user workflows.

## Overview

The UI testing framework uses **Appium WebDriver** with **xUnit** to automate testing of the PKHeX macOS application. Demo mode is automatically enabled for all tests to provide access to editors without requiring real save files.

The framework covers two main test scenarios:

1. **Pokemon Creation and Editing**: Complete workflow for creating and editing Pokemon
2. **Inventory Management**: Testing item management and pouch functionality

## Project Structure

```
PKHeX.MAUI.UITests/
├── PKHeX.MAUI.UITests.csproj          # Project file with dependencies
├── Tests/                              # Test classes
│   ├── BaseTest.cs                     # Base class with demo mode auto-enablement
│   ├── PokemonCreationTests.cs        # Pokemon creation workflow tests
│   └── InventoryManagementTests.cs    # Inventory management tests
├── PageObjects/                        # Page Object Model implementation
│   ├── MainPage.cs                     # Main page interactions
│   ├── PokemonBoxPage.cs              # Pokemon box editor page
│   ├── PokemonEditorPage.cs           # Detailed Pokemon editor page
│   └── InventoryEditorPage.cs         # Inventory editor page
├── Utilities/                          # Helper classes and utilities
│   ├── ScreenshotHelper.cs            # Screenshot capture functionality
│   ├── WaitHelper.cs                  # Wait strategies and element detection
│   └── TestDataHelper.cs              # Test data constants and helpers
└── Screenshots/                        # Generated screenshots (runtime)
```

## Test Framework Features

### Automatic Demo Mode
All tests automatically enable demo mode during setup, which:
- Provides access to all editors without requiring real save files
- Ensures consistent test environment across all test runs
- Eliminates the need for save file management in testing

### Test Coverage

### 1. Pokemon Creation Tests (`PokemonCreationTests.cs`)

Tests the complete Pokemon creation and editing workflow.

**Key Test Cases:**
- `PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi`: Full Wishiwashi creation workflow
- `PokemonCreation_NavigationFlow_ShouldAllowBackAndForthNavigation`: Navigation testing
- `PokemonCreation_EmptySlotSelection_ShouldAllowSlotInteraction`: Slot selection testing
- `PokemonCreation_MoveConfiguration_ShouldAllowMoveSelection`: Move editing testing
- `PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties`: Property editing testing
- `PokemonCreation_SaveProcess_ShouldPersistChanges`: Save persistence testing

**Screenshots Captured:**
- Box editor interface
- Empty slot selection
- Pokemon creation process
- Pokemon editor with configured data
- Move configuration
- Save confirmation

### 2. Inventory Management Tests (`InventoryManagementTests.cs`)

Tests inventory management functionality including pouch navigation and item manipulation.

**Key Test Cases:**
- `InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch`: Complete Master Ball addition
- `InventoryManagement_PouchNavigation_ShouldAllowAccessToAllPouches`: Pouch switching
- `InventoryManagement_ItemQuantityManagement_ShouldAllowQuantityChanges`: Quantity modification
- `InventoryManagement_SaveAndPersistence_ShouldMaintainChanges`: Save persistence
- `InventoryManagement_MasterBallSpecific_ShouldHandleMasterBallCorrectly`: Master Ball specific testing
- `InventoryManagement_UIValidation_ShouldDisplayCorrectInformation`: UI validation
- `InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation`: Navigation testing

**Screenshots Captured:**
- Inventory editor interface
- Pouch selection
- Item addition process
- Quantity modification
- Save confirmation
- Navigation between pages

## Technologies Used

### Core Framework
- **xUnit**: Test framework
- **FluentAssertions**: Assertion library for readable test expectations
- **Appium WebDriver**: UI automation driver for macOS applications
- **Selenium Support**: Additional wait helpers and utilities

### Page Object Model
The framework implements the Page Object Model pattern for maintainable and reusable UI interactions:

- **MainPage**: Handles main page interactions including demo mode activation
- **PokemonBoxPage**: Manages Pokemon box editor functionality
- **PokemonEditorPage**: Handles detailed Pokemon editing
- **InventoryEditorPage**: Manages inventory and pouch operations

### Utilities

#### ScreenshotHelper
- Captures screenshots at key test steps
- Supports both full-screen and element-specific screenshots
- Uses ImageSharp for PNG compression
- Automatic timestamping and naming

#### WaitHelper
- Implements robust wait strategies
- Element visibility and clickability checks
- Text appearance waiting
- Retry mechanisms for flaky UI elements

#### TestDataHelper
- Centralized test data constants
- Pokemon species and move definitions
- Item and pouch information
- UI text expectations
- Timeout configurations

## Running Tests Locally

### Prerequisites

1. **macOS with Xcode Command Line Tools**
2. **.NET 8 SDK**
3. **Node.js 18+**
4. **Appium Server 2.x**

### Setup

1. **Install Appium and macOS driver:**
   ```bash
   npm install -g appium@2.10.2
   npm install -g @appium/mac2-driver
   ```

2. **Build the PKHeX MAUI application:**
   ```bash
   cd PKHeX.MAUI
   dotnet build --configuration Release -f net8.0-maccatalyst
   ```

3. **Set the app path environment variable:**
   ```bash
   export PKHEX_APP_PATH="$(pwd)/PKHeX.MAUI/bin/Release/net8.0-maccatalyst/maccatalyst-arm64/PKHeX.MAUI.app"
   ```

4. **Start Appium server:**
   ```bash
   appium server --port 4723
   ```

5. **Run the tests:**
   ```bash
   cd PKHeX.MAUI.UITests
   dotnet test --configuration Release
   ```

### Running Individual Test Classes

```bash
# Pokemon creation tests only
dotnet test --filter "FullyQualifiedName~PokemonCreationTests"

# Inventory management tests only
dotnet test --filter "FullyQualifiedName~InventoryManagementTests"
```

## GitHub Actions Integration

The UI tests are integrated into the GitHub Actions workflow (`macos-ui-tests.yml`) which:

1. **Sets up the macOS environment** with necessary permissions
2. **Installs dependencies** (.NET, Node.js, Appium)
3. **Builds the MAUI application**
4. **Starts Appium server**
5. **Runs UI tests** with screenshot capture
6. **Uploads artifacts** (screenshots, test results, build outputs)
7. **Generates comprehensive test reports**

### Artifacts Generated

- **Screenshots**: Visual evidence of each test step
- **Test Results**: Detailed xUnit test reports
- **Build Artifacts**: Deployable macOS .app bundle
- **Appium Logs**: Debug information for troubleshooting

## Configuration

### Element Locators

The framework uses multiple locator strategies for robustness:

1. **XPath with text content**: `//Button[contains(@Text, 'Demo Mode')]`
2. **Accessibility identifiers**: `By.Id("DemoModeButton")`
3. **Element attributes**: `//Label[@x:Name='StatusLabel']`

### Timeouts

Configurable timeouts for different scenarios:
- **Short waits**: 5 seconds for immediate responses
- **Medium waits**: 15 seconds for page loads
- **Long waits**: 30 seconds for complex operations
- **App startup**: 60 seconds for application launch

### Retry Logic

Robust retry mechanisms for:
- Element interactions
- Screenshot capture
- Navigation operations
- Text input operations

## Best Practices

### Test Design
1. **Independent tests**: Each test can run independently
2. **Setup/Teardown**: Proper resource cleanup
3. **Screenshot documentation**: Visual evidence at each step
4. **Assertion clarity**: Clear, descriptive assertions

### Maintenance
1. **Page Object separation**: UI logic separated from test logic
2. **Data centralization**: Test data in helper classes
3. **Error handling**: Graceful failure handling
4. **Wait strategies**: Avoid fixed sleeps, use smart waits

### CI/CD Integration
1. **Parallel execution**: Tests can run in parallel when possible
2. **Artifact collection**: All relevant artifacts uploaded
3. **Failure analysis**: Comprehensive logging and screenshots
4. **Report generation**: Automated test summaries

## Troubleshooting

### Common Issues

1. **App not found**: Ensure `PKHEX_APP_PATH` is correctly set
2. **Appium connection**: Verify Appium server is running on port 4723
3. **Element not found**: Check locator strategies and wait conditions
4. **Permission errors**: Ensure macOS accessibility permissions are granted

### Debug Information

- **Screenshots**: Captured at each major step
- **Appium logs**: Detailed driver communication logs
- **Test output**: Verbose test execution information
- **Console output**: Application and test debug messages

## Future Enhancements

1. **Cross-platform support**: Extend to Windows UI testing
2. **Performance testing**: Add performance benchmarks
3. **Visual regression**: Compare screenshots across builds
4. **Test data generation**: Dynamic test data creation
5. **Parallel execution**: Optimize test execution time
6. **Mobile testing**: Extend to iOS/Android if MAUI expands

This framework provides a solid foundation for automated UI testing of the PKHeX macOS application, ensuring quality and preventing regressions in critical user workflows.