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
            HeaderLabel.Text = $"Editing: Species {_pokemon.Species}";
            SpeciesEntry.Text = _pokemon.Species.ToString();
            NicknameEntry.Text = _pokemon.Nickname;
            LevelEntry.Text = _pokemon.CurrentLevel.ToString();
            NatureEntry.Text = _pokemon.Nature.ToString();

            // IVs
            HPEntry.Text = _pokemon.IV_HP.ToString();
            AttackEntry.Text = _pokemon.IV_ATK.ToString();
            DefenseEntry.Text = _pokemon.IV_DEF.ToString();
            SpAttackEntry.Text = _pokemon.IV_SPA.ToString();
            SpDefenseEntry.Text = _pokemon.IV_SPD.ToString();
            SpeedEntry.Text = _pokemon.IV_SPE.ToString();

            // Moves
            Move1Entry.Text = _pokemon.Move1.ToString();
            Move2Entry.Text = _pokemon.Move2.ToString();
            Move3Entry.Text = _pokemon.Move3.ToString();
            Move4Entry.Text = _pokemon.Move4.ToString();

            // Special Properties
            ShinyCheckBox.IsChecked = _pokemon.IsShiny;
            EggCheckBox.IsChecked = _pokemon.IsEgg;
            HeldItemEntry.Text = _pokemon.HeldItem.ToString();

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

    private void OnHPChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_HP = iv;
        }
    }

    private void OnAttackChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_ATK = iv;
        }
    }

    private void OnDefenseChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_DEF = iv;
        }
    }

    private void OnSpAttackChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_SPA = iv;
        }
    }

    private void OnSpDefenseChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.IV_SPD = iv;
        }
    }

    private void OnSpeedChanged(object sender, TextChangedEventArgs e)
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

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}