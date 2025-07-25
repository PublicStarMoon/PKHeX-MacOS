using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class MemoryAmieEditorPage : ContentPage
{
    private PKM? _pokemon;
    private bool _isUpdating;

    public MemoryAmieEditorPage()
    {
        InitializeComponent();
    }

    public MemoryAmieEditorPage(PKM pokemon) : this()
    {
        _pokemon = pokemon;
        LoadMemoryData();
    }

    private void LoadMemoryData()
    {
        if (_pokemon == null) return;

        _isUpdating = true;

        try
        {
            // Load basic affection/happiness data
            if (_pokemon is IHappiness happiness)
            {
                HappinessSlider.Value = happiness.CurrentFriendship;
                HappinessLabel.Text = happiness.CurrentFriendship.ToString();
            }

            if (_pokemon is IAffection affection)
            {
                AffectionSlider.Value = affection.Affection;
                AffectionLabel.Text = affection.Affection.ToString();
            }

            // Load memory data for supported generations
            LoadMemories();
            LoadAmieData();
            LoadRefreshData();

            UpdateMemoryDescription();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void LoadMemories()
    {
        if (_pokemon == null) return;

        // Original Trainer Memory (OT)
        if (_pokemon is IMemoryOT memoryOT)
        {
            OTMemorySlider.Value = memoryOT.OT_Memory;
            OTIntensitySlider.Value = memoryOT.OT_Intensity;
            OTFeelingSlider.Value = memoryOT.OT_Feeling;
            OTMemoryLabel.Text = GetMemoryDescription(memoryOT.OT_Memory, true);
        }

        // Handler Memory (HT)
        if (_pokemon is IMemoryHT memoryHT)
        {
            HTMemorySlider.Value = memoryHT.HT_Memory;
            HTIntensitySlider.Value = memoryHT.HT_Intensity;
            HTFeelingSlider.Value = memoryHT.HT_Feeling;
            HTMemoryLabel.Text = GetMemoryDescription(memoryHT.HT_Memory, false);
        }
    }

    private void LoadAmieData()
    {
        if (_pokemon == null) return;

        // Pokémon-Amie specific data (Gen 6)
        if (_pokemon is IAmie amie)
        {
            FullnessSlider.Value = amie.Fullness;
            EnjoymentSlider.Value = amie.Enjoyment;
            FullnessLabel.Text = amie.Fullness.ToString();
            EnjoymentLabel.Text = amie.Enjoyment.ToString();
        }
    }

    private void LoadRefreshData()
    {
        if (_pokemon == null) return;

        // Pokémon Refresh specific data (Gen 7)
        if (_pokemon is IFormArgument form && _pokemon.Species == (int)Species.Furfrou)
        {
            // Furfrou trim data
            FormArgumentSlider.Value = Math.Min(5, form.FormArgument);
            FormArgumentLabel.Text = $"Trim Days: {form.FormArgument}";
        }
    }

    private string GetMemoryDescription(byte memoryId, bool isOT)
    {
        if (memoryId == 0) return "No memory";

        try
        {
            var memories = GameInfo.Strings.memories;
            if (memoryId < memories.Count)
            {
                var prefix = isOT ? "OT: " : "HT: ";
                return prefix + memories[memoryId];
            }
        }
        catch { }

        return $"Memory {memoryId}";
    }

    private void UpdateMemoryDescription()
    {
        if (_pokemon == null) return;

        var description = "";

        if (_pokemon is IMemoryOT memoryOT && memoryOT.OT_Memory > 0)
        {
            description += $"OT Memory: {GetMemoryDescription(memoryOT.OT_Memory, true)}\n";
            description += $"Intensity: {memoryOT.OT_Intensity}, Feeling: {GetFeelingDescription(memoryOT.OT_Feeling)}\n\n";
        }

        if (_pokemon is IMemoryHT memoryHT && memoryHT.HT_Memory > 0)
        {
            description += $"Handler Memory: {GetMemoryDescription(memoryHT.HT_Memory, false)}\n";
            description += $"Intensity: {memoryHT.HT_Intensity}, Feeling: {GetFeelingDescription(memoryHT.HT_Feeling)}\n\n";
        }

        if (string.IsNullOrEmpty(description))
            description = "No memories recorded for this Pokémon.";

        MemoryDescriptionLabel.Text = description.Trim();
    }

    private string GetFeelingDescription(byte feeling)
    {
        return feeling switch
        {
            0 => "Neutral",
            1 => "Happy",
            2 => "Sad",
            3 => "Angry",
            4 => "Worried",
            5 => "Relieved",
            _ => $"Unknown ({feeling})"
        };
    }

    // Event Handlers
    private void OnHappinessChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IHappiness happiness)
        {
            happiness.CurrentFriendship = value;
            HappinessLabel.Text = value.ToString();
        }
    }

    private void OnAffectionChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IAffection affection)
        {
            affection.Affection = value;
            AffectionLabel.Text = value.ToString();
        }
    }

    private void OnOTMemoryChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IMemoryOT memoryOT)
        {
            memoryOT.OT_Memory = value;
            OTMemoryLabel.Text = GetMemoryDescription(value, true);
            UpdateMemoryDescription();
        }
    }

    private void OnOTIntensityChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IMemoryOT memoryOT)
        {
            memoryOT.OT_Intensity = value;
            UpdateMemoryDescription();
        }
    }

    private void OnOTFeelingChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IMemoryOT memoryOT)
        {
            memoryOT.OT_Feeling = value;
            UpdateMemoryDescription();
        }
    }

    private void OnHTMemoryChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IMemoryHT memoryHT)
        {
            memoryHT.HT_Memory = value;
            HTMemoryLabel.Text = GetMemoryDescription(value, false);
            UpdateMemoryDescription();
        }
    }

    private void OnHTIntensityChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IMemoryHT memoryHT)
        {
            memoryHT.HT_Intensity = value;
            UpdateMemoryDescription();
        }
    }

    private void OnHTFeelingChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IMemoryHT memoryHT)
        {
            memoryHT.HT_Feeling = value;
            UpdateMemoryDescription();
        }
    }

    private void OnFullnessChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IAmie amie)
        {
            amie.Fullness = value;
            FullnessLabel.Text = value.ToString();
        }
    }

    private void OnEnjoymentChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (byte)e.NewValue;
        if (_pokemon is IAmie amie)
        {
            amie.Enjoyment = value;
            EnjoymentLabel.Text = value.ToString();
        }
    }

    private void OnFormArgumentChanged(object sender, ValueChangedEventArgs e)
    {
        if (_pokemon == null || _isUpdating) return;

        var value = (uint)e.NewValue;
        if (_pokemon is IFormArgument form)
        {
            form.FormArgument = value;
            FormArgumentLabel.Text = _pokemon.Species == (int)Species.Furfrou 
                ? $"Trim Days: {value}" 
                : $"Form Arg: {value}";
        }
    }

    private async void OnMaxBondClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        try
        {
            // Max out friendship/affection
            if (_pokemon is IHappiness happiness)
                happiness.CurrentFriendship = 255;
            
            if (_pokemon is IAffection affection)
                affection.Affection = 255;

            // Max out Amie stats
            if (_pokemon is IAmie amie)
            {
                amie.Fullness = 255;
                amie.Enjoyment = 255;
            }

            LoadMemoryData();
            await DisplayAlert("Success", "Bond values maximized!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to maximize bond: {ex.Message}", "OK");
        }
    }

    private async void OnClearMemoriesClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        var result = await DisplayAlert("Confirm", 
            "This will clear all memories. Are you sure?", 
            "Yes", "No");

        if (!result) return;

        try
        {
            // Clear OT memories
            if (_pokemon is IMemoryOT memoryOT)
            {
                memoryOT.OT_Memory = 0;
                memoryOT.OT_Intensity = 0;
                memoryOT.OT_Feeling = 0;
            }

            // Clear HT memories
            if (_pokemon is IMemoryHT memoryHT)
            {
                memoryHT.HT_Memory = 0;
                memoryHT.HT_Intensity = 0;
                memoryHT.HT_Feeling = 0;
            }

            LoadMemoryData();
            await DisplayAlert("Success", "Memories cleared!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to clear memories: {ex.Message}", "OK");
        }
    }

    private async void OnRandomizeMemoriesClicked(object sender, EventArgs e)
    {
        if (_pokemon == null) return;

        try
        {
            var random = new Random();

            // Randomize OT memories
            if (_pokemon is IMemoryOT memoryOT)
            {
                memoryOT.OT_Memory = (byte)random.Next(1, 70); // Valid memory range
                memoryOT.OT_Intensity = (byte)random.Next(1, 8);
                memoryOT.OT_Feeling = (byte)random.Next(0, 6);
            }

            // Randomize HT memories if handler exists
            if (_pokemon is IMemoryHT memoryHT && !string.IsNullOrEmpty(_pokemon.HT_Name))
            {
                memoryHT.HT_Memory = (byte)random.Next(1, 70);
                memoryHT.HT_Intensity = (byte)random.Next(1, 8);
                memoryHT.HT_Feeling = (byte)random.Next(0, 6);
            }

            LoadMemoryData();
            await DisplayAlert("Success", "Memories randomized!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to randomize memories: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
