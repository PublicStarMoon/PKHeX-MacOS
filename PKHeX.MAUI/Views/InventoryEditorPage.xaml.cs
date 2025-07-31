using PKHeX.Core;
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using PKHeX.MAUI.Services;
using PKHeX.MAUI.Models;

namespace PKHeX.MAUI.Views;

public partial class InventoryEditorPage : ContentPage
{
    private SaveFile? _saveFile;
    private InventoryPouch? _currentPouch;
    private readonly List<ItemEntry> _itemEntries = new();

    public InventoryEditorPage(SaveFile saveFile)
    {
        InitializeComponent();
        _saveFile = saveFile;
        LoadSaveInfo();
        LoadPouches();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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
        try
        {
            PouchContainer.Children.Clear();
            
            if (_saveFile?.Inventory == null)
            {
                CreatePouchButton("No Inventory Available", false);
                return;
            }

            var validPouches = 0;
            foreach (var pouch in _saveFile.Inventory)
            {
                if (pouch?.Items != null && pouch.Items.Length > 0)
                {
                    CreatePouchButton(pouch.Type.ToString(), true);
                    validPouches++;
                }
            }

            if (validPouches == 0)
            {
                // Add default pouches if none found (Demo Mode)
                CreatePouchButton("Items", true);
                CreatePouchButton("Medicine", true);
                CreatePouchButton("Pokeballs", true);
                CreatePouchButton("TMs & HMs", true);
                CreatePouchButton("Berries", true);
                CreatePouchButton("Key Items", true);
            }
        }
        catch (Exception ex)
        {
            PouchContainer.Children.Clear();
            CreatePouchButton($"Error: {ex.Message}", false);
            DisplayAlert("Error", $"Failed to load pouches: {ex.Message}", "OK");
        }
    }

    private void CreatePouchButton(string pouchName, bool isEnabled)
    {
        var button = new Button
        {
            Text = pouchName,
            BackgroundColor = isEnabled ? Color.FromArgb("#3498DB") : Color.FromArgb("#BDC3C7"),
            TextColor = Colors.White,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 10,
            Margin = new Thickness(2, 0),
            HeightRequest = 46,
            MinimumWidthRequest = 100,
            Padding = new Thickness(12, 8),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            IsEnabled = isEnabled
        };

        if (isEnabled)
        {
            button.Clicked += (s, e) => OnPouchSelected(pouchName);
        }

        PouchContainer.Children.Add(button);
    }

    private void OnPouchSelected(string selectedPouch)
    {
        try
        {
            // Handle error states
            if (selectedPouch == "No Inventory Available" || selectedPouch.StartsWith("Error:"))
            {
                DisplayAlert("Info", "Inventory editing is not available for this save file format.", "OK");
                return;
            }

            // Update button colors to show selection
            foreach (var child in PouchContainer.Children)
            {
                if (child is Button btn)
                {
                    if (btn.Text == selectedPouch)
                    {
                        btn.BackgroundColor = Color.FromArgb("#E74C3C"); // Selected color
                    }
                    else if (btn.IsEnabled)
                    {
                        btn.BackgroundColor = Color.FromArgb("#3498DB"); // Normal color
                    }
                }
            }

            // Try to find the actual pouch
            if (_saveFile?.Inventory != null && Enum.TryParse<InventoryType>(selectedPouch, out var pouchType))
            {
                _currentPouch = _saveFile.Inventory.FirstOrDefault(p => p.Type == pouchType);
                if (_currentPouch != null)
                {
                    LoadCurrentPouch();
                    return;
                }
            }

            // Demo mode - create a simulated pouch
            LoadDemoPouch(selectedPouch);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load pouch: {ex.Message}", "OK");
        }
    }

    private void LoadDemoPouch(string pouchName)
    {
        try
        {
            // Show frames with demo content
            PouchNameLabel.Text = $"Pouch: {pouchName} (Demo Mode)";
            PouchStatsLabel.Text = "Items: 5/20 (Demo Data)";
            
            PouchInfoFrame.IsVisible = true;
            ToolsFrame.IsVisible = true;
            ItemsFrame.IsVisible = true;

            // Create demo items display
            LoadDemoItems(pouchName);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load demo pouch: {ex.Message}", "OK");
        }
    }

    private void LoadDemoItems(string pouchName)
    {
        try
        {
            ItemsContainer.Children.Clear();
            _itemEntries.Clear();

            // Create demo items based on pouch type
            var demoItems = GetDemoItemsForPouch(pouchName);

            // Show only first 3 items as preview in demo mode
            var itemsToShow = Math.Min(3, demoItems.Length);

            for (int i = 0; i < itemsToShow; i++)
            {
                var (itemName, count) = demoItems[i];
                
                // Create item entry UI
                var frame = new Microsoft.Maui.Controls.Frame
                {
                    BackgroundColor = Colors.White,
                    Padding = 15,
                    Margin = new Thickness(0, 4),
                    CornerRadius = 8,
                    HasShadow = true
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                var itemLabel = new Label
                {
                    Text = itemName,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 14,
                    TextColor = Color.FromArgb("#2C3E50"),
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetColumn(itemLabel, 0);

                var countEntry = new Microsoft.Maui.Controls.Entry
                {
                    Text = count.ToString(),
                    Keyboard = Keyboard.Numeric,
                    Placeholder = "Count",
                    IsReadOnly = true, // Demo mode - read only
                    BackgroundColor = Color.FromArgb("#ECF0F1"),
                    TextColor = Color.FromArgb("#7F8C8D"),
                    FontSize = 14
                };
                Grid.SetColumn(countEntry, 1);

                var clearButton = new Button
                {
                    Text = "Demo",
                    FontSize = 12,
                    IsEnabled = false, // Demo mode - disabled
                    BackgroundColor = Color.FromArgb("#BDC3C7"),
                    TextColor = Color.FromArgb("#7F8C8D"),
                    CornerRadius = 4
                };
                Grid.SetColumn(clearButton, 2);

                grid.Children.Add(itemLabel);
                grid.Children.Add(countEntry);
                grid.Children.Add(clearButton);
                frame.Content = grid;
                
                ItemsContainer.Children.Add(frame);
            }

            // Show preview message for demo mode
            if (demoItems.Length > itemsToShow)
            {
                ItemsPreviewLabel.Text = $"Showing first {itemsToShow} demo items. Total demo items: {demoItems.Length}. Click 'Full View' to see all items.";
                ItemsPreviewLabel.IsVisible = true;
            }
            else
            {
                ItemsPreviewLabel.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load demo items: {ex.Message}", "OK");
        }
    }

    private (string, int)[] GetDemoItemsForPouch(string pouchName)
    {
        return pouchName switch
        {
            "Items" => new[]
            {
                ("Potion", 10),
                ("Super Potion", 5),
                ("Hyper Potion", 3),
                ("Max Potion", 1),
                ("Revive", 2)
            },
            "Medicine" => new[]
            {
                ("Antidote", 5),
                ("Burn Heal", 3),
                ("Ice Heal", 3),
                ("Awakening", 4),
                ("Paralyze Heal", 3)
            },
            "Pokeballs" => new[]
            {
                ("Poke Ball", 20),
                ("Great Ball", 10),
                ("Ultra Ball", 5),
                ("Master Ball", 1),
                ("Premier Ball", 3)
            },
            "TMs & HMs" => new[]
            {
                ("TM01", 1),
                ("TM02", 1),
                ("TM03", 1),
                ("HM01", 1),
                ("HM02", 1)
            },
            "Berries" => new[]
            {
                ("Oran Berry", 10),
                ("Pecha Berry", 8),
                ("Chesto Berry", 6),
                ("Rawst Berry", 5),
                ("Aspear Berry", 4)
            },
            "Key Items" => new[]
            {
                ("Bike", 1),
                ("Pokedex", 1),
                ("Town Map", 1),
                ("Vs. Seeker", 1),
                ("Old Rod", 1)
            },
            _ => new[]
            {
                ("Unknown Item 1", 1),
                ("Unknown Item 2", 1),
                ("Unknown Item 3", 1),
                ("Unknown Item 4", 1),
                ("Unknown Item 5", 1)
            }
        };
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

            // Add header with "Add New Item" button
            var headerFrame = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = Color.FromArgb("#3498DB"),
                Padding = 15,
                Margin = new Thickness(0, 4),
                CornerRadius = 8,
                HasShadow = true
            };

            var addButton = new Button
            {
                Text = "Add New Item",
                FontSize = 16,
                BackgroundColor = Color.FromArgb("#27AE60"),
                TextColor = Colors.White,
                CornerRadius = 6,
                FontAttributes = FontAttributes.Bold
            };
            addButton.Clicked += OnAddNewItemClicked;
            headerFrame.Content = addButton;
            ItemsContainer.Children.Add(headerFrame);

            // Add column header
            var columnHeaderFrame = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = Color.FromArgb("#34495E"),
                Padding = 10,
                Margin = new Thickness(0, 2),
                CornerRadius = 6,
                HasShadow = false
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            var idLabel = new Label { Text = "Item ID", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
            var countLabel = new Label { Text = "Count", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
            var newLabel = new Label { Text = "New", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };
            var actionLabel = new Label { Text = "Action", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center };

            Grid.SetColumn(idLabel, 0);
            Grid.SetColumn(countLabel, 1);
            Grid.SetColumn(newLabel, 2);
            Grid.SetColumn(actionLabel, 3);

            headerGrid.Children.Add(idLabel);
            headerGrid.Children.Add(countLabel);
            headerGrid.Children.Add(newLabel);
            headerGrid.Children.Add(actionLabel);
            
            columnHeaderFrame.Content = headerGrid;
            ItemsContainer.Children.Add(columnHeaderFrame);

            // Show only first 5 items as preview, count total items
            var totalItems = _currentPouch.Items.Count(item => item.Index != 0 || item.Count != 0);
            var itemsToShow = Math.Min(5, _currentPouch.Items.Length);
            var itemsShown = 0;

            for (int i = 0; i < _currentPouch.Items.Length && itemsShown < itemsToShow; i++)
            {
                var item = _currentPouch.Items[i];
                
                // Skip empty items in preview unless we haven't shown any items yet
                if (item.Index == 0 && item.Count == 0 && itemsShown > 0)
                    continue;

                // Create item entry UI
                var frame = new Microsoft.Maui.Controls.Frame
                {
                    BackgroundColor = Colors.White,
                    Padding = 15,
                    Margin = new Thickness(0, 4),
                    CornerRadius = 8,
                    HasShadow = true
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

                // Item ID entry - show actual ID or placeholder for empty slots
                var itemIdEntry = new Microsoft.Maui.Controls.Entry
                {
                    Text = item.Index == 0 ? "" : item.Index.ToString(),
                    Keyboard = Keyboard.Numeric,
                    Placeholder = item.Index == 0 ? "Item ID (e.g. 15)" : "Item ID",
                    BackgroundColor = Color.FromArgb("#ECF0F1"),
                    TextColor = Color.FromArgb("#2C3E50"),
                    FontSize = 14
                };
                Grid.SetColumn(itemIdEntry, 0);

                var countEntry = new Microsoft.Maui.Controls.Entry
                {
                    Text = item.Count == 0 ? "" : item.Count.ToString(),
                    Keyboard = Keyboard.Numeric,
                    Placeholder = "Count",
                    BackgroundColor = Color.FromArgb("#ECF0F1"),
                    TextColor = Color.FromArgb("#2C3E50"),
                    FontSize = 14
                };
                Grid.SetColumn(countEntry, 1);

                // New checkbox - check if the item supports IsNew property
                CheckBox? newCheckBox = null;
                if (item is IItemNewFlag newFlagItem)
                {
                    newCheckBox = new CheckBox
                    {
                        IsChecked = newFlagItem.IsNew,
                        Color = Color.FromArgb("#27AE60"),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetColumn(newCheckBox, 2);
                }
                else
                {
                    // For items that don't support IsNew, add a label to show it's not available
                    var naLabel = new Label
                    {
                        Text = "N/A",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#BDC3C7"),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetColumn(naLabel, 2);
                }

                var clearButton = new Button
                {
                    Text = "Clear",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#E74C3C"),
                    TextColor = Colors.White,
                    CornerRadius = 4
                };
                Grid.SetColumn(clearButton, 3);

                // Store reference for later updates
                var entry = new ItemEntry { ItemIndex = i, CountEntry = countEntry, ItemIdEntry = itemIdEntry, NewCheckBox = newCheckBox };
                _itemEntries.Add(entry);

                // Event handlers
                itemIdEntry.TextChanged += (s, e) => OnItemIdChanged(entry, e.NewTextValue);
                countEntry.TextChanged += (s, e) => OnItemCountChanged(entry, e.NewTextValue);
                clearButton.Clicked += (s, e) => OnClearItemClicked(entry);
                if (newCheckBox != null)
                {
                    newCheckBox.CheckedChanged += (s, e) => OnNewFlagChanged(entry, e.Value);
                }

                grid.Children.Add(itemIdEntry);
                grid.Children.Add(countEntry);
                if (newCheckBox != null)
                {
                    grid.Children.Add(newCheckBox);
                }
                else
                {
                    grid.Children.Add(new Label
                    {
                        Text = "N/A",
                        FontSize = 12,
                        TextColor = Color.FromArgb("#BDC3C7"),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    });
                }
                grid.Children.Add(clearButton);
                frame.Content = grid;
                
                ItemsContainer.Children.Add(frame);
                itemsShown++;
            }

            // Show preview message if there are more items
            if (totalItems > itemsToShow || _currentPouch.Items.Length > itemsToShow)
            {
                ItemsPreviewLabel.Text = $"Showing first {itemsShown} items. Total items in pouch: {totalItems}. Click 'Full View' to see all items.";
                ItemsPreviewLabel.IsVisible = true;
            }
            else
            {
                ItemsPreviewLabel.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load items: {ex.Message}", "OK");
        }
    }

    private void OnItemCountChanged(ItemEntry entry, string newValue)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            if (string.IsNullOrWhiteSpace(newValue))
            {
                _currentPouch.Items[entry.ItemIndex].Count = 0;
            }
            else if (int.TryParse(newValue, out var count))
            {
                count = Math.Max(0, Math.Min(count, _currentPouch.MaxCount));
                _currentPouch.Items[entry.ItemIndex].Count = count;

                // CRITICAL: Enable SetNew flag on pouches for Gen8/Gen9 compatibility when modifying items
                if (_currentPouch is InventoryPouch8 pouch8)
                {
                    pouch8.SetNew = true;
                }
                else if (_currentPouch is InventoryPouch9 pouch9)
                {
                    pouch9.SetNew = true;
                }

                // For Gen9, ensure proper configuration for in-game visibility
                if (_currentPouch.Items[entry.ItemIndex] is InventoryItem9 item9 && _currentPouch is InventoryPouch9 p9)
                {
                    item9.Pouch = p9.PouchIndex;
                    item9.IsUpdated = true;
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update item count: {ex.Message}", "OK");
        }
    }

    private void OnClearItemClicked(ItemEntry entry)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            _currentPouch.Items[entry.ItemIndex].Clear();
            entry.CountEntry.Text = "";
            entry.ItemIdEntry.Text = "";
            
            // Update the New checkbox if it exists
            if (entry.NewCheckBox != null && _currentPouch.Items[entry.ItemIndex] is IItemNewFlag newFlagItem)
            {
                entry.NewCheckBox.IsChecked = newFlagItem.IsNew;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to clear item: {ex.Message}", "OK");
        }
    }

    private void OnItemIdChanged(ItemEntry entry, string newValue)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            if (string.IsNullOrWhiteSpace(newValue))
            {
                _currentPouch.Items[entry.ItemIndex].Index = 0;
            }
            else if (int.TryParse(newValue, out var itemId))
            {
                itemId = Math.Max(0, itemId);
                _currentPouch.Items[entry.ItemIndex].Index = itemId;

                // CRITICAL: Enable SetNew flag on pouches for Gen8/Gen9 compatibility when modifying items
                if (_currentPouch is InventoryPouch8 pouch8)
                {
                    pouch8.SetNew = true;
                }
                else if (_currentPouch is InventoryPouch9 pouch9)
                {
                    pouch9.SetNew = true;
                }

                // For Gen9, ensure proper configuration for in-game visibility
                if (_currentPouch.Items[entry.ItemIndex] is InventoryItem9 item9 && _currentPouch is InventoryPouch9 p9)
                {
                    item9.Pouch = p9.PouchIndex;
                    item9.IsUpdated = true;
                    // Mark as new if it's a newly added item (previously had index 0)
                    if (itemId > 0 && !item9.IsValidPouch)
                    {
                        item9.IsNew = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update item ID: {ex.Message}", "OK");
        }
    }

    private void OnNewFlagChanged(ItemEntry entry, bool isNew)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            var item = _currentPouch.Items[entry.ItemIndex];
            if (item is IItemNewFlag newFlagItem)
            {
                newFlagItem.IsNew = isNew;

                // CRITICAL: Enable SetNew flag on pouches for Gen8/Gen9 compatibility
                if (_currentPouch is InventoryPouch8 pouch8)
                {
                    pouch8.SetNew = true;
                }
                else if (_currentPouch is InventoryPouch9 pouch9)
                {
                    pouch9.SetNew = true;
                }

                // For Gen9, ensure item is marked as updated for proper saving
                if (item is InventoryItem9 item9 && _currentPouch is InventoryPouch9 p9)
                {
                    item9.IsUpdated = true;
                    item9.Pouch = p9.PouchIndex;
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update new flag: {ex.Message}", "OK");
        }
    }

    private async void OnAddNewItemClicked(object? sender, EventArgs e)
    {
        if (_currentPouch == null) return;

        try
        {
            // Get items data immediately - no blocking!
            var itemsList = await CachedDataService.GetItemsAsync();
            
            // Create picker page
            var pickerPage = new SearchablePickerPage();
            
            // If data is empty (still loading), show loading spinner and wait briefly
            if (!itemsList.Any())
            {
                pickerPage.LoadingSpinnerControl.Show("Loading items...");
                
                // Wait a bit for data to load, but don't block forever
                for (int i = 0; i < 20 && !itemsList.Any(); i++) // Max 2 seconds
                {
                    await Task.Delay(100);
                    itemsList = await CachedDataService.GetItemsAsync();
                }
                
                pickerPage.LoadingSpinnerControl.Hide();
            }
            
            // If we still have no data, show error
            if (!itemsList.Any())
            {
                await DisplayAlert("Error", "Item data is still loading. Please try again in a moment.", "OK");
                return;
            }
            
            pickerPage.SetItems(itemsList, "Select Item to Add");
            
            var completionSource = new TaskCompletionSource<IPickerItem?>();
            pickerPage.CompletionSource = completionSource;
            
            await Navigation.PushModalAsync(pickerPage);
            var selectedItem = await completionSource.Task;
            
            if (selectedItem == null) return;

            var itemId = selectedItem.Id;
            
            // Ask for count with a better default
            var countResult = await DisplayPromptAsync("Add New Item", 
                $"Enter count for {selectedItem.DisplayName}:", 
                placeholder: "e.g., 1", 
                keyboard: Keyboard.Numeric,
                initialValue: "1");

            if (countResult == null) return;

            if (!int.TryParse(countResult, out var count))
            {
                await DisplayAlert("Error", "Invalid count. Please enter a number.", "OK");
                return;
            }

            count = Math.Max(0, Math.Min(count, _currentPouch.MaxCount));

            // CRITICAL: Enable SetNew flag on pouches for Gen8/Gen9 compatibility
            if (_currentPouch is InventoryPouch8 pouch8)
            {
                pouch8.SetNew = true;
            }
            else if (_currentPouch is InventoryPouch9 pouch9)
            {
                pouch9.SetNew = true;
            }

            // Find an empty slot or use the first available slot
            for (int i = 0; i < _currentPouch.Items.Length; i++)
            {
                if (_currentPouch.Items[i].Index == 0 || _currentPouch.Items[i].Count == 0)
                {
                    _currentPouch.Items[i].Index = itemId;
                    _currentPouch.Items[i].Count = count;
                    
                    // Set the IsNew flag for newly added items (important for Gen8+)
                    if (_currentPouch.Items[i] is IItemNewFlag newFlagItem)
                    {
                        newFlagItem.IsNew = true;
                    }

                    // For Gen9, ensure proper Pouch assignment for in-game visibility
                    if (_currentPouch.Items[i] is InventoryItem9 item9 && _currentPouch is InventoryPouch9 p9)
                    {
                        item9.Pouch = p9.PouchIndex;
                        item9.IsUpdated = true; // Mark as updated so it gets saved properly
                    }

                    // For Gen8b, set appropriate SortOrder for new items
                    if (_currentPouch.Items[i] is InventoryItem8b item8b)
                    {
                        // Find the next available sort order
                        ushort maxSort = 0;
                        foreach (var existingItem in _currentPouch.Items)
                        {
                            if (existingItem is InventoryItem8b existing && existing.SortOrder > maxSort)
                                maxSort = existing.SortOrder;
                        }
                        item8b.SortOrder = (ushort)(maxSort + 1);
                    }
                    
                    // Refresh the display
                    LoadItems();
                    await DisplayAlert("Success", $"Added {selectedItem.DisplayName} with count {count}! (Configured for in-game visibility)", "OK");
                    return;
                }
            }

            await DisplayAlert("Error", "No empty slots available in this pouch.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to add item: {ex.Message}", "OK");
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
            // Flush inventory changes to SaveFile.Data
            FlushInventoryChanges();
            
            // Mark the save file as edited
            _saveFile.State.Edited = true;
            
            // Mark changes as unsaved in PageManager
            PageManager.MarkChangesUnsaved();
            
            await DisplayAlert("Success", 
                "Inventory changes saved to memory!\n\n" +
                "To persist changes permanently:\n" +
                "1. Go back to Main Page\n" +
                "2. Click 'Export Save' button\n" +
                "3. Save the file to disk\n\n" +
                "Changes will be lost if you close the app without exporting!", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Flushes in-memory inventory changes back to the SaveFile's underlying data array.
    /// This ensures that item modifications, New flags, and other properties are persisted.
    /// </summary>
    private void FlushInventoryChanges()
    {
        if (_saveFile == null) return;
        
        // Trigger the SaveFile's inventory persistence mechanism
        // This calls LoadFromPouches() which executes pouches.SaveAll(Data)
        var currentInventory = _saveFile.Inventory;
        _saveFile.Inventory = currentInventory;
    }

    private string GetItemName(int itemIndex)
    {
        if (_saveFile == null || itemIndex == 0) 
            return "None";
        
        try
        {
            // Get English name
            var englishStrings = GameInfo.GetStrings("en");
            string englishName = "Unknown";
            if (englishStrings?.itemlist != null && itemIndex < englishStrings.itemlist.Length)
            {
                englishName = englishStrings.itemlist[itemIndex];
            }

            // Get Chinese name (try simplified first, then traditional)
            var chineseStrings = GameInfo.GetStrings("zh");
            string chineseName = "";
            if (chineseStrings?.itemlist != null && itemIndex < chineseStrings.itemlist.Length)
            {
                chineseName = chineseStrings.itemlist[itemIndex];
            }
            else
            {
                // Try traditional Chinese if simplified not available
                var traditionalStrings = GameInfo.GetStrings("zh2");
                if (traditionalStrings?.itemlist != null && itemIndex < traditionalStrings.itemlist.Length)
                {
                    chineseName = traditionalStrings.itemlist[itemIndex];
                }
            }

            // Format the display name with both languages
            if (!string.IsNullOrEmpty(chineseName) && chineseName != englishName)
            {
                return $"{englishName} ({chineseName})";
            }
            
            return englishName;
        }
        catch
        {
            // Fall back to generic name if lookup fails
        }
        
        return $"Item {itemIndex}";
    }

    private void OnItemIdChanged(Microsoft.Maui.Controls.Entry itemIdEntry, Microsoft.Maui.Controls.Entry countEntry, Label itemLabel, int arrayIndex)
    {
        if (_currentPouch?.Items == null || arrayIndex >= _currentPouch.Items.Length) return;

        try
        {
            if (int.TryParse(itemIdEntry.Text, out var itemId))
            {
                _currentPouch.Items[arrayIndex].Index = (ushort)Math.Max(0, itemId);
                
                // Update item name label
                itemLabel.Text = GetItemName(itemId);
                
                // Enable SetNew flag on pouches for Gen8/Gen9 compatibility
                if (_currentPouch is InventoryPouch8 pouch8)
                {
                    pouch8.SetNew = true;
                }
                else if (_currentPouch is InventoryPouch9 pouch9)
                {
                    pouch9.SetNew = true;
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update item ID: {ex.Message}", "OK");
        }
    }

    private void OnItemCountChanged(Microsoft.Maui.Controls.Entry countEntry, int arrayIndex)
    {
        if (_currentPouch?.Items == null || arrayIndex >= _currentPouch.Items.Length) return;

        try
        {
            if (int.TryParse(countEntry.Text, out var count))
            {
                _currentPouch.Items[arrayIndex].Count = Math.Max(0, Math.Min(count, _currentPouch.MaxCount));
                
                // Enable SetNew flag on pouches for Gen8/Gen9 compatibility
                if (_currentPouch is InventoryPouch8 pouch8)
                {
                    pouch8.SetNew = true;
                }
                else if (_currentPouch is InventoryPouch9 pouch9)
                {
                    pouch9.SetNew = true;
                }
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update item count: {ex.Message}", "OK");
        }
    }

    private void OnNewFlagChanged(CheckBox newCheckBox, int arrayIndex)
    {
        if (_currentPouch?.Items == null || arrayIndex >= _currentPouch.Items.Length) return;

        try
        {
            if (_currentPouch.Items[arrayIndex] is IItemNewFlag newFlagItem)
            {
                newFlagItem.IsNew = newCheckBox.IsChecked;
            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to update new flag: {ex.Message}", "OK");
        }
    }

    private void OnClearItemClicked(int arrayIndex)
    {
        if (_currentPouch?.Items == null || arrayIndex >= _currentPouch.Items.Length) return;

        try
        {
            _currentPouch.Items[arrayIndex].Clear();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to clear item: {ex.Message}", "OK");
        }
    }

    #region Popup Modal Methods

    private void OnFullViewClicked(object sender, EventArgs e)
    {
        try
        {
            ShowPopupModal();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to open full view: {ex.Message}", "OK");
        }
    }

    private void ShowPopupModal()
    {
        // Update popup header info
        if (_currentPouch != null)
        {
            PopupPouchNameLabel.Text = $"📋 {_currentPouch.Type} - Full View";
            PopupPouchStatsLabel.Text = PouchStatsLabel.Text;
        }
        else
        {
            PopupPouchNameLabel.Text = "📋 All Items - Full View";
            PopupPouchStatsLabel.Text = "Demo Mode";
        }

        // Load all items into popup
        LoadPopupItems();
        
        // Show the popup overlay
        ItemsPopupOverlay.IsVisible = true;
    }

    private void LoadPopupItems()
    {
        try
        {
            PopupItemsContainer.Children.Clear();

            // Add column headers first
            CreatePopupColumnHeaders();

            if (_currentPouch != null)
            {
                // Load real items
                LoadRealItemsIntoPopup();
            }
            else
            {
                // Load demo items
                LoadDemoItemsIntoPopup();
            }

            // Update stats
            UpdatePopupStats();
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load popup items: {ex.Message}", "OK");
        }
    }

    private void LoadRealItemsIntoPopup()
    {
        if (_currentPouch?.Items == null) return;

        for (int i = 0; i < _currentPouch.Items.Length; i++)
        {
            var item = _currentPouch.Items[i];
            if (item.Index == 0 && item.Count == 0) continue; // Skip empty slots

            CreatePopupItemUI(item.Index, item.Count, i, isDemo: false);
        }

        // Add "Add New Item" button at the end
        CreateAddNewItemButton();
    }

    private void LoadDemoItemsIntoPopup()
    {
        var demoItems = GetDemoItemsForPouch(_currentPouch?.Type.ToString() ?? "Items");
        
        for (int i = 0; i < demoItems.Length; i++)
        {
            var (itemName, count) = demoItems[i];
            CreatePopupItemUI(i + 1, count, i, isDemo: true, itemName);
        }
    }

    private void CreatePopupColumnHeaders()
    {
        var headerFrame = new Microsoft.Maui.Controls.Frame
        {
            BackgroundColor = Color.FromArgb("#34495E"),
            Padding = new Thickness(15, 12),
            Margin = new Thickness(0, 0, 0, 8),
            CornerRadius = 8,
            HasShadow = false
        };

        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // Item ID
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Item Name
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Count
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // New flag
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Actions

        var headers = new[] { "ID", "Item Name", "Count", "New", "Actions" };
        for (int i = 0; i < headers.Length; i++)
        {
            var label = new Label
            {
                Text = headers[i],
                TextColor = Colors.White,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = i == 1 ? TextAlignment.Start : TextAlignment.Center
            };
            Grid.SetColumn(label, i);
            headerGrid.Children.Add(label);
        }

        headerFrame.Content = headerGrid;
        PopupItemsContainer.Children.Add(headerFrame);
    }

    private void CreatePopupItemUI(int itemIndex, int count, int arrayIndex, bool isDemo, string? itemName = null)
    {
        var frame = new Microsoft.Maui.Controls.Frame
        {
            BackgroundColor = Colors.White,
            Padding = new Thickness(15, 12),
            Margin = new Thickness(0, 2),
            CornerRadius = 8,
            HasShadow = false,
            BorderColor = Color.FromArgb("#E8E8E8")
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // Item ID
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Item Name
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Count
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // New flag
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Actions

        // Item ID Entry
        var itemIdEntry = new Microsoft.Maui.Controls.Entry
        {
            Text = itemIndex.ToString(),
            Keyboard = Keyboard.Numeric,
            Placeholder = "ID",
            FontSize = 13,
            IsReadOnly = isDemo,
            BackgroundColor = isDemo ? Color.FromArgb("#F8F9FA") : Colors.White,
            TextColor = Color.FromArgb("#2C3E50"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        Grid.SetColumn(itemIdEntry, 0);

        // Item Name Display with rich formatting
        var displayName = itemName ?? GetItemName(itemIndex);
        var nameStackLayout = new StackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 2
        };

        var itemNameLabel = new Label
        {
            Text = displayName,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2C3E50"),
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var itemSubLabel = new Label
        {
            Text = $"ID: {itemIndex}",
            FontSize = 10,
            TextColor = Color.FromArgb("#7F8C8D")
        };

        nameStackLayout.Children.Add(itemNameLabel);
        nameStackLayout.Children.Add(itemSubLabel);
        Grid.SetColumn(nameStackLayout, 1);

        // Count Entry with styling
        var countEntry = new Microsoft.Maui.Controls.Entry
        {
            Text = count.ToString(),
            Keyboard = Keyboard.Numeric,
            Placeholder = "0",
            FontSize = 13,
            IsReadOnly = isDemo,
            BackgroundColor = isDemo ? Color.FromArgb("#F8F9FA") : Colors.White,
            TextColor = Color.FromArgb("#2C3E50"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        Grid.SetColumn(countEntry, 2);

        // New flag indicator
        View newFlagView;
        CheckBox? newCheckBox = null;
        
        if (_saveFile != null && _saveFile.Generation >= 8 && !isDemo)
        {
            newCheckBox = new CheckBox
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Color = Color.FromArgb("#27AE60")
            };
            
            // Set New flag state
            if (_currentPouch?.Items != null && arrayIndex < _currentPouch.Items.Length)
            {
                var item = _currentPouch.Items[arrayIndex];
                if (item is IItemNewFlag newFlagItem)
                {
                    newCheckBox.IsChecked = newFlagItem.IsNew;
                }
            }
            
            newFlagView = newCheckBox;
        }
        else
        {
            var badge = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = isDemo ? Color.FromArgb("#3498DB") : Color.FromArgb("#95A5A6"),
                Padding = new Thickness(8, 4),
                CornerRadius = 10,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var badgeLabel = new Label
            {
                Text = isDemo ? "DEMO" : "N/A",
                FontSize = 9,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center
            };

            badge.Content = badgeLabel;
            newFlagView = badge;
        }
        
        Grid.SetColumn(newFlagView, 3);

        // Action buttons in a horizontal stack
        var actionStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Edit button
        var editButton = new Button
        {
            Text = "✏️",
            FontSize = 11,
            BackgroundColor = Color.FromArgb("#3498DB"),
            TextColor = Colors.White,
            CornerRadius = 4,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = 0,
            IsEnabled = !isDemo
        };

        // Delete button
        var deleteButton = new Button
        {
            Text = "🗑️",
            FontSize = 11,
            BackgroundColor = isDemo ? Color.FromArgb("#BDC3C7") : Color.FromArgb("#E74C3C"),
            TextColor = Colors.White,
            CornerRadius = 4,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = 0,
            IsEnabled = !isDemo
        };

        actionStack.Children.Add(editButton);
        actionStack.Children.Add(deleteButton);
        Grid.SetColumn(actionStack, 4);

        // Add event handlers for real items
        if (!isDemo && _currentPouch?.Items != null && arrayIndex < _currentPouch.Items.Length)
        {
            itemIdEntry.TextChanged += (s, e) => {
                OnItemIdChanged(itemIdEntry, countEntry, itemNameLabel, arrayIndex);
                // Update the sub-label
                if (int.TryParse(e.NewTextValue, out var newId))
                {
                    itemSubLabel.Text = $"ID: {newId}";
                }
            };
            
            countEntry.TextChanged += (s, e) => OnItemCountChanged(countEntry, arrayIndex);
            
            if (newCheckBox != null)
            {
                newCheckBox.CheckedChanged += (s, e) => OnNewFlagChanged(newCheckBox, arrayIndex);
            }

            editButton.Clicked += async (s, e) => await OnEditItemClicked(arrayIndex);
            deleteButton.Clicked += (s, e) => OnClearItemClicked(arrayIndex);
        }

        grid.Children.Add(itemIdEntry);
        grid.Children.Add(nameStackLayout);
        grid.Children.Add(countEntry);
        grid.Children.Add(newFlagView);
        grid.Children.Add(actionStack);

        frame.Content = grid;
        PopupItemsContainer.Children.Add(frame);
    }

    private void CreateAddNewItemButton()
    {
        var addFrame = new Microsoft.Maui.Controls.Frame
        {
            BackgroundColor = Color.FromArgb("#27AE60"),
            Padding = 15,
            Margin = new Thickness(0, 10),
            CornerRadius = 8,
            HasShadow = true
        };

        var addButton = new Button
        {
            Text = "➕ Add New Item",
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };
        addButton.Clicked += OnAddNewItemClicked;
        
        addFrame.Content = addButton;
        PopupItemsContainer.Children.Add(addFrame);
    }

    private void OnClosePopupClicked(object sender, EventArgs e)
    {
        ItemsPopupOverlay.IsVisible = false;
        
        // Refresh the main view to show any changes made in popup
        if (_currentPouch != null)
        {
            LoadCurrentPouch();
        }
    }

    private void OnSaveFromPopupClicked(object sender, EventArgs e)
    {
        OnSaveClicked(sender, e);
    }

    private async Task OnEditItemClicked(int arrayIndex)
    {
        if (_currentPouch?.Items == null || arrayIndex >= _currentPouch.Items.Length) return;

        try
        {
            var item = _currentPouch.Items[arrayIndex];
            
            var itemIdResult = await DisplayPromptAsync("Edit Item", 
                $"Current Item ID: {item.Index}\nEnter new Item ID:", 
                placeholder: $"{item.Index}", 
                initialValue: item.Index.ToString(),
                keyboard: Keyboard.Numeric);

            if (itemIdResult == null) return;

            if (!int.TryParse(itemIdResult, out var newItemId))
            {
                await DisplayAlert("Error", "Invalid Item ID. Please enter a number.", "OK");
                return;
            }

            var countResult = await DisplayPromptAsync("Edit Item", 
                $"Current Count: {item.Count}\nEnter new count:", 
                placeholder: $"{item.Count}", 
                initialValue: item.Count.ToString(),
                keyboard: Keyboard.Numeric);

            if (countResult == null) return;

            if (!int.TryParse(countResult, out var newCount))
            {
                await DisplayAlert("Error", "Invalid count. Please enter a number.", "OK");
                return;
            }

            // Update the item
            newCount = Math.Max(0, Math.Min(newCount, _currentPouch.MaxCount));
            item.Index = newItemId;
            item.Count = newCount;

            // Enable SetNew flag for Gen8+ compatibility
            if (_currentPouch is InventoryPouch8 pouch8)
            {
                pouch8.SetNew = true;
            }
            else if (_currentPouch is InventoryPouch9 pouch9)
            {
                pouch9.SetNew = true;
            }

            // Refresh the popup display
            LoadPopupItems();
            
            await DisplayAlert("Success", $"Item updated: ID {newItemId}, Count {newCount}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to edit item: {ex.Message}", "OK");
        }
    }

    private void UpdatePopupStats()
    {
        if (_currentPouch != null)
        {
            var totalItems = _currentPouch.Items.Count(item => item.Index != 0 || item.Count != 0);
            var maxSlots = _currentPouch.Items.Length;
            PopupPouchStatsLabel.Text = $"Items: {totalItems}/{maxSlots} • Max Count: {_currentPouch.MaxCount}";
        }
        else
        {
            PopupPouchStatsLabel.Text = "Demo Mode - Sample Items";
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var searchText = e.NewTextValue?.ToLower() ?? "";
            
            foreach (var child in PopupItemsContainer.Children)
            {
                if (child is Microsoft.Maui.Controls.Frame frame && frame.Content is Grid grid)
                {
                    // Skip the header row
                    if (frame.BackgroundColor == Color.FromArgb("#34495E")) continue;
                    
                    // Check if this is an item row (has item name in column 1)
                    if (grid.Children.Count >= 2 && grid.Children[1] is StackLayout nameStack)
                    {
                        var nameLabel = nameStack.Children.FirstOrDefault() as Label;
                        var itemName = nameLabel?.Text?.ToLower() ?? "";
                        var itemId = "";
                        
                        // Also check ID for search
                        if (grid.Children[0] is Microsoft.Maui.Controls.Entry idEntry)
                        {
                            itemId = idEntry.Text?.ToLower() ?? "";
                        }
                        
                        var isVisible = string.IsNullOrEmpty(searchText) || 
                                       itemName.Contains(searchText) || 
                                       itemId.Contains(searchText);
                        
                        frame.IsVisible = isVisible;
                    }
                }
            }
        }
        catch (Exception)
        {
            // Silently handle search errors
        }
    }

    #endregion

    public class ItemEntry
    {
        public int ItemIndex { get; set; }
        public Microsoft.Maui.Controls.Entry CountEntry { get; set; } = null!;
        public Microsoft.Maui.Controls.Entry ItemIdEntry { get; set; } = null!;
        public CheckBox? NewCheckBox { get; set; }
    }
}