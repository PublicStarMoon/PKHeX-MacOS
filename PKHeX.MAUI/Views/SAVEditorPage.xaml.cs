using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class SAVEditorPage : ContentPage
{
    private SaveFile? _saveFile;
    private bool _isUpdating;

    public SAVEditorPage()
    {
        InitializeComponent();
    }

    public SAVEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadSaveData();
    }

    private void LoadSaveData()
    {
        if (_saveFile == null) return;

        _isUpdating = true;

        try
        {
            LoadTrainerData();
            LoadGameProgress();
            LoadStatistics();
            LoadMiscData();
            UpdateGameInfo();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void LoadTrainerData()
    {
        if (_saveFile == null) return;

        TrainerNameEntry.Text = _saveFile.OT;
        TrainerIDLabel.Text = $"ID: {_saveFile.DisplayTID:D5}";
        
        if (_saveFile.Generation >= 3)
            TrainerIDLabel.Text += $"/{_saveFile.DisplaySID:D5}";

        GenderLabel.Text = _saveFile.Gender == 0 ? "Male" : "Female";
        LanguageLabel.Text = $"Language: {(LanguageID)_saveFile.Language}";

        // Money
        if (_saveFile.Money is var money && money > 0)
        {
            MoneyEntry.Text = money.ToString();
        }

        // Location
        LocationLabel.Text = $"Location: {GameInfo.Strings.metLocations[_saveFile.Generation][_saveFile.Situation.M]}";
    }

    private void LoadGameProgress()
    {
        if (_saveFile == null) return;

        try
        {
            // Playtime
            if (_saveFile is ISaveBlock6Main sav6)
            {
                PlaytimeLabel.Text = $"Playtime: {sav6.PlayedHours:D2}:{sav6.PlayedMinutes:D2}:{sav6.PlayedSeconds:D2}";
            }
            else if (_saveFile.PlayedHours > 0 || _saveFile.PlayedMinutes > 0)
            {
                PlaytimeLabel.Text = $"Playtime: {_saveFile.PlayedHours:D2}:{_saveFile.PlayedMinutes:D2}:{_saveFile.PlayedSeconds:D2}";
            }

            // Badges
            var badgeCount = GetBadgeCount();
            BadgesLabel.Text = $"Badges: {badgeCount}/8";

            // Pokédex
            var pokedexCount = GetPokedexCount();
            PokedexLabel.Text = $"Pokédex: {pokedexCount.seen} seen, {pokedexCount.caught} caught";

        }
        catch (Exception ex)
        {
            PlaytimeLabel.Text = "Playtime: Unknown";
            BadgesLabel.Text = "Badges: Unknown";
            PokedexLabel.Text = $"Pokédex: Error - {ex.Message}";
        }
    }

    private void LoadStatistics()
    {
        if (_saveFile == null) return;

        try
        {
            // Basic stats that most saves have
            var stats = new List<string>();

            if (_saveFile is SAV4 sav4)
            {
                stats.Add($"Steps: {sav4.M.PoketchStep}");
            }

            if (_saveFile is ISaveBlock6Main sav6)
            {
                stats.Add($"Battles: {sav6.Record.GetRecord6(000)}"); // Battle count
                stats.Add($"Trades: {sav6.Record.GetRecord6(005)}");   // Trade count
            }

            StatisticsLabel.Text = stats.Count > 0 ? string.Join("\n", stats) : "No statistics available";
        }
        catch
        {
            StatisticsLabel.Text = "Statistics: Unable to load";
        }
    }

    private void LoadMiscData()
    {
        if (_saveFile == null) return;

        try
        {
            // Game version info
            GameVersionLabel.Text = $"Version: {_saveFile.Version}";
            GenerationLabel.Text = $"Generation: {_saveFile.Generation}";
            
            // Save type
            var saveType = _saveFile.GetType().Name;
            SaveTypeLabel.Text = $"Save Type: {saveType}";

            // File size
            FileSizeLabel.Text = $"File Size: {_saveFile.Data.Length:N0} bytes";
        }
        catch (Exception ex)
        {
            GameVersionLabel.Text = $"Version: Error - {ex.Message}";
        }
    }

    private void UpdateGameInfo()
    {
        if (_saveFile == null) return;

        var info = $"{_saveFile.Version} - Generation {_saveFile.Generation}";
        GameInfoHeaderLabel.Text = info;
    }

    private int GetBadgeCount()
    {
        if (_saveFile == null) return 0;

        try
        {
            return _saveFile switch
            {
                SAV1 sav1 => sav1.Badges,
                SAV2 sav2 => sav2.Badges,
                SAV3 sav3 => sav3.Badges,
                SAV4 sav4 => sav4.Badges,
                SAV5 sav5 => sav5.Badges,
                SAV6XY sav6xy => sav6xy.Badges,
                SAV6AO sav6ao => sav6ao.Badges,
                SAV7SM sav7sm => sav7sm.Misc.Badges,
                SAV7USUM sav7usum => sav7usum.Misc.Badges,
                SAV8SWSH sav8 => sav8.Badges,
                SAV8BS sav8bs => sav8bs.Badges,
                SAV9SV sav9 => sav9.Badges,
                _ => 0
            };
        }
        catch
        {
            return 0;
        }
    }

    private (int seen, int caught) GetPokedexCount()
    {
        if (_saveFile == null) return (0, 0);

        try
        {
            var seen = 0;
            var caught = 0;

            // Get species count for this generation
            var maxSpecies = _saveFile.Personal.MaxSpeciesID;

            for (ushort species = 1; species <= maxSpecies; species++)
            {
                if (_saveFile.GetSeen(species))
                    seen++;
                if (_saveFile.GetCaught(species))
                    caught++;
            }

            return (seen, caught);
        }
        catch
        {
            return (0, 0);
        }
    }

    // Event Handlers
    private void OnTrainerNameChanged(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        _saveFile.OT = e.NewTextValue ?? "";
    }

    private void OnMoneyChanged(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        if (uint.TryParse(e.NewTextValue, out var money))
        {
            _saveFile.Money = Math.Min(money, 999999); // Cap at typical max
        }
    }

    private async void OnInventoryClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var inventoryPage = new InventoryEditorPage(_saveFile);
            await Navigation.PushAsync(inventoryPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open inventory editor: {ex.Message}", "OK");
        }
    }

    private async void OnPokedexClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var pokedexPage = new PokedexEditorPage(_saveFile);
            await Navigation.PushAsync(pokedexPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open Pokédex editor: {ex.Message}", "OK");
        }
    }

    private async void OnEventFlagsClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var eventFlagsPage = new EventFlagsEditorPage(_saveFile);
            await Navigation.PushAsync(eventFlagsPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open event flags editor: {ex.Message}", "OK");
        }
    }

    private async void OnMiscDataClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var miscPage = new MiscDataEditorPage(_saveFile);
            await Navigation.PushAsync(miscPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open misc data editor: {ex.Message}", "OK");
        }
    }

    private async void OnMaxMoneyClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            _saveFile.Money = 999999;
            MoneyEntry.Text = "999999";
            await DisplayAlert("Success", "Money set to maximum!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to set max money: {ex.Message}", "OK");
        }
    }

    private async void OnCompletePokedexClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        var result = await DisplayAlert("Confirm", 
            "This will mark all Pokémon as seen and caught. Continue?", 
            "Yes", "No");

        if (!result) return;

        try
        {
            var maxSpecies = _saveFile.Personal.MaxSpeciesID;

            for (ushort species = 1; species <= maxSpecies; species++)
            {
                _saveFile.SetSeen(species, true);
                _saveFile.SetCaught(species, true);
            }

            LoadGameProgress(); // Refresh display
            await DisplayAlert("Success", "Pokédex completed!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to complete Pokédex: {ex.Message}", "OK");
        }
    }

    private async void OnMaxBadgesClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            // Set all badges for the respective generation
            switch (_saveFile)
            {
                case SAV1 sav1:
                    sav1.Badges = 0xFF; // All 8 badges
                    break;
                case SAV2 sav2:
                    sav2.Badges = 0xFFFF; // All 16 badges (Johto + Kanto)
                    break;
                case SAV3 sav3:
                    sav3.Badges = 0xFF; // All 8 badges
                    break;
                case SAV4 sav4:
                    sav4.Badges = 0xFF; // All 8 badges
                    break;
                case SAV5 sav5:
                    sav5.Badges = 0xFF; // All 8 badges
                    break;
                case SAV6XY sav6xy:
                    sav6xy.Badges = 0xFF; // All 8 badges
                    break;
                case SAV6AO sav6ao:
                    sav6ao.Badges = 0xFF; // All 8 badges
                    break;
                case SAV7SM sav7sm:
                    sav7sm.Misc.Badges = 0xFF; // All Z-Crystals
                    break;
                case SAV7USUM sav7usum:
                    sav7usum.Misc.Badges = 0xFF; // All Z-Crystals
                    break;
                case SAV8SWSH sav8:
                    sav8.Badges = 0xFF; // All 8 badges
                    break;
                case SAV8BS sav8bs:
                    sav8bs.Badges = 0xFF; // All 8 badges
                    break;
                case SAV9SV sav9:
                    sav9.Badges = 0xFF; // All badges
                    break;
            }

            LoadGameProgress(); // Refresh display
            await DisplayAlert("Success", "All badges obtained!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to set badges: {ex.Message}", "OK");
        }
    }

    private async void OnResetPlaytimeClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        var result = await DisplayAlert("Confirm", 
            "Reset playtime to 00:00:00?", 
            "Yes", "No");

        if (!result) return;

        try
        {
            _saveFile.PlayedHours = 0;
            _saveFile.PlayedMinutes = 0;
            _saveFile.PlayedSeconds = 0;

            LoadGameProgress(); // Refresh display
            await DisplayAlert("Success", "Playtime reset!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to reset playtime: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
