using PKHeX.Core;
using Microsoft.Maui.Storage;
using System.Reflection;
using PKHeX.MAUI.Services;

namespace PKHeX.MAUI.Views;

public partial class MainPage : ContentPage
{
    private SaveFile? _currentSave;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Update UI when page appears
        if (_currentSave != null)
        {
            StatusLabel.Text = $"Save file loaded: {_currentSave.GetType().Name}";
        }
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
                
                // Mark changes as saved since they're now on disk
                PageManager.MarkChangesSaved();
                
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
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select location to save file"
                // No FileTypes specified = allow all files for saving
            });

            if (result != null)
            {
                var data = _currentSave.Write();
                await File.WriteAllBytesAsync(result.FullPath, data);
                
                // Mark changes as saved since they're now on disk
                PageManager.MarkChangesSaved();
                
                StatusLabel.Text = $"Save file exported to: {result.FileName}";
                await DisplayAlert("Success", "Save file exported successfully!", "OK");
            }
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
        try
        {
            // If no save file is loaded, create a demo save file
            if (_currentSave == null)
            {
                var result = await DisplayAlert("No Save File", 
                    "No save file is currently loaded. Would you like to:\n\n" +
                    "• Load a save file first, or\n" +
                    "• Continue with demo mode for testing?", 
                    "Demo Mode", "Load Save File");

                if (result)
                {
                    // Create demo save file
                    await CreateDemoSaveFile();
                }
                else
                {
                    // User wants to load a save file first
                    await OpenSaveFile();
                    return;
                }
            }

            if (_currentSave != null)
            {
                var inventoryPage = PageManager.GetInventoryEditorPage(_currentSave);
                await Navigation.PushAsync(inventoryPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open inventory editor: {ex.Message}", "OK");
        }
    }

    private async void OnPartyEditorClicked(object sender, EventArgs e)
    {
        try
        {
            // If no save file is loaded, create a demo save file
            if (_currentSave == null)
            {
                var result = await DisplayAlert("No Save File", 
                    "No save file is currently loaded. Would you like to:\n\n" +
                    "• Load a save file first, or\n" +
                    "• Continue with demo mode for testing?", 
                    "Demo Mode", "Load Save File");

                if (result)
                {
                    // Create demo save file
                    await CreateDemoSaveFile();
                }
                else
                {
                    // User wants to load a save file first
                    await OpenSaveFile();
                    return;
                }
            }

            if (_currentSave != null)
            {
                var partyPage = PageManager.GetPartyEditorPage(_currentSave);
                await Navigation.PushAsync(partyPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open party editor: {ex.Message}", "OK");
        }
    }

    private async void OnBoxEditorClicked(object sender, EventArgs e)
    {
        try
        {
            // If no save file is loaded, create a demo save file
            if (_currentSave == null)
            {
                var result = await DisplayAlert("No Save File", 
                    "No save file is currently loaded. Would you like to:\n\n" +
                    "• Load a save file first, or\n" +
                    "• Continue with demo mode for testing?", 
                    "Demo Mode", "Load Save File");

                if (result)
                {
                    // Create demo save file
                    await CreateDemoSaveFile();
                }
                else
                {
                    // User wants to load a save file first
                    await OpenSaveFile();
                    return;
                }
            }

            if (_currentSave != null)
            {
                var boxPage = PageManager.GetPokemonBoxPage(_currentSave);
                await Navigation.PushAsync(boxPage);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open box editor: {ex.Message}", "OK");
        }
    }

    private async void OnDemoModeClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("Demo Mode", 
            "Enable demo mode? This will allow you to access the editors without a save file for testing purposes.\n\n" +
            "⚠️ Warning: Changes won't be saved and some features may not work properly.", 
            "Enable", "Cancel");

        if (result)
        {
            await CreateDemoSaveFile();
            await DisplayAlert("Demo Mode", "Demo mode enabled successfully! You can now access the Pokemon Box Editor and Inventory Editor for testing.", "OK");
        }
    }

    private async Task CreateDemoSaveFile()
    {
        try
        {
            // Create a basic SAV8SWSH save file for demo purposes
            var demoSave = new SAV8SWSH();
            
            // Clear page cache when loading new save file
            PageManager.ClearCache();
            _currentSave = demoSave;
            
            CurrentSaveLabel.Text = "Current Save File: Demo Mode (SAV8SWSH)";
            GameVersionLabel.Text = "Game: Pokémon Sword/Shield (Generation 8)";
            TrainerNameLabel.Text = "Trainer: Demo";
            TrainerIdLabel.Text = "Trainer ID: 000000";
            PlayTimeLabel.Text = "Play Time: 00:00";
            
            // Force UI update on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BoxEditorButton.IsEnabled = true;
                ItemEditorButton.IsEnabled = true;
                SaveButton.IsEnabled = true;
                if (DemoModeButton != null)
                {
                    DemoModeButton.Text = "✅ Demo Mode Enabled";
                    DemoModeButton.IsEnabled = false;
                    DemoModeButton.BackgroundColor = Colors.Green;
                }
            });
            
            StatusLabel.Text = "Demo mode enabled! Using temporary save file for testing.";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create demo save file: {ex.Message}", "OK");
        }
    }

    private async Task OpenSaveFile()
    {
        // Check for unsaved changes
        if (PageManager.HasUnsavedChanges)
        {
            var shouldContinue = await DisplayAlert("Unsaved Changes", 
                "You have unsaved changes that will be lost if you load a new save file.\n\n" +
                "Do you want to:\n" +
                "• Export your current changes first, or\n" +
                "• Continue and lose your changes?", 
                "Continue (Lose Changes)", "Export First");
                
            if (!shouldContinue)
            {
                // User wants to export first
                return;
            }
        }
        
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

                // Clear page cache when loading new save file
                PageManager.ClearCache();
                _currentSave = sav;
                CurrentSaveLabel.Text = $"Current Save File: {result.FileName}";
                
                // Update save file info
                GameVersionLabel.Text = $"Game: {sav.Generation} (Generation {sav.Generation})";
                TrainerNameLabel.Text = $"Trainer: {sav.OT}";
                TrainerIdLabel.Text = $"Trainer ID: {sav.TID16:D6}";
                
                // Try to get and display play time
                if (sav is ITrainerInfo trainer)
                {
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
                else
                {
                    PlayTimeLabel.Text = "Play Time: Not available";
                }
                
                // Force UI update on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    BoxEditorButton.IsEnabled = true;
                    ItemEditorButton.IsEnabled = true;
                    SaveButton.IsEnabled = true;
                });
                
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
