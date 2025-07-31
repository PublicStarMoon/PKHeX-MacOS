using PKHeX.MAUI.Views.Components;

namespace PKHeX.MAUI.Views.Base;

/// <summary>
/// Base content page with built-in loading spinner support
/// Provides consistent loading UI across all pages that need data initialization
/// </summary>
public abstract class LoadingContentPage : ContentPage
{
    protected LoadingSpinner? LoadingSpinner { get; private set; }
    
    protected LoadingContentPage()
    {
        // The loading spinner will be added to the page's main content in derived classes
    }

    /// <summary>
    /// Initialize the loading spinner overlay for this page
    /// Call this in the derived page's constructor after InitializeComponent()
    /// </summary>
    protected void InitializeLoadingSpinner()
    {
        // Create loading spinner if not already created
        if (LoadingSpinner == null)
        {
            LoadingSpinner = new LoadingSpinner();
            
            // Add spinner as overlay to existing content
            if (Content is Grid mainGrid)
            {
                // If main content is already a Grid, add spinner to it
                mainGrid.Children.Add(LoadingSpinner);
                Grid.SetRowSpan(LoadingSpinner, Math.Max(1, mainGrid.RowDefinitions.Count));
                Grid.SetColumnSpan(LoadingSpinner, Math.Max(1, mainGrid.ColumnDefinitions.Count));
            }
            else if (Content != null)
            {
                // Wrap existing content in a Grid and add spinner
                var originalContent = Content;
                var gridWrapper = new Grid();
                
                gridWrapper.Children.Add(originalContent);
                gridWrapper.Children.Add(LoadingSpinner);
                
                Content = gridWrapper;
            }
        }
    }

    /// <summary>
    /// Show loading spinner with optional custom text
    /// </summary>
    protected void ShowLoading(string text = "刷新加载中...")
    {
        LoadingSpinner?.Show(text);
    }

    /// <summary>
    /// Hide loading spinner
    /// </summary>
    protected void HideLoading()
    {
        LoadingSpinner?.Hide();
    }

    /// <summary>
    /// Execute an async operation with loading spinner
    /// </summary>
    protected async Task ExecuteWithLoadingAsync(Func<Task> operation, string loadingText = "刷新加载中...")
    {
        try
        {
            ShowLoading(loadingText);
            await operation();
        }
        finally
        {
            HideLoading();
        }
    }

    /// <summary>
    /// Execute an async operation with loading spinner and return result
    /// </summary>
    protected async Task<T> ExecuteWithLoadingAsync<T>(Func<Task<T>> operation, string loadingText = "刷新加载中...")
    {
        try
        {
            ShowLoading(loadingText);
            return await operation();
        }
        finally
        {
            HideLoading();
        }
    }
}
