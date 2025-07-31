using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using PKHeX.MAUI.Services;
using PKHeX.MAUI.Models;

namespace PKHeX.MAUI.Views;

public partial class SearchablePickerPage : ContentPage
{
    private string _pageTitle = "";
    private List<IPickerItem> _allItems = new();
    private ObservableCollection<IPickerItem> _filteredItems = new();
    private IPickerItem? _selectedItem;
    private string _searchText = "";

    public SearchablePickerPage()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("SearchablePickerPage: 开始初始化");
            
            // 初始化所有属性 - 在InitializeComponent之前
            _pageTitle = "Select Item"; // 确保有默认值
            _filteredItems = new ObservableCollection<IPickerItem>();
            _allItems = new List<IPickerItem>();
            _searchText = "";
            
            InitializeComponent();
            
            // 设置绑定上下文
            BindingContext = this;
            
            System.Diagnostics.Debug.WriteLine("SearchablePickerPage: 初始化完成");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SearchablePickerPage 构造函数异常: {ex}");
            
            // 确保即使出错也有基本的初始化
            _pageTitle ??= "Select Item";
            _filteredItems ??= new ObservableCollection<IPickerItem>();
            _allItems ??= new List<IPickerItem>();
            _searchText ??= "";
            
            // 重新抛出异常，让调用者知道有问题
            throw new InvalidOperationException($"SearchablePickerPage 初始化失败: {ex.Message}", ex);
        }
    }

    public string PageTitle
    {
        get => _pageTitle;
        set
        {
            _pageTitle = value;
            NotifyPropertyChanged();
        }
    }

    public ObservableCollection<IPickerItem> FilteredItems
    {
        get => _filteredItems;
        set
        {
            _filteredItems = value;
            NotifyPropertyChanged();
        }
    }

    public IPickerItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            NotifyPropertyChanged();
        }
    }

    public TaskCompletionSource<IPickerItem?>? CompletionSource { get; set; }

    public void SetItems(List<IPickerItem> items, string title, IPickerItem? currentSelection = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"SetItems: 设置 {items?.Count ?? 0} 个道具，标题: {title}");
            
            // 确保在主线程上执行所有UI相关操作
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    PageTitle = title ?? "Select Item";
                    SelectedItem = currentSelection;
                    
                    // 安全地设置道具列表
                    _allItems = items != null ? new List<IPickerItem>(items) : new List<IPickerItem>();
                    
                    FilterItems("");
                    System.Diagnostics.Debug.WriteLine($"SetItems: UI更新完成，显示 {FilteredItems.Count} 个道具");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SetItems UI更新异常: {ex}");
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SetItems 异常: {ex}");
            throw;
        }
    }

    private void ScrollToCurrentSelection()
    {
        try
        {
            if (SelectedItem != null && FilteredItems.Any())
            {
                var selectedInFiltered = FilteredItems.FirstOrDefault(x => x.Id == SelectedItem.Id);
                if (selectedInFiltered != null)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await Task.Delay(100); // 等待UI加载
                            ItemsCollectionView.SelectedItem = selectedInFiltered;
                            ItemsCollectionView.ScrollTo(selectedInFiltered, position: ScrollToPosition.Center, animate: false);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"ScrollToCurrentSelection 异常: {ex}");
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ScrollToCurrentSelection 外部异常: {ex}");
        }
    }

    private void FilterItems(string searchText)
    {
        try
        {
            if (_allItems == null)
            {
                System.Diagnostics.Debug.WriteLine("FilterItems: _allItems 为空");
                return;
            }

            List<IPickerItem> filtered;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // 显示前100个道具以提高性能
                filtered = _allItems.Take(100).ToList();
            }
            else
            {
                var searchLower = searchText.ToLowerInvariant();
                filtered = _allItems
                    .Where(item => 
                        item.DisplayName.ToLowerInvariant().Contains(searchLower) ||
                        item.Id.ToString().Contains(searchText))
                    .Take(100)
                    .ToList();
            }

            // 安全地更新UI
            FilteredItems.Clear();
            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }
            
            System.Diagnostics.Debug.WriteLine($"FilterItems: 显示 {filtered.Count} 个道具");
            
            // 更新信息标签
            if (SearchInfoLabel != null)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    SearchInfoLabel.Text = _allItems.Count > 100 
                        ? $"显示前 100 个道具，共 {_allItems.Count} 个。输入文字搜索更多..."
                        : $"显示全部 {_allItems.Count} 个道具";
                }
                else
                {
                    SearchInfoLabel.Text = filtered.Count == 0 
                        ? "未找到匹配的道具" 
                        : $"找到 {filtered.Count} 个道具";
                }
                SearchInfoLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FilterItems 异常: {ex}");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _searchText = e.NewTextValue ?? "";
            FilterItems(_searchText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnSearchTextChanged 异常: {ex}");
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is IPickerItem selectedItem)
            {
                SelectedItem = selectedItem;
                System.Diagnostics.Debug.WriteLine($"选择了道具: {selectedItem.DisplayName}");
                
                // 更新页面标题来显示当前选择
                PageTitle = $"选择 - 当前: {selectedItem.DisplayName}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnSelectionChanged 异常: {ex}");
        }
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"确定按钮点击，选择的道具: {SelectedItem?.DisplayName ?? "无"}");
            
            // 显示调试信息给用户
            if (SelectedItem == null)
            {
                await DisplayAlert("调试信息", "没有选择任何选项！请先点击一个选项来选择它。", "OK");
                return;
            }

            var confirmResult = await DisplayAlert("确认选择",
                $"您选择了: {SelectedItem.DisplayName} (ID: {SelectedItem.Id})\n\n确定要选择这个选项吗？",
                "确定", "取消");
                
            if (!confirmResult)
            {
                return; // 用户取消了选择
            }
            
            // 在关闭页面之前设置结果
            var result = SelectedItem;
            CompletionSource?.SetResult(result);
            
            // 等待一小段时间确保结果被处理
            await Task.Delay(50);
            
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnOkClicked 异常: {ex}");
            await DisplayAlert("错误", $"选择时发生错误: {ex.Message}", "OK");
            CompletionSource?.SetException(ex);
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("取消按钮点击");
            
            // 在关闭页面之前设置null结果
            CompletionSource?.SetResult(null);
            
            // 等待一小段时间确保结果被处理
            await Task.Delay(50);
            
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnCancelClicked 异常: {ex}");
            CompletionSource?.SetException(ex);
        }
    }

    protected override void OnDisappearing()
    {
        try
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("SearchablePickerPage OnDisappearing");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnDisappearing 异常: {ex}");
        }
    }

    public void RefreshItems(List<IPickerItem> newItems)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                _allItems = newItems;
                FilterItems(_searchText);
                ScrollToCurrentSelection();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshItems 异常: {ex}");
            }
        });
    }

    protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(propertyName);
    }
}
