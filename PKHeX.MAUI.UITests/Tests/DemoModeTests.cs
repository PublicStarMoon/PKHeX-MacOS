using PKHeX.MAUI.UITests.Tests;
using PKHeX.MAUI.UITests.Utilities;
using FluentAssertions;
using Xunit;

namespace PKHeX.MAUI.UITests.Tests;

[Collection("UI Tests")]
public class DemoModeTests : BaseTest, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await SetupAppiumDriver();
    }

    public async Task DisposeAsync()
    {
        await TearDown();
    }

    [Fact]
    public async Task DemoMode_WhenActivated_ShouldEnableAllEditors()
    {
        // Arrange
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenActivated_ShouldEnableAllEditors), "initial_state");

        // Act & Assert - Step 1: Verify MainPage loads correctly
        await MainPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenActivated_ShouldEnableAllEditors), "main_page_loaded");
        
        MainPage.IsPageLoaded().Should().BeTrue("Main page should be loaded");

        // Act & Assert - Step 2: Click Demo Mode button
        await MainPage.ClickDemoMode();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenActivated_ShouldEnableAllEditors), "demo_mode_clicked");

        // Wait for demo mode to be enabled
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);

        // Act & Assert - Step 3: Verify demo save file is created and loaded
        var currentSaveText = MainPage.GetCurrentSaveText();
        currentSaveText.Should().Contain(TestDataHelper.UI.DemoSaveFileText, 
            "Demo save file should be loaded");
        
        var gameVersionText = MainPage.GetGameVersionText();
        gameVersionText.Should().Contain(TestDataHelper.UI.DemoGameVersion, 
            "Game version should be shown for demo mode");

        var trainerNameText = MainPage.GetTrainerNameText();
        trainerNameText.Should().Contain(TestDataHelper.UI.DemoTrainerName, 
            "Trainer name should be set to Demo");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenActivated_ShouldEnableAllEditors), "demo_save_loaded");

        // Act & Assert - Step 4: Verify all editor buttons become enabled
        MainPage.AreEditorButtonsEnabled().Should().BeTrue("All editor buttons should be enabled after demo mode");
        MainPage.IsDemoModeEnabled().Should().BeTrue("Demo mode button should show as enabled");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenActivated_ShouldEnableAllEditors), "editors_enabled");

        // Act & Assert - Step 5: Verify status message
        var statusText = MainPage.GetStatusText();
        statusText.Should().Contain("demo").And.Contain("enabled");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenActivated_ShouldEnableAllEditors), "final_state");
    }

    [Fact]
    public async Task DemoMode_WhenEnabled_ShouldAllowNavigationToBoxEditor()
    {
        // Arrange - Enable demo mode first
        await MainPage.WaitForPageToLoad();
        await MainPage.ClickDemoMode();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToBoxEditor), "demo_mode_ready");

        // Act - Navigate to Box Editor
        await MainPage.NavigateToBoxEditor();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToBoxEditor), "navigating_to_box_editor");

        // Assert - Verify Box Editor page loads
        await PokemonBoxPage.WaitForPageToLoad();
        PokemonBoxPage.IsPageLoaded().Should().BeTrue("Pokemon Box page should be loaded");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToBoxEditor), "box_editor_loaded");

        // Verify we can navigate back
        await PokemonBoxPage.GoBack();
        await MainPage.WaitForPageToLoad();
        MainPage.IsPageLoaded().Should().BeTrue("Should be able to navigate back to main page");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToBoxEditor), "back_to_main");
    }

    [Fact]
    public async Task DemoMode_WhenEnabled_ShouldAllowNavigationToInventoryEditor()
    {
        // Arrange - Enable demo mode first
        await MainPage.WaitForPageToLoad();
        await MainPage.ClickDemoMode();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToInventoryEditor), "demo_mode_ready");

        // Act - Navigate to Inventory Editor
        await MainPage.NavigateToInventoryEditor();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToInventoryEditor), "navigating_to_inventory");

        // Assert - Verify Inventory Editor page loads
        await InventoryEditorPage.WaitForPageToLoad();
        InventoryEditorPage.IsPageLoaded().Should().BeTrue("Inventory Editor page should be loaded");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToInventoryEditor), "inventory_editor_loaded");

        // Verify we can navigate back
        await InventoryEditorPage.GoBack();
        await MainPage.WaitForPageToLoad();
        MainPage.IsPageLoaded().Should().BeTrue("Should be able to navigate back to main page");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_WhenEnabled_ShouldAllowNavigationToInventoryEditor), "back_to_main");
    }

    [Fact]
    public async Task DemoMode_BeforeActivation_ShouldHaveDisabledEditors()
    {
        // Arrange & Act
        await MainPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_BeforeActivation_ShouldHaveDisabledEditors), "initial_state");

        // Assert - Editor buttons should be disabled initially
        MainPage.AreEditorButtonsEnabled().Should().BeFalse("Editor buttons should be disabled before demo mode");
        MainPage.IsDemoModeEnabled().Should().BeFalse("Demo mode should not be enabled initially");

        // Verify current save shows no file loaded
        var currentSaveText = MainPage.GetCurrentSaveText();
        currentSaveText.Should().Contain("No save file");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_BeforeActivation_ShouldHaveDisabledEditors), "verified_disabled_state");
    }

    [Fact]
    public async Task DemoMode_ActivationFlow_ShouldHandleConfirmationDialogs()
    {
        // Arrange
        await MainPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_ActivationFlow_ShouldHandleConfirmationDialogs), "before_demo_activation");

        // Act - Click demo mode button and handle dialogs
        await MainPage.ClickDemoMode();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_ActivationFlow_ShouldHandleConfirmationDialogs), "after_demo_click");

        // Allow time for dialogs and processing
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.LongWait);

        // Assert - Demo mode should be successfully enabled
        MainPage.IsDemoModeEnabled().Should().BeTrue("Demo mode should be enabled after confirmation");
        
        var statusText = MainPage.GetStatusText();
        statusText.Should().NotContain("No save file", "Status should no longer show 'No save file'");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(DemoMode_ActivationFlow_ShouldHandleConfirmationDialogs), "demo_mode_confirmed");
    }
}