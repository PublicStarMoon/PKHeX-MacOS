using PKHeX.Core;
using Microsoft.Maui.Storage;
using System.Reflection;

namespace PKHeX.MAUI.Views;

public partial class MainPage : ContentPage
{
    private SaveFile? _currentSave;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnLoadSaveClicked(object sender, EventArgs e)
    {
        await OpenSaveFile();
    }

    private async void OnOpenSaveClicked(object sender, EventArgs e)
    {
        await OpenSaveFile();
    }

    private async void OnExportSaveClicked(object sender, EventArgs e)
    {
        if (_currentSave == null)
        {
            await DisplayAlert("Error", "No save file loaded!", "OK");
            return;
        }

        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select location to save file"
                // No FileTypes specified = allow all files for saving
            });

            if (result != null)
            {
                var data = _currentSave.Write();
                await File.WriteAllBytesAsync(result.FullPath, data);
                StatusLabel.Text = $"Save file exported to: {result.FileName}";
                await DisplayAlert("Success", "Save file exported successfully!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to export save file: {ex.Message}", "OK");
        }
    }

    private async void OnSaveChangesClicked(object sender, EventArgs e)
    {
        if (_currentSave == null)
        {
            await DisplayAlert("Error", "No save file loaded!", "OK");
            return;
        }

        try
        {
            // For now, just show a placeholder
            await DisplayAlert("Info", "Save changes functionality will be implemented in the full editor.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
        }
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("About PKHeX", 
            "PKHeX for macOS\n\n" +
            "A Pokémon save file editor ported to macOS using .NET MAUI.\n\n" +
            "Based on PKHeX by Kaphotics and contributors.\n\n" +
            "This version allows you to run PKHeX natively on macOS without needing Windows or virtual machines.", 
            "OK");
    }

    private async void OnItemEditorClicked(object sender, EventArgs e)
    {
        if (_currentSave == null)
        {
            await DisplayAlert("Error", "No save file loaded!", "OK");
            return;
        }

        try
        {
            var itemPage = new InventoryEditorPage(_currentSave);
            await Navigation.PushAsync(itemPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open item editor: {ex.Message}", "OK");
        }
    }

    private async void OnBoxEditorClicked(object sender, EventArgs e)
    {
        if (_currentSave == null)
        {
            await DisplayAlert("Error", "No save file loaded!", "OK");
            return;
        }

        try
        {
            var boxPage = new PokemonBoxPage(_currentSave);
            await Navigation.PushAsync(boxPage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open box editor: {ex.Message}", "OK");
        }
    }

    private async Task OpenSaveFile()
    {
        try
        {
            // Try with specific file types first, then fallback to any file
            FileResult? result = null;
            
            try
            {
                // Create file type for macOS with proper UTI (Uniform Type Identifiers)
                var customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.macOS, new[] { 
                            "sav", "dat", "bin", "bak", 
                            "pk8", "pk9", "pb8", "pa8", "pb7", 
                            "pk7", "pk6", "pk5", "pk4", "pk3",
                            "public.data", "*"  // Allow all files as fallback
                        }}
                    });

                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a Pokémon save file",
                    FileTypes = customFileType
                });
            }
            catch (Exception ex) when (ex.Message.Contains("file type") || ex.Message.Contains("platform does not support"))
            {
                // Fallback: Allow any file type
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a Pokémon save file (All Files)"
                    // No FileTypes specified = allow all files
                });
            }

            if (result != null)
            {
                StatusLabel.Text = "Loading save file...";
                
                // Read the file data
                byte[] data;
                try
                {
                    data = await File.ReadAllBytesAsync(result.FullPath);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Failed to read file: {ex.Message}", "OK");
                    StatusLabel.Text = "Failed to read save file.";
                    return;
                }
                
                // Validate file size (basic check)
                if (data.Length == 0)
                {
                    await DisplayAlert("Error", "The selected file is empty!", "OK");
                    StatusLabel.Text = "Failed to load save file.";
                    return;
                }
                
                if (data.Length < 100) // Most save files are much larger
                {
                    await DisplayAlert("Error", "The selected file appears too small to be a valid save file.", "OK");
                    StatusLabel.Text = "Failed to load save file.";
                    return;
                }
                
                // Try to parse the save file
                var sav = SaveUtil.GetVariantSAV(data);

                if (sav == null)
                {
                    await DisplayAlert("Error", 
                        $"Invalid or unsupported save file format!\n\n" +
                        $"File: {result.FileName}\n" +
                        $"Size: {data.Length:N0} bytes\n\n" +
                        $"Supported formats: .sav, .dat, .bin (from Pokémon games)\n" +
                        $"Make sure this is a genuine Pokémon save file.", "OK");
                    StatusLabel.Text = "Failed to load save file.";
                    return;
                }

                _currentSave = sav;
                CurrentSaveLabel.Text = $"Current Save File: {result.FileName}";
                
                // Update save file info
                GameVersionLabel.Text = $"Game: {sav.Generation} (Generation {sav.Generation})";
                TrainerNameLabel.Text = $"Trainer: {sav.OT}";
                TrainerIdLabel.Text = $"Trainer ID: {sav.TID16:D6}";
                
                if (sav is ITrainerInfo trainer)
                {
                    // Try to get play time through reflection since the properties may vary
                    try
                    {
                        var type = sav.GetType();
                        var hoursProperty = type.GetProperty("PlayedHours");
                        var minutesProperty = type.GetProperty("PlayedMinutes");
                        
                        if (hoursProperty != null && minutesProperty != null)
                        {
                            var hours = hoursProperty.GetValue(sav);
                            var minutes = minutesProperty.GetValue(sav);
                            PlayTimeLabel.Text = $"Play Time: {hours:D2}:{minutes:D2}";
                        }
                        else
                        {
                            PlayTimeLabel.Text = "Play Time: Not available";
                        }
                    }
                    catch
                    {
                        PlayTimeLabel.Text = "Play Time: Not available";
                    }
                }

                SaveInfoFrame.IsVisible = true;
                SaveChangesButton.IsEnabled = true;
                BoxEditorButton.IsEnabled = true;
                ItemEditorButton.IsEnabled = true;
                
                StatusLabel.Text = $"Successfully loaded {sav.GetType().Name} save file!";
                
                // Show detailed success message
                var gameInfo = $"Game: Generation {sav.Generation}";
                if (!string.IsNullOrEmpty(sav.Version.ToString()))
                {
                    gameInfo += $" ({sav.Version})";
                }
                
                await DisplayAlert("Success", 
                    $"Save file loaded successfully!\n\n" +
                    $"File: {result.FileName}\n" +
                    $"{gameInfo}\n" +
                    $"Trainer: {sav.OT}\n" +
                    $"Save Type: {sav.GetType().Name}", "OK");
            }
        }
        catch (Exception ex)
        {
            // Provide detailed error information
            var errorMsg = $"Failed to load save file: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMsg += $"\n\nDetails: {ex.InnerException.Message}";
            }
            
            await DisplayAlert("Error", errorMsg, "OK");
            StatusLabel.Text = "Failed to load save file.";
        }
    }
}
