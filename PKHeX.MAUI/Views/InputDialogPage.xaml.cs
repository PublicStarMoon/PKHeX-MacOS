using Microsoft.Maui.Controls;

namespace PKHeX.MAUI.Views;

public partial class InputDialogPage : ContentPage
{
    public TaskCompletionSource<string?> CompletionSource { get; set; } = new();

    public InputDialogPage()
    {
        InitializeComponent();
        
        // Focus on input field when page appears
        Loaded += (s, e) => InputEntry.Focus();
    }

    public void SetMessage(string title, string message, string placeholder = "", string initialValue = "")
    {
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        
        if (!string.IsNullOrEmpty(placeholder))
        {
            InputEntry.Placeholder = placeholder;
        }
        
        if (!string.IsNullOrEmpty(initialValue))
        {
            InputEntry.Text = initialValue;
        }
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        var result = InputEntry.Text?.Trim();
        CompletionSource.SetResult(result);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        CompletionSource.SetResult(null);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        // Handle hardware back button
        CompletionSource.SetResult(null);
        return base.OnBackButtonPressed();
    }
}
