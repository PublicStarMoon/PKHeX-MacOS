using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using PKHeX.MAUI.Utilities;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;

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

        // Create 30 Pokemon slots
        for (int i = 0; i < _saveFile.BoxSlotCount; i++)
        {
            int row = i / 6;
            int col = i % 6;
            
            var button = new Button
            {
                Text = "Empty",
                BackgroundColor = Colors.LightGray,
                BorderColor = Colors.Gray,
                BorderWidth = 1,
                FontSize = 10,
                Padding = new Thickness(2),
                HeightRequest = 75,
                WidthRequest = 80
            };

            button.Clicked += (s, e) => OnPokemonSlotClicked(i);
            
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
            // Clear current data
            _currentBoxPokemon = new PKM[_saveFile.BoxSlotCount];
            
            // Load Pokemon from current box
            for (int slot = 0; slot < _saveFile.BoxSlotCount; slot++)
            {
                var pkm = _saveFile.GetBoxSlotAtIndex(_currentBox, slot);
                _currentBoxPokemon[slot] = pkm;
                UpdatePokemonSlot(slot, pkm);
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
            button.BackgroundColor = Colors.LightGray;
        }
        else
        {
            var speciesName = GameInfo.GetStrings(1).specieslist[pokemon.Species];
            button.Text = $"{speciesName}\nLv.{pokemon.CurrentLevel}";
            
            // Color coding based on Pokemon properties
            if (pokemon.IsShiny)
                button.BackgroundColor = Colors.Gold;
            else if (pokemon.IsEgg)
                button.BackgroundColor = Colors.LightPink;
            else
                button.BackgroundColor = Colors.LightBlue;
        }
    }

    private void UpdateBoxCountLabel()
    {
        int count = PokemonHelper.CountBoxPokemon(_saveFile, _currentBox);
        BoxCountLabel.Text = $"{count}/{_saveFile.BoxSlotCount}";
    }

    private async void OnPokemonSlotClicked(int slot)
    {
        try
        {
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
            await DisplayAlert("Error", $"Failed to process slot: {ex.Message}", "OK");
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
                var newPokemon = PokemonHelper.CreateLegalPokemon(_saveFile, species, 5);
                
                // Place in box
                _saveFile.SetBoxSlotAtIndex(newPokemon, _currentBox, slot);
                _currentBoxPokemon[slot] = newPokemon;
                
                UpdatePokemonSlot(slot, newPokemon);
                UpdateBoxCountLabel();
                StatusLabel.Text = $"Added {GameInfo.GetStrings(1).specieslist[species]} to slot {slot + 1}";
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
            var speciesName = GameInfo.GetStrings(1).specieslist[pokemon.Species];
            var options = new string[]
            {
                "View Details",
                "Edit in Detail Editor",
                "Edit Level",
                "Make Shiny",
                "Heal Pokemon", 
                "Delete Pokemon",
                "Cancel"
            };

            var choice = await DisplayActionSheet($"{speciesName} (Slot {slot + 1})", 
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
                case "Make Shiny":
                    await ToggleShiny(slot, pokemon);
                    break;
                case "Heal Pokemon":
                    pokemon.Heal();
                    _saveFile.SetBoxSlotAtIndex(pokemon, _currentBox, slot);
                    StatusLabel.Text = $"{speciesName} has been healed!";
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
        var speciesName = GameInfo.GetStrings(1).specieslist[pokemon.Species];
        var details = $"Species: {speciesName}\n" +
                     $"Level: {pokemon.CurrentLevel}\n" +
                     $"Nature: {(Nature)pokemon.Nature}\n" +
                     $"Ability: {pokemon.Ability}\n" +
                     $"HP: {pokemon.HP_Current}/{pokemon.Stat_HP}\n" +
                     $"Attack: {pokemon.Stat_ATK}\n" +
                     $"Defense: {pokemon.Stat_DEF}\n" +
                     $"Sp.Atk: {pokemon.Stat_SPA}\n" +
                     $"Sp.Def: {pokemon.Stat_SPD}\n" +
                     $"Speed: {pokemon.Stat_SPE}\n" +
                     $"Shiny: {(pokemon.IsShiny ? "Yes" : "No")}\n" +
                     $"OT: {pokemon.OT_Name}\n" +
                     $"TID: {pokemon.TID16}";

        await DisplayAlert($"{speciesName} Details", details, "OK");
    }

    private async Task OpenDetailedEditor(int slot, PKM pokemon)
    {
        try
        {
            var editorPage = new PokemonEditorPage(pokemon, _saveFile);
            await Navigation.PushAsync(editorPage);
            
            // When we return, refresh the slot display
            // Note: The pokemon object is modified by reference in the editor
            _saveFile.SetBoxSlotAtIndex(pokemon, _currentBox, slot);
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
            $"Enter new level for {GameInfo.GetStrings(1).specieslist[pokemon.Species]}:", 
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

    private async Task ToggleShiny(int slot, PKM pokemon)
    {
        pokemon.SetShiny();
        _saveFile.SetBoxSlotAtIndex(pokemon, _currentBox, slot);
        UpdatePokemonSlot(slot, pokemon);
        
        var speciesName = GameInfo.GetStrings(1).specieslist[pokemon.Species];
        StatusLabel.Text = $"{speciesName} is now {(pokemon.IsShiny ? "shiny" : "not shiny")}!";
    }

    private async Task DeletePokemon(int slot)
    {
        var pokemon = _currentBoxPokemon[slot];
        var speciesName = GameInfo.GetStrings(1).specieslist[pokemon!.Species];
        
        bool confirm = await DisplayAlert("Delete Pokemon", 
            $"Are you sure you want to delete {speciesName}? This cannot be undone!", 
            "Delete", "Cancel");

        if (confirm)
        {
            var blankPokemon = _saveFile.BlankPKM;
            _saveFile.SetBoxSlotAtIndex(blankPokemon, _currentBox, slot);
            _currentBoxPokemon[slot] = null;
            
            UpdatePokemonSlot(slot, null);
            UpdateBoxCountLabel();
            StatusLabel.Text = $"{speciesName} has been deleted.";
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
            // The changes are already saved to the SaveFile object
            // Here we could trigger a save to disk if needed
            StatusLabel.Text = "Changes saved to memory. Use Export Save from main menu to save to file.";
            await DisplayAlert("Success", "All changes have been saved to memory!", "OK");
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
                
                for (int slot = 0; slot < _saveFile.BoxSlotCount; slot++)
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
        for (int i = 0; i < _saveFile.BoxSlotCount; i++)
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
}
