using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class EventFlagsEditorPage : ContentPage
{
    private SaveFile? _saveFile;

    public EventFlagsEditorPage()
    {
        InitializeComponent();
    }

    public EventFlagsEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadEventFlagsData();
    }

    private void LoadEventFlagsData()
    {
        if (_saveFile == null) return;

        StatusLabel.Text = $"Event Flags for {_saveFile.Version} save file";
        InfoLabel.Text = "Event flags control game progression and story events";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
