using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class TrainerIDEditorPage : ContentPage
{
    private SaveFile? _saveFile;
    private bool _isUpdating;

    public TrainerIDEditorPage()
    {
        InitializeComponent();
    }

    public TrainerIDEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadTrainerData();
    }

    private void LoadTrainerData()
    {
        if (_saveFile == null) return;

        _isUpdating = true;

        try
        {
            // Load basic trainer info
            TrainerNameEntry.Text = _saveFile.OT;
            GenderPicker.SelectedIndex = _saveFile.Gender;
            LanguagePicker.SelectedIndex = Math.Max(0, _saveFile.Language - 1);

            // Load ID format based on generation
            var format = _saveFile.GetTrainerIDFormat();
            SetupIDFormat(format);

            // Load ID values
            LoadIDValues();

            // Update display info
            UpdateGameInfo();
            UpdateTSV();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void SetupIDFormat(TrainerIDFormat format)
    {
        switch (format)
        {
            case TrainerIDFormat.SixDigit:
                // Gen 7+ format
                SixDigitFrame.IsVisible = true;
                SixteenBitFrame.IsVisible = false;
                break;
            
            case TrainerIDFormat.SixteenBitSingle:
                // Gen 1/2 format
                SixDigitFrame.IsVisible = false;
                SixteenBitFrame.IsVisible = true;
                SidFrame.IsVisible = false;
                break;
            
            default:
                // Gen 3-6 format
                SixDigitFrame.IsVisible = false;
                SixteenBitFrame.IsVisible = true;
                SidFrame.IsVisible = true;
                break;
        }
    }

    private void LoadIDValues()
    {
        if (_saveFile == null) return;

        var format = _saveFile.GetTrainerIDFormat();

        switch (format)
        {
            case TrainerIDFormat.SixDigit:
                TID7Entry.Text = _saveFile.GetTrainerTID7().ToString();
                SID7Entry.Text = _saveFile.GetTrainerSID7().ToString();
                break;
            
            case TrainerIDFormat.SixteenBitSingle:
                TIDEntry.Text = _saveFile.TID16.ToString();
                break;
            
            default:
                TIDEntry.Text = _saveFile.TID16.ToString();
                SIDEntry.Text = _saveFile.SID16.ToString();
                break;
        }
    }

    private void UpdateGameInfo()
    {
        if (_saveFile == null) return;

        var info = $"Game: {_saveFile.Version} (Generation {_saveFile.Generation})";
        GameInfoLabel.Text = info;
    }

    private void UpdateTSV()
    {
        if (_saveFile == null) return;

        var tsv = GetTSV();
        if (tsv <= ushort.MaxValue)
        {
            TSVLabel.Text = $"TSV: {tsv:D4}";
        }
        else
        {
            TSVLabel.Text = "TSV: Invalid";
        }

        // Show alternate representation
        var format = _saveFile.GetTrainerIDFormat();
        var altRep = GetAlternateRepresentation(format);
        AlternateLabel.Text = altRep;
    }

    private uint GetTSV()
    {
        if (_saveFile == null) return uint.MaxValue;

        var xor = (uint)(_saveFile.SID16 ^ _saveFile.TID16);
        if (_saveFile.Generation <= 5)
            return xor >> 3;
        return xor >> 4;
    }

    private string GetAlternateRepresentation(TrainerIDFormat format)
    {
        if (_saveFile == null) return "";

        if (format != TrainerIDFormat.SixteenBit)
            return $"ID: {_saveFile.TID16:D5}/{_saveFile.SID16:D5}";
        
        var id = _saveFile.ID32;
        return $"G7ID: ({id / 1_000_000:D4}){id % 1_000_000:D6}";
    }

    private void OnTrainerNameChanged(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        _saveFile.OT = e.NewTextValue ?? "";
    }

    private void OnGenderChanged(object sender, EventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        _saveFile.Gender = (byte)GenderPicker.SelectedIndex;
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        _saveFile.Language = LanguagePicker.SelectedIndex + 1;
    }

    private void OnTIDChanged(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        if (ushort.TryParse(e.NewTextValue, out var value))
        {
            _saveFile.TID16 = value;
            UpdateTSV();
        }
    }

    private void OnSIDChanged(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        if (ushort.TryParse(e.NewTextValue, out var value))
        {
            _saveFile.SID16 = value;
            UpdateTSV();
        }
    }

    private void OnTID7Changed(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        if (uint.TryParse(e.NewTextValue, out var tid))
        {
            if (tid > 999_999)
            {
                TID7Entry.Text = "999999";
                return;
            }

            if (uint.TryParse(SID7Entry.Text, out var sid))
            {
                SanityCheckSID7(tid, sid);
            }
        }
    }

    private void OnSID7Changed(object sender, TextChangedEventArgs e)
    {
        if (_saveFile == null || _isUpdating) return;

        if (uint.TryParse(e.NewTextValue, out var sid))
        {
            if (sid > 4294)
            {
                SID7Entry.Text = "4294";
                return;
            }

            if (uint.TryParse(TID7Entry.Text, out var tid))
            {
                SanityCheckSID7(tid, sid);
            }
        }
    }

    private void SanityCheckSID7(uint tid, uint sid)
    {
        if (_isUpdating) return;

        var repack = ((ulong)sid * 1_000_000) + tid;
        if (repack > uint.MaxValue)
        {
            SID7Entry.Text = (sid - 1).ToString();
            return;
        }

        _saveFile!.SetTrainerID7(sid, tid);
        UpdateTSV();
    }

    private async void OnRandomizeClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        var random = new Random();
        
        _isUpdating = true;
        
        var format = _saveFile.GetTrainerIDFormat();
        switch (format)
        {
            case TrainerIDFormat.SixDigit:
                var randomTID7 = (uint)random.Next(0, 1_000_000);
                var randomSID7 = (uint)random.Next(0, 4295);
                TID7Entry.Text = randomTID7.ToString();
                SID7Entry.Text = randomSID7.ToString();
                _saveFile.SetTrainerID7(randomSID7, randomTID7);
                break;
            
            default:
                var randomTID = (ushort)random.Next(0, 65536);
                var randomSID = (ushort)random.Next(0, 65536);
                TIDEntry.Text = randomTID.ToString();
                if (format != TrainerIDFormat.SixteenBitSingle)
                    SIDEntry.Text = randomSID.ToString();
                
                _saveFile.TID16 = randomTID;
                if (format != TrainerIDFormat.SixteenBitSingle)
                    _saveFile.SID16 = randomSID;
                break;
        }
        
        _isUpdating = false;
        UpdateTSV();

        await DisplayAlert("Success", "Trainer ID randomized!", "OK");
    }

    private async void OnMatchTSVClicked(object sender, EventArgs e)
    {
        var result = await DisplayPromptAsync("Match TSV", 
            "Enter desired TSV (0-4095):", 
            "OK", "Cancel", 
            placeholder: "e.g. 1234");

        if (string.IsNullOrEmpty(result) || !ushort.TryParse(result, out var targetTSV) || targetTSV > 4095)
        {
            await DisplayAlert("Invalid Input", "Please enter a valid TSV between 0 and 4095.", "OK");
            return;
        }

        if (_saveFile == null) return;

        // Simple algorithm to match TSV by adjusting SID
        var format = _saveFile.GetTrainerIDFormat();
        var shiftValue = _saveFile.Generation <= 5 ? 3 : 4;

        _isUpdating = true;

        if (format == TrainerIDFormat.SixDigit)
        {
            var currentTID7 = uint.Parse(TID7Entry.Text);
            var tid16 = (ushort)(currentTID7 & 0xFFFF);
            var desiredXor = (uint)(targetTSV << shiftValue);
            var newSID = (ushort)(desiredXor ^ tid16);
            
            // Calculate corresponding SID7
            var newSID7 = (uint)((currentTID7 >> 16) | (newSID << 16));
            SID7Entry.Text = newSID7.ToString();
            _saveFile.SetTrainerID7(newSID7, currentTID7);
        }
        else
        {
            var desiredXor = (uint)(targetTSV << shiftValue);
            var newSID = (ushort)(desiredXor ^ _saveFile.TID16);
            SIDEntry.Text = newSID.ToString();
            _saveFile.SID16 = newSID;
        }

        _isUpdating = false;
        UpdateTSV();

        await DisplayAlert("Success", $"TSV matched to {targetTSV}!", "OK");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
