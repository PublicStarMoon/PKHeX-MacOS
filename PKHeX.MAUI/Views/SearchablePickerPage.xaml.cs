using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using PKHeX.MAUI.Services;
using PKHeX.MAUI.Models;

namespace PKHeX.MAUI.Views;

public partial class SearchablePickerPage : ContentPage, INotifyPropertyChanged
{
    private string _title = "";
    private List<IPickerItem> _allItems = new();
    private ObservableCollection<IPickerItem> _filteredItems = new();
    private IPickerItem? _selectedItem;
    private string _searchText = "";
    private System.Timers.Timer? _searchTimer;

    public SearchablePickerPage()
    {
        InitializeComponent();
        BindingContext = this;
        
        // Initialize search timer for better performance
        _searchTimer = new System.Timers.Timer(300); // 300ms delay
        _searchTimer.AutoReset = false;
        _searchTimer.Elapsed += OnSearchTimerElapsed;
    }

    /// <summary>
    /// Public access to the loading spinner for external components
    /// </summary>
    public Views.Components.LoadingSpinner LoadingSpinnerControl => LoadingSpinner;

    public new string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<IPickerItem> FilteredItems
    {
        get => _filteredItems;
        set
        {
            _filteredItems = value;
            OnPropertyChanged();
        }
    }

    public IPickerItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public TaskCompletionSource<IPickerItem?>? CompletionSource { get; set; }

    public void SetItems(List<IPickerItem> items, string title, IPickerItem? currentSelection = null)
    {
        _allItems = new List<IPickerItem>(items);
        Title = title;
        SelectedItem = currentSelection;
        FilterItems("");
        
        // Auto-scroll to current selection if it exists
        if (currentSelection != null)
        {
            var index = FilteredItems.ToList().FindIndex(x => x.Id == currentSelection.Id);
            if (index >= 0)
            {
                Application.Current?.Dispatcher.Dispatch(async () =>
                {
                    await Task.Delay(100); // Small delay to ensure UI is loaded
                    ItemsCollectionView.ScrollTo(index, position: ScrollToPosition.Center, animate: true);
                    ItemsCollectionView.SelectedItem = FilteredItems[index];
                });
            }
        }
    }

    private void FilterItems(string searchText)
    {
        List<IPickerItem> filtered;
        string searchInfo = "";
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Show ALL items when no search filter is applied
            filtered = _allItems;
            searchInfo = $"Showing all {_allItems.Count} items";
        }
        else
        {
            // Optimized search with multiple criteria and scoring
            var searchLower = searchText.ToLowerInvariant();
            var searchTerms = searchLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Use parallel processing for large datasets
            var searchResults = _allItems.AsParallel()
                .Select(item => new { Item = item, Score = CalculateSearchScore(item, searchTerms, searchLower) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ToList();
            
            var totalMatches = searchResults.Count;
            var limitedResults = searchResults.Take(500).Select(x => x.Item).ToList();
            
            filtered = limitedResults;
            
            if (totalMatches > 500)
            {
                searchInfo = $"Showing top 500 of {totalMatches} matches. Refine search for more specific results.";
            }
            else if (totalMatches > 0)
            {
                searchInfo = $"Found {totalMatches} matching items";
            }
            else
            {
                searchInfo = "No items found matching your search";
            }
        }

        Application.Current?.Dispatcher.Dispatch(() =>
        {
            FilteredItems.Clear();
            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }
            
            // Update search info
            SearchInfoLabel.Text = searchInfo;
            SearchInfoLabel.IsVisible = !string.IsNullOrWhiteSpace(searchText);
        });
    }

    private int CalculateSearchScore(IPickerItem item, string[] searchTerms, string fullSearchText)
    {
        var displayLower = item.DisplayName.ToLowerInvariant();
        var idStr = item.Id.ToString();
        var score = 0;

        // Exact ID match gets highest priority
        if (idStr == fullSearchText)
            return 1000;

        // ID starts with search text
        if (idStr.StartsWith(fullSearchText))
            score += 500;

        // Display name starts with search text
        if (displayLower.StartsWith(fullSearchText))
            score += 400;

        // All search terms must be found
        var foundTerms = 0;
        foreach (var term in searchTerms)
        {
            if (displayLower.Contains(term))
            {
                foundTerms++;
                score += 100;
            }
            else if (idStr.Contains(term))
            {
                foundTerms++;
                score += 50;
            }
        }

        // Return 0 if not all terms were found
        return foundTerms == searchTerms.Length ? score : 0;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? "";
        
        // Stop the previous timer
        _searchTimer?.Stop();
        
        // Start a new timer to delay the search
        _searchTimer?.Start();
    }
    
    private void OnSearchTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        FilterItems(_searchText);
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is IPickerItem selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        CompletionSource?.SetResult(SelectedItem);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        CompletionSource?.SetResult(null);
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _searchTimer?.Stop();
        _searchTimer?.Dispose();
    }

    /// <summary>
    /// Refresh the items list with new data (useful for reactive data loading)
    /// </summary>
    public void RefreshItems(List<IPickerItem> newItems)
    {
        Application.Current?.Dispatcher.Dispatch(() =>
        {
            _allItems = newItems;
            FilterItems(_searchText); // Re-apply current filter to new data
        });
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
