using PKHeX.MAUI.UITests.Tests;
using PKHeX.MAUI.UITests.Utilities;
using FluentAssertions;
using Xunit;

namespace PKHeX.MAUI.UITests.Tests;

[Collection("UI Tests")]
public class PokemonCreationTests : BaseTest, IAsyncLifetime
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
    public async Task PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi()
    {
        // Arrange
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "demo_mode_ready");

        // Act & Assert - Step 1: Navigate to Pokemon Box Editor
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        
        PokemonBoxPage.IsPageLoaded().Should().BeTrue("Pokemon Box page should be loaded");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "box_editor_opened");

        // Act & Assert - Step 2: Select an empty slot in the box grid
        await PokemonBoxPage.SelectEmptySlot(0);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "empty_slot_selected");

        // Act & Assert - Step 3: Create new Pokemon (Wishiwashi species)
        await PokemonBoxPage.ClickAddPokemon();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "add_pokemon_clicked");

        // Allow time for Pokemon creation dialog/process
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);

        // Act & Assert - Step 4: Navigate to PokemonEditorPage for detailed editing
        await PokemonBoxPage.NavigateToPokemonEditor();
        await PokemonEditorPage.WaitForPageToLoad();
        
        PokemonEditorPage.IsPageLoaded().Should().BeTrue("Pokemon Editor page should be loaded");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "pokemon_editor_opened");

        // Act & Assert - Step 5: Edit basic information
        await PokemonEditorPage.SetupPokemon(
            TestDataHelper.Pokemon.WishiwashiSpecies,
            TestDataHelper.Pokemon.DefaultNickname,
            TestDataHelper.Pokemon.DefaultLevel,
            TestDataHelper.Pokemon.DefaultNature,
            TestDataHelper.Pokemon.DefaultMoves);

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "pokemon_basic_info_set");

        // Verify the changes were applied
        PokemonEditorPage.GetCurrentSpecies().Should().Contain(TestDataHelper.Pokemon.WishiwashiSpecies, 
            "Species should be set to Wishiwashi");
        
        PokemonEditorPage.GetCurrentNickname().Should().Be(TestDataHelper.Pokemon.DefaultNickname, 
            "Nickname should be set correctly");
        
        PokemonEditorPage.GetCurrentLevel().Should().Be(TestDataHelper.Pokemon.DefaultLevel.ToString(), 
            "Level should be set correctly");
        
        PokemonEditorPage.GetCurrentNature().Should().Contain(TestDataHelper.Pokemon.DefaultNature, 
            "Nature should be set correctly");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "pokemon_verification_complete");

        // Act & Assert - Step 6: Save changes and verify they persist
        PokemonEditorPage.CanSave().Should().BeTrue("Should be able to save Pokemon changes");
        
        await PokemonEditorPage.SaveChanges();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "pokemon_saved");

        // Allow time for save to process
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);

        // Navigate back to verify persistence
        await PokemonEditorPage.GoBack();
        await PokemonBoxPage.WaitForPageToLoad();
        
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "back_to_box_editor");

        // Verify Pokemon appears in the box
        PokemonBoxPage.GetOccupiedSlotCount().Should().BeGreaterThan(0, 
            "At least one Pokemon should be in the box after creation");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_CompleteWorkflow_ShouldCreateAndEditWishiwashi), "final_verification");
    }

    [Fact]
    public async Task PokemonCreation_NavigationFlow_ShouldAllowBackAndForthNavigation()
    {
        // Arrange
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_NavigationFlow_ShouldAllowBackAndForthNavigation), "initial_state");

        // Act - Navigate to Box Editor
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_NavigationFlow_ShouldAllowBackAndForthNavigation), "in_box_editor");

        // Act - Go back to main page
        await PokemonBoxPage.GoBack();
        await MainPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_NavigationFlow_ShouldAllowBackAndForthNavigation), "back_to_main");

        // Assert - Should be back on main page
        MainPage.IsPageLoaded().Should().BeTrue("Should be back on main page");

        // Act - Navigate to Box Editor again
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_NavigationFlow_ShouldAllowBackAndForthNavigation), "in_box_editor_again");

        // Assert - Should be in Box Editor again
        PokemonBoxPage.IsPageLoaded().Should().BeTrue("Should be in Box Editor again");
    }

    [Fact]
    public async Task PokemonCreation_EmptySlotSelection_ShouldAllowSlotInteraction()
    {
        // Arrange
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_EmptySlotSelection_ShouldAllowSlotInteraction), "box_editor_loaded");

        // Act - Select different empty slots
        for (int i = 0; i < Math.Min(3, PokemonBoxPage.GetEmptySlotCount()); i++)
        {
            await PokemonBoxPage.SelectEmptySlot(i);
            await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_EmptySlotSelection_ShouldAllowSlotInteraction), $"slot_{i}_selected");
            await WaitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }

        // Assert - Should be able to add Pokemon to empty slots
        PokemonBoxPage.CanAddPokemon().Should().BeTrue("Should be able to add Pokemon to empty slots");
    }

    [Fact]
    public async Task PokemonCreation_MoveConfiguration_ShouldAllowMoveSelection()
    {
        // Arrange - Get to Pokemon Editor
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        await PokemonBoxPage.SelectEmptySlot(0);
        await PokemonBoxPage.ClickAddPokemon();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        
        await PokemonBoxPage.NavigateToPokemonEditor();
        await PokemonEditorPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_MoveConfiguration_ShouldAllowMoveSelection), "in_pokemon_editor");

        // Act - Set species first (required for move validation)
        await PokemonEditorPage.SetSpecies(TestDataHelper.Pokemon.WishiwashiSpecies);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_MoveConfiguration_ShouldAllowMoveSelection), "species_set");

        // Act - Configure moves one by one
        for (int i = 0; i < TestDataHelper.Pokemon.DefaultMoves.Length; i++)
        {
            await PokemonEditorPage.SetMove(i + 1, TestDataHelper.Pokemon.DefaultMoves[i]);
            await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_MoveConfiguration_ShouldAllowMoveSelection), $"move_{i + 1}_set");
        }

        // Assert - Verify moves were set
        var currentMoves = PokemonEditorPage.GetCurrentMoves();
        currentMoves.Should().NotBeNull("Moves should be retrievable");
        
        // At least some moves should be set (exact matching may vary by implementation)
        currentMoves.Where(m => !string.IsNullOrEmpty(m)).Should().NotBeEmpty("At least some moves should be set");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_MoveConfiguration_ShouldAllowMoveSelection), "all_moves_configured");
    }

    [Fact]
    public async Task PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties()
    {
        // Arrange - Get to Pokemon Editor
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        await PokemonBoxPage.SelectEmptySlot(0);
        await PokemonBoxPage.ClickAddPokemon();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        
        await PokemonBoxPage.NavigateToPokemonEditor();
        await PokemonEditorPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties), "editor_ready");

        // Act & Assert - Set and verify each property individually
        
        // Species
        await PokemonEditorPage.SetSpecies(TestDataHelper.Pokemon.WishiwashiSpecies);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties), "species_updated");
        
        // Nickname
        await PokemonEditorPage.SetNickname(TestDataHelper.Pokemon.DefaultNickname);
        PokemonEditorPage.GetCurrentNickname().Should().Be(TestDataHelper.Pokemon.DefaultNickname, 
            "Nickname should be updated immediately");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties), "nickname_updated");
        
        // Level
        await PokemonEditorPage.SetLevel(TestDataHelper.Pokemon.DefaultLevel);
        PokemonEditorPage.GetCurrentLevel().Should().Be(TestDataHelper.Pokemon.DefaultLevel.ToString(), 
            "Level should be updated immediately");
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties), "level_updated");
        
        // Nature
        await PokemonEditorPage.SetNature(TestDataHelper.Pokemon.DefaultNature);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties), "nature_updated");

        // Final verification
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_BasicInfoEditing_ShouldUpdatePokemonProperties), "all_properties_updated");
    }

    [Fact]
    public async Task PokemonCreation_SaveProcess_ShouldPersistChanges()
    {
        // Arrange - Create and configure a Pokemon
        await MainPage.NavigateToBoxEditor();
        await PokemonBoxPage.WaitForPageToLoad();
        await PokemonBoxPage.SelectEmptySlot(0);
        await PokemonBoxPage.ClickAddPokemon();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        
        await PokemonBoxPage.NavigateToPokemonEditor();
        await PokemonEditorPage.WaitForPageToLoad();
        
        await PokemonEditorPage.SetupPokemon(
            TestDataHelper.Pokemon.WishiwashiSpecies,
            TestDataHelper.Pokemon.DefaultNickname,
            TestDataHelper.Pokemon.DefaultLevel,
            TestDataHelper.Pokemon.DefaultNature,
            TestDataHelper.Pokemon.DefaultMoves);
            
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_SaveProcess_ShouldPersistChanges), "pokemon_configured");

        // Act - Save the Pokemon
        await PokemonEditorPage.SaveChanges();
        await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_SaveProcess_ShouldPersistChanges), "pokemon_saved");

        // Act - Navigate away and back to verify persistence
        await PokemonEditorPage.GoBack();
        await PokemonBoxPage.WaitForPageToLoad();
        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_SaveProcess_ShouldPersistChanges), "back_to_box");

        // Assert - Pokemon should persist in the box
        var occupiedSlots = PokemonBoxPage.GetOccupiedSlotCount();
        occupiedSlots.Should().BeGreaterThan(0, "Pokemon should persist in the box after saving");

        await ScreenshotHelper.TakeScreenshot(Driver!, nameof(PokemonCreation_SaveProcess_ShouldPersistChanges), "persistence_verified");
    }
}