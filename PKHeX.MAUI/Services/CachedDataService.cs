using PKHeX.Core;
using PKHeX.MAUI.Views;
using PKHeX.MAUI.Models;
using System.Collections.Concurrent;

namespace PKHeX.MAUI.Services;

/// <summary>
/// Caches all game data in memory to prevent repeated disk I/O operations
/// This significantly improves search performance in pickers
/// Data is initialized asynchronously in the background at app startup
/// </summary>
public static class CachedDataService
{
    private static readonly ConcurrentDictionary<string, List<IPickerItem>> _cache = new();
    private static readonly object _lockObject = new();
    private static bool _isInitialized = false;
    private static bool _isInitializing = false;
    private static TaskCompletionSource<bool>? _initializationTcs;

    /// <summary>
    /// Start background initialization of all cached data at app startup
    /// </summary>
    public static void StartBackgroundInitialization()
    {
        if (_isInitialized || _isInitializing) return;
        
        lock (_lockObject)
        {
            if (_isInitialized || _isInitializing) return;
            
            _isInitializing = true;
            _initializationTcs = new TaskCompletionSource<bool>();
            
            // Start initialization in background thread to avoid blocking the UI
            Task.Run(() =>
            {
                try
                {
                    // Load all data in background thread
                    System.Diagnostics.Debug.WriteLine("Starting background initialization of cached data...");
                    
                    InitializeMoves();
                    InitializeAbilities();
                    InitializeNatures();
                    InitializeItems();
                    InitializeBalls();
                    InitializeSpecies();
                    InitializeForms();
                    
                    _isInitialized = true;
                    _isInitializing = false;
                    
                    System.Diagnostics.Debug.WriteLine("Background initialization of cached data completed successfully.");
                    _initializationTcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash the app
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize cached data: {ex.Message}");
                    _isInitialized = true; // Still mark as initialized to prevent infinite retries
                    _isInitializing = false;
                    _initializationTcs.SetResult(false);
                }
            });
        }
    }

    /// <summary>
    /// Wait for background initialization to complete (if still in progress)
    /// This method will NOT block the UI thread - it returns immediately if data is not ready
    /// </summary>
    private static async Task<bool> EnsureInitializedAsync()
    {
        if (_isInitialized) return true;
        
        if (_isInitializing && _initializationTcs != null)
        {
            // Wait for background initialization to complete, but with a timeout to avoid blocking
            try
            {
                return await _initializationTcs.Task.WaitAsync(TimeSpan.FromMilliseconds(100));
            }
            catch (TimeoutException)
            {
                // Timeout - return false to indicate data is not ready yet
                return false;
            }
        }
        
        // Fallback: if background initialization hasn't started yet, start it now but don't wait
        lock (_lockObject)
        {
            if (_isInitialized) return true;
            
            if (!_isInitializing)
            {
                // Start background initialization if not already started
                StartBackgroundInitialization();
            }
        }
        
        // Don't block - return false to indicate data is not ready yet
        // UI components should show loading states and retry later
        return false;
    }
    
    /// <summary>
    /// Initialize all cached data once at startup (legacy method for backward compatibility)
    /// </summary>
    [Obsolete("Use StartBackgroundInitialization() at app startup instead")]
    public static void Initialize()
    {
        // For backward compatibility, wait for async initialization
        EnsureInitializedAsync().Wait();
    }

    private static void InitializeMoves()
    {
        var moveItems = new List<IPickerItem>();
        var englishMoves = GameInfo.GetStrings("en").movelist;
        var chineseMoves = GameInfo.GetStrings("zh2").movelist ?? GameInfo.GetStrings("zh").movelist;
        
        moveItems.Add(new MoveItem { Id = 0, DisplayName = "None" });
        for (int i = 1; i < englishMoves.Length && i < 1000; i++)
        {
            var englishName = englishMoves[i];
            var chineseName = chineseMoves != null && i < chineseMoves.Length ? chineseMoves[i] : "";
            
            var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                ? $"{englishName} ({chineseName})" 
                : englishName;
                
            moveItems.Add(new MoveItem { Id = i, DisplayName = displayName });
        }
        
        _cache["moves"] = moveItems;
    }

    private static void InitializeAbilities()
    {
        var abilityItems = new List<IPickerItem>();
        var englishAbilities = GameInfo.GetStrings("en").abilitylist;
        var chineseAbilities = GameInfo.GetStrings("zh2").abilitylist ?? GameInfo.GetStrings("zh").abilitylist;
        
        abilityItems.Add(new AbilityItem { Id = 0, DisplayName = "None" });
        for (int i = 1; i < englishAbilities.Length && i < 300; i++)
        {
            var englishName = englishAbilities[i];
            var chineseName = chineseAbilities != null && i < chineseAbilities.Length ? chineseAbilities[i] : "";
            
            var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                ? $"{englishName} ({chineseName})" 
                : englishName;
                
            abilityItems.Add(new AbilityItem { Id = i, DisplayName = displayName });
        }
        
        _cache["abilities"] = abilityItems;
    }

    private static void InitializeNatures()
    {
        var natureItems = new List<IPickerItem>();
        var englishNatures = GameInfo.GetStrings("en").natures;
        var chineseNatures = GameInfo.GetStrings("zh2").natures ?? GameInfo.GetStrings("zh").natures;
        
        for (int i = 0; i < englishNatures.Length && i < 25; i++)
        {
            var englishName = englishNatures[i];
            var chineseName = chineseNatures != null && i < chineseNatures.Length ? chineseNatures[i] : "";
            
            var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                ? $"{englishName} ({chineseName})" 
                : englishName;
                
            natureItems.Add(new NatureItem { Id = i, DisplayName = displayName });
        }
        
        _cache["natures"] = natureItems;
    }

    private static void InitializeItems()
    {
        var itemItems = new List<IPickerItem>();
        var englishItems = GameInfo.GetStrings("en").itemlist;
        var chineseItems = GameInfo.GetStrings("zh2").itemlist ?? GameInfo.GetStrings("zh").itemlist;
        
        itemItems.Add(new ItemItem { Id = 0, DisplayName = "None" });
        for (int i = 1; i < englishItems.Length && i < 2000; i++)
        {
            var englishName = englishItems[i];
            var chineseName = chineseItems != null && i < chineseItems.Length ? chineseItems[i] : "";
            
            var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                ? $"{englishName} ({chineseName})" 
                : englishName;
                
            itemItems.Add(new ItemItem { Id = i, DisplayName = displayName });
        }
        
        _cache["items"] = itemItems;
    }

    private static void InitializeBalls()
    {
        var ballItems = new List<IPickerItem>();
        
        // Basic ball list with Chinese names
        ballItems.Add(new BallItem { Id = 0, DisplayName = "None" });
        ballItems.Add(new BallItem { Id = 1, DisplayName = "Master Ball (大师球)" });
        ballItems.Add(new BallItem { Id = 2, DisplayName = "Ultra Ball (高级球)" });
        ballItems.Add(new BallItem { Id = 3, DisplayName = "Great Ball (超级球)" });
        ballItems.Add(new BallItem { Id = 4, DisplayName = "Poké Ball (精灵球)" });
        ballItems.Add(new BallItem { Id = 5, DisplayName = "Safari Ball (狩猎球)" });
        ballItems.Add(new BallItem { Id = 6, DisplayName = "Net Ball (捕网球)" });
        ballItems.Add(new BallItem { Id = 7, DisplayName = "Dive Ball (潜水球)" });
        ballItems.Add(new BallItem { Id = 8, DisplayName = "Nest Ball (巢穴球)" });
        ballItems.Add(new BallItem { Id = 9, DisplayName = "Repeat Ball (重复球)" });
        ballItems.Add(new BallItem { Id = 10, DisplayName = "Timer Ball (计时球)" });
        ballItems.Add(new BallItem { Id = 11, DisplayName = "Luxury Ball (豪华球)" });
        ballItems.Add(new BallItem { Id = 12, DisplayName = "Premier Ball (纪念球)" });
        
        _cache["balls"] = ballItems;
    }

    private static void InitializeSpecies()
    {
        var speciesItems = new List<IPickerItem>();
        var englishSpecies = GameInfo.GetStrings("en").specieslist;
        var chineseSpecies = GameInfo.GetStrings("zh2").specieslist ?? GameInfo.GetStrings("zh").specieslist;
        
        speciesItems.Add(new SpeciesItem { Id = 0, DisplayName = "None" });
        for (int i = 1; i < englishSpecies.Length && i < 1011; i++) // Up to current max species
        {
            var englishName = englishSpecies[i];
            var chineseName = chineseSpecies != null && i < chineseSpecies.Length ? chineseSpecies[i] : "";
            
            var displayName = !string.IsNullOrEmpty(chineseName) && chineseName != englishName 
                ? $"{englishName} ({chineseName})" 
                : englishName;
                
            speciesItems.Add(new SpeciesItem { Id = i, DisplayName = $"{i:000} - {displayName}" });
        }
        
        _cache["species"] = speciesItems;
    }

    private static void InitializeForms()
    {
        // Forms are pokemon-specific, so we'll just create an empty cache
        // Individual pokemon editors will populate this as needed
        _cache["forms"] = new List<IPickerItem>();
    }

    /// <summary>
    /// Get cached moves list - returns immediately available data or empty list if not ready
    /// For UI components: use GetMovesAsync() with loading spinner instead
    /// </summary>
    [Obsolete("Use GetMovesAsync() with loading spinner for better UI experience")]
    public static List<IPickerItem> GetMoves()
    {
        // Don't block the UI thread - return what's available immediately
        return _cache.TryGetValue("moves", out var moves) ? moves : new List<IPickerItem>();
    }

    /// <summary>
    /// Get cached abilities list - returns immediately available data or empty list if not ready
    /// For UI components: use GetAbilitiesAsync() with loading spinner instead
    /// </summary>
    [Obsolete("Use GetAbilitiesAsync() with loading spinner for better UI experience")]
    public static List<IPickerItem> GetAbilities()
    {
        // Don't block the UI thread - return what's available immediately
        return _cache.TryGetValue("abilities", out var abilities) ? abilities : new List<IPickerItem>();
    }

    /// <summary>
    /// Get cached natures list - returns immediately available data or empty list if not ready
    /// For UI components: use GetNaturesAsync() with loading spinner instead
    /// </summary>
    [Obsolete("Use GetNaturesAsync() with loading spinner for better UI experience")]
    public static List<IPickerItem> GetNatures()
    {
        // Don't block the UI thread - return what's available immediately
        return _cache.TryGetValue("natures", out var natures) ? natures : new List<IPickerItem>();
    }

    /// <summary>
    /// Get cached items list - returns immediately available data or empty list if not ready
    /// For UI components: use GetItemsAsync() with loading spinner instead
    /// </summary>
    [Obsolete("Use GetItemsAsync() with loading spinner for better UI experience")]
    public static List<IPickerItem> GetItems()
    {
        // Don't block the UI thread - return what's available immediately
        return _cache.TryGetValue("items", out var items) ? items : new List<IPickerItem>();
    }

    /// <summary>
    /// Get cached balls list - returns immediately available data or empty list if not ready
    /// For UI components: use GetBallsAsync() with loading spinner instead
    /// </summary>
    [Obsolete("Use GetBallsAsync() with loading spinner for better UI experience")]
    public static List<IPickerItem> GetBalls()
    {
        // Don't block the UI thread - return what's available immediately
        return _cache.TryGetValue("balls", out var balls) ? balls : new List<IPickerItem>();
    }

    /// <summary>
    /// Get cached species list - returns immediately available data or empty list if not ready
    /// For UI components: use GetSpeciesAsync() with loading spinner instead
    /// </summary>
    [Obsolete("Use GetSpeciesAsync() with loading spinner for better UI experience")]
    public static List<IPickerItem> GetSpecies()
    {
        // Don't block the UI thread - return what's available immediately
        return _cache.TryGetValue("species", out var species) ? species : new List<IPickerItem>();
    }

    // Async versions for better performance in async contexts
    
    /// <summary>
    /// Get cached species list (async version) - returns immediately without blocking
    /// Shows loading spinner if data is not ready yet
    /// </summary>
    public static Task<List<IPickerItem>> GetSpeciesAsync()
    {
        // If data is ready, return it immediately
        if (_isInitialized)
        {
            return Task.FromResult(_cache.TryGetValue("species", out var species) ? species : new List<IPickerItem>());
        }
        
        // If data is not ready, start initialization if needed and return empty list immediately
        if (!_isInitializing)
        {
            StartBackgroundInitialization();
        }
        
        // Return empty list immediately - DO NOT WAIT
        return Task.FromResult(new List<IPickerItem>());
    }

    /// <summary>
    /// Get cached moves list (async version) - returns immediately without blocking
    /// </summary>
    public static Task<List<IPickerItem>> GetMovesAsync()
    {
        if (_isInitialized)
        {
            return Task.FromResult(_cache.TryGetValue("moves", out var moves) ? moves : new List<IPickerItem>());
        }
        
        if (!_isInitializing)
        {
            StartBackgroundInitialization();
        }
        
        return Task.FromResult(new List<IPickerItem>());
    }

    /// <summary>
    /// Get cached abilities list (async version) - returns immediately without blocking
    /// </summary>
    public static Task<List<IPickerItem>> GetAbilitiesAsync()
    {
        if (_isInitialized)
        {
            return Task.FromResult(_cache.TryGetValue("abilities", out var abilities) ? abilities : new List<IPickerItem>());
        }
        
        if (!_isInitializing)
        {
            StartBackgroundInitialization();
        }
        
        return Task.FromResult(new List<IPickerItem>());
    }

    /// <summary>
    /// Get cached natures list (async version) - returns immediately without blocking
    /// </summary>
    public static Task<List<IPickerItem>> GetNaturesAsync()
    {
        if (_isInitialized)
        {
            return Task.FromResult(_cache.TryGetValue("natures", out var natures) ? natures : new List<IPickerItem>());
        }
        
        if (!_isInitializing)
        {
            StartBackgroundInitialization();
        }
        
        return Task.FromResult(new List<IPickerItem>());
    }

    /// <summary>
    /// Get cached items list (async version) - returns immediately without blocking
    /// </summary>
    public static Task<List<IPickerItem>> GetItemsAsync()
    {
        if (_isInitialized)
        {
            return Task.FromResult(_cache.TryGetValue("items", out var items) ? items : new List<IPickerItem>());
        }
        
        if (!_isInitializing)
        {
            StartBackgroundInitialization();
        }
        
        return Task.FromResult(new List<IPickerItem>());
    }

    /// <summary>
    /// Get cached balls list (async version) - returns immediately without blocking
    /// </summary>
    public static Task<List<IPickerItem>> GetBallsAsync()
    {
        if (_isInitialized)
        {
            return Task.FromResult(_cache.TryGetValue("balls", out var balls) ? balls : new List<IPickerItem>());
        }
        
        if (!_isInitializing)
        {
            StartBackgroundInitialization();
        }
        
        return Task.FromResult(new List<IPickerItem>());
    }

    /// <summary>
    /// Check if data initialization is complete
    /// </summary>
    public static bool IsInitialized => _isInitialized;

    /// <summary>
    /// Check if data initialization is in progress
    /// </summary> 
    public static bool IsInitializing => _isInitializing;

    /// <summary>
    /// Get forms for a specific pokemon species
    /// </summary>
    public static List<IPickerItem> GetForms(ushort species, EntityContext context)
    {
        var formItems = new List<IPickerItem>();
        
        try
        {
            var englishStrings = GameInfo.GetStrings("en");
            var formList = PKHeX.Core.FormConverter.GetFormList(species, englishStrings.types, englishStrings.forms, GameInfo.GenderSymbolUnicode, context);
            
            for (int i = 0; i < formList.Length; i++)
            {
                formItems.Add(new FormItem { Id = i, DisplayName = formList[i] });
            }
        }
        catch
        {
            // Fallback to basic form
            formItems.Add(new FormItem { Id = 0, DisplayName = "Normal Form" });
        }
        
        return formItems;
    }

    /// <summary>
    /// Search items by text with fast in-memory filtering
    /// </summary>
    public static async Task<List<IPickerItem>> SearchItemsAsync(string searchText, int maxResults = 100)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            var items = await GetItemsAsync();
            return items.Take(maxResults).ToList();
        }

        var allItems = await GetItemsAsync();
        var searchLower = searchText.ToLowerInvariant();
        
        var results = allItems
            .Where(item => 
                item.DisplayName.ToLowerInvariant().Contains(searchLower) || 
                item.Id.ToString().Contains(searchText))
            .Take(maxResults)
            .ToList();
            
        return results;
    }

    /// <summary>
    /// Search items by text with fast in-memory filtering (legacy synchronous version)
    /// For better UI experience, use SearchItemsAsync instead
    /// </summary>
    [Obsolete("Use SearchItemsAsync() for better UI experience")]
    public static List<IPickerItem> SearchItems(string searchText, int maxResults = 100)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return _cache.TryGetValue("items", out var items) ? items.Take(maxResults).ToList() : new List<IPickerItem>();

        var allItems = _cache.TryGetValue("items", out var cachedItems) ? cachedItems : new List<IPickerItem>();
        var searchLower = searchText.ToLowerInvariant();
        
        var results = allItems
            .Where(item => 
                item.DisplayName.ToLowerInvariant().Contains(searchLower) || 
                item.Id.ToString().Contains(searchText))
            .Take(maxResults)
            .ToList();
            
        return results;
    }

    /// <summary>
    /// Clear all cached data (useful for language changes)
    /// </summary>
    public static void ClearCache()
    {
        lock (_lockObject)
        {
            _cache.Clear();
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Get cache status for debugging
    /// </summary>
    public static Dictionary<string, int> GetCacheStatus()
    {
        return _cache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
    }
}
