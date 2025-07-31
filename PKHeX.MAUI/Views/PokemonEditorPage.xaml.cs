using PKHeX.Core;
using PKHeX.MAUI.Services;
using PKHeX.MAUI.Models;

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
        
        // Load data immediately without any background operations
        LoadPokemonData();
    }

    /// <summary>
    /// Safe wrapper for SearchablePickerPage operations
    /// </summary>
    private async Task<IPickerItem?> ShowPickerSafely(List<IPickerItem> items, string title, IPickerItem? currentSelection = null)
    {
        try
        {
            if (items == null || items.Count == 0)
            {
                await DisplayAlert("错误", "没有可选择的项目", "确定");
                return null;
            }

            SearchablePickerPage pickerPage;
            try
            {
                pickerPage = new SearchablePickerPage();
            }
            catch (Exception initEx)
            {
                await DisplayAlert("错误", $"无法创建选择器界面: {initEx.Message}", "确定");
                return null;
            }

            try
            {
                pickerPage.SetItems(items, title, currentSelection);
            }
            catch (Exception setEx)
            {
                await DisplayAlert("错误", $"无法设置选择器数据: {setEx.Message}", "确定");
                return null;
            }
            
            var completionSource = new TaskCompletionSource<IPickerItem?>();
            pickerPage.CompletionSource = completionSource;
            
            try
            {
                await Navigation.PushModalAsync(pickerPage);
                return await completionSource.Task;
            }
            catch (Exception navEx)
            {
                await DisplayAlert("错误", $"无法打开选择器: {navEx.Message}", "确定");
                return null;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择器操作失败: {ex.Message}", "确定");
            return null;
        }
    }

    private void LoadPokemonData()
    {
        if (_pokemon == null) return;

        try
        {
            _isUpdating = true;

            // Basic information  
            var generationInfo = GetGenerationInfo(_pokemon);
            HeaderLabel.Text = $"编辑中: {GetSpeciesName(_pokemon.Species)} (第{_pokemon.Format}世代)";
            GenerationLabel.Text = generationInfo;
            SpeciesEntry.Text = _pokemon.Species.ToString();
            NicknameEntry.Text = _pokemon.Nickname;
            LevelEntry.Text = _pokemon.CurrentLevel.ToString();
            
            // Load only essential data (natures and balls are small lists)
            if (!_natureItems.Any())
            {
                _natureItems = CachedDataService.GetNatures().Cast<NatureItem>().ToList();
            }
            if (!_ballItems.Any())
            {
                _ballItems = CachedDataService.GetBalls().Cast<BallItem>().ToList();
            }
            if (!_formItems.Any() && _pokemon != null)
            {
                _formItems = CachedDataService.GetForms(_pokemon.Species, _pokemon.Context).Cast<FormItem>().ToList();
            }
            
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

            // Moves - Show actual move names instead of just IDs
            Move1Button.Text = _pokemon.Move1 == 0 ? "选择招式..." : GetMoveName((ushort)_pokemon.Move1);
            Move2Button.Text = _pokemon.Move2 == 0 ? "选择招式..." : GetMoveName((ushort)_pokemon.Move2);
            Move3Button.Text = _pokemon.Move3 == 0 ? "选择招式..." : GetMoveName((ushort)_pokemon.Move3);
            Move4Button.Text = _pokemon.Move4 == 0 ? "选择招式..." : GetMoveName((ushort)_pokemon.Move4);

            // Physical Properties
            GenderEntry.Text = _pokemon.Gender.ToString();
            
            // Set ability button text - show actual ability name
            AbilityButton.Text = _pokemon.Ability == 0 ? "选择特性..." : GetAbilityName(_pokemon.Ability);
            
            // Set form button text
            var selectedForm = _formItems.FirstOrDefault(x => x.Id == _pokemon.Form);
            FormButton.Text = selectedForm?.DisplayName ?? "选择形态...";
            
            // Set ball button text
            var selectedBall = _ballItems.FirstOrDefault(x => x.Id == _pokemon.Ball);
            BallButton.Text = selectedBall?.DisplayName ?? "选择精灵球...";
            
            ShinyCheckBox.IsChecked = _pokemon.IsShiny;
            EggCheckBox.IsChecked = _pokemon.IsEgg;
            
            // Set held item button text - show actual item name
            HeldItemButton.Text = _pokemon.HeldItem == 0 ? "选择道具..." : GetItemName(_pokemon.HeldItem);

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
        if (_pokemon == null) return;
        
        try
        {
            // Load moves only when needed
            if (!_moveItems.Any())
            {
                _moveItems = CachedDataService.GetMoves().Cast<MoveItem>().ToList();
            }
            
            var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move1);
            var result = await ShowPickerSafely(_moveItems.Cast<IPickerItem>().ToList(), "选择招式 1", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Move1 = (ushort)result.Id;
                Move1Button.Text = GetMoveName((ushort)result.Id); // Use our Chinese-enabled method
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择招式失败: {ex.Message}", "确定");
        }
    }

    private async void OnMove2ButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            // Load moves only when needed
            if (!_moveItems.Any())
            {
                _moveItems = CachedDataService.GetMoves().Cast<MoveItem>().ToList();
            }
            
            var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move2);
            var result = await ShowPickerSafely(_moveItems.Cast<IPickerItem>().ToList(), "选择招式 2", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Move2 = (ushort)result.Id;
                Move2Button.Text = GetMoveName((ushort)result.Id); // Use our Chinese-enabled method
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择招式失败: {ex.Message}", "确定");
        }
    }

    private async void OnMove3ButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            // Load moves only when needed
            if (!_moveItems.Any())
            {
                _moveItems = CachedDataService.GetMoves().Cast<MoveItem>().ToList();
            }
            
            var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move3);
            var result = await ShowPickerSafely(_moveItems.Cast<IPickerItem>().ToList(), "选择招式 3", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Move3 = (ushort)result.Id;
                Move3Button.Text = GetMoveName((ushort)result.Id); // Use our Chinese-enabled method
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择招式失败: {ex.Message}", "确定");
        }
    }

    private async void OnMove4ButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            // Load moves only when needed
            if (!_moveItems.Any())
            {
                _moveItems = CachedDataService.GetMoves().Cast<MoveItem>().ToList();
            }
            
            var currentSelection = _moveItems.FirstOrDefault(x => x.Id == _pokemon.Move4);
            var result = await ShowPickerSafely(_moveItems.Cast<IPickerItem>().ToList(), "选择招式 4", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Move4 = (ushort)result.Id;
                Move4Button.Text = GetMoveName((ushort)result.Id); // Use our Chinese-enabled method
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择招式失败: {ex.Message}", "确定");
        }
    }

    private async void OnAbilityButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            // Load abilities only when needed
            if (!_abilityItems.Any())
            {
                _abilityItems = CachedDataService.GetAbilities().Cast<AbilityItem>().ToList();
            }
            
            var currentSelection = _abilityItems.FirstOrDefault(x => x.Id == _pokemon.Ability);
            var result = await ShowPickerSafely(_abilityItems.Cast<IPickerItem>().ToList(), "选择特性", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Ability = result.Id;
                AbilityButton.Text = GetAbilityName(result.Id); // Use our Chinese-enabled method
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择特性失败: {ex.Message}", "确定");
        }
    }

    private async void OnNatureButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            var currentSelection = _natureItems.FirstOrDefault(x => x.Id == _pokemon.Nature);
            var result = await ShowPickerSafely(_natureItems.Cast<IPickerItem>().ToList(), "选择性格", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Nature = result.Id;
                NatureButton.Text = result.DisplayName; // Nature items should already have Chinese names
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择性格失败: {ex.Message}", "确定");
        }
    }

    private async void OnFormButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            var currentSelection = _formItems.FirstOrDefault(x => x.Id == _pokemon.Form);
            var result = await ShowPickerSafely(_formItems.Cast<IPickerItem>().ToList(), "选择形态", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Form = (byte)result.Id;
                FormButton.Text = GetFormName(_pokemon.Species, (byte)result.Id); // Use our Chinese-enabled method
                HeaderLabel.Text = $"编辑中: {GetSpeciesName(_pokemon.Species)} {GetFormName(_pokemon.Species, (byte)result.Id)} (第{_pokemon.Format}世代)";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择形态失败: {ex.Message}", "确定");
        }
    }

    private async void OnBallButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            var currentSelection = _ballItems.FirstOrDefault(x => x.Id == _pokemon.Ball);
            var result = await ShowPickerSafely(_ballItems.Cast<IPickerItem>().ToList(), "选择精灵球", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.Ball = result.Id;
                BallButton.Text = result.DisplayName; // Ball items should already have Chinese names
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择精灵球失败: {ex.Message}", "确定");
        }
    }

    private async void OnHeldItemButtonClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;
        
        try
        {
            // Load items only when needed
            if (!_itemItems.Any())
            {
                _itemItems = CachedDataService.GetItems().Cast<ItemItem>().ToList();
            }
            
            var currentSelection = _itemItems.FirstOrDefault(x => x.Id == _pokemon.HeldItem);
            var result = await ShowPickerSafely(_itemItems.Cast<IPickerItem>().ToList(), "选择携带道具", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.HeldItem = result.Id;
                HeldItemButton.Text = GetItemName(result.Id); // Use our Chinese-enabled method
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"选择携带道具失败: {ex.Message}", "确定");
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
    /// Gets the item name for display with Chinese priority
    /// </summary>
    private string GetItemName(int itemId)
    {
        try
        {
            if (itemId == 0) return "无道具";

            // Get Chinese name first (try simplified, then traditional)
            var chineseStrings = GameInfo.GetStrings("zh");
            string chineseName = "";
            if (chineseStrings?.itemlist != null && itemId < chineseStrings.itemlist.Length)
            {
                chineseName = chineseStrings.itemlist[itemId];
            }
            else
            {
                // Try traditional Chinese if simplified not available
                var traditionalStrings = GameInfo.GetStrings("zh2");
                if (traditionalStrings?.itemlist != null && itemId < traditionalStrings.itemlist.Length)
                {
                    chineseName = traditionalStrings.itemlist[itemId];
                }
            }

            // Get English name as fallback/supplement
            var englishStrings = GameInfo.GetStrings("en");
            string englishName = "Unknown";
            if (englishStrings?.itemlist != null && itemId < englishStrings.itemlist.Length)
            {
                englishName = englishStrings.itemlist[itemId];
            }

            // Format the display name with Chinese as primary
            if (!string.IsNullOrEmpty(chineseName))
            {
                // If Chinese and English are different, show both
                if (!string.IsNullOrEmpty(englishName) && chineseName != englishName)
                {
                    return $"{chineseName} ({englishName})";
                }
                return chineseName;
            }
            
            return englishName; // Fallback to English if Chinese not available
        }
        catch
        {
            return $"道具 {itemId}";
        }
    }

    /// <summary>
    /// Gets the move name with Chinese priority
    /// </summary>
    private string GetMoveName(ushort moveId)
    {
        try
        {
            if (moveId == 0) return "无招式";

            // Get Chinese name first (try simplified, then traditional)
            var chineseStrings = GameInfo.GetStrings("zh");
            string chineseName = "";
            if (chineseStrings?.movelist != null && moveId < chineseStrings.movelist.Length)
            {
                chineseName = chineseStrings.movelist[moveId];
            }
            else
            {
                // Try traditional Chinese if simplified not available
                var traditionalStrings = GameInfo.GetStrings("zh2");
                if (traditionalStrings?.movelist != null && moveId < traditionalStrings.movelist.Length)
                {
                    chineseName = traditionalStrings.movelist[moveId];
                }
            }

            // Get English name as fallback/supplement
            var englishStrings = GameInfo.GetStrings("en");
            string englishName = "Unknown";
            if (englishStrings?.movelist != null && moveId < englishStrings.movelist.Length)
            {
                englishName = englishStrings.movelist[moveId];
            }

            // Format the display name with Chinese as primary
            if (!string.IsNullOrEmpty(chineseName))
            {
                // If Chinese and English are different, show both
                if (!string.IsNullOrEmpty(englishName) && chineseName != englishName)
                {
                    return $"{chineseName} ({englishName})";
                }
                return chineseName;
            }
            
            return englishName; // Fallback to English if Chinese not available
        }
        catch
        {
            return $"招式 {moveId}";
        }
    }

    /// <summary>
    /// Gets the ability name with Chinese priority
    /// </summary>
    private string GetAbilityName(int abilityId)
    {
        try
        {
            if (abilityId == 0) return "无特性";

            // Get Chinese name first (try simplified, then traditional)
            var chineseStrings = GameInfo.GetStrings("zh");
            string chineseName = "";
            if (chineseStrings?.abilitylist != null && abilityId < chineseStrings.abilitylist.Length)
            {
                chineseName = chineseStrings.abilitylist[abilityId];
            }
            else
            {
                // Try traditional Chinese if simplified not available
                var traditionalStrings = GameInfo.GetStrings("zh2");
                if (traditionalStrings?.abilitylist != null && abilityId < traditionalStrings.abilitylist.Length)
                {
                    chineseName = traditionalStrings.abilitylist[abilityId];
                }
            }

            // Get English name as fallback/supplement
            var englishStrings = GameInfo.GetStrings("en");
            string englishName = "Unknown";
            if (englishStrings?.abilitylist != null && abilityId < englishStrings.abilitylist.Length)
            {
                englishName = englishStrings.abilitylist[abilityId];
            }

            // Format the display name with Chinese as primary
            if (!string.IsNullOrEmpty(chineseName))
            {
                // If Chinese and English are different, show both
                if (!string.IsNullOrEmpty(englishName) && chineseName != englishName)
                {
                    return $"{chineseName} ({englishName})";
                }
                return chineseName;
            }
            
            return englishName; // Fallback to English if Chinese not available
        }
        catch
        {
            return $"特性 {abilityId}";
        }
    }

    /// <summary>
    /// Gets the form name for a species with Chinese priority
    /// </summary>
    private string GetFormName(ushort species, byte form)
    {
        try
        {
            if (form == 0) return "普通形态";

            // Use the pokemon's context if available, otherwise default to Gen 9
            var context = _pokemon?.Context ?? EntityContext.Gen9;
            
            // Get Chinese form names first
            var chineseStrings = GameInfo.GetStrings("zh") ?? GameInfo.GetStrings("zh2");
            var englishStrings = GameInfo.GetStrings("en");
            
            var englishForms = FormConverter.GetFormList(species, englishStrings.types, englishStrings.forms, GameInfo.GenderSymbolUnicode, context);
            var chineseForms = chineseStrings != null ? 
                FormConverter.GetFormList(species, chineseStrings.types ?? englishStrings.types, 
                    chineseStrings.forms ?? englishStrings.forms, GameInfo.GenderSymbolUnicode, context) :
                null;

            string englishName = "";
            string chineseName = "";
            
            // Get English form name
            if (englishForms != null && form < englishForms.Length)
            {
                englishName = englishForms[form];
            }
            
            // Get Chinese form name
            if (chineseForms != null && form < chineseForms.Length)
            {
                chineseName = chineseForms[form];
            }

            // Format the display name with Chinese as primary
            if (!string.IsNullOrEmpty(chineseName))
            {
                // If Chinese and English are different, show both
                if (!string.IsNullOrEmpty(englishName) && chineseName != englishName)
                {
                    return $"{chineseName} ({englishName})";
                }
                return chineseName;
            }
            else if (!string.IsNullOrEmpty(englishName))
            {
                return englishName;
            }

            return $"形态 {form}";
        }
        catch
        {
            return $"形态 {form}";
        }
    }
}