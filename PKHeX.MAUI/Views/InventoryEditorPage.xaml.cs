using PKHeX.Core;
using System;
using System.Linq;

namespace PKHeX.MAUI.Views;

public partial class InventoryEditorPage : ContentPage
{
    private SaveFile? _saveFile;
    private InventoryPouch? _currentPouch;
    private readonly List<Entry> _itemEntries = new();

    public InventoryEditorPage(SaveFile saveFile)
    {
        InitializeComponent();
        _saveFile = saveFile;
        LoadSaveInfo();
        LoadPouches();
    }

    private void LoadSaveInfo()
    {
        if (_saveFile == null) return;

        try
        {
            var generationInfo = GetGenerationInfo(_saveFile);
            HeaderLabel.Text = $"Inventory Editor - {_saveFile.OT}";
            GenerationLabel.Text = generationInfo;
        }
        catch (Exception ex)
        {
            HeaderLabel.Text = "Inventory Editor";
            GenerationLabel.Text = $"Error loading save info: {ex.Message}";
        }
    }

    private string GetGenerationInfo(SaveFile sav)
    {
        var genInfo = $"Generation {sav.Generation}";
        
        return sav switch
        {
            SAV9SV sv => $"{genInfo} - Pokémon Scarlet/Violet (PK9)",
            SAV8SWSH swsh => $"{genInfo} - Pokémon Sword/Shield (PK8)",
            SAV8BS bs => $"{genInfo} - Pokémon Brilliant Diamond/Shining Pearl (PK8)",
            SAV8LA la => $"{genInfo} - Pokémon Legends: Arceus (PK8)",
            SAV7SM sm => $"{genInfo} - Pokémon Sun/Moon (PK7)",
            SAV7USUM usum => $"{genInfo} - Pokémon Ultra Sun/Ultra Moon (PK7)",
            _ => $"{genInfo} - {sav.GetType().Name}"
        };
    }

    private void LoadPouches()
    {
        if (_saveFile?.Inventory == null) return;

        try
        {
            PouchPicker.Items.Clear();
            
            foreach (var pouch in _saveFile.Inventory)
            {
                if (pouch.Items.Length > 0)
                {
                    PouchPicker.Items.Add(pouch.Type.ToString());
                }
            }

            if (PouchPicker.Items.Count > 0)
            {
                PouchPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load pouches: {ex.Message}", "OK");
        }
    }

    private void OnPouchChanged(object sender, EventArgs e)
    {
        if (PouchPicker.SelectedIndex < 0 || _saveFile?.Inventory == null) return;

        try
        {
            var selectedType = PouchPicker.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedType)) return;

            if (Enum.TryParse<InventoryType>(selectedType, out var pouchType))
            {
                _currentPouch = _saveFile.Inventory.FirstOrDefault(p => p.Type == pouchType);
                LoadCurrentPouch();
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load pouch: {ex.Message}", "OK");
        }
    }

    private void LoadCurrentPouch()
    {
        if (_currentPouch == null) return;

        try
        {
            // Update pouch info
            PouchNameLabel.Text = $"Pouch: {_currentPouch.Type}";
            var activeItems = _currentPouch.Items.Count(item => item.Count > 0);
            PouchStatsLabel.Text = $"Items: {activeItems}/{_currentPouch.Items.Length}";
            
            // Show frames
            PouchInfoFrame.IsVisible = true;
            ToolsFrame.IsVisible = true;
            ItemsFrame.IsVisible = true;

            // Load items
            LoadItems();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load current pouch: {ex.Message}", "OK");
        }
    }

    private void LoadItems()
    {
        if (_currentPouch == null) return;

        try
        {
            ItemsContainer.Children.Clear();
            _itemEntries.Clear();

            for (int i = 0; i < _currentPouch.Items.Length; i++)
            {
                var item = _currentPouch.Items[i];
                
                // Create item entry UI
                var frame = new Frame
                {
                    BackgroundColor = Colors.White,
                    Padding = 10,
                    Margin = new Thickness(0, 2)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                var itemLabel = new Label
                {
                    Text = $"Item {item.Index}",
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(itemLabel, 0);

                var countEntry = new Entry
                {
                    Text = item.Count.ToString(),
                    Keyboard = Keyboard.Numeric,
                    Placeholder = "Count"
                };
                Grid.SetColumn(countEntry, 1);

                var clearButton = new Button
                {
                    Text = "Clear",
                    FontSize = 12
                };
                Grid.SetColumn(clearButton, 2);

                // Store reference for later updates
                var entry = new Entry { ItemIndex = i, CountEntry = countEntry };
                _itemEntries.Add(entry);

                // Event handlers
                countEntry.TextChanged += (s, e) => OnItemCountChanged(entry, e.NewTextValue);
                clearButton.Clicked += (s, e) => OnClearItemClicked(entry);

                grid.Children.Add(itemLabel);
                grid.Children.Add(countEntry);
                grid.Children.Add(clearButton);
                frame.Content = grid;
                
                ItemsContainer.Children.Add(frame);
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load items: {ex.Message}", "OK");
        }
    }

    private void OnItemCountChanged(Entry entry, string newValue)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            if (int.TryParse(newValue, out var count))
            {
                count = Math.Max(0, Math.Min(count, _currentPouch.MaxCount));
                _currentPouch.Items[entry.ItemIndex].Count = count;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update item count: {ex.Message}", "OK");
        }
    }

    private void OnClearItemClicked(Entry entry)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            _currentPouch.Items[entry.ItemIndex].Clear();
            entry.CountEntry.Text = "0";
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to clear item: {ex.Message}", "OK");
        }
    }

    private async void OnMaxAllClicked(object sender, EventArgs e)
    {
        if (_currentPouch == null) return;

        try
        {
            _currentPouch.ModifyAllCount(_currentPouch.MaxCount);
            LoadItems(); // Refresh display
            await DisplayAlert("Success", "All items set to maximum count!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to maximize items: {ex.Message}", "OK");
        }
    }

    private async void OnClearAllClicked(object sender, EventArgs e)
    {
        if (_currentPouch == null) return;

        try
        {
            _currentPouch.RemoveAll();
            LoadItems(); // Refresh display
            await DisplayAlert("Success", "All items cleared!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to clear items: {ex.Message}", "OK");
        }
    }

    private async void OnGiveAllLegalClicked(object sender, EventArgs e)
    {
        if (_currentPouch == null || _saveFile == null) return;

        try
        {
            _currentPouch.GiveAllItems(_saveFile, 1);
            LoadItems(); // Refresh display
            await DisplayAlert("Success", "All legal items added!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to give all legal items: {ex.Message}", "OK");
        }
    }

    private async void OnSortByIndexClicked(object sender, EventArgs e)
    {
        if (_currentPouch == null) return;

        try
        {
            _currentPouch.SortByIndex();
            LoadItems(); // Refresh display
            await DisplayAlert("Success", "Items sorted by ID!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to sort items: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            _saveFile.State.Edited = true;
            await DisplayAlert("Success", "Inventory changes saved to save file!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
        }
    }

    private class Entry
    {
        public int ItemIndex { get; set; }
        public Microsoft.Maui.Controls.Entry CountEntry { get; set; } = null!;
    }
}