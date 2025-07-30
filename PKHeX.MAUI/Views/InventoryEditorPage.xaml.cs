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
        
        HeaderLabel.Text = $"Items - {_saveFile.Version}";
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
        if (_currentPouch == null) return;
        
        for (int i = 0; i < _currentPouch.Items.Length; i++)
        {
            if (_currentPouch.Items[i].Index == item.ID)
            {
                _currentPouch.Items[i] = new InventoryItem { Index = (ushort)item.ID, Count = (ushort)item.Count };
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
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Success", "Items saved to save file!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save items: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
