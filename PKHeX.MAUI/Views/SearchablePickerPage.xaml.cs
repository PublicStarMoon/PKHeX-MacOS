using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace PKHeX.MAUI.Views;

public partial class SearchablePickerPage : ContentPage, INotifyPropertyChanged
{
    private string _title = "";
    private List<IPickerItem> _allItems = new();
    private ObservableCollection<IPickerItem> _filteredItems = new();
    private IPickerItem? _selectedItem;
    private string _searchText = "";

    public SearchablePickerPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

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
        var filtered = string.IsNullOrWhiteSpace(searchText)
            ? _allItems
            : _allItems.Where(item => 
                item.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                item.Id.ToString().Contains(searchText)).ToList();

        FilteredItems.Clear();
        foreach (var item in filtered)
        {
            FilteredItems.Add(item);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? "";
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

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Interface for picker items
public interface IPickerItem
{
    int Id { get; set; }
    string DisplayName { get; set; }
}

// Updated data model classes to implement the interface
public class MoveItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

public class AbilityItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

public class NatureItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

public class ItemItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

public class BallItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

public class FormItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}
