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
    private List<SpeciesItem> _speciesItems = new();
    private List<MoveItem> _moveItems = new();
    private List<AbilityItem> _abilityItems = new();
    private List<NatureItem> _natureItems = new();
    private List<ItemItem> _itemItems = new();
    private List<BallItem> _ballItems = new();
    private List<FormItem> _formItems = new();
    private List<TypeItem> _typeItems = new();

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
            
            // Load species data if not already loaded
            if (!_speciesItems.Any())
            {
                _speciesItems = CachedDataService.GetSpecies().Cast<SpeciesItem>().ToList();
            }
            
            // Set species button text - show actual species name
            var selectedSpecies = _speciesItems.FirstOrDefault(x => x.Id == _pokemon.Species);
            SpeciesButton.Text = selectedSpecies?.DisplayName ?? "Select Species...";
            
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
            
            // Obedience Level - only available in Generation 9
            bool isGen8 = _pokemon.Format == 8;
            bool isGen9 = _pokemon.Format == 9;
            
            // Show/hide generation-specific frames
            Gen8Frame.IsVisible = isGen8;
            Gen9Frame.IsVisible = isGen9;
            
            // Load Type items for Tera Types if needed
            if ((isGen8 || isGen9) && !_typeItems.Any())
            {
                _typeItems = CachedDataService.GetTypes().Cast<TypeItem>().ToList();
                
                // Add special Tera Type options
                _typeItems.Insert(0, new TypeItem { Id = 19, DisplayName = "--- (None)" }); // TeraTypeUtil.OverrideNone
                _typeItems.Add(new TypeItem { Id = 99, DisplayName = "Stellar" }); // TeraTypeUtil.Stellar
            }
            
            if (isGen8)
            {
                // Load Gen 8 specific data
                if (_pokemon is IDynamaxLevel dynamaxPokemon)
                {
                    DynamaxLevelEntry.Text = dynamaxPokemon.DynamaxLevel.ToString();
                }
                if (_pokemon is IGigantamax gigantamaxPokemon)
                {
                    CanGigantamaxCheckBox.IsChecked = gigantamaxPokemon.CanGigantamax;
                }
                if (_pokemon.StatNature != _pokemon.Nature)
                {
                    var selectedStatNature = _natureItems.FirstOrDefault(x => x.Id == (int)_pokemon.StatNature);
                    StatNatureButton.Text = selectedStatNature?.DisplayName ?? "Select Stat Nature...";
                }
                else
                {
                    StatNatureButton.Text = "Same as Nature";
                }
            }
            
            if (isGen9)
            {
                // Load Gen 9 specific data
                if (_pokemon is ITeraType teraPokemon)
                {
                    var selectedTeraOriginal = _typeItems.FirstOrDefault(x => x.Id == (int)teraPokemon.TeraTypeOriginal);
                    TeraTypeOriginalButton.Text = selectedTeraOriginal?.DisplayName ?? "Select Tera Type...";
                    
                    var selectedTeraOverride = _typeItems.FirstOrDefault(x => x.Id == (int)teraPokemon.TeraTypeOverride);
                    TeraTypeOverrideButton.Text = selectedTeraOverride?.DisplayName ?? "Select Tera Type...";
                }
                if (_pokemon.StatNature != _pokemon.Nature)
                {
                    var selectedStatNature = _natureItems.FirstOrDefault(x => x.Id == (int)_pokemon.StatNature);
                    StatNatureGen9Button.Text = selectedStatNature?.DisplayName ?? "Select Stat Nature...";
                }
                else
                {
                    StatNatureGen9Button.Text = "Same as Nature";
                }
            }
            
            ObedienceLevelLabel.IsVisible = isGen9;
            ObedienceLevelEntry.IsVisible = isGen9;
            
            if (isGen9 && _pokemon is IObedienceLevel obediencePokemon)
            {
                ObedienceLevelEntry.Text = obediencePokemon.Obedience_Level.ToString();
            }
            else if (isGen9)
            {
                // Fallback for Gen 9 pokemon that somehow don't implement IObedienceLevel
                ObedienceLevelEntry.Text = "0";
            }

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

    private async void OnSpeciesButtonClicked(object sender, EventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        try
        {
            // Load species data if not already loaded
            if (!_speciesItems.Any())
            {
                _speciesItems = CachedDataService.GetSpecies().Cast<SpeciesItem>().ToList();
            }

            var currentSpecies = _speciesItems.FirstOrDefault(x => x.Id == _pokemon.Species);
            var result = await ShowPickerSafely(_speciesItems.Cast<IPickerItem>().ToList(), "Select Species", currentSpecies);
            
            if (result != null)
            {
                _pokemon.Species = (ushort)result.Id;
                SpeciesButton.Text = result.DisplayName;
                HeaderLabel.Text = $"编辑中: {GetSpeciesName(_pokemon.Species)} (第{_pokemon.Format}世代)";
                
                // Clear form items when species changes as they are species-specific
                _formItems.Clear();
                
                StatusLabel.Text = $"Species changed to: {result.DisplayName}";
                StatusLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to change species: {ex.Message}", "OK");
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
            _pokemon.CurrentLevel = level; // This automatically updates EXP and Stat_Level
            _pokemon.ResetPartyStats(); // Recalculate stats after level change
        }
    }

    private void OnHPIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.SetIV(0, iv); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after IV change
        }
    }

    private void OnAttackIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.SetIV(1, iv); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after IV change
        }
    }

    private void OnDefenseIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.SetIV(2, iv); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after IV change
        }
    }

    private void OnSpAttackIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.SetIV(4, iv); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after IV change
        }
    }

    private void OnSpDefenseIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.SetIV(5, iv); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after IV change
        }
    }

    private void OnSpeedIVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int iv) && iv >= 0 && iv <= 31)
        {
            _pokemon.SetIV(3, iv); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after IV change
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
            var currentSelection = _natureItems.FirstOrDefault(x => x.Id == (int)_pokemon.Nature);
            var result = await ShowPickerSafely(_natureItems.Cast<IPickerItem>().ToList(), "选择性格", currentSelection);
            
            if (result != null && _pokemon != null)
            {
                _pokemon.SetNature((Nature)result.Id);  // Use the proper extension method with Nature enum
                NatureButton.Text = result.DisplayName; // Nature items should already have Chinese names
                
                // Recalculate stats to apply nature changes
                _pokemon.ResetPartyStats();
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
            _pokemon.SetEV(0, ev); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after EV change
        }
    }

    private void OnAttackEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.SetEV(1, ev); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after EV change
        }
    }

    private void OnDefenseEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.SetEV(2, ev); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after EV change
        }
    }

    private void OnSpAttackEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.SetEV(4, ev); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after EV change
        }
    }

    private void OnSpDefenseEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.SetEV(5, ev); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after EV change
        }
    }

    private void OnSpeedEVChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (int.TryParse(e.NewTextValue, out int ev) && ev >= 0 && ev <= 252)
        {
            _pokemon.SetEV(3, ev); // Use Core library method
            _pokemon.ResetPartyStats(); // Recalculate stats after EV change
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

    private void OnObedienceLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        // Only available for Generation 9 Pokemon
        if (_pokemon.Format == 9 && _pokemon is IObedienceLevel obediencePokemon)
        {
            if (byte.TryParse(e.NewTextValue, out byte obedienceLevel) && obedienceLevel >= 0 && obedienceLevel <= 100)
            {
                obediencePokemon.Obedience_Level = obedienceLevel;
            }
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
            if (_pokemon == null)
            {
                await DisplayAlert("Error", "Cannot save: Pokemon data is null.", "OK");
                return;
            }

            if (_saveFile == null)
            {
                await DisplayAlert("Error", "Cannot save: Save file is null.", "OK");
                return;
            }

            // Prepare the Pokemon properly using the PKHeX Core patterns
            var preparedPokemon = PreparePokemonForSave(_pokemon);

            if (_boxIndex >= 0 && _slotIndex >= 0)
            {
                // Set the Pokemon to the specific box slot using the correct Core method
                _saveFile.SetBoxSlotAtIndex(preparedPokemon, _boxIndex, _slotIndex);
                await DisplayAlert("Success", $"Pokemon saved to Box {_boxIndex + 1}, Slot {_slotIndex + 1}!", "OK");
            }
            else if (_boxIndex == -1 && _slotIndex >= 0)
            {
                // Save to party slot
                if (_slotIndex < _saveFile.PartyCount)
                {
                    _saveFile.SetPartySlotAtIndex(preparedPokemon, _slotIndex);
                    await DisplayAlert("Success", $"Pokemon saved to Party position {_slotIndex + 1}!", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Invalid party position.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Success", "Pokemon data has been updated!", "OK");
            }
            
            StatusLabel.Text = "Pokemon saved successfully!";
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error saving: {ex.Message}";
            await DisplayAlert("Error", $"Failed to save Pokemon: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Prepares the Pokemon for saving using PKHeX Core patterns
    /// </summary>
    private PKM PreparePokemonForSave(PKM pokemon)
    {
        // Create a clone to avoid modifying the original
        var prepared = pokemon.Clone();
        
        // Apply all UI field changes to the Pokemon using SaveMisc pattern
        SaveAllFieldsToEntity(prepared);
        
        // Apply fixes in the correct order as per PKHeX Core patterns
        prepared.FixMoves();
        
        if (prepared.Format >= 6)
        {
            prepared.FixRelearn();
        }
        
        // Fix memories for newer generations
        if (prepared.Format >= 6)
        {
            prepared.FixMemories();
        }
        
        // Recalculate stats to ensure they're up to date
        prepared.ResetPartyStats();
        
        // Refresh checksum (must be last)
        prepared.RefreshChecksum();
        
        return prepared;
    }

    /// <summary>
    /// Saves all UI field values to the Pokemon entity (similar to WinForms SaveMisc methods)
    /// </summary>
    private void SaveAllFieldsToEntity(PKM pokemon)
    {
        if (pokemon == null) return;

        try
        {
            // Save basic info (similar to SaveMisc1)
            if (ushort.TryParse(SpeciesButton.Text.Split(' ')[0], out var species))
                pokemon.Species = species;
            
            if (int.TryParse(LevelEntry.Text, out var level) && level >= 1 && level <= 100)
                pokemon.CurrentLevel = level;
            
            if (!string.IsNullOrEmpty(NicknameEntry.Text))
                pokemon.Nickname = NicknameEntry.Text;

            // Save IVs using proper Core methods
            if (int.TryParse(HPIVEntry.Text, out var hpIV) && hpIV >= 0 && hpIV <= 31)
                pokemon.SetIV(0, hpIV);
            if (int.TryParse(AttackIVEntry.Text, out var atkIV) && atkIV >= 0 && atkIV <= 31)
                pokemon.SetIV(1, atkIV);
            if (int.TryParse(DefenseIVEntry.Text, out var defIV) && defIV >= 0 && defIV <= 31)
                pokemon.SetIV(2, defIV);
            if (int.TryParse(SpAttackIVEntry.Text, out var spaIV) && spaIV >= 0 && spaIV <= 31)
                pokemon.SetIV(4, spaIV);
            if (int.TryParse(SpDefenseIVEntry.Text, out var spdIV) && spdIV >= 0 && spdIV <= 31)
                pokemon.SetIV(5, spdIV);
            if (int.TryParse(SpeedIVEntry.Text, out var speIV) && speIV >= 0 && speIV <= 31)
                pokemon.SetIV(3, speIV);

            // Save EVs using proper Core methods
            if (int.TryParse(HPEVEntry.Text, out var hpEV) && hpEV >= 0 && hpEV <= 252)
                pokemon.SetEV(0, hpEV);
            if (int.TryParse(AttackEVEntry.Text, out var atkEV) && atkEV >= 0 && atkEV <= 252)
                pokemon.SetEV(1, atkEV);
            if (int.TryParse(DefenseEVEntry.Text, out var defEV) && defEV >= 0 && defEV <= 252)
                pokemon.SetEV(2, defEV);
            if (int.TryParse(SpAttackEVEntry.Text, out var spaEV) && spaEV >= 0 && spaEV <= 252)
                pokemon.SetEV(4, spaEV);
            if (int.TryParse(SpDefenseEVEntry.Text, out var spdEV) && spdEV >= 0 && spdEV <= 252)
                pokemon.SetEV(5, spdEV);
            if (int.TryParse(SpeedEVEntry.Text, out var speEV) && speEV >= 0 && speEV <= 252)
                pokemon.SetEV(3, speEV);

            // Save other fields
            if (int.TryParse(GenderEntry.Text, out var gender) && gender >= 0 && gender <= 2)
                pokemon.Gender = (byte)gender;
            
            if (int.TryParse(FriendshipEntry.Text, out var friendship) && friendship >= 0 && friendship <= 255)
                pokemon.CurrentFriendship = (byte)friendship;
            
            if (int.TryParse(LanguageEntry.Text, out var language) && language >= 0)
                pokemon.Language = language;
            
            if (int.TryParse(VersionEntry.Text, out var version) && version >= 0)
                pokemon.Version = (GameVersion)version;

            // Save trainer info
            if (!string.IsNullOrEmpty(OTNameEntry.Text))
                pokemon.OT_Name = OTNameEntry.Text;
            
            if (int.TryParse(OTGenderEntry.Text, out var otGender) && otGender >= 0 && otGender <= 1)
                pokemon.OT_Gender = (byte)otGender;
            
            if (ushort.TryParse(TIDEntry.Text, out var tid))
                pokemon.TID16 = tid;
            
            if (ushort.TryParse(SIDEntry.Text, out var sid))
                pokemon.SID16 = sid;
            
            if (int.TryParse(MetLocationEntry.Text, out var metLoc) && metLoc >= 0)
                pokemon.Met_Location = (ushort)metLoc;
            
            if (int.TryParse(MetLevelEntry.Text, out var metLevel) && metLevel >= 0 && metLevel <= 100)
                pokemon.Met_Level = (byte)metLevel;

            // Save boolean flags
            pokemon.FatefulEncounter = FatefulCheckBox.IsChecked == true;
            pokemon.IsEgg = EggCheckBox.IsChecked == true;

            // Handle shiny
            if (ShinyCheckBox.IsChecked == true)
            {
                var shinyVal = pokemon.ShinyXor;
                if (shinyVal >= 16)
                    pokemon.PID = (uint)(((pokemon.TID16 ^ pokemon.SID16) ^ (pokemon.PID & 0xFFFF)) << 16) | (pokemon.PID & 0xFFFF);
            }

            // Handle Obedience Level for Gen 9
            if (pokemon.Format == 9 && pokemon is IObedienceLevel obediencePokemon)
            {
                if (byte.TryParse(ObedienceLevelEntry.Text, out var obedienceLevel) && obedienceLevel >= 0 && obedienceLevel <= 100)
                    obediencePokemon.Obedience_Level = obedienceLevel;
            }

            // Handle held item
            // Note: Held item is set directly through the button click handler
            // The HeldItemButton text contains the item name, not the ID

            // Generation-specific properties
            if (pokemon.Format == 8)
            {
                // Gen 8 specific properties
                if (pokemon is IDynamaxLevel dynamaxPokemon && byte.TryParse(DynamaxLevelEntry.Text, out var dynamaxLevel) && dynamaxLevel <= 10)
                {
                    dynamaxPokemon.DynamaxLevel = dynamaxLevel;
                }
                
                if (pokemon is IGigantamax gigantamaxPokemon)
                {
                    gigantamaxPokemon.CanGigantamax = CanGigantamaxCheckBox.IsChecked == true;
                }
            }
            else if (pokemon.Format == 9)
            {
                // Gen 9 specific properties
                if (pokemon is ITeraType teraPokemon)
                {
                    // Note: Tera types are handled by the button click events
                    // as they're set directly when the user selects them
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SaveAllFieldsToEntity: {ex.Message}");
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

    // Gen 8 Event Handlers
    private void OnDynamaxLevelChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (byte.TryParse(e.NewTextValue, out byte level) && level <= 10 && _pokemon is IDynamaxLevel dynamaxPokemon)
        {
            dynamaxPokemon.DynamaxLevel = level;
        }
    }

    private void OnCanGigantamaxChanged(object sender, CheckedChangedEventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        if (_pokemon is IGigantamax gigantamaxPokemon)
        {
            gigantamaxPokemon.CanGigantamax = e.Value;
        }
    }

    private async void OnStatNatureButtonClicked(object sender, EventArgs e)
    {
        if (_isUpdating || _pokemon == null) return;

        try
        {
            if (!_natureItems.Any())
            {
                _natureItems = CachedDataService.GetNatures().Cast<NatureItem>().ToList();
            }

            var currentNature = _natureItems.FirstOrDefault(x => x.Id == (int)_pokemon.StatNature);
            var result = await ShowPickerSafely(_natureItems.Cast<IPickerItem>().ToList(), "Select Stat Nature", currentNature);

            if (result != null)
            {
                _pokemon.StatNature = (Nature)result.Id;
                
                // Update the appropriate button text based on generation
                if (_pokemon.Format == 8)
                {
                    StatNatureButton.Text = result.DisplayName;
                }
                else if (_pokemon.Format == 9)
                {
                    StatNatureGen9Button.Text = result.DisplayName;
                }
                
                // Recalculate stats after nature change
                _pokemon.ResetPartyStats();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to change stat nature: {ex.Message}", "OK");
        }
    }

    // Gen 9 Event Handlers
    private async void OnTeraTypeOriginalButtonClicked(object sender, EventArgs e)
    {
        if (_isUpdating || _pokemon == null || _pokemon is not ITeraType teraPokemon) return;

        try
        {
            if (!_typeItems.Any())
            {
                _typeItems = CachedDataService.GetTypes().Cast<TypeItem>().ToList();
                _typeItems.Insert(0, new TypeItem { Id = 19, DisplayName = "--- (None)" });
                _typeItems.Add(new TypeItem { Id = 99, DisplayName = "Stellar" });
            }

            var currentType = _typeItems.FirstOrDefault(x => x.Id == (int)teraPokemon.TeraTypeOriginal);
            var result = await ShowPickerSafely(_typeItems.Cast<IPickerItem>().ToList(), "Select Original Tera Type", currentType);

            if (result != null)
            {
                teraPokemon.TeraTypeOriginal = (MoveType)result.Id;
                TeraTypeOriginalButton.Text = result.DisplayName;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to change original tera type: {ex.Message}", "OK");
        }
    }

    private async void OnTeraTypeOverrideButtonClicked(object sender, EventArgs e)
    {
        if (_isUpdating || _pokemon == null || _pokemon is not ITeraType teraPokemon) return;

        try
        {
            if (!_typeItems.Any())
            {
                _typeItems = CachedDataService.GetTypes().Cast<TypeItem>().ToList();
                _typeItems.Insert(0, new TypeItem { Id = 19, DisplayName = "--- (None)" });
                _typeItems.Add(new TypeItem { Id = 99, DisplayName = "Stellar" });
            }

            var currentType = _typeItems.FirstOrDefault(x => x.Id == (int)teraPokemon.TeraTypeOverride);
            var result = await ShowPickerSafely(_typeItems.Cast<IPickerItem>().ToList(), "Select Override Tera Type", currentType);

            if (result != null)
            {
                teraPokemon.TeraTypeOverride = (MoveType)result.Id;
                TeraTypeOverrideButton.Text = result.DisplayName;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to change override tera type: {ex.Message}", "OK");
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

    /// <summary>
    /// Gets the type name with Chinese priority
    /// </summary>
    private string GetTypeName(int typeId)
    {
        try
        {
            if (typeId == 19) return "--- (None)"; // TeraTypeUtil.OverrideNone
            if (typeId == 99) return "Stellar"; // TeraTypeUtil.Stellar
            if (typeId == 0) return "普通"; // Normal type
            
            // Get Chinese type names first
            var chineseStrings = GameInfo.GetStrings("zh") ?? GameInfo.GetStrings("zh2");
            var englishStrings = GameInfo.GetStrings("en");
            
            string chineseName = "";
            string englishName = "";

            // Get Chinese type name
            if (chineseStrings?.types != null && typeId < chineseStrings.types.Length)
            {
                chineseName = chineseStrings.types[typeId];
            }

            // Get English type name
            if (englishStrings?.types != null && typeId < englishStrings.types.Length)
            {
                englishName = englishStrings.types[typeId];
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
            return $"属性 {typeId}";
        }
    }

    /// <summary>
    /// Gets the nature name with Chinese priority
    /// </summary>
    private string GetNatureName(Nature nature)
    {
        try
        {
            int natureId = (int)nature;
            
            // Get Chinese nature names first
            var chineseStrings = GameInfo.GetStrings("zh") ?? GameInfo.GetStrings("zh2");
            var englishStrings = GameInfo.GetStrings("en");
            
            string chineseName = "";
            string englishName = "";

            // Get Chinese nature name
            if (chineseStrings?.natures != null && natureId < chineseStrings.natures.Length)
            {
                chineseName = chineseStrings.natures[natureId];
            }

            // Get English nature name
            if (englishStrings?.natures != null && natureId < englishStrings.natures.Length)
            {
                englishName = englishStrings.natures[natureId];
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
            return $"性格 {(int)nature}";
        }
    }

    /// <summary>
    /// Gets the ball name with Chinese priority
    /// </summary>
    private string GetBallName(int ballId)
    {
        try
        {
            if (ballId == 0) return "无";
            
            // Get Chinese ball names first
            var chineseStrings = GameInfo.GetStrings("zh") ?? GameInfo.GetStrings("zh2");
            var englishStrings = GameInfo.GetStrings("en");
            
            string chineseName = "";
            string englishName = "";

            // Get Chinese ball name (ball names are part of items)
            if (chineseStrings?.itemlist != null && ballId < chineseStrings.itemlist.Length)
            {
                chineseName = chineseStrings.itemlist[ballId];
            }

            // Get English ball name
            if (englishStrings?.itemlist != null && ballId < englishStrings.itemlist.Length)
            {
                englishName = englishStrings.itemlist[ballId];
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
            return $"精灵球 {ballId}";
        }
    }
}