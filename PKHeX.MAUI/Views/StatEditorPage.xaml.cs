using PKHeX.Core;
using PKHeX.MAUI.Utilities;

namespace PKHeX.MAUI.Views;

public partial class StatEditorPage : ContentPage
{
    private PKM? _pokemon;
    private SaveFile? _saveFile;
    private bool _isUpdating;

    // Stat arrays for easier management
    private Slider[] _ivSliders = null!;
    private Label[] _ivLabels = null!;
    private Slider[] _evSliders = null!;
    private Label[] _evLabels = null!;
    private Label[] _statLabels = null!;
    private Label[] _baseStatLabels = null!;

    public StatEditorPage()
    {
        InitializeComponent();
        InitializeArrays();
        SetupEventHandlers();
    }

    public StatEditorPage(PKM pokemon, SaveFile saveFile) : this()
    {
        _pokemon = pokemon;
        _saveFile = saveFile;
        LoadPokemonStats();
    }

    private void InitializeArrays()
    {
        _ivSliders = new[] { HpIvSlider, AttackIvSlider, DefenseIvSlider, SpAttackIvSlider, SpDefenseIvSlider, SpeedIvSlider };
        _ivLabels = new[] { HpIvLabel, AttackIvLabel, DefenseIvLabel, SpAttackIvLabel, SpDefenseIvLabel, SpeedIvLabel };
        _evSliders = new[] { HpEvSlider, AttackEvSlider, DefenseEvSlider, SpAttackEvSlider, SpDefenseEvSlider, SpeedEvSlider };
        _evLabels = new[] { HpEvLabel, AttackEvLabel, DefenseEvLabel, SpAttackEvLabel, SpDefenseEvLabel, SpeedEvLabel };
        _statLabels = new[] { HpStatLabel, AttackStatLabel, DefenseStatLabel, SpAttackStatLabel, SpDefenseStatLabel, SpeedStatLabel };
        _baseStatLabels = new[] { HpBaseLabel, AttackBaseLabel, DefenseBaseLabel, SpAttackBaseLabel, SpDefenseBaseLabel, SpeedBaseLabel };
    }

    private void SetupEventHandlers()
    {
        for (int i = 0; i < 6; i++)
        {
            int index = i; // Capture for closure
            _ivSliders[i].ValueChanged += (s, e) => OnIvChanged(index, (int)e.NewValue);
            _evSliders[i].ValueChanged += (s, e) => OnEvChanged(index, (int)e.NewValue);
        }
    }

    private void LoadPokemonStats()
    {
        if (_pokemon == null) return;

        _isUpdating = true;

        try
        {
            // Load current IVs
            var ivs = new[] { _pokemon.IV_HP, _pokemon.IV_ATK, _pokemon.IV_DEF, _pokemon.IV_SPA, _pokemon.IV_SPD, _pokemon.IV_SPE };
            for (int i = 0; i < 6; i++)
            {
                _ivSliders[i].Value = ivs[i];
                _ivLabels[i].Text = ivs[i].ToString();
                
                // Set IV slider color based on value
                _ivLabels[i].TextColor = GetIvColor(ivs[i]);
            }

            // Load current EVs
            var evs = new[] { _pokemon.EV_HP, _pokemon.EV_ATK, _pokemon.EV_DEF, _pokemon.EV_SPA, _pokemon.EV_SPD, _pokemon.EV_SPE };
            for (int i = 0; i < 6; i++)
            {
                _evSliders[i].Value = evs[i];
                _evLabels[i].Text = evs[i].ToString();
            }

            // Load base stats
            LoadBaseStats();

            // Update calculated stats
            UpdateCalculatedStats();

            // Update totals
            UpdateTotals();

            // Update Hyper Training display if applicable
            UpdateHyperTrainingDisplay();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void LoadBaseStats()
    {
        if (_pokemon == null) return;

        var baseStats = PokemonHelper.GetBaseStats(_pokemon.Species);
        var stats = new[] { baseStats.HP, baseStats.ATK, baseStats.DEF, baseStats.SPA, baseStats.SPD, baseStats.SPE };

        for (int i = 0; i < 6; i++)
        {
            _baseStatLabels[i].Text = stats[i].ToString();
        }
    }

    private void UpdateCalculatedStats()
    {
        if (_pokemon == null || _isUpdating) return;

        var stats = new[]
        {
            _pokemon.Stat_HP,
            _pokemon.Stat_ATK,
            _pokemon.Stat_DEF,
            _pokemon.Stat_SPA,
            _pokemon.Stat_SPD,
            _pokemon.Stat_SPE
        };

        for (int i = 0; i < 6; i++)
        {
            _statLabels[i].Text = stats[i].ToString();
            
            // Color coding based on nature
            _statLabels[i].TextColor = GetStatColor(i);
        }

        BstLabel.Text = (stats[0] + stats[1] + stats[2] + stats[3] + stats[4] + stats[5]).ToString();
    }

    private void UpdateTotals()
    {
        if (_pokemon == null) return;

        var ivTotal = _pokemon.IV_HP + _pokemon.IV_ATK + _pokemon.IV_DEF + _pokemon.IV_SPA + _pokemon.IV_SPD + _pokemon.IV_SPE;
        var evTotal = _pokemon.EV_HP + _pokemon.EV_ATK + _pokemon.EV_DEF + _pokemon.EV_SPA + _pokemon.EV_SPD + _pokemon.EV_SPE;

        IvTotalLabel.Text = $"{ivTotal}/186";
        EvTotalLabel.Text = $"{evTotal}/510";

        // Color code EV total
        EvTotalLabel.TextColor = evTotal > 510 ? Colors.Red : evTotal == 510 ? Colors.Green : Colors.Black;
    }

    private void UpdateHyperTrainingDisplay()
    {
        if (_pokemon is not IHyperTrain ht) 
        {
            HyperTrainingFrame.IsVisible = false;
            return;
        }

        HyperTrainingFrame.IsVisible = true;
        
        var htFlags = new[] { ht.HT_HP, ht.HT_ATK, ht.HT_DEF, ht.HT_SPA, ht.HT_SPD, ht.HT_SPE };
        var htLabels = new[] { HtHpLabel, HtAttackLabel, HtDefenseLabel, HtSpAttackLabel, HtSpDefenseLabel, HtSpeedLabel };

        for (int i = 0; i < 6; i++)
        {
            htLabels[i].Text = htFlags[i] ? "✓" : "";
            htLabels[i].TextColor = htFlags[i] ? Colors.Gold : Colors.Gray;
        }
    }

    private Color GetIvColor(int iv)
    {
        return iv switch
        {
            31 => Colors.Green,
            0 => Colors.Red,
            _ => Colors.Black
        };
    }

    private Color GetStatColor(int statIndex)
    {
        if (_pokemon == null) return Colors.Black;

        var nature = _pokemon.Nature;
        var increased = Nature.GetStatMultiplier(nature, statIndex + 1) > 1.0f;
        var decreased = Nature.GetStatMultiplier(nature, statIndex + 1) < 1.0f;

        if (increased) return Colors.Red;
        if (decreased) return Colors.Blue;
        return Colors.Black;
    }

    private void OnIvChanged(int statIndex, int newValue)
    {
        if (_pokemon == null || _isUpdating) return;

        // Update Pokemon IV
        switch (statIndex)
        {
            case 0: _pokemon.IV_HP = newValue; break;
            case 1: _pokemon.IV_ATK = newValue; break;
            case 2: _pokemon.IV_DEF = newValue; break;
            case 3: _pokemon.IV_SPA = newValue; break;
            case 4: _pokemon.IV_SPD = newValue; break;
            case 5: _pokemon.IV_SPE = newValue; break;
        }

        // Update label
        _ivLabels[statIndex].Text = newValue.ToString();
        _ivLabels[statIndex].TextColor = GetIvColor(newValue);

        // Refresh stats
        _pokemon.RefreshChecksum();
        UpdateCalculatedStats();
        UpdateTotals();
    }

    private void OnEvChanged(int statIndex, int newValue)
    {
        if (_pokemon == null || _isUpdating) return;

        // Check EV total constraint
        var currentTotal = _pokemon.EV_HP + _pokemon.EV_ATK + _pokemon.EV_DEF + _pokemon.EV_SPA + _pokemon.EV_SPD + _pokemon.EV_SPE;
        var currentStatEv = statIndex switch
        {
            0 => _pokemon.EV_HP,
            1 => _pokemon.EV_ATK,
            2 => _pokemon.EV_DEF,
            3 => _pokemon.EV_SPA,
            4 => _pokemon.EV_SPD,
            5 => _pokemon.EV_SPE,
            _ => 0
        };

        var newTotal = currentTotal - currentStatEv + newValue;
        if (newTotal > 510)
        {
            // Adjust to max allowed
            newValue = 510 - (currentTotal - currentStatEv);
            _evSliders[statIndex].Value = newValue;
            DisplayAlert("EV Limit", $"EV total cannot exceed 510. Adjusted to {newValue}.", "OK");
        }

        // Update Pokemon EV
        switch (statIndex)
        {
            case 0: _pokemon.EV_HP = newValue; break;
            case 1: _pokemon.EV_ATK = newValue; break;
            case 2: _pokemon.EV_DEF = newValue; break;
            case 3: _pokemon.EV_SPA = newValue; break;
            case 4: _pokemon.EV_SPD = newValue; break;
            case 5: _pokemon.EV_SPE = newValue; break;
        }

        // Update label
        _evLabels[statIndex].Text = newValue.ToString();

        // Refresh stats
        _pokemon.RefreshChecksum();
        UpdateCalculatedStats();
        UpdateTotals();
    }

    private async void OnMaxIvsClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        _isUpdating = true;
        for (int i = 0; i < 6; i++)
        {
            var maxIv = _pokemon.GetMaximumIV(i, true);
            _ivSliders[i].Value = maxIv;
            OnIvChanged(i, maxIv);
        }
        _isUpdating = false;

        await DisplayAlert("Success", "All IVs set to maximum values!", "OK");
    }

    private async void OnRandomIvsClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        var random = new Random();
        _isUpdating = true;
        for (int i = 0; i < 6; i++)
        {
            var randomIv = random.Next(0, 32);
            _ivSliders[i].Value = randomIv;
            OnIvChanged(i, randomIv);
        }
        _isUpdating = false;

        await DisplayAlert("Success", "IVs randomized!", "OK");
    }

    private async void OnMaxEvsClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        _isUpdating = true;
        
        // Distribute 510 EVs optimally (252/252/6 spread)
        var evs = new[] { 6, 252, 0, 252, 0, 0 }; // HP, ATK, DEF, SPA, SPD, SPE
        
        for (int i = 0; i < 6; i++)
        {
            _evSliders[i].Value = evs[i];
            OnEvChanged(i, evs[i]);
        }
        _isUpdating = false;

        await DisplayAlert("Success", "EVs set to optimal competitive spread (252/252/6)!", "OK");
    }

    private async void OnClearEvsClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        _isUpdating = true;
        for (int i = 0; i < 6; i++)
        {
            _evSliders[i].Value = 0;
            OnEvChanged(i, 0);
        }
        _isUpdating = false;

        await DisplayAlert("Success", "All EVs cleared!", "OK");
    }

    private async void OnHyperTrainAllClicked(object sender, EventArgs e)
    {
        if (_pokemon is not IHyperTrain ht)
        {
            await DisplayAlert("Not Available", "Hyper Training is not available for this Pokemon format.", "OK");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            if (_pokemon.GetIV(i) < 31)
            {
                ht.SetHyperTraining(i, true);
            }
        }

        UpdateHyperTrainingDisplay();
        UpdateCalculatedStats();

        await DisplayAlert("Success", "Hyper Training applied to all stats below 31!", "OK");
    }

    private async void OnClearHyperTrainClicked(object sender, EventArgs e)
    {
        if (_pokemon is not IHyperTrain ht)
        {
            await DisplayAlert("Not Available", "Hyper Training is not available for this Pokemon format.", "OK");
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            ht.SetHyperTraining(i, false);
        }

        UpdateHyperTrainingDisplay();
        UpdateCalculatedStats();

        await DisplayAlert("Success", "All Hyper Training cleared!", "OK");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
