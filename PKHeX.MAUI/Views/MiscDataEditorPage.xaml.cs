using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class MiscDataEditorPage : ContentPage
{
    private SaveFile? _saveFile;

    public MiscDataEditorPage()
    {
        InitializeComponent();
    }

    public MiscDataEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadMiscData();
    }

    private void LoadMiscData()
    {
        if (_saveFile == null) return;

        StatusLabel.Text = $"Misc Data for {_saveFile.Version} save file";
        InfoLabel.Text = "Options, settings, and miscellaneous game data";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
