using PKHeX.Core;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;
using PKHeX.MAUI.Services;

namespace PKHeX.MAUI.Views;

public partial class PokemonBoxPage : ContentPage
{
    private SaveFile _saveFile;
    private int _currentBox = 0;
    private readonly List<Button> _pokemonSlots = new();
    private PKM?[] _currentBoxPokemon = new PKM[30];

    public PokemonBoxPage(SaveFile saveFile)
    {
        InitializeComponent();
        _saveFile = saveFile;
        
        SetupUI();
        LoadBoxData();
    }

    private void SetupUI()
    {
        // Setup box picker
        BoxPicker.Items.Clear();
        for (int i = 0; i < _saveFile.BoxCount; i++)
        {
            var boxName = _saveFile.GetBoxName(i);
            BoxPicker.Items.Add($"Box {i + 1}: {boxName}");
        }
        BoxPicker.SelectedIndex = 0;

        // Setup Pokemon grid (6x5 layout for 30 Pokemon per box)
        CreatePokemonGrid();
    }

    private void CreatePokemonGrid()
    {
        PokemonGrid.Children.Clear();
        _pokemonSlots.Clear();

        // Create 6 columns and 5 rows
        for (int col = 0; col < 6; col++)
        {
            PokemonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        for (int row = 0; row < 5; row++)
        {
            PokemonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
        }

        // Create 30 Pokemon slots (standard box size)
        int slotsPerBox = 30; // Standard Pokemon box size
        for (int i = 0; i < slotsPerBox; i++)
        {
            int row = i / 6;
            int col = i % 6;
            
            var button = new Button
            {
                Text = "Empty",
                BackgroundColor = Color.FromArgb("#ECF0F1"),
                TextColor = Color.FromArgb("#BDC3C7"),
                BorderColor = Color.FromArgb("#BDC3C7"),
                BorderWidth = 1,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                Padding = new Thickness(4),
                HeightRequest = 80,
                WidthRequest = 85,
                CornerRadius = 6
            };

            int slotIndex = i; // Capture the slot index for the closure
            button.Clicked += (s, e) => OnPokemonSlotClicked(slotIndex);
            
            Grid.SetRow(button, row);
            Grid.SetColumn(button, col);
            
            PokemonGrid.Children.Add(button);
            _pokemonSlots.Add(button);
        }
    }

    private void LoadBoxData()
    {
        try
        {
            // Use standard box size of 30 slots
            int slotsPerBox = 30;
            _currentBoxPokemon = new PKM[slotsPerBox];
            
            // Load Pokemon from current box
            for (int slot = 0; slot < slotsPerBox; slot++)
            {
                try
                {
                    var pkm = _saveFile.GetBoxSlotAtIndex(_currentBox, slot);
                    _currentBoxPokemon[slot] = pkm;
                    UpdatePokemonSlot(slot, pkm);
                }
                catch (Exception ex)
                {
                    // If we can't load a specific slot, mark it as empty
                    _currentBoxPokemon[slot] = null;
                    UpdatePokemonSlot(slot, null);
                    StatusLabel.Text = $"Warning: Could not load slot {slot}: {ex.Message}";
                }
            }

            UpdateBoxCountLabel();
            StatusLabel.Text = $"Loaded Box {_currentBox + 1}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error loading box data: {ex.Message}";
        }
    }

    private void UpdatePokemonSlot(int slot, PKM? pokemon)
    {
        if (slot < 0 || slot >= _pokemonSlots.Count) return;

        var button = _pokemonSlots[slot];

        if (pokemon == null || pokemon.Species == 0)
        {
            button.Text = "Empty";
            button.BackgroundColor = Color.FromArgb("#ECF0F1");
            button.TextColor = Color.FromArgb("#BDC3C7");
        }
        else
        {
            button.Text = $"{GetSpeciesName(pokemon.Species)}\nLv.{pokemon.CurrentLevel}";
            button.TextColor = Colors.White;
            
            // Color coding based on Pokemon properties
            if (pokemon.IsShiny)
                button.BackgroundColor = Color.FromArgb("#F39C12"); // Golden orange for shiny
            else if (pokemon.IsEgg)
                button.BackgroundColor = Color.FromArgb("#E91E63"); // Pink for eggs
            else
                button.BackgroundColor = Color.FromArgb("#3498DB"); // Blue for normal Pokemon
        }
    }

    private void UpdateBoxCountLabel()
    {
        int count = 0;
        int slotsPerBox = 30;
        
        for (int slot = 0; slot < slotsPerBox && slot < _currentBoxPokemon.Length; slot++)
        {
            try
            {
                var pkm = _currentBoxPokemon[slot];
                if (pkm != null && pkm.Species != 0)
                {
                    count++;
                }
            }
            catch
            {
                // Skip invalid slots
            }
        }
        BoxCountLabel.Text = $"{count}/{slotsPerBox}";
    }

    private async void OnPokemonSlotClicked(int slot)
    {
        try
        {
            // Validate slot index
            if (slot < 0 || slot >= _currentBoxPokemon.Length)
            {
                await DisplayAlert("Error", $"Invalid slot index: {slot}", "OK");
                return;
            }

            var pokemon = _currentBoxPokemon[slot];
            
            if (pokemon == null || pokemon.Species == 0)
            {
                // Empty slot - offer to add a Pokemon
                bool result = await DisplayAlert("Empty Slot", 
                    $"Slot {slot + 1} is empty. Would you like to add a Pokemon?", 
                    "Add Pokemon", "Cancel");
                
                if (result)
                {
                    await AddPokemonToSlot(slot);
                }
            }
            else
            {
                // Show Pokemon details and edit options
                await ShowPokemonEditor(slot, pokemon);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to process slot {slot}: {ex.Message}\n\nStack trace: {ex.StackTrace}", "OK");
            StatusLabel.Text = $"Error accessing slot {slot}: {ex.Message}";
        }
    }

    private async Task AddPokemonToSlot(int slot)
    {
        try
        {
            // For now, create a simple demo Pokemon
            var result = await DisplayPromptAsync("Add Pokemon", 
                "Enter Pokemon species number (1-1010):", 
                placeholder: "25 for Pikachu");

            if (result != null && int.TryParse(result, out int species))
            {
                if (species <= 0 || species > 1010)
                {
                    await DisplayAlert("Error", "Invalid species number. Please enter a number between 1 and 1010.", "OK");
                    return;
                }

                // Create a new Pokemon based on the save file's format
                var newPokemon = _saveFile.BlankPKM;
                newPokemon.Species = (ushort)species;
                newPokemon.CurrentLevel = 5;
                newPokemon.Heal();
                
                // Place in box
                _saveFile.SetBoxSlotAtIndex(newPokemon, _currentBox, slot);
                _currentBoxPokemon[slot] = newPokemon;
                
                UpdatePokemonSlot(slot, newPokemon);
                UpdateBoxCountLabel();
                StatusLabel.Text = $"Added {GetSpeciesName((ushort)species)} to slot {slot + 1}";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to add Pokemon: {ex.Message}", "OK");
        }
    }

    private async Task ShowPokemonEditor(int slot, PKM pokemon)
    {
        try
        {
            var options = new string[]
            {
                "View Details",
                "Edit in Detail Editor",
                "Edit Level",
                "Heal Pokemon", 
                "Delete Pokemon",
                "Cancel"
            };

            var choice = await DisplayActionSheet($"{GetSpeciesName(pokemon.Species)} (Slot {slot + 1})", 
                "Cancel", null, options);

            switch (choice)
            {
                case "View Details":
                    await ShowPokemonDetails(pokemon);
                    break;
                case "Edit in Detail Editor":
                    await OpenDetailedEditor(slot, pokemon);
                    break;
                case "Edit Level":
                    await EditPokemonLevel(slot, pokemon);
                    break;
                case "Heal Pokemon":
                    pokemon.Heal();
                    _saveFile.SetBoxSlotAtIndex(pokemon, _currentBox, slot);
                    StatusLabel.Text = $"Pokemon has been healed!";
                    break;
                case "Delete Pokemon":
                    await DeletePokemon(slot);
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to edit Pokemon: {ex.Message}", "OK");
        }
    }

    private async Task ShowPokemonDetails(PKM pokemon)
    {
        var details = $"Species: {GetSpeciesName(pokemon.Species)}\n" +
                     $"Level: {pokemon.CurrentLevel}\n" +
                     $"Nature: {pokemon.Nature}\n" +
                     $"Ability: {pokemon.Ability}\n" +
                     $"Shiny: {(pokemon.IsShiny ? "Yes" : "No")}\n" +
                     $"OT: {pokemon.OT_Name}\n" +
                     $"TID: {pokemon.TID16}";

        await DisplayAlert($"{GetSpeciesName(pokemon.Species)} Details", details, "OK");
    }

    private async Task OpenDetailedEditor(int slot, PKM pokemon)
    {
        try
        {
            var editorPage = new PokemonEditorPage(pokemon, _saveFile, _currentBox, slot);
            await Navigation.PushAsync(editorPage);
            
            // When we return, refresh the slot display
            // Note: The pokemon object is modified by reference in the editor
            // The editor now saves to the slot itself, but we still refresh the display
            UpdatePokemonSlot(slot, pokemon);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open detailed editor: {ex.Message}", "OK");
        }
    }

    private async Task EditPokemonLevel(int slot, PKM pokemon)
    {
        var result = await DisplayPromptAsync("Edit Level", 
            $"Enter new level for {GetSpeciesName(pokemon.Species)}:", 
            initialValue: pokemon.CurrentLevel.ToString());

        if (result != null && int.TryParse(result, out int newLevel))
        {
            if (newLevel < 1 || newLevel > 100)
            {
                await DisplayAlert("Error", "Level must be between 1 and 100.", "OK");
                return;
            }

            pokemon.CurrentLevel = newLevel;
            pokemon.Heal(); // Recalculate stats
            
            _saveFile.SetBoxSlotAtIndex(pokemon, _currentBox, slot);
            UpdatePokemonSlot(slot, pokemon);
            StatusLabel.Text = $"Level changed to {newLevel}";
        }
    }



    private async Task DeletePokemon(int slot)
    {
        var pokemon = _currentBoxPokemon[slot];
        
        bool confirm = await DisplayAlert("Delete Pokemon", 
            $"Are you sure you want to delete Species {pokemon!.Species}? This cannot be undone!", 
            "Delete", "Cancel");

        if (confirm)
        {
            var blankPokemon = _saveFile.BlankPKM;
            _saveFile.SetBoxSlotAtIndex(blankPokemon, _currentBox, slot);
            _currentBoxPokemon[slot] = null;
            
            UpdatePokemonSlot(slot, null);
            UpdateBoxCountLabel();
            StatusLabel.Text = $"Pokemon has been deleted.";
        }
    }

    private void OnBoxChanged(object sender, EventArgs e)
    {
        if (BoxPicker.SelectedIndex >= 0)
        {
            _currentBox = BoxPicker.SelectedIndex;
            LoadBoxData();
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // The changes are already saved to the SaveFile object through individual operations
            // Mark the save file as edited and track unsaved changes
            _saveFile.State.Edited = true;
            PageManager.MarkChangesUnsaved();
            
            StatusLabel.Text = "Changes saved to memory. Use Export Save from main menu to save to file.";
            await DisplayAlert("Success", 
                "Box changes saved to memory!\n\n" +
                "To persist changes permanently:\n" +
                "• Go back to Main Page\n" +
                "• Click 'Export Save' button\n" +
                "• Save the file to disk", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
        }
    }

    private async void OnExportBoxClicked(object sender, EventArgs e)
    {
        try
        {
            // Count non-empty Pokemon
            var pokemon = _currentBoxPokemon.Where(p => p != null && p.Species != 0).ToList();
            
            if (pokemon.Count == 0)
            {
                await DisplayAlert("Info", "This box is empty - nothing to export.", "OK");
                return;
            }

            await DisplayAlert("Export Box", 
                $"Box {_currentBox + 1} contains {pokemon.Count} Pokemon.\n\n" +
                "Individual Pokemon export functionality can be added in future updates.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to export box: {ex.Message}", "OK");
        }
    }

    private async void OnClearBoxClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Clear Box", 
            $"Are you sure you want to clear all Pokemon from Box {_currentBox + 1}? This cannot be undone!", 
            "Clear", "Cancel");

        if (confirm)
        {
            try
            {
                var blankPokemon = _saveFile.BlankPKM;
                
                for (int slot = 0; slot < 30; slot++) // Standard box size
                {
                    _saveFile.SetBoxSlotAtIndex(blankPokemon, _currentBox, slot);
                    _currentBoxPokemon[slot] = null;
                    UpdatePokemonSlot(slot, null);
                }

                UpdateBoxCountLabel();
                StatusLabel.Text = $"Box {_currentBox + 1} has been cleared.";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to clear box: {ex.Message}", "OK");
            }
        }
    }

    private async void OnAddPokemonClicked(object sender, EventArgs e)
    {
        // Find first empty slot
        int emptySlot = -1;
        for (int i = 0; i < 30; i++) // Standard box size
        {
            if (_currentBoxPokemon[i] == null || _currentBoxPokemon[i]!.Species == 0)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot == -1)
        {
            await DisplayAlert("Box Full", "This box is full! Please clear a slot first or switch to another box.", "OK");
            return;
        }

        await AddPokemonToSlot(emptySlot);
    }

    /// <summary>
    /// Gets the multilingual species name in both English and Chinese
    /// </summary>
    private string GetSpeciesName(ushort species)
    {
        try
        {
            if (species == 0) return "Empty";

            // Get English name (language ID 2)
            var englishName = SpeciesName.GetSpeciesName(species, 2);

            // Get Chinese Traditional name (language ID 10) or Simplified (language ID 9) as fallback
            var chineseName = SpeciesName.GetSpeciesName(species, 10);
            if (string.IsNullOrEmpty(chineseName))
                chineseName = SpeciesName.GetSpeciesName(species, 9);

            // Return format: "English Name (Chinese Name)" or just English if Chinese not available
            if (!string.IsNullOrEmpty(chineseName) && chineseName != englishName)
                return $"{englishName} ({chineseName})";
            else
                return englishName;
        }
        catch
        {
            return $"Species {species}";
        }
    }
}
