using PKHeX.Core;
using Microsoft.Maui.Controls;
using PKHeX.MAUI.Services;

namespace PKHeX.MAUI.Views;

public partial class PartyEditorPage : ContentPage
{
    private SaveFile _save;
    private PKM[] _party;

    public PartyEditorPage(SaveFile save)
    {
        InitializeComponent();
        _save = save;
        _party = new PKM[6];
        
        LoadPartyData();
        CreatePartyUI();
    }

    private void LoadPartyData()
    {
        // Load the current party from the save file
        for (int i = 0; i < 6; i++)
        {
            _party[i] = _save.GetPartySlotAtIndex(i);
        }
    }

    private void CreatePartyUI()
    {
        PartyContainer.Children.Clear();

        for (int i = 0; i < 6; i++)
        {
            var slotFrame = CreatePartySlot(i, _party[i]);
            PartyContainer.Children.Add(slotFrame);
        }
    }

    private Microsoft.Maui.Controls.Frame CreatePartySlot(int slotIndex, PKM? pokemon)
    {
        var frame = new Microsoft.Maui.Controls.Frame
        {
            BackgroundColor = Colors.White,
            Padding = 20,
            Margin = new Thickness(0, 4),
            CornerRadius = 8,
            HasShadow = true
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Pokemon info
        var infoStack = new StackLayout();
        
        if (pokemon?.Species > 0)
        {
            var nameLabel = new Label 
            { 
                Text = $"Slot {slotIndex + 1}: {pokemon.Nickname} ({GetSpeciesName(pokemon.Species)})",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#2C3E50")
            };
            
            var detailsLabel = new Label
            {
                Text = $"Level {pokemon.CurrentLevel} • HP: {pokemon.Stat_HPCurrent}/{pokemon.Stat_HPMax} • Type: {pokemon.GetType().Name}",
                FontSize = 14,
                TextColor = Color.FromArgb("#7F8C8D")
            };
            
            var movesLabel = new Label
            {
                Text = $"Moves: {pokemon.Move1}, {pokemon.Move2}, {pokemon.Move3}, {pokemon.Move4}",
                FontSize = 12,
                TextColor = Color.FromArgb("#3498DB")
            };

            infoStack.Children.Add(nameLabel);
            infoStack.Children.Add(detailsLabel);
            infoStack.Children.Add(movesLabel);
        }
        else
        {
            var emptyLabel = new Label
            {
                Text = $"Slot {slotIndex + 1}: Empty",
                FontSize = 16,
                FontAttributes = FontAttributes.Italic,
                TextColor = Color.FromArgb("#BDC3C7")
            };
            infoStack.Children.Add(emptyLabel);
        }

        grid.Children.Add(infoStack);
        Grid.SetColumn(infoStack, 0);

        // Edit button
        var editButton = new Button
        {
            Text = pokemon?.Species > 0 ? "Edit" : "Add",
            BackgroundColor = pokemon?.Species > 0 ? Color.FromArgb("#3498DB") : Color.FromArgb("#27AE60"),
            TextColor = Colors.White,
            Margin = new Thickness(8, 0),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 6
        };
        
        editButton.Clicked += async (s, e) => await OnEditPokemonClicked(slotIndex);
        grid.Children.Add(editButton);
        Grid.SetColumn(editButton, 1);

        // Remove button (only show for occupied slots)
        if (pokemon?.Species > 0)
        {
            var removeButton = new Button
            {
                Text = "Remove",
                BackgroundColor = Color.FromArgb("#E74C3C"),
                TextColor = Colors.White,
                Margin = new Thickness(8, 0),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 6
            };
            
            removeButton.Clicked += async (s, e) => await OnRemovePokemonClicked(slotIndex);
            grid.Children.Add(removeButton);
            Grid.SetColumn(removeButton, 2);
        }

        frame.Content = grid;
        return frame;
    }

    private async Task OnEditPokemonClicked(int slotIndex)
    {
        try
        {
            PKM pokemon = _party[slotIndex];
            
            // If slot is empty, create a new Pokemon
            if (pokemon?.Species == 0 || pokemon == null)
            {
                pokemon = _save.BlankPKM;
                pokemon.Species = 1; // Default to Bulbasaur
                pokemon.Nickname = GetSpeciesName(pokemon.Species).Split(' ')[0]; // Use just the English name for nickname
                pokemon.CurrentLevel = 50;
                pokemon.Move1 = 1; // Pound
                pokemon.RefreshChecksum();
            }

            var editorPage = new PokemonEditorPage(pokemon, _save, -1, slotIndex); // Use -1 for box since this is party
            await Navigation.PushAsync(editorPage);
            
            // When we return, update the party array and save the changes
            _party[slotIndex] = pokemon;
            _save.SetPartySlotAtIndex(pokemon, slotIndex);
            CreatePartyUI(); // Refresh the UI
            StatusLabel.Text = $"Updated Pokémon in slot {slotIndex + 1}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to edit Pokémon: {ex.Message}", "OK");
        }
    }

    private async Task OnRemovePokemonClicked(int slotIndex)
    {
        var result = await DisplayAlert("Remove Pokémon", 
            $"Are you sure you want to remove the Pokémon from slot {slotIndex + 1}?", 
            "Remove", "Cancel");

        if (result)
        {
            _party[slotIndex] = _save.BlankPKM;
            _save.SetPartySlotAtIndex(_save.BlankPKM, slotIndex);
            CreatePartyUI(); // Refresh the UI
            StatusLabel.Text = $"Removed Pokémon from slot {slotIndex + 1}";
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
            // Save all party Pokemon to the save file
            for (int i = 0; i < 6; i++)
            {
                _save.SetPartySlotAtIndex(_party[i], i);
            }
            
            // Mark save file as edited and track unsaved changes
            _save.State.Edited = true;
            PageManager.MarkChangesUnsaved();
            
            StatusLabel.Text = "Party changes saved to memory! Use Export Save to persist to disk.";
            await DisplayAlert("Success", 
                "Party changes saved to memory!\n\n" +
                "To persist changes permanently:\n" +
                "• Go back to Main Page\n" +
                "• Click 'Export Save' button\n" +
                "• Save the file to disk", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save party changes: {ex.Message}", "OK");
        }
    }

    private async void OnHealAllClicked(object sender, EventArgs e)
    {
        try
        {
            int healedCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (_party[i]?.Species > 0)
                {
                    _party[i].Heal();
                    _save.SetPartySlotAtIndex(_party[i], i);
                    healedCount++;
                }
            }
            
            CreatePartyUI(); // Refresh the UI
            StatusLabel.Text = $"Healed {healedCount} Pokémon in party";
            await DisplayAlert("Success", $"Healed {healedCount} Pokémon in your party!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to heal party: {ex.Message}", "OK");
        }
    }

    private async void OnClearPartyClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Clear Party", 
            "Are you sure you want to remove all Pokémon from your party? This cannot be undone!", 
            "Clear All", "Cancel");

        if (result)
        {
            for (int i = 0; i < 6; i++)
            {
                _party[i] = _save.BlankPKM;
                _save.SetPartySlotAtIndex(_save.BlankPKM, i);
            }
            
            CreatePartyUI(); // Refresh the UI
            StatusLabel.Text = "Party cleared";
            await DisplayAlert("Success", "Party has been cleared!", "OK");
        }
    }

    private async void OnFillPartyClicked(object sender, EventArgs e)
    {
        try
        {
            int filledCount = 0;
            for (int i = 0; i < 6; i++)
            {
                if (_party[i]?.Species == 0 || _party[i] == null)
                {
                    // Create a basic Pokemon for empty slots
                    var newPokemon = _save.BlankPKM;
                    newPokemon.Species = (ushort)(1 + (i * 3)); // Different species for variety
                    newPokemon.Nickname = GetSpeciesName(newPokemon.Species).Split(' ')[0]; // Use just the English name for nickname
                    newPokemon.CurrentLevel = 50;
                    newPokemon.Move1 = 1; // Pound
                    newPokemon.RefreshChecksum();
                    
                    _party[i] = newPokemon;
                    _save.SetPartySlotAtIndex(newPokemon, i);
                    filledCount++;
                }
            }
            
            CreatePartyUI(); // Refresh the UI
            StatusLabel.Text = $"Added {filledCount} Pokémon to empty slots";
            
            if (filledCount > 0)
            {
                await DisplayAlert("Success", $"Added {filledCount} Pokémon to empty party slots!", "OK");
            }
            else
            {
                await DisplayAlert("Info", "Party is already full!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to fill party: {ex.Message}", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Only refresh UI without reloading data to preserve state
        // Data is loaded once in constructor and maintained in memory
        CreatePartyUI();
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
