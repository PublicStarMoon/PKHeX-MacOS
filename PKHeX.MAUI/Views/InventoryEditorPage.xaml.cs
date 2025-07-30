using PKHeX.Core;

namespace PKHeX.MAUI.Views;

public partial class InventoryEditorPage : ContentPage
{
    private SaveFile? _saveFile;

    public InventoryEditorPage()
    {
        InitializeComponent();
    }

    public InventoryEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadInventoryData();
    }

    private void LoadInventoryData()
    {
        if (_saveFile == null) return;

        try
        {
            // Display save file generation and version information
            StatusLabel.Text = $"Generation {_saveFile.Generation} - {_saveFile.Version}";
            
            // Count inventory pouches
            var inventory = _saveFile.Inventory;
            PouchCountLabel.Text = $"Pouches: {inventory.Count}";
            
            // Display generation-specific save file type
            var saveType = _saveFile switch
            {
                SAV9SV sav9 => $"Gen 9 Scarlet/Violet (Rev: {sav9.SaveRevision})",
                SAV8SWSH sav8swsh => "Gen 8 Sword/Shield",
                SAV8BS sav8bs => "Gen 8 Brilliant Diamond/Shining Pearl", 
                SAV8LA sav8la => "Gen 8 Legends Arceus",
                SAV7SM sav7sm => "Gen 7 Sun/Moon",
                SAV7USUM sav7usum => "Gen 7 Ultra Sun/Ultra Moon",
                _ => $"Unknown save type: {_saveFile.GetType().Name}"
            };
            
            HeaderLabel.Text = $"Inventory - {saveType}";
            OperationStatusLabel.Text = "Ready - Use Core API patterns for item management";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error loading inventory: {ex.Message}";
            OperationStatusLabel.Text = "Error loading data";
        }
    }

    private async void OnSetItemClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            if (!int.TryParse(ItemIDEntry.Text, out int itemId) || itemId < 0)
            {
                await DisplayAlert("Error", "Please enter a valid item ID", "OK");
                return;
            }

            if (!int.TryParse(ItemCountEntry.Text, out int count) || count < 0)
            {
                await DisplayAlert("Error", "Please enter a valid count", "OK");
                return;
            }

            // Get the first inventory pouch for demonstration
            var inventory = _saveFile.Inventory;
            if (inventory.Count > 0)
            {
                var pouch = inventory[0];
                
                // Find or create item
                var existingItem = Array.Find(pouch.Items, item => item.Index == itemId);
                if (existingItem != null)
                {
                    existingItem.Count = Math.Min(count, pouch.MaxCount);
                    OperationStatusLabel.Text = $"Updated item {itemId} to count {existingItem.Count}";
                }
                else
                {
                    // Find empty slot
                    var emptySlot = Array.Find(pouch.Items, item => item.Count == 0);
                    if (emptySlot != null)
                    {
                        emptySlot.Index = itemId;
                        emptySlot.Count = Math.Min(count, pouch.MaxCount);
                        OperationStatusLabel.Text = $"Added item {itemId} with count {emptySlot.Count}";
                    }
                    else
                    {
                        OperationStatusLabel.Text = "No empty slots available";
                    }
                }
            }
            else
            {
                OperationStatusLabel.Text = "No inventory pouches available";
            }
        }
        catch (Exception ex)
        {
            OperationStatusLabel.Text = $"Error setting item: {ex.Message}";
            await DisplayAlert("Error", $"Failed to set item: {ex.Message}", "OK");
        }
    }

    private async void OnMaxAllClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var inventory = _saveFile.Inventory;
            int totalItems = 0;
            
            foreach (var pouch in inventory)
            {
                foreach (var item in pouch.Items)
                {
                    if (item.Index > 0) // Valid item
                    {
                        item.Count = pouch.MaxCount;
                        totalItems++;
                    }
                }
            }
            
            OperationStatusLabel.Text = $"Maximized {totalItems} items across all pouches";
            await DisplayAlert("Success", $"Set all {totalItems} items to maximum count", "OK");
        }
        catch (Exception ex)
        {
            OperationStatusLabel.Text = $"Error maximizing items: {ex.Message}";
            await DisplayAlert("Error", $"Failed to maximize items: {ex.Message}", "OK");
        }
    }

    private async void OnClearAllClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var inventory = _saveFile.Inventory;
            int clearedItems = 0;
            
            foreach (var pouch in inventory)
            {
                foreach (var item in pouch.Items)
                {
                    if (item.Index > 0 && item.Count > 0)
                    {
                        item.Count = 0;
                        clearedItems++;
                    }
                }
            }
            
            OperationStatusLabel.Text = $"Cleared {clearedItems} items from all pouches";
            await DisplayAlert("Success", $"Cleared {clearedItems} items", "OK");
        }
        catch (Exception ex)
        {
            OperationStatusLabel.Text = $"Error clearing items: {ex.Message}";
            await DisplayAlert("Error", $"Failed to clear items: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (_saveFile != null)
            {
                _saveFile.State.Edited = true;
                OperationStatusLabel.Text = "Save file marked as edited";
            }
            
            await DisplayAlert("Success", "Inventory changes saved!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            OperationStatusLabel.Text = $"Error saving: {ex.Message}";
            await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
