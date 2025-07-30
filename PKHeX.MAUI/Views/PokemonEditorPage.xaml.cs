using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class PokemonEditorPage : ContentPage
{
    private PKM? _pokemon;
    private SaveFile? _saveFile;
    private bool _isUpdating = false;

    public PokemonEditorPage(PKM pokemon, SaveFile saveFile)
    {
        InitializeComponent();
        _pokemon = pokemon;
        _saveFile = saveFile;
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
            HeaderLabel.Text = $"Editing: Species {_pokemon.Species} (Gen {_pokemon.Format})";
            GenerationLabel.Text = generationInfo;
            SpeciesEntry.Text = _pokemon.Species.ToString();
            NicknameEntry.Text = _pokemon.Nickname;
            LevelEntry.Text = _pokemon.CurrentLevel.ToString();
            NatureEntry.Text = _pokemon.Nature.ToString();

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

            // Moves
            Move1Entry.Text = _pokemon.Move1.ToString();
            Move2Entry.Text = _pokemon.Move2.ToString();
            Move3Entry.Text = _pokemon.Move3.ToString();
            Move4Entry.Text = _pokemon.Move4.ToString();

            // Physical Properties
            GenderEntry.Text = _pokemon.Gender.ToString();
            AbilityEntry.Text = _pokemon.Ability.ToString();
            FormEntry.Text = _pokemon.Form.ToString();
            BallEntry.Text = _pokemon.Ball.ToString();
            ShinyCheckBox.IsChecked = _pokemon.IsShiny;
            EggCheckBox.IsChecked = _pokemon.IsEgg;
            HeldItemEntry.Text = _pokemon.HeldItem.ToString();

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

    private void OnNatureChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int nature) && nature >= 0 && nature <= 24)
        {
            _pokemon.Nature = nature;
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

    private void OnMove1Changed(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort move))
        {
            _pokemon.Move1 = move;
        }
    }

    private void OnMove2Changed(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort move))
        {
            _pokemon.Move2 = move;
        }
    }

    private void OnMove3Changed(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort move))
        {
            _pokemon.Move3 = move;
        }
    }

    private void OnMove4Changed(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (ushort.TryParse(e.NewTextValue, out ushort move))
        {
            _pokemon.Move4 = move;
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

    private void OnAbilityChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ability) && ability >= 0)
        {
            _pokemon.Ability = ability;
        }
    }

    private void OnFormChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (byte.TryParse(e.NewTextValue, out byte form))
        {
            _pokemon.Form = form;
            HeaderLabel.Text = $"Editing: Species {_pokemon.Species} Form {form} (Gen {_pokemon.Format})";
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
                    _pokemon.PID = (pid & 0xFFFF0000) | ((pid & 0xFFFF) ^ (_pokemon.TID16 ^ _pokemon.SID16));
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
            // Mark save file as edited if available
            if (_saveFile != null)
            {
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
}