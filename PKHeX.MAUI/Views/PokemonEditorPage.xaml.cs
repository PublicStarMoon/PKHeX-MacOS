using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class PokemonEditorPage : ContentPage
{
    private PKM _pokemon;
    private SaveFile _saveFile;
    private bool _isUpdating = false;

    public PokemonEditorPage(PKM pokemon, SaveFile saveFile)
    {
        InitializeComponent();
        _pokemon = pokemon;
        _saveFile = saveFile;
        
        SetupUI();
        LoadPokemonData();
    }

    private void SetupUI()
    {
        try
        {
            // Setup species picker
            SpeciesPicker.Items.Clear();
            var speciesList = GameInfo.GetStrings(1).specieslist;
            for (int i = 1; i < Math.Min(speciesList.Length, 1011); i++)
            {
                if (!string.IsNullOrEmpty(speciesList[i]))
                {
                    SpeciesPicker.Items.Add($"{i:000} - {speciesList[i]}");
                }
            }

            // Setup nature picker
            NaturePicker.Items.Clear();
            var natureNames = Enum.GetNames(typeof(Nature));
            for (int i = 0; i < natureNames.Length && i < 25; i++)
            {
                NaturePicker.Items.Add(natureNames[i]);
            }

            // Setup move pickers
            SetupMovePickers();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to setup UI: {ex.Message}", "OK");
        }
    }

    private void SetupMovePickers()
    {
        try
        {
            var moveList = GameInfo.GetStrings(1).movelist;
            var pickers = new[] { Move1Picker, Move2Picker, Move3Picker, Move4Picker };

            foreach (var picker in pickers)
            {
                picker.Items.Clear();
                picker.Items.Add("000 - (None)");
                
                for (int i = 1; i < Math.Min(moveList.Length, 800); i++)
                {
                    if (!string.IsNullOrEmpty(moveList[i]))
                    {
                        picker.Items.Add($"{i:000} - {moveList[i]}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to setup move pickers: {ex.Message}", "OK");
        }
    }

    private void LoadPokemonData()
    {
        try
        {
            _isUpdating = true;

            // Basic information
            HeaderLabel.Text = $"Editing: {GameInfo.GetStrings(1).specieslist[_pokemon.Species]}";
            
            // Set species
            SpeciesPicker.SelectedIndex = Math.Max(0, _pokemon.Species - 1);
            
            // Set nickname
            NicknameEntry.Text = _pokemon.Nickname;
            
            // Set level
            LevelSlider.Value = _pokemon.CurrentLevel;
            
            // Set nature
            if (_pokemon.Nature < NaturePicker.Items.Count)
                NaturePicker.SelectedIndex = _pokemon.Nature;

            // Set IVs
            HPSlider.Value = _pokemon.IV_HP;
            AttackSlider.Value = _pokemon.IV_ATK;
            DefenseSlider.Value = _pokemon.IV_DEF;
            SpAttackSlider.Value = _pokemon.IV_SPA;
            SpDefenseSlider.Value = _pokemon.IV_SPD;
            SpeedSlider.Value = _pokemon.IV_SPE;

            // Set moves
            Move1Picker.SelectedIndex = Math.Max(0, _pokemon.Move1);
            Move2Picker.SelectedIndex = Math.Max(0, _pokemon.Move2);
            Move3Picker.SelectedIndex = Math.Max(0, _pokemon.Move3);
            Move4Picker.SelectedIndex = Math.Max(0, _pokemon.Move4);

            // Set special properties
            ShinyCheckBox.IsChecked = _pokemon.IsShiny;
            EggCheckBox.IsChecked = _pokemon.IsEgg;

            // Load held items with safety validation
            LoadHeldItemPicker();
            SetHeldItemSelection(_pokemon.HeldItem);

            UpdateStatLabels();
            UpdateCurrentStats();

            _isUpdating = false;
        }
        catch (Exception ex)
        {
            _isUpdating = false;
            DisplayAlert("Error", $"Failed to load Pokemon data: {ex.Message}", "OK");
        }
    }

    private void UpdateStatLabels()
    {
        HPLabel.Text = ((int)HPSlider.Value).ToString();
        AttackLabel.Text = ((int)AttackSlider.Value).ToString();
        DefenseLabel.Text = ((int)DefenseSlider.Value).ToString();
        SpAttackLabel.Text = ((int)SpAttackSlider.Value).ToString();
        SpDefenseLabel.Text = ((int)SpDefenseSlider.Value).ToString();
        SpeedLabel.Text = ((int)SpeedSlider.Value).ToString();
    }

    private void UpdateCurrentStats()
    {
        try
        {
            // Recalculate stats to show current values
            _pokemon.RefreshAbility(_pokemon.AbilityNumber);
            
            var stats = $"Current Battle Stats:\n" +
                       $"HP: {_pokemon.Stat_HP}\n" +
                       $"Attack: {_pokemon.Stat_ATK}\n" +
                       $"Defense: {_pokemon.Stat_DEF}\n" +
                       $"Sp. Attack: {_pokemon.Stat_SPA}\n" +
                       $"Sp. Defense: {_pokemon.Stat_SPD}\n" +
                       $"Speed: {_pokemon.Stat_SPE}";

            CurrentStatsLabel.Text = stats;
        }
        catch (Exception ex)
        {
            CurrentStatsLabel.Text = $"Stats calculation error: {ex.Message}";
        }
    }

    private void OnSpeciesChanged(object sender, EventArgs e)
    {
        if (_isUpdating) return;

        try
        {
            var selectedIndex = SpeciesPicker.SelectedIndex;
            if (selectedIndex >= 0)
            {
                _pokemon.Species = (ushort)(selectedIndex + 1);
                HeaderLabel.Text = $"Editing: {GameInfo.GetStrings(1).specieslist[_pokemon.Species]}";
                UpdateCurrentStats();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to change species: {ex.Message}", "OK");
        }
    }

    private void OnNicknameChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.Nickname = e.NewTextValue ?? "";
    }

    private void OnLevelChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;

        try
        {
            _pokemon.CurrentLevel = (int)e.NewValue;
            UpdateCurrentStats();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to change level: {ex.Message}", "OK");
        }
    }

    private void OnNatureChanged(object sender, EventArgs e)
    {
        if (_isUpdating) return;

        try
        {
            var selectedIndex = NaturePicker.SelectedIndex;
            if (selectedIndex >= 0)
            {
                _pokemon.Nature = selectedIndex;
                UpdateCurrentStats();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to change nature: {ex.Message}", "OK");
        }
    }

    private void OnHPChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IV_HP = (int)e.NewValue;
        UpdateStatLabels();
        UpdateCurrentStats();
    }

    private void OnAttackChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IV_ATK = (int)e.NewValue;
        UpdateStatLabels();
        UpdateCurrentStats();
    }

    private void OnDefenseChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IV_DEF = (int)e.NewValue;
        UpdateStatLabels();
        UpdateCurrentStats();
    }

    private void OnSpAttackChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IV_SPA = (int)e.NewValue;
        UpdateStatLabels();
        UpdateCurrentStats();
    }

    private void OnSpDefenseChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IV_SPD = (int)e.NewValue;
        UpdateStatLabels();
        UpdateCurrentStats();
    }

    private void OnSpeedChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IV_SPE = (int)e.NewValue;
        UpdateStatLabels();
        UpdateCurrentStats();
    }

    private void OnMove1Changed(object sender, EventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.Move1 = (ushort)Move1Picker.SelectedIndex;
    }

    private void OnMove2Changed(object sender, EventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.Move2 = (ushort)Move2Picker.SelectedIndex;
    }

    private void OnMove3Changed(object sender, EventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.Move3 = (ushort)Move3Picker.SelectedIndex;
    }

    private void OnMove4Changed(object sender, EventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.Move4 = (ushort)Move4Picker.SelectedIndex;
    }

    private void OnShinyChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isUpdating) return;

        try
        {
            if (e.Value)
                _pokemon.SetShiny();
            else
                _pokemon.SetUnshiny();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to change shiny status: {ex.Message}", "OK");
        }
    }

    private void OnEggChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isUpdating) return;
        _pokemon.IsEgg = e.Value;
    }

    private async void OnRandomizeIVsClicked(object sender, EventArgs e)
    {
        try
        {
            _pokemon.SetRandomIVs();
            LoadPokemonData(); // Reload to update sliders
            await DisplayAlert("Success", "IVs have been randomized!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to randomize IVs: {ex.Message}", "OK");
        }
    }

    private async void OnSuggestMovesClicked(object sender, EventArgs e)
    {
        try
        {
            _pokemon.SetSuggestedMoves();
            LoadPokemonData(); // Reload to update move pickers
            await DisplayAlert("Success", "Legal moves have been suggested!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to suggest moves: {ex.Message}", "OK");
        }
    }

    private async void OnHealClicked(object sender, EventArgs e)
    {
        try
        {
            _pokemon.Heal();
            UpdateCurrentStats();
            await DisplayAlert("Success", "Pokemon has been healed!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to heal Pokemon: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Pokemon is already updated by reference
            await DisplayAlert("Success", "Pokemon changes have been saved!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save Pokemon: {ex.Message}", "OK");
        }
    }

    private async void OnValidateClicked(object sender, EventArgs e)
    {
        if (_pokemon == null || _saveFile == null) return;
        
        try
        {
            // Check basic legality
            var summary = PokemonHelper.GetLegalitySummary(_pokemon, _saveFile);
            
            // Additional trainer info validation for SV
            var isTrainerValid = PokemonHelper.ValidateTrainerInfo(_pokemon, _saveFile);
            var warningMessage = "";
            
            if (!isTrainerValid && _saveFile.Generation >= 9)
            {
                warningMessage = "\n\n⚠️ WARNING: Trainer info mismatch detected! This Pokemon may disobey you in Scarlet/Violet. Use 'Fix Trainer Info' to resolve.";
            }
            
            await DisplayAlert("Legality Check", summary + warningMessage, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Validation failed: {ex.Message}", "OK");
        }
    }

    private async void OnFixTrainerInfoClicked(object sender, EventArgs e)
    {
        if (_pokemon == null || _saveFile == null) return;
        
        try
        {
            PokemonHelper.FixTrainerInfo(_pokemon, _saveFile);
            LoadPokemonData(_pokemon);
            await DisplayAlert("Success", "Trainer info has been fixed to prevent disobedience issues.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to fix trainer info: {ex.Message}", "OK");
        }
    }

    private async void OnRandomizeAllClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            // Randomize basic stats
            var random = new Random();
            
            // Randomize IVs
            _pokemon.IV_HP = random.Next(32);
            _pokemon.IV_ATK = random.Next(32);
            _pokemon.IV_DEF = random.Next(32);
            _pokemon.IV_SPA = random.Next(32);
            _pokemon.IV_SPD = random.Next(32);
            _pokemon.IV_SPE = random.Next(32);
            
            // Randomize nature
            _pokemon.Nature = random.Next(25);
            
            // Reload the UI
            LoadPokemonData(_pokemon);
            
            await DisplayAlert("Success", "Pokemon stats randomized!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to randomize: {ex.Message}", "OK");
        }
    }



    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void LoadHeldItemPicker()
    {
        try
        {
            HeldItemPicker.Items.Clear();
            
            if (_saveFile != null)
            {
                var safeItems = ItemHelper.GetSafeHeldItems(_saveFile);
                foreach (var item in safeItems)
                {
                    HeldItemPicker.Items.Add(item.Name);
                }
            }
            else
            {
                HeldItemPicker.Items.Add("None");
            }
        }
        catch (Exception ex)
        {
            HeldItemPicker.Items.Clear();
            HeldItemPicker.Items.Add("None");
            Console.WriteLine($"Failed to load held items: {ex.Message}");
        }
    }

    private void SetHeldItemSelection(int itemId)
    {
        if (_saveFile == null) return;
        
        try
        {
            var safeItems = ItemHelper.GetSafeHeldItems(_saveFile);
            var itemIndex = safeItems.FindIndex(x => x.Id == itemId);
            if (itemIndex >= 0)
            {
                HeldItemPicker.SelectedIndex = itemIndex;
            }
            else
            {
                HeldItemPicker.SelectedIndex = 0; // Default to "None"
            }
        }
        catch
        {
            HeldItemPicker.SelectedIndex = 0;
        }
    }

    private void OnHeldItemChanged(object sender, EventArgs e)
    {
        if (_isUpdating || _pokemon == null || _saveFile == null) return;

        try
        {
            var selectedIndex = HeldItemPicker.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var safeItems = ItemHelper.GetSafeHeldItems(_saveFile);
                if (selectedIndex < safeItems.Count)
                {
                    var selectedItem = safeItems[selectedIndex];
                    var success = ItemHelper.SafelyApplyHeldItem(_pokemon, _saveFile, selectedItem.Id);
                    
                    if (!success && selectedItem.Id != 0)
                    {
                        DisplayAlert("Warning", 
                            $"Item '{selectedItem.Name}' could not be safely applied. It may cause issues in {_saveFile.Version}.", 
                            "OK");
                        
                        // Reset to "None"
                        HeldItemPicker.SelectedIndex = 0;
                        _pokemon.HeldItem = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to change held item: {ex.Message}", "OK");
        }
    }
}
