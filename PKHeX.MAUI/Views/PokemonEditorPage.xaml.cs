using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class PokemonEditorPage : ContentPage
{
    private PKM? _pokemon;
    private SaveFile? _saveFile;
    private bool _isUpdating = false;
    private int _boxIndex = -1;
    private int _slotIndex = -1;

    // Data sources for pickers
    private List<MoveItem> _moveItems = new();
    private List<AbilityItem> _abilityItems = new();
    private List<NatureItem> _natureItems = new();
    private List<ItemItem> _itemItems = new();
    private List<BallItem> _ballItems = new();
    private List<FormItem> _formItems = new();

    public PokemonEditorPage(PKM pokemon, SaveFile saveFile, int boxIndex = -1, int slotIndex = -1)
    {
        InitializeComponent();
        _pokemon = pokemon;
        _saveFile = saveFile;
        _boxIndex = boxIndex;
        _slotIndex = slotIndex;
        InitializePickerData();
        LoadPokemonData();
    }

    private void LoadPokemonData()
    {
        if (_pokemon == null) return;

        try
        {
            _isUpdating = true;

            // Basic information  
            var generationInfo = GetGenerationInfo(_pokemon);
            HeaderLabel.Text = $"Editing: {GetSpeciesName(_pokemon.Species)} (Gen {_pokemon.Format})";
            GenerationLabel.Text = generationInfo;
            SpeciesEntry.Text = _pokemon.Species.ToString();
            NicknameEntry.Text = _pokemon.Nickname;
            LevelEntry.Text = _pokemon.CurrentLevel.ToString();
            
            // Set nature button text
            var selectedNature = _natureItems.FirstOrDefault(x => x.Id == _pokemon.Nature);
            NatureButton.Text = selectedNature?.DisplayName ?? "Select Nature...";

            // IVs
            HPIVEntry.Text = _pokemon.IV_HP.ToString();
            AttackIVEntry.Text = _pokemon.IV_ATK.ToString();
            DefenseIVEntry.Text = _pokemon.IV_DEF.ToString();
            SpAttackIVEntry.Text = _pokemon.IV_SPA.ToString();
            SpDefenseIVEntry.Text = _pokemon.IV_SPD.ToString();
            SpeedIVEntry.Text = _pokemon.IV_SPE.ToString();

            // EVs
            HPEVEntry.Text = _pokemon.EV_HP.ToString();
            AttackEVEntry.Text = _pokemon.EV_ATK.ToString();
            DefenseEVEntry.Text = _pokemon.EV_DEF.ToString();
            SpAttackEVEntry.Text = _pokemon.EV_SPA.ToString();
            SpDefenseEVEntry.Text = _pokemon.EV_SPD.ToString();
            SpeedEVEntry.Text = _pokemon.EV_SPE.ToString();

            // Moves - set button texts
            var selectedMove1 = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move1);
            Move1Button.Text = selectedMove1?.DisplayName ?? "Select Move...";
            
            var selectedMove2 = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move2);
            Move2Button.Text = selectedMove2?.DisplayName ?? "Select Move...";
            
            var selectedMove3 = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move3);
            Move3Button.Text = selectedMove3?.DisplayName ?? "Select Move...";
            
            var selectedMove4 = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move4);
            Move4Button.Text = selectedMove4?.DisplayName ?? "Select Move...";

            // Physical Properties
            GenderEntry.Text = _pokemon.Gender.ToString();
            
            // Set ability button text
            var selectedAbility = _abilityItems.FirstOrDefault(x => x.Id == _pokemon.Ability);
            AbilityButton.Text = selectedAbility?.DisplayName ?? "Select Ability...";
            
            // Set form button text
            var selectedForm = _formItems.FirstOrDefault(x => x.Id == _pokemon.Form);
            FormButton.Text = selectedForm?.DisplayName ?? "Select Form...";
            
            // Set ball button text
            var selectedBall = _ballItems.FirstOrDefault(x => x.Id == _pokemon.Ball);
            BallButton.Text = selectedBall?.DisplayName ?? "Select Ball...";
            
            ShinyCheckBox.IsChecked = _pokemon.IsShiny;
            EggCheckBox.IsChecked = _pokemon.IsEgg;
            
            // Set held item button text
            var selectedHeldItem = _itemItems.FirstOrDefault(x => x.Id == _pokemon.HeldItem);
            HeldItemButton.Text = selectedHeldItem?.DisplayName ?? "Select Item...";

            // Origin & Met Information
            OTNameEntry.Text = _pokemon.OT_Name;
            OTGenderEntry.Text = _pokemon.OT_Gender.ToString();
            TIDEntry.Text = _pokemon.TID16.ToString();
            SIDEntry.Text = _pokemon.SID16.ToString();
            MetLocationEntry.Text = _pokemon.Met_Location.ToString();
            MetLevelEntry.Text = _pokemon.Met_Level.ToString();

            // Friendship & Language
            FriendshipEntry.Text = _pokemon.CurrentFriendship.ToString();
            LanguageEntry.Text = _pokemon.Language.ToString();
            VersionEntry.Text = _pokemon.Version.ToString();
            FatefulCheckBox.IsChecked = _pokemon.FatefulEncounter;

            _isUpdating = false;
        }
        catch (Exception ex)
        {
            _isUpdating = false;
            StatusLabel.Text = $"Error loading data: {ex.Message}";
        }
    }

    private void OnSpeciesChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort species))
        {
            _pokemon.Species = species;
            HeaderLabel.Text = $"Editing: Species {species}";
        }
    }

    private void OnNicknameChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;
        _pokemon.Nickname = e.NewTextValue ?? "";
    }

    private void OnLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int level) && level >= 1 && level <= 100)
        {
            _pokemon.CurrentLevel = level;
        }
    }

    private void OnHPIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_HP = iv;
        }
    }

    private void OnAttackIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_ATK = iv;
        }
    }

    private void OnDefenseIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_DEF = iv;
        }
    }

    private void OnSpAttackIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_SPA = iv;
        }
    }

    private void OnSpDefenseIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_SPD = iv;
        }
    }

    private void OnSpeedIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_SPE = iv;
        }
    }



    // New Button click handlers for searchable pickers
    private async void OnMove1ButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move1);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_moveItems.Cast<IPickerItem>().ToList(), "Select Move 1", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Move1 = (ushort)result.Id;
            Move1Button.Text = result.DisplayName;
        }
    }

    private async void OnMove2ButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move2);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_moveItems.Cast<IPickerItem>().ToList(), "Select Move 2", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Move2 = (ushort)result.Id;
            Move2Button.Text = result.DisplayName;
        }
    }

    private async void OnMove3ButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move3);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_moveItems.Cast<IPickerItem>().ToList(), "Select Move 3", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Move3 = (ushort)result.Id;
            Move3Button.Text = result.DisplayName;
        }
    }

    private async void OnMove4ButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move4);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_moveItems.Cast<IPickerItem>().ToList(), "Select Move 4", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Move4 = (ushort)result.Id;
            Move4Button.Text = result.DisplayName;
        }
    }

    private async void OnAbilityButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _abilityItems.FirstOrDefault(x => x.Id == _pokemon.Ability);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_abilityItems.Cast<IPickerItem>().ToList(), "Select Ability", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Ability = result.Id;
            AbilityButton.Text = result.DisplayName;
        }
    }

    private async void OnNatureButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _natureItems.FirstOrDefault(x => x.Id == _pokemon.Nature);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_natureItems.Cast<IPickerItem>().ToList(), "Select Nature", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Nature = result.Id;
            NatureButton.Text = result.DisplayName;
        }
    }

    private async void OnFormButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _formItems.FirstOrDefault(x => x.Id == _pokemon.Form);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_formItems.Cast<IPickerItem>().ToList(), "Select Form", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Form = (byte)result.Id;
            FormButton.Text = result.DisplayName;
            HeaderLabel.Text = $"Editing: {GetSpeciesName(_pokemon.Species)} {GetFormName(_pokemon.Species, (byte)result.Id)} (Gen {_pokemon.Format})";
        }
    }

    private async void OnBallButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _ballItems.FirstOrDefault(x => x.Id == _pokemon.Ball);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_ballItems.Cast<IPickerItem>().ToList(), "Select Ball", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.Ball = result.Id;
            BallButton.Text = result.DisplayName;
        }
    }

    private async void OnHeldItemButtonClicked(object sender, EventArgs e)
    {
        var currentSelection = _itemItems.FirstOrDefault(x => x.Id == _pokemon.HeldItem);
        var pickerPage = new SearchablePickerPage();
        pickerPage.SetItems(_itemItems.Cast<IPickerItem>().ToList(), "Select Held Item", currentSelection);
        
        var completionSource = new TaskCompletionSource<IPickerItem?>();
        pickerPage.CompletionSource = completionSource;
        
        await Navigation.PushModalAsync(pickerPage);
        var result = await completionSource.Task;
        
        if (result != null && _pokemon != null)
        {
            _pokemon.HeldItem = result.Id;
            HeldItemButton.Text = result.DisplayName;
        }
    }

    // EV Handlers
    private void OnHPEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.EV_HP = ev;
        }
    }

    private void OnAttackEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.EV_ATK = ev;
        }
    }

    private void OnDefenseEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.EV_DEF = ev;
        }
    }

    private void OnSpAttackEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.EV_SPA = ev;
        }
    }

    private void OnSpDefenseEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.EV_SPD = ev;
        }
    }

    private void OnSpeedEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.EV_SPE = ev;
        }
    }

    // Physical Properties Handlers
    private void OnGenderChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int gender) && gender >= 0 && gender <= 2)
        {
            _pokemon.Gender = gender;
        }
    }

    private void OnBallChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ball) && ball >= 0)
        {
            _pokemon.Ball = ball;
        }
    }

    // Origin & Met Information Handlers
    private void OnOTNameChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;
        _pokemon.OT_Name = e.NewTextValue ?? "";
    }

    private void OnOTGenderChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int gender) && gender >= 0 && gender <= 1)
        {
            _pokemon.OT_Gender = gender;
        }
    }

    private void OnTIDChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort tid))
        {
            _pokemon.TID16 = tid;
        }
    }

    private void OnSIDChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort sid))
        {
            _pokemon.SID16 = sid;
        }
    }

    private void OnMetLocationChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int location) && location >= 0)
        {
            _pokemon.Met_Location = location;
        }
    }

    private void OnMetLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int level) && level >= 0 && level <= 100)
        {
            _pokemon.Met_Level = level;
        }
    }

    // Friendship & Language Handlers
    private void OnFriendshipChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int friendship) && friendship >= 0 && friendship <= 255)
        {
            _pokemon.CurrentFriendship = friendship;
        }
    }

    private void OnLanguageChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int language) && language >= 0)
        {
            _pokemon.Language = language;
        }
    }

    private void OnVersionChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int version) && version >= 0)
        {
            _pokemon.Version = version;
        }
    }

    private void OnFatefulChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;
        _pokemon.FatefulEncounter = e.Value;
    }

    private void OnShinyChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        try
        {
            if (e.Value)
            {
                // Make it shiny by setting a shiny PID
                var random = new Random();
                uint pid = (uint)random.Next();
                _pokemon.PID = pid;
                
                // Simple shiny calculation for demonstration
                var shinyVal = ((_pokemon.TID16 ^ _pokemon.SID16) ^ (pid >> 16) ^ (pid & 0xFFFF));
                if (shinyVal >= 16)
                {
                    // Adjust PID to make it shiny
                    _pokemon.PID = (uint)((pid & 0xFFFF0000) | ((pid & 0xFFFF) ^ (_pokemon.TID16 ^ _pokemon.SID16)));
                }
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error setting shiny: {ex.Message}";
        }
    }

    private void OnEggChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;
        _pokemon.IsEgg = e.Value;
    }

    private void OnHeldItemChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int item) && item >= 0)
        {
            _pokemon.HeldItem = item;
        }
    }

    private async void OnHealClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        try
        {
            _pokemon.Heal();
            StatusLabel.Text = "Pokemon healed successfully!";
            await DisplayAlert("Success", "Pokemon has been fully healed!", "OK");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error healing: {ex.Message}";
            await DisplayAlert("Error", $"Failed to heal Pokemon: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            // Save changes back to the box slot if this Pokemon is from a box
            if (_saveFile != null && _pokemon != null && _boxIndex >= 0 && _slotIndex >= 0)
            {
                _saveFile.SetBoxSlotAtIndex(_pokemon, _boxIndex, _slotIndex);
                _saveFile.State.Edited = true;
            }
            // Save changes back to party slot if this Pokemon is from party
            else if (_saveFile != null && _pokemon != null && _boxIndex == -1 && _slotIndex >= 0)
            {
                _saveFile.SetPartySlotAtIndex(_pokemon, _slotIndex);
                _saveFile.State.Edited = true;
            }
            else if (_saveFile != null)
            {
                // Mark save file as edited if no specific slot
                _saveFile.State.Edited = true;
            }
            
            StatusLabel.Text = "Pokemon saved successfully!";
            await DisplayAlert("Success", "Pokemon changes have been saved!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error saving: {ex.Message}";
            await DisplayAlert("Error", $"Failed to save Pokemon: {ex.Message}", "OK");
        }
    }

    private string GetGenerationInfo(PKM pokemon)
    {
        var info = $"Generation {pokemon.Format} Pokemon";
        
        // Add generation-specific information
        switch (pokemon.Format)
        {
            case 7:
                info += " (Gen 7: Sun/Moon/Ultra Sun/Ultra Moon)";
                if (pokemon is PK7 pk7)
                {
                    // Add any Gen 7 specific properties if needed
                    info += $" | Z-Crystal support available";
                }
                break;
            case 8:
                info += " (Gen 8: Sword/Shield/BDSP/Legends Arceus)";
                if (pokemon is PK8 pk8)
                {
                    // Add any Gen 8 specific properties if needed
                    info += $" | Dynamax/Gigantamax support";
                }
                break;
            case 9:
                info += " (Gen 9: Scarlet/Violet)";
                if (pokemon is PK9 pk9)
                {
                    // Add any Gen 9 specific properties if needed
                    info += $" | Tera Type support available";
                }
                break;
            default:
                info += $" | Format: {pokemon.Format}";
                break;
        }
        
        return info;
    }

    private async void OnMaxIVsClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        try
        {
            _pokemon.IV_HP = 31;
            _pokemon.IV_ATK = 31;
            _pokemon.IV_DEF = 31;
            _pokemon.IV_SPA = 31;
            _pokemon.IV_SPD = 31;
            _pokemon.IV_SPE = 31;
            
            // Update the UI
            LoadPokemonData();
            
            StatusLabel.Text = "All IVs set to maximum (31)!";
            await DisplayAlert("Success", "All IVs have been set to maximum (31)!", "OK");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error setting max IVs: {ex.Message}";
            await DisplayAlert("Error", $"Failed to set max IVs: {ex.Message}", "OK");
        }
    }

    private async void OnClearEVsClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        try
        {
            _pokemon.EV_HP = 0;
            _pokemon.EV_ATK = 0;
            _pokemon.EV_DEF = 0;
            _pokemon.EV_SPA = 0;
            _pokemon.EV_SPD = 0;
            _pokemon.EV_SPE = 0;
            
            // Update the UI
            LoadPokemonData();
            
            StatusLabel.Text = "All EVs cleared!";
            await DisplayAlert("Success", "All EVs have been cleared (set to 0)!", "OK");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error clearing EVs: {ex.Message}";
            await DisplayAlert("Error", $"Failed to clear EVs: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Gets the multilingual species name in both English and Chinese
    /// </summary>
    private string GetSpeciesName(ushort species)
    {
        try
        {
            if (species == 0) return "None";

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

    /// <summary>
    /// Gets the multilingual move name in both English and Chinese
    /// </summary>
    private string GetMoveName(ushort moveId)
    {
        try
        {
            if (moveId == 0) return "None";

            // Get move names using GameInfo.GetStrings
            var englishMoves = GameInfo.GetStrings("en").movelist;
            var chineseMoves = GameInfo.GetStrings("zh2").movelist ?? GameInfo.GetStrings("zh").movelist;

            var englishName = moveId < englishMoves.Length ? englishMoves[moveId] : $"Move {moveId}";
            var chineseName = "";

            if (chineseMoves != null && moveId < chineseMoves.Length)
                chineseName = chineseMoves[moveId];

            // Return format: "English Name (Chinese Name)" or just English if Chinese not available
            if (!string.IsNullOrEmpty(chineseName) && chineseName != englishName)
                return $"{englishName} ({chineseName})";
            else
                return englishName;
        }
        catch
        {
            return $"Move {moveId}";
        }
    }

    /// <summary>
    /// Gets the multilingual ability name in both English and Chinese
    /// </summary>
    private string GetAbilityName(int abilityId)
    {
        try
        {
            if (abilityId == 0) return "None";

            // Get ability names using GameInfo.GetStrings
            var englishAbilities = GameInfo.GetStrings("en").abilitylist;
            var chineseAbilities = GameInfo.GetStrings("zh2").abilitylist ?? GameInfo.GetStrings("zh").abilitylist;

            var englishName = abilityId < englishAbilities.Length ? englishAbilities[abilityId] : $"Ability {abilityId}";
            var chineseName = "";

            if (chineseAbilities != null && abilityId < chineseAbilities.Length)
                chineseName = chineseAbilities[abilityId];

            // Return format: "English Name (Chinese Name)" or just English if Chinese not available
            if (!string.IsNullOrEmpty(chineseName) && chineseName != englishName)
                return $"{englishName} ({chineseName})";
            else
                return englishName;
        }
        catch
        {
            return $"Ability {abilityId}";
        }
    }

    /// <summary>
    /// Gets the form name for a species with multilingual support
    /// </summary>
    private string GetFormName(ushort species, byte form)
    {
        try
        {
            if (form == 0) return "Normal Form";

            // Get form names using PKHeX's FormConverter
            var englishStrings = GameInfo.GetStrings("en");
            var chineseStrings = GameInfo.GetStrings("zh2") ?? GameInfo.GetStrings("zh");
            
            // Use the pokemon's context if available, otherwise default to Gen 9
            var context = _pokemon?.Context ?? EntityContext.Gen9;
            
            var forms = FormConverter.GetFormList(species, englishStrings.types, englishStrings.forms, GameInfo.GenderSymbolUnicode, context);
            var formsChinese = FormConverter.GetFormList(species, chineseStrings?.types ?? englishStrings.types, 
                chineseStrings?.forms ?? englishStrings.forms, GameInfo.GenderSymbolUnicode, context);

            if (forms != null && form < forms.Length)
            {
                var englishName = forms[form];
                var chineseName = "";
                
                if (formsChinese != null && form < formsChinese.Length)
                    chineseName = formsChinese[form];

                // Return format: "English Name (Chinese Name)" or just English if Chinese not available
                if (!string.IsNullOrEmpty(chineseName) && chineseName != englishName)
                    return $"{englishName} ({chineseName})";
                else
                    return englishName;
            }

            return $"Form {form}";
        }
        catch
        {
            return $"Form {form}";
        }
    }

    /// <summary>
    /// Initialize picker data sources
    /// </summary>
    private void InitializePickerData()
    {
        try
        {
            // Initialize move list
            _moveItems.Clear();
            var englishMoves = GameInfo.GetStrings("en").movelist;
            var chineseMoves = GameInfo.GetStrings("zh2").movelist ?? GameInfo.GetStrings("zh").movelist;
            
            _moveItems.Add(new MoveItem { Id = 0, DisplayName = "None" });
            for (int i = 1; i < englishMoves.Length && i < 1000; i++)
            {
                var englishName = englishMoves[i];
                var chineseName = chineseMoves != null && i < chineseMoves.Length ? chineseMoves[i] : "";
                
                var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                    ? $"{englishName} ({chineseName})" 
                    : englishName;
                    
                _moveItems.Add(new MoveItem { Id = i, DisplayName = displayName });
            }

            // Initialize ability list
            _abilityItems.Clear();
            var englishAbilities = GameInfo.GetStrings("en").abilitylist;
            var chineseAbilities = GameInfo.GetStrings("zh2").abilitylist ?? GameInfo.GetStrings("zh").abilitylist;
            
            _abilityItems.Add(new AbilityItem { Id = 0, DisplayName = "None" });
            for (int i = 1; i < englishAbilities.Length && i < 300; i++)
            {
                var englishName = englishAbilities[i];
                var chineseName = chineseAbilities != null && i < chineseAbilities.Length ? chineseAbilities[i] : "";
                
                var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                    ? $"{englishName} ({chineseName})" 
                    : englishName;
                    
                _abilityItems.Add(new AbilityItem { Id = i, DisplayName = displayName });
            }

            // Initialize nature list
            _natureItems.Clear();
            var englishNatures = GameInfo.GetStrings("en").natures;
            var chineseNatures = GameInfo.GetStrings("zh2").natures ?? GameInfo.GetStrings("zh").natures;
            
            for (int i = 0; i < englishNatures.Length && i < 25; i++)
            {
                var englishName = englishNatures[i];
                var chineseName = chineseNatures != null && i < chineseNatures.Length ? chineseNatures[i] : "";
                
                var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                    ? $"{englishName} ({chineseName})" 
                    : englishName;
                    
                _natureItems.Add(new NatureItem { Id = i, DisplayName = displayName });
            }

            // Initialize item list (for held items)
            _itemItems.Clear();
            var englishItems = GameInfo.GetStrings("en").itemlist;
            var chineseItems = GameInfo.GetStrings("zh2").itemlist ?? GameInfo.GetStrings("zh").itemlist;
            
            _itemItems.Add(new ItemItem { Id = 0, DisplayName = "None" });
            for (int i = 1; i < englishItems.Length && i < 2000; i++)
            {
                var englishName = englishItems[i];
                var chineseName = chineseItems != null && i < chineseItems.Length ? chineseItems[i] : "";
                
                var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                    ? $"{englishName} ({chineseName})" 
                    : englishName;
                    
                _itemItems.Add(new ItemItem { Id = i, DisplayName = displayName });
            }

            // Initialize ball list (simplified - just basic balls)
            _ballItems.Clear();
            _ballItems.Add(new BallItem { Id = 0, DisplayName = "None" });
            _ballItems.Add(new BallItem { Id = 1, DisplayName = "Master Ball (大师球)" });
            _ballItems.Add(new BallItem { Id = 2, DisplayName = "Ultra Ball (高级球)" });
            _ballItems.Add(new BallItem { Id = 3, DisplayName = "Great Ball (超级球)" });
            _ballItems.Add(new BallItem { Id = 4, DisplayName = "Poké Ball (精灵球)" });
            _ballItems.Add(new BallItem { Id = 5, DisplayName = "Safari Ball (狩猎球)" });
            _ballItems.Add(new BallItem { Id = 6, DisplayName = "Net Ball (捕网球)" });
            _ballItems.Add(new BallItem { Id = 7, DisplayName = "Dive Ball (潜水球)" });
            _ballItems.Add(new BallItem { Id = 8, DisplayName = "Nest Ball (巢穴球)" });
            _ballItems.Add(new BallItem { Id = 9, DisplayName = "Repeat Ball (重复球)" });
            _ballItems.Add(new BallItem { Id = 10, DisplayName = "Timer Ball (计时球)" });
            _ballItems.Add(new BallItem { Id = 11, DisplayName = "Luxury Ball (豪华球)" });
            _ballItems.Add(new BallItem { Id = 12, DisplayName = "Premier Ball (纪念球)" });

            // Initialize form list
            _formItems.Clear();
            if (_pokemon != null)
            {
                // Get the number of forms for this species
                var englishStrings = GameInfo.GetStrings("en");
                var formCount = PKHeX.Core.FormConverter.GetFormList(_pokemon.Species, englishStrings.types, englishStrings.forms, GameInfo.GenderSymbolUnicode, _pokemon.Context).Length;
                
                for (int i = 0; i < formCount; i++)
                {
                    var formName = GetFormName(_pokemon.Species, (byte)i);
                    _formItems.Add(new FormItem { Id = i, DisplayName = formName });
                }
            }
            else
            {
                // Default forms when no Pokemon loaded
                _formItems.Add(new FormItem { Id = 0, DisplayName = "Normal Form" });
            }

            // Form and picker data initialization complete
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Error initializing picker data: {ex.Message}");
        }
    }
}

// Data model classes for pickers are now defined in SearchablePickerPage.xaml.cs