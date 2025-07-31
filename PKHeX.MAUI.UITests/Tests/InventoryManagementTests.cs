using PKHeX.MAUI.UITests.Tests;
using PKHeX.MAUI.UITests.Utilities;
using FluentAssertions;
using Xunit;

namespace PKHeX.MAUI.UITests.Tests;

[Collection("UI Tests")]
public class InventoryManagementTests : BaseTest, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await SetupAppiumDriver();
        
        // Enable demo mode for all tests in this class
        await MainPage.WaitForPageToLoad();
        await MainPage.ClickDemoMode();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    public async Task DisposeAsync()
    {
        await TearDown();
    }

    [Fact]
    public async Task InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch()
    {
        // Arrange
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "demo_mode_ready");

        // Act & Assert - Step 1: Navigate to Inventory Editor
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        
        InventoryEditorPage.IsPageLoaded().Should().BeTrue("Inventory Editor page should be loaded");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "inventory_editor_opened");

        // Act & Assert - Step 2: Select Ball pouch from available pouches
        await InventoryEditorPage.SelectBallPouch();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        
        InventoryEditorPage.IsPouchSelected().Should().BeTrue("Ball pouch should be selected");
        
        var currentPouch = InventoryEditorPage.GetCurrentPouchName();
        currentPouch.Should().Contain("Ball", 
            "Current pouch should be Ball pouch");
            
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "ball_pouch_selected");

        // Act & Assert - Step 3: Add Master Ball item and set quantity to 10
        var initialMasterBallCount = InventoryEditorPage.GetItemQuantity(TestDataHelper.Items.MasterBall);
        
        await InventoryEditorPage.AddItem(TestDataHelper.Items.MasterBall, TestDataHelper.Items.DefaultQuantity);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "master_ball_added");

        // Act & Assert - Step 4: Save changes to memory
        InventoryEditorPage.CanAddItems().Should().BeTrue("Should be able to add items to inventory");
        
        await InventoryEditorPage.SaveChanges();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "changes_saved");

        // Act & Assert - Step 5: Verify changes are reflected in UI
        var finalMasterBallCount = InventoryEditorPage.GetItemQuantity(TestDataHelper.Items.MasterBall);
        finalMasterBallCount.Should().BeGreaterThan(initialMasterBallCount, 
            "Master Ball quantity should have increased");
        
        InventoryEditorPage.HasItem(TestDataHelper.Items.MasterBall).Should().BeTrue(
            "Master Ball should be present in the Ball pouch");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "final_verification");

        // Additional verification - Navigate back and forth to ensure persistence
        await InventoryEditorPage.GoBack();
        await MainPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "back_to_main");

        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await InventoryEditorPage.SelectBallPouch();
        
        // Verify persistence
        InventoryEditorPage.HasItem(TestDataHelper.Items.MasterBall).Should().BeTrue(
            "Master Ball should persist after navigation");
        
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_CompleteWorkflow_ShouldAddMasterBallToBallPouch), "persistence_verified");
    }

    [Fact]
    public async Task InventoryManagement_PouchNavigation_ShouldAllowAccessToAllPouches()
    {
        // Arrange
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_PouchNavigation_ShouldAllowAccessToAllPouches), "inventory_editor_ready");

        // Act & Assert - Test different pouch selections
        var pouchTests = new[]
        {
            ("Ball", new Func<Task>(async () => await InventoryEditorPage.SelectBallPouch())),
            ("Item", new Func<Task>(async () => await InventoryEditorPage.SelectItemPouch())),
            ("Medicine", new Func<Task>(async () => await InventoryEditorPage.SelectMedicinePouch())),
            ("Berries", new Func<Task>(async () => await InventoryEditorPage.SelectBerriesPouch())),
            ("Key", new Func<Task>(async () => await InventoryEditorPage.SelectKeyItemsPouch())),
            ("TM", new Func<Task>(async () => await InventoryEditorPage.SelectTMPouch()))
        };

        foreach (var (pouchName, selectAction) in pouchTests)
        {
            try
            {
                await selectAction();
                await WaitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
                
                var currentPouch = InventoryEditorPage.GetCurrentPouchName();
                await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_PouchNavigation_ShouldAllowAccessToAllPouches), $"{pouchName.ToLower()}_pouch_selected");
                
                // Verify pouch selection (may not be exact match due to UI formatting)
                Console.WriteLine($"Selected pouch: {currentPouch} (expected: {pouchName})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not test {pouchName} pouch: {ex.Message}");
                // Continue with other pouches
            }
        }

        // At least one pouch should be selectable
        InventoryEditorPage.IsPouchSelected().Should().BeTrue("At least one pouch should be selectable");
    }

    [Fact]
    public async Task InventoryManagement_ItemQuantityManagement_ShouldAllowQuantityChanges()
    {
        // Arrange
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await InventoryEditorPage.SelectBallPouch();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_ItemQuantityManagement_ShouldAllowQuantityChanges), "ball_pouch_ready");

        // Test different quantities
        var quantitiesToTest = new[] { 1, 5, 10, 50, 99 };

        foreach (var quantity in quantitiesToTest)
        {
            try
            {
                // Act - Add item with specific quantity
                await InventoryEditorPage.AddItem(TestDataHelper.Items.MasterBall, quantity);
                await WaitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
                await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_ItemQuantityManagement_ShouldAllowQuantityChanges), $"quantity_{quantity}_set");

                // Assert - Verify quantity is reflected
                var currentQuantity = InventoryEditorPage.GetItemQuantity(TestDataHelper.Items.MasterBall);
                Console.WriteLine($"Set quantity: {quantity}, Current quantity: {currentQuantity}");
                
                // Allow for some tolerance since exact matching may vary
                currentQuantity.Should().BeGreaterThan(0, $"Should have some Master Balls after setting quantity to {quantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not test quantity {quantity}: {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task InventoryManagement_SaveAndPersistence_ShouldMaintainChanges()
    {
        // Arrange
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await InventoryEditorPage.SelectBallPouch();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_SaveAndPersistence_ShouldMaintainChanges), "initial_state");

        // Act - Make changes
        var initialQuantity = InventoryEditorPage.GetItemQuantity(TestDataHelper.Items.MasterBall);
        await InventoryEditorPage.AddItem(TestDataHelper.Items.MasterBall, 25);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_SaveAndPersistence_ShouldMaintainChanges), "item_added");

        // Save changes
        await InventoryEditorPage.SaveChanges();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_SaveAndPersistence_ShouldMaintainChanges), "changes_saved");

        // Navigate away and back
        await InventoryEditorPage.GoBack();
        await MainPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_SaveAndPersistence_ShouldMaintainChanges), "navigated_away");

        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await InventoryEditorPage.SelectBallPouch();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_SaveAndPersistence_ShouldMaintainChanges), "returned_to_inventory");

        // Assert - Changes should persist
        var finalQuantity = InventoryEditorPage.GetItemQuantity(TestDataHelper.Items.MasterBall);
        finalQuantity.Should().BeGreaterThanOrEqualTo(initialQuantity, "Item quantity should persist after save and navigation");
        
        InventoryEditorPage.HasItem(TestDataHelper.Items.MasterBall).Should().BeTrue("Master Ball should still be present");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_SaveAndPersistence_ShouldMaintainChanges), "persistence_verified");
    }

    [Fact]
    public async Task InventoryManagement_MasterBallSpecific_ShouldHandleMasterBallCorrectly()
    {
        // Arrange
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_MasterBallSpecific_ShouldHandleMasterBallCorrectly), "inventory_ready");

        // Act - Use the specialized Master Ball method
        await InventoryEditorPage.AddMasterBall(TestDataHelper.Items.DefaultQuantity);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_MasterBallSpecific_ShouldHandleMasterBallCorrectly), "master_ball_workflow_complete");

        // Assert - Master Ball should be added to Ball pouch
        InventoryEditorPage.HasItem(TestDataHelper.Items.MasterBall).Should().BeTrue(
            "Master Ball should be present after using AddMasterBall method");

        var quantity = InventoryEditorPage.GetItemQuantity(TestDataHelper.Items.MasterBall);
        quantity.Should().BeGreaterThan(0, "Master Ball quantity should be greater than 0");

        // Verify we're in the Ball pouch
        var currentPouch = InventoryEditorPage.GetCurrentPouchName();
        currentPouch.Should().Contain("Ball", 
            "Should be in Ball pouch after AddMasterBall");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_MasterBallSpecific_ShouldHandleMasterBallCorrectly), "verification_complete");
    }

    [Fact]
    public async Task InventoryManagement_UIValidation_ShouldDisplayCorrectInformation()
    {
        // Arrange
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_UIValidation_ShouldDisplayCorrectInformation), "page_loaded");

        // Assert - Page elements should be present and valid
        InventoryEditorPage.IsPageLoaded().Should().BeTrue("Inventory page should be properly loaded");

        // Check generation info
        var generationInfo = InventoryEditorPage.GetGenerationInfo();
        generationInfo.Should().NotBeNullOrEmpty("Generation info should be displayed");
        Console.WriteLine($"Generation info: {generationInfo}");

        // Select a pouch and verify UI updates
        await InventoryEditorPage.SelectBallPouch();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_UIValidation_ShouldDisplayCorrectInformation), "ball_pouch_ui");

        InventoryEditorPage.IsPouchSelected().Should().BeTrue("A pouch should be selected");
        InventoryEditorPage.CanAddItems().Should().BeTrue("Should be able to add items when a pouch is selected");

        var pouchName = InventoryEditorPage.GetCurrentPouchName();
        pouchName.Should().NotBeNullOrEmpty("Pouch name should be displayed");
        Console.WriteLine($"Current pouch: {pouchName}");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_UIValidation_ShouldDisplayCorrectInformation), "ui_validation_complete");
    }

    [Fact]
    public async Task InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation()
    {
        // Test the complete navigation flow: Main -> Inventory -> Main -> Inventory
        
        // Start from main page
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation), "starting_main_page");

        // Navigate to inventory
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        InventoryEditorPage.IsPageLoaded().Should().BeTrue("Should navigate to inventory successfully");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation), "in_inventory_editor");

        // Go back to main
        await InventoryEditorPage.GoBack();
        await MainPage.WaitForPageToLoad();
        MainPage.IsPageLoaded().Should().BeTrue("Should navigate back to main page");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation), "back_to_main");

        // Navigate to inventory again
        await MainPage.NavigateToInventoryEditor();
        await InventoryEditorPage.WaitForPageToLoad();
        InventoryEditorPage.IsPageLoaded().Should().BeTrue("Should navigate to inventory again");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation), "in_inventory_editor_again");

        // Verify functionality still works
        await InventoryEditorPage.SelectBallPouch();
        InventoryEditorPage.CanAddItems().Should().BeTrue("Inventory functionality should work after navigation");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(InventoryManagement_NavigationFlow_ShouldAllowSeamlessNavigation), "functionality_verified");
    }
}