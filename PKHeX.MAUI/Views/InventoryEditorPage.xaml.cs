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

            // Remove ItemsPreviewLabel references since we don't have it in the new design
            // Demo items are now shown directly in the container
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

            // Add column header
            var columnHeaderFrame = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = Color.FromArgb("#34495E"),
                Padding = new Thickness(15, 12),
                Margin = new Thickness(0, 0, 0, 8),
                CornerRadius = 8,
                HasShadow = false
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            var nameLabel = new Label { Text = "道具名称", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Start };
            var countLabel = new Label { Text = "数量", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
            var newLabel = new Label { Text = "新", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
            var favoriteLabel = new Label { Text = "收藏", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
            var actionLabel = new Label { Text = "操作", FontSize = 13, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };

            Grid.SetColumn(nameLabel, 0);
            Grid.SetColumn(countLabel, 1);
            Grid.SetColumn(newLabel, 2);
            Grid.SetColumn(favoriteLabel, 3);
            Grid.SetColumn(actionLabel, 4);

            headerGrid.Children.Add(nameLabel);
            headerGrid.Children.Add(countLabel);
            headerGrid.Children.Add(newLabel);
            headerGrid.Children.Add(favoriteLabel);
            headerGrid.Children.Add(actionLabel);
            
            columnHeaderFrame.Content = headerGrid;
            ItemsContainer.Children.Add(columnHeaderFrame);

            // Only show items that have a count > 0 and index > 0 (non-empty items)
            System.Diagnostics.Debug.WriteLine($"LoadItems: 开始显示道具，总槽位数: {_currentPouch.Items.Length}");
            
            for (int i = 0; i < _currentPouch.Items.Length; i++)
            {
                var item = _currentPouch.Items[i];
                System.Diagnostics.Debug.WriteLine($"LoadItems: 槽位 {i}: ID={item.Index}, Count={item.Count}");
                
                if (item.Index > 0 && item.Count > 0) // Only show actual items
                {
                    System.Diagnostics.Debug.WriteLine($"LoadItems: 显示槽位 {i} 的道具: {GetItemName(item.Index)}");
                    CreateItemUI(item.Index, item.Count, i, isDemo: false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"LoadItems: 跳过槽位 {i} (ID={item.Index}, Count={item.Count})");
                }
            }

            // Update pouch stats to show only active items
            var activeItems = _currentPouch.Items.Count(item => item.Index > 0 && item.Count > 0);
            PouchStatsLabel.Text = $"Items: {activeItems}/{_currentPouch.Items.Length}";
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load items: {ex.Message}", "OK");
        }
    }

    private void CreateItemUI(int itemIndex, int count, int arrayIndex, bool isDemo)
    {
        // Use simple Frame instead of Border with RoundRectangle to avoid compilation issues
        var frame = new Microsoft.Maui.Controls.Frame
        {
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 12),
            Margin = new Thickness(0, 2),
            CornerRadius = 10,
            HasShadow = false,
            BorderColor = Color.FromArgb("#E2E8F0")
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

        // Item name and ID display
        var nameStackLayout = new StackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Spacing = 2
        };

        var itemNameLabel = new Label
        {
            Text = GetItemName(itemIndex),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0F172A"),
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var itemSubLabel = new Label
        {
            Text = itemIndex == 0 ? "空槽位" : $"ID: {itemIndex}",
            FontSize = 11,
            TextColor = Color.FromArgb("#64748B")
        };

        nameStackLayout.Children.Add(itemNameLabel);
        nameStackLayout.Children.Add(itemSubLabel);
        Grid.SetColumn(nameStackLayout, 0);

        // Count Entry
        var countEntry = new Microsoft.Maui.Controls.Entry
        {
            Text = count == 0 ? "" : count.ToString(),
            Keyboard = Keyboard.Numeric,
            Placeholder = "0",
            FontSize = 14,
            IsReadOnly = isDemo,
            BackgroundColor = isDemo ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#F1F5F9"),
            TextColor = Color.FromArgb("#1E293B"),
            HorizontalTextAlignment = TextAlignment.Center,
            HeightRequest = 40
        };
        Grid.SetColumn(countEntry, 1);

        // New flag indicator
        View newFlagView;
        CheckBox? newCheckBox = null;
        
        if (_saveFile != null && _saveFile.Generation >= 8 && !isDemo && _currentPouch?.Items != null && arrayIndex < _currentPouch.Items.Length)
        {
            var item = _currentPouch.Items[arrayIndex];
            if (item is IItemNewFlag newFlagItem)
            {
                newCheckBox = new CheckBox
                {
                    IsChecked = newFlagItem.IsNew,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Color = Color.FromArgb("#10B981")
                };
                newFlagView = newCheckBox;
            }
            else
            {
                newFlagView = new Label { Text = "N/A", FontSize = 10, TextColor = Color.FromArgb("#94A3B8"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };
            }
        }
        else
        {
            var badge = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = isDemo ? Color.FromArgb("#3B82F6") : Color.FromArgb("#94A3B8"),
                Padding = new Thickness(6, 4),
                CornerRadius = 6,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var badgeLabel = new Label { Text = isDemo ? "DEMO" : "N/A", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
            badge.Content = badgeLabel;
            newFlagView = badge;
        }
        Grid.SetColumn(newFlagView, 2);

        // Favorite flag indicator
        View favoriteFlagView;
        CheckBox? favoriteCheckBox = null;
        
        if (_saveFile != null && _saveFile.Generation >= 8 && !isDemo && _currentPouch?.Items != null && arrayIndex < _currentPouch.Items.Length)
        {
            var item = _currentPouch.Items[arrayIndex];
            if (item is IItemFavorite favoriteItem)
            {
                favoriteCheckBox = new CheckBox
                {
                    IsChecked = favoriteItem.IsFavorite,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Color = Color.FromArgb("#F59E0B")
                };
                favoriteFlagView = favoriteCheckBox;
            }
            else
            {
                favoriteFlagView = new Label { Text = "N/A", FontSize = 10, TextColor = Color.FromArgb("#94A3B8"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };
            }
        }
        else
        {
            var badge = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = isDemo ? Color.FromArgb("#3B82F6") : Color.FromArgb("#94A3B8"),
                Padding = new Thickness(6, 4),
                CornerRadius = 6,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var badgeLabel = new Label { Text = isDemo ? "DEMO" : "N/A", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
            badge.Content = badgeLabel;
            favoriteFlagView = badge;
        }
        Grid.SetColumn(favoriteFlagView, 3);

        // Free space flag indicator
        View freeSpaceFlagView;
        CheckBox? freeSpaceCheckBox = null;
        
        if (_saveFile != null && _saveFile.Generation >= 8 && !isDemo && _currentPouch?.Items != null && arrayIndex < _currentPouch.Items.Length)
        {
            var item = _currentPouch.Items[arrayIndex];
            if (item is IItemFreeSpace freeSpaceItem)
            {
                freeSpaceCheckBox = new CheckBox
                {
                    IsChecked = freeSpaceItem.IsFree,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Color = Color.FromArgb("#6366F1")
                };
                freeSpaceFlagView = freeSpaceCheckBox;
            }
            else
            {
                freeSpaceFlagView = new Label { Text = "N/A", FontSize = 10, TextColor = Color.FromArgb("#94A3B8"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };
            }
        }
        else
        {
            var badge = new Microsoft.Maui.Controls.Frame
            {
                BackgroundColor = isDemo ? Color.FromArgb("#3B82F6") : Color.FromArgb("#94A3B8"),
                Padding = new Thickness(6, 4),
                CornerRadius = 6,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            var badgeLabel = new Label { Text = isDemo ? "DEMO" : "N/A", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
            badge.Content = badgeLabel;
            freeSpaceFlagView = badge;
        }
        Grid.SetColumn(freeSpaceFlagView, 4);

        // Action buttons
        var actionStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var editButton = new Button
        {
            Text = "✏️",
            FontSize = 10,
            BackgroundColor = Color.FromArgb("#3B82F6"),
            TextColor = Colors.White,
            CornerRadius = 6,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = 0,
            IsEnabled = !isDemo
        };

        var deleteButton = new Button
        {
            Text = "🗑️",
            FontSize = 10,
            BackgroundColor = isDemo ? Color.FromArgb("#CBD5E1") : Color.FromArgb("#EF4444"),
            TextColor = Colors.White,
            CornerRadius = 6,
            WidthRequest = 32,
            HeightRequest = 32,
            Padding = 0,
            IsEnabled = !isDemo
        };

        actionStack.Children.Add(editButton);
        actionStack.Children.Add(deleteButton);
        Grid.SetColumn(actionStack, 5);

        // Event handlers for real items
        if (!isDemo && _currentPouch?.Items != null && arrayIndex < _currentPouch.Items.Length)
        {
            var itemEntry = new ItemEntry 
            { 
                ItemIndex = arrayIndex, 
                CountEntry = countEntry, 
                ItemIdEntry = new Microsoft.Maui.Controls.Entry { Text = itemIndex.ToString() }, 
                NewCheckBox = newCheckBox,
                FavoriteCheckBox = favoriteCheckBox,
                FreeSpaceCheckBox = freeSpaceCheckBox
            };

            countEntry.TextChanged += (s, e) => {
                OnItemCountChanged(itemEntry, e.NewTextValue);
                // Update display
                itemNameLabel.Text = GetItemName(_currentPouch.Items[arrayIndex].Index);
                itemSubLabel.Text = _currentPouch.Items[arrayIndex].Index == 0 ? "空槽位" : $"ID: {_currentPouch.Items[arrayIndex].Index}";
            };
            
            if (newCheckBox != null)
            {
                newCheckBox.CheckedChanged += (s, e) => OnNewFlagChanged(itemEntry, e.Value);
            }

            if (favoriteCheckBox != null)
            {
                favoriteCheckBox.CheckedChanged += (s, e) => OnFavoriteFlagChanged(itemEntry, e.Value);
            }

            if (freeSpaceCheckBox != null)
            {
                freeSpaceCheckBox.CheckedChanged += (s, e) => OnFreeSpaceFlagChanged(itemEntry, e.Value);
            }

            editButton.Clicked += async (s, e) => await OnEditItemClicked(arrayIndex);
            deleteButton.Clicked += (s, e) => OnClearItemClicked(itemEntry);

            _itemEntries.Add(itemEntry);
        }

        grid.Children.Add(nameStackLayout);
        grid.Children.Add(countEntry);
        grid.Children.Add(newFlagView);
        grid.Children.Add(favoriteFlagView);
        grid.Children.Add(actionStack);

        frame.Content = grid;
        ItemsContainer.Children.Add(frame);
    }

    private async Task OnEditItemClicked(int arrayIndex)
    {
        if (_currentPouch?.Items == null || arrayIndex >= _currentPouch.Items.Length) return;

        try
        {
            var item = _currentPouch.Items[arrayIndex];
            
            var itemIdResult = await DisplayPromptAsync("编辑道具", 
                $"当前道具ID: {item.Index}\n输入新的道具ID:", 
                placeholder: $"{item.Index}", 
                initialValue: item.Index.ToString(),
                keyboard: Keyboard.Numeric);

            if (itemIdResult == null) return;

            if (!int.TryParse(itemIdResult, out var newItemId))
            {
                await DisplayAlert("错误", "无效的道具ID，请输入数字。", "确定");
                return;
            }

            var countResult = await DisplayPromptAsync("编辑道具", 
                $"当前数量: {item.Count}\n输入新的数量:", 
                placeholder: $"{item.Count}", 
                initialValue: item.Count.ToString(),
                keyboard: Keyboard.Numeric);

            if (countResult == null) return;

            if (!int.TryParse(countResult, out var newCount))
            {
                await DisplayAlert("错误", "无效的数量，请输入数字。", "确定");
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

            // Refresh the display
            LoadItems();
            
            await DisplayAlert("成功", $"道具已更新: ID {newItemId}, 数量 {newCount}", "确定");
        }
        catch (Exception ex)
        {
            await DisplayAlert("错误", $"编辑道具失败: {ex.Message}", "确定");
        }
    }

    private void OnItemSearchChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var searchText = e.NewTextValue?.ToLower() ?? "";
            
            foreach (var child in ItemsContainer.Children)
            {
                if (child is Microsoft.Maui.Controls.Frame frame && frame.Content is Grid grid)
                {
                    // Skip the header row
                    if (frame.BackgroundColor == Color.FromArgb("#34495E")) continue;
                    
                    // Check if this is an item row (has item name in column 0)
                    if (grid.Children.Count >= 1 && grid.Children[0] is StackLayout nameStack)
                    {
                        var nameLabel = nameStack.Children.FirstOrDefault() as Label;
                        var itemName = nameLabel?.Text?.ToLower() ?? "";
                        
                        var isVisible = string.IsNullOrEmpty(searchText) || 
                                       itemName.Contains(searchText);
                        
                        frame.IsVisible = isVisible;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnItemSearchChanged 异常: {ex}");
            // Silently handle search errors
        }
    }

    private void FlushInventoryChanges()
    {
        if (_saveFile == null) return;
        
        try
        {
            // Apply the changes using the correct PKHeX Core pattern
            // This ensures all inventory modifications are properly applied to the save file
            // Note: The inventory pouches should already be modified in-place,
            // so we just need to reassign the inventory to trigger any necessary updates
            _saveFile.Inventory = _saveFile.Inventory;
            
            System.Diagnostics.Debug.WriteLine("Successfully applied inventory changes to SaveFile");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FlushInventoryChanges error: {ex.Message}");
        }
    }

    /// <summary>
    /// 使用PKHeX.Core的正确方式获取道具过滤列表
    /// </summary>
    private List<IPickerItem> GetFilteredItemsForPouch(InventoryType pouchType)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"开始获取 {pouchType} 类型的道具");
            
            if (_saveFile == null || _currentPouch == null) 
            {
                System.Diagnostics.Debug.WriteLine("SaveFile 或 CurrentPouch 为空");
                return new List<IPickerItem>();
            }

            // 获取所有道具
            var allItems = CachedDataService.GetItems();
            System.Diagnostics.Debug.WriteLine($"获取到 {allItems.Count} 个总道具");
            
            // 使用当前pouch的LegalItems数组 - 这是PKHeX.Core的标准方式
            if (_currentPouch.LegalItems == null || _currentPouch.LegalItems.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("当前背包没有合法道具列表，返回前100个道具作为安全后备");
                return allItems.Take(100).ToList();
            }
            
            // 创建合法道具ID的HashSet
            var legalItemIds = new HashSet<int>();
            foreach (var legalItemId in _currentPouch.LegalItems)
            {
                legalItemIds.Add((int)legalItemId);
            }
            
            System.Diagnostics.Debug.WriteLine($"当前背包有 {legalItemIds.Count} 个合法道具ID");
            
            // 过滤道具列表
            var filteredItems = new List<IPickerItem>();
            foreach (var item in allItems)
            {
                if (legalItemIds.Contains(item.Id))
                {
                    filteredItems.Add(item);
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"过滤后剩余 {filteredItems.Count} 个道具");
            
            // 如果过滤后没有道具，返回所有道具作为后备
            if (filteredItems.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("过滤后没有道具，返回前100个道具作为后备");
                return allItems.Take(100).ToList();
            }
            
            return filteredItems;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetFilteredItemsForPouch 异常: {ex}");
            // 发生异常时返回前100个道具作为安全后备
            try
            {
                var allItems = CachedDataService.GetItems();
                return allItems.Take(100).ToList();
            }
            catch
            {
                return new List<IPickerItem>();
            }
        }
    }

    private async void OnAddNewItemClicked(object? sender, EventArgs e)
    {
        if (_currentPouch == null) 
        {
            await DisplayAlert("错误", "当前背包为空，无法添加道具。", "确定");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"开始添加道具到 {_currentPouch.Type} pouch");
            
            // 简化错误处理 - 直接获取道具列表
            var filteredItems = GetFilteredItemsForPouch(_currentPouch.Type);
            System.Diagnostics.Debug.WriteLine($"获取到 {filteredItems.Count} 个可用道具");
            
            if (filteredItems.Count == 0)
            {
                await DisplayAlert("提示", $"{_currentPouch.Type} 背包暂无可用道具。", "确定");
                return;
            }
            
            try
            {
                // 创建并使用SearchablePickerPage
                SearchablePickerPage pickerPage;
                try
                {
                    pickerPage = new SearchablePickerPage();
                }
                catch (Exception initEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SearchablePickerPage 创建失败: {initEx}");
                    await DisplayAlert("错误", $"无法创建选择器界面: {initEx.Message}", "确定");
                    return;
                }
                
                // 设置道具列表
                try
                {
                    await Task.Run(() => {
                        pickerPage.SetItems(filteredItems, $"选择 {_currentPouch.Type} 道具");
                    });
                    
                    // 等待一小段时间确保SetItems完成
                    await Task.Delay(100);
                }
                catch (Exception setItemsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"设置道具列表失败: {setItemsEx}");
                    await DisplayAlert("错误", $"无法设置道具列表: {setItemsEx.Message}", "确定");
                    return;
                }
                
                // 设置完成回调
                var completionSource = new TaskCompletionSource<IPickerItem?>();
                pickerPage.CompletionSource = completionSource;
                
                // 推送模态页面
                try
                {
                    await Navigation.PushModalAsync(pickerPage);
                }
                catch (Exception navEx)
                {
                    System.Diagnostics.Debug.WriteLine($"推送模态页面失败: {navEx}");
                    await DisplayAlert("错误", $"无法打开选择器: {navEx.Message}", "确定");
                    return;
                }
                
                // 等待用户选择
                var selectedItem = await completionSource.Task;
                
                // 添加调试日志（而非用户可见的对话框）
                System.Diagnostics.Debug.WriteLine($"接收到的选择结果: {selectedItem?.DisplayName ?? "null"}, ID: {selectedItem?.Id ?? -1}");
                
                if (selectedItem == null)
                {
                    System.Diagnostics.Debug.WriteLine("用户取消了选择");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"用户选择了道具: {selectedItem.DisplayName} (ID: {selectedItem.Id})");

                // 直接添加道具到背包，不用单独的方法
                await Task.Delay(500); // 给UI时间恢复
                
                // 添加道具到背包
                try
                {
                    System.Diagnostics.Debug.WriteLine($"开始添加道具: {selectedItem.DisplayName} (ID: {selectedItem.Id})");
                    
                    if (_currentPouch?.Items == null)
                    {
                        await DisplayAlert("错误", "当前背包为空，无法添加道具", "确定");
                        return;
                    }
                    
                    // 查找空槽位或相同道具的槽位
                    bool added = false;
                    int emptySlot = -1;
                    int sameItemSlot = -1;
                    
                    for (int i = 0; i < _currentPouch.Items.Length; i++)
                    {
                        var item = _currentPouch.Items[i];
                        
                        // 找到相同道具且未满的槽位
                        if (item.Index == selectedItem.Id && item.Count > 0 && item.Count < _currentPouch.MaxCount)
                        {
                            sameItemSlot = i;
                            break;
                        }
                        
                        // 记录第一个空槽位
                        if (emptySlot == -1 && (item.Index == 0 || item.Count == 0))
                        {
                            emptySlot = i;
                        }
                    }
                    
                    int targetSlot = -1;
                    
                    // 优先增加已有相同道具的数量
                    if (sameItemSlot >= 0)
                    {
                        _currentPouch.Items[sameItemSlot].Count++;
                        targetSlot = sameItemSlot;
                        added = true;
                        System.Diagnostics.Debug.WriteLine($"增加已有道具数量，槽位: {sameItemSlot}");
                    }
                    // 否则使用空槽位
                    else if (emptySlot >= 0)
                    {
                        _currentPouch.Items[emptySlot].Index = selectedItem.Id;
                        _currentPouch.Items[emptySlot].Count = 1;
                        targetSlot = emptySlot;
                        added = true;
                        System.Diagnostics.Debug.WriteLine($"添加到空槽位: {emptySlot}");
                    }
                    
                    if (added)
                    {
                        // CRITICAL: Enable SetNew flag for Gen8/Gen9 compatibility
                        if (_currentPouch is InventoryPouch8 pouch8)
                        {
                            pouch8.SetNew = true;
                            System.Diagnostics.Debug.WriteLine("设置 Gen8 SetNew 标志");
                        }
                        else if (_currentPouch is InventoryPouch9 pouch9)
                        {
                            pouch9.SetNew = true;
                            System.Diagnostics.Debug.WriteLine("设置 Gen9 SetNew 标志");
                        }
                        
                        // For Gen9, ensure proper configuration for in-game visibility
                        if (_currentPouch.Items[targetSlot] is InventoryItem9 item9 && _currentPouch is InventoryPouch9 p9)
                        {
                            item9.Pouch = p9.PouchIndex;
                            item9.IsUpdated = true;
                            item9.IsNew = true;
                            System.Diagnostics.Debug.WriteLine("设置 Gen9 道具属性");
                        }
                        
                        // 刷新显示
                        LoadItems();
                        
                        await DisplayAlert("添加成功", 
                            $"道具已添加！\n" +
                            $"名称: {selectedItem.DisplayName}\n" +
                            $"ID: {selectedItem.Id}\n" +
                            $"槽位: {targetSlot}\n" +
                            $"数量: {_currentPouch.Items[targetSlot].Count}", 
                            "确定");
                        
                        System.Diagnostics.Debug.WriteLine($"道具添加成功：槽位 {targetSlot}, 数量 {_currentPouch.Items[targetSlot].Count}");
                    }
                    else
                    {
                        await DisplayAlert("背包已满", "所有槽位都已被使用且已达到最大数量", "确定");
                        System.Diagnostics.Debug.WriteLine("背包已满，无法添加道具");
                    }
                }
                catch (Exception addEx)
                {
                    System.Diagnostics.Debug.WriteLine($"添加道具异常: {addEx}");
                    await DisplayAlert("添加失败", $"添加道具时发生错误: {addEx.Message}", "确定");
                }
            }
            catch (Exception pickerEx)
            {
                System.Diagnostics.Debug.WriteLine($"SearchablePickerPage 异常: {pickerEx}");
                await DisplayAlert("错误", $"选择道具时出错: {pickerEx.Message}", "确定");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnAddNewItemClicked 总异常: {ex}");
            await DisplayAlert("错误", $"添加道具失败: {ex.Message}", "确定");
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
                // When count becomes 0, refresh the display to hide this item
                LoadItems();
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

                // If count becomes 0, refresh to hide the item
                if (count == 0)
                {
                    LoadItems();
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
            
            // Refresh the display to remove the cleared item from view
            LoadItems();
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

    private void OnFavoriteFlagChanged(ItemEntry entry, bool isFavorite)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            var item = _currentPouch.Items[entry.ItemIndex];
            if (item is IItemFavorite favoriteItem)
            {
                favoriteItem.IsFavorite = isFavorite;

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
            DisplayAlert("Error", $"Failed to update favorite flag: {ex.Message}", "OK");
        }
    }

    private void OnFreeSpaceFlagChanged(ItemEntry entry, bool isFree)
    {
        if (_currentPouch == null || entry.ItemIndex >= _currentPouch.Items.Length) return;

        try
        {
            var item = _currentPouch.Items[entry.ItemIndex];
            if (item is IItemFreeSpace freeSpaceItem)
            {
                freeSpaceItem.IsFree = isFree;

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
            DisplayAlert("Error", $"Failed to update free space flag: {ex.Message}", "OK");
        }
    }

    private string GetItemName(int itemIndex)
    {
        if (_saveFile == null || itemIndex == 0) 
            return "无道具";
        
        try
        {
            // Get Chinese name first (try simplified, then traditional)
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

            // Get English name as fallback/supplement
            var englishStrings = GameInfo.GetStrings("en");
            string englishName = "Unknown";
            if (englishStrings?.itemlist != null && itemIndex < englishStrings.itemlist.Length)
            {
                englishName = englishStrings.itemlist[itemIndex];
            }

            // Format the display name with Chinese as primary
            if (!string.IsNullOrEmpty(chineseName))
            {
                // If Chinese and English are different, show both
                if (!string.IsNullOrEmpty(englishName) && chineseName != englishName)
                {
                    return $"{chineseName} ({englishName})";
                }
                return chineseName;
            }
            
            return englishName; // Fallback to English if Chinese not available
        }
        catch
        {
            // Fall back to generic name if lookup fails
        }
        
        return $"道具 {itemIndex}";
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
        if (_saveFile == null) 
        {
            await DisplayAlert("Error", "Save file is null.", "OK");
            return;
        }

        try
        {
            // Apply inventory changes using PKHeX Core pattern
            // The inventory modifications should already be applied to the pouches in-place
            // We just need to ensure the save file recognizes the changes
            
            // Mark the save file as edited so changes are properly tracked
            _saveFile.State.Edited = true;
            
            // Mark changes as unsaved in PageManager
            PageManager.MarkChangesUnsaved();
            
            await DisplayAlert("Success", 
                "Inventory changes saved!\n\n" +
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

    public class ItemEntry
    {
        public int ItemIndex { get; set; }
        public Microsoft.Maui.Controls.Entry CountEntry { get; set; } = null!;
        public Microsoft.Maui.Controls.Entry ItemIdEntry { get; set; } = null!;
        public CheckBox? NewCheckBox { get; set; }
        public CheckBox? FavoriteCheckBox { get; set; }
        public CheckBox? FreeSpaceCheckBox { get; set; }
    }
}