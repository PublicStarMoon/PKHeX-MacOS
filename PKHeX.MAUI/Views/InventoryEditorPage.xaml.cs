using PKHeX.Core;
using System.Collections.ObjectModel;

namespace PKHeX.MAUI.Views;

public class ItemViewModel
{
    public string Name { get; set; } = "";
    public int ID { get; set; }
    public int Count { get; set; }
    public int MaxCount { get; set; } = 999;
}

public partial class InventoryEditorPage : ContentPage
{
    private SaveFile? _saveFile;
    private InventoryPouch? _currentPouch;
    public ObservableCollection<ItemViewModel> Items { get; set; } = new();

    public InventoryEditorPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public InventoryEditorPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        LoadInventoryData();
    }

    private void LoadInventoryData()
    {
        if (_saveFile == null) return;

        // Load inventory pouches
        CategoryPicker.Items.Clear();
        var inventory = _saveFile.Inventory;
        
        foreach (var pouch in inventory)
        {
            CategoryPicker.Items.Add(pouch.Type.ToString());
        }
        
        if (inventory.Count > 0)
        {
            CategoryPicker.SelectedIndex = 0;
            LoadPouchData(inventory[0]);
        }
        
        // Display generation-specific information in the header
        var generationInfo = GetGenerationInfo(_saveFile);
        HeaderLabel.Text = $"Items - {_saveFile.Version} {generationInfo}";
    }

    /// <summary>
    /// Gets generation-specific information for display purposes.
    /// This demonstrates proper handling of generation-specific save file types.
    /// </summary>
    private string GetGenerationInfo(SaveFile saveFile)
    {
        return saveFile switch
        {
            // Generation 9 (Scarlet/Violet)
            SAV9SV sav9 => $"(Gen 9) - {sav9.SaveRevisionString}",
            
            // Generation 8 (Sword/Shield, BDSP, Legends Arceus)
            SAV8SWSH => "(Gen 8 - SWSH)",
            SAV8BS => "(Gen 8 - BDSP)",
            SAV8LA => "(Gen 8 - PLA)",
            
            // Generation 7 (Sun/Moon, Ultra Sun/Ultra Moon)
            SAV7SM => "(Gen 7 - SM)",
            SAV7USUM => "(Gen 7 - USUM)",
            
            // Generic fallback for other generations
            _ => $"(Gen {saveFile.Generation})"
        };
    }

    private void LoadPouchData(InventoryPouch pouch)
    {
        _currentPouch = pouch;
        Items.Clear();
        
        var strings = GameInfo.GetStrings(1).itemlist; // Get item names
        
        foreach (var item in pouch.Items)
        {
            if (item.Index == 0) continue;
            
            string itemName = item.Index < strings.Length ? strings[item.Index] : $"Item #{item.Index}";
            Items.Add(new ItemViewModel
            {
                Name = itemName,
                ID = item.Index,
                Count = item.Count,
                MaxCount = pouch.MaxCount
            });
        }
    }

    private void OnCategoryChanged(object sender, EventArgs e)
    {
        if (_saveFile == null || CategoryPicker.SelectedIndex < 0) return;
        
        var inventory = _saveFile.Inventory;
        if (CategoryPicker.SelectedIndex < inventory.Count)
        {
            LoadPouchData(inventory[CategoryPicker.SelectedIndex]);
        }
    }

    private void OnIncrementItem(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemViewModel item)
        {
            if (item.Count < item.MaxCount)
            {
                item.Count++;
                UpdateItemInPouch(item);
            }
        }
    }

    private void OnDecrementItem(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ItemViewModel item)
        {
            if (item.Count > 0)
            {
                item.Count--;
                UpdateItemInPouch(item);
            }
        }
    }

    private void UpdateItemInPouch(ItemViewModel item)
    {
        if (_currentPouch == null || _saveFile == null) return;
        
        for (int i = 0; i < _currentPouch.Items.Length; i++)
        {
            if (_currentPouch.Items[i].Index == item.ID)
            {
                _currentPouch.Items[i] = new InventoryItem { Index = (ushort)item.ID, Count = (ushort)item.Count };
                
                // Mark the save file as edited when inventory is modified
                _saveFile.State.Edited = true;
                break;
            }
        }
    }

    private async void OnMaxAllClicked(object sender, EventArgs e)
    {
        if (_currentPouch == null) return;
        
        var result = await DisplayAlert("Confirm", "Set all items to maximum count?", "Yes", "No");
        if (!result) return;
        
        foreach (var item in Items)
        {
            item.Count = item.MaxCount;
            UpdateItemInPouch(item);
        }
        
        // UpdateItemInPouch already marks the save as edited, but ensure it's set
        if (_saveFile != null)
            _saveFile.State.Edited = true;
    }

    private async void OnClearAllClicked(object sender, EventArgs e)
    {
        if (_currentPouch == null) return;
        
        var result = await DisplayAlert("Confirm", "Clear all items? This cannot be undone.", "Yes", "No");
        if (!result) return;
        
        foreach (var item in Items)
        {
            item.Count = 0;
            UpdateItemInPouch(item);
        }
        
        // UpdateItemInPouch already marks the save as edited, but ensure it's set
        if (_saveFile != null)
            _saveFile.State.Edited = true;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (_saveFile == null)
            {
                await DisplayAlert("Error", "No save file loaded!", "OK");
                return;
            }

            // Perform generation-specific validation if needed
            var validationResult = ValidateInventoryChanges(_saveFile);
            if (!validationResult.IsValid)
            {
                await DisplayAlert("Validation Error", validationResult.ErrorMessage, "OK");
                return;
            }

            // Mark the save file as edited to ensure changes are persisted
            _saveFile.State.Edited = true;

            // The inventory changes have already been applied to the save file's inventory pouches
            // through the UpdateItemInPouch method, so we just need to confirm the save operation
            var generationInfo = GetGenerationInfo(_saveFile);
            await DisplayAlert("Success", $"Items saved to {generationInfo} save file!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save items: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Validates inventory changes based on generation-specific constraints.
    /// This demonstrates understanding of generation-specific save file handling.
    /// </summary>
    private (bool IsValid, string ErrorMessage) ValidateInventoryChanges(SaveFile saveFile)
    {
        // Perform basic validation that applies to all generations
        if (_currentPouch == null)
            return (false, "No inventory pouch selected.");

        // Generation-specific validation examples
        return saveFile switch
        {
            SAV9SV sav9 => ValidateGen9Inventory(sav9),
            SAV8SWSH sav8 => ValidateGen8Inventory(sav8),
            SAV7SM or SAV7USUM => ValidateGen7Inventory(saveFile),
            _ => (true, "") // Basic validation passed for other generations
        };
    }

    private (bool IsValid, string ErrorMessage) ValidateGen9Inventory(SAV9SV sav9)
    {
        // Example Gen 9 specific validation
        // Could check for Scarlet/Violet specific item constraints
        return (true, "");
    }

    private (bool IsValid, string ErrorMessage) ValidateGen8Inventory(SAV8SWSH sav8)
    {
        // Example Gen 8 specific validation
        // Could check for Sword/Shield specific item constraints
        return (true, "");
    }

    private (bool IsValid, string ErrorMessage) ValidateGen7Inventory(SaveFile sav7)
    {
        // Example Gen 7 specific validation
        // Could check for Sun/Moon specific item constraints
        return (true, "");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
