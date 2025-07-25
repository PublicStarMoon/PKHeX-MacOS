using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class PokedexEditorPage : ContentPage
{
    private SaveFile? _saveFile;

    public PokedexEditorPage()
    {
        InitializeComponent();
    }

    public PokedexEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadPokedexData();
    }

    private void LoadPokedexData()
    {
        if (_saveFile == null) return;

        StatusLabel.Text = $"Pokédex for {_saveFile.Version} save file";
        
        // Get counts
        var seen = 0;
        var caught = 0;
        var maxSpecies = _saveFile.Personal.MaxSpeciesID;

        try
        {
            for (ushort species = 1; species <= maxSpecies; species++)
            {
                if (_saveFile.GetSeen(species)) seen++;
                if (_saveFile.GetCaught(species)) caught++;
            }
        }
        catch { }

        CountLabel.Text = $"Seen: {seen}, Caught: {caught} / {maxSpecies}";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
