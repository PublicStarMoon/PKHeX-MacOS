using PKHeX.MAUI.Views;
using PKHeX.Core;
using System.Collections.Generic;

namespace PKHeX.MAUI.Services;

/// <summary>
/// Manages page instances to maintain state across navigation
/// </summary>
public static class PageManager
{
    private static readonly Dictionary<string, ContentPage> _pageCache = new();
    private static SaveFile? _currentSaveFile;
    private static bool _hasUnsavedChanges = false;

    /// <summary>
    /// Get or create an InventoryEditorPage instance
    /// </summary>
    public static InventoryEditorPage GetInventoryEditorPage(SaveFile saveFile)
    {
        const string key = "InventoryEditor";
        
        // If save file changed, clear cache to reload data
        if (_currentSaveFile != saveFile)
        {
            ClearCache();
            _currentSaveFile = saveFile;
            _hasUnsavedChanges = false;
        }
        
        if (!_pageCache.ContainsKey(key))
        {
            _pageCache[key] = new InventoryEditorPage(saveFile);
        }
        
        return (InventoryEditorPage)_pageCache[key];
    }

    /// <summary>
    /// Get or create a PartyEditorPage instance
    /// </summary>
    public static PartyEditorPage GetPartyEditorPage(SaveFile saveFile)
    {
        const string key = "PartyEditor";
        
        // If save file changed, clear cache to reload data
        if (_currentSaveFile != saveFile)
        {
            ClearCache();
            _currentSaveFile = saveFile;
            _hasUnsavedChanges = false;
        }
        
        if (!_pageCache.ContainsKey(key))
        {
            _pageCache[key] = new PartyEditorPage(saveFile);
        }
        
        return (PartyEditorPage)_pageCache[key];
    }

    /// <summary>
    /// Get or create a PokemonBoxPage instance
    /// </summary>
    public static PokemonBoxPage GetPokemonBoxPage(SaveFile saveFile)
    {
        const string key = "PokemonBox";
        
        // If save file changed, clear cache to reload data
        if (_currentSaveFile != saveFile)
        {
            ClearCache();
            _currentSaveFile = saveFile;
            _hasUnsavedChanges = false;
        }
        
        if (!_pageCache.ContainsKey(key))
        {
            _pageCache[key] = new PokemonBoxPage(saveFile);
        }
        
        return (PokemonBoxPage)_pageCache[key];
    }

    /// <summary>
    /// Mark that changes have been made in editors (called by editor save methods)
    /// </summary>
    public static void MarkChangesUnsaved()
    {
        _hasUnsavedChanges = true;
    }

    /// <summary>
    /// Mark changes as saved to disk (called after successful export)
    /// </summary>
    public static void MarkChangesSaved()
    {
        _hasUnsavedChanges = false;
    }

    /// <summary>
    /// Check if there are unsaved changes
    /// </summary>
    public static bool HasUnsavedChanges => _hasUnsavedChanges;

    /// <summary>
    /// Clear all cached pages (when loading a new save file)
    /// </summary>
    public static void ClearCache()
    {
        _pageCache.Clear();
        _hasUnsavedChanges = false;
    }

    /// <summary>
    /// Force refresh a specific page (recreate it)
    /// </summary>
    public static void RefreshPage(string pageKey)
    {
        if (_pageCache.ContainsKey(pageKey))
        {
            _pageCache.Remove(pageKey);
        }
    }

    /// <summary>
    /// Force refresh all cached pages
    /// </summary>
    public static void RefreshAllPages()
    {
        ClearCache();
    }

    /// <summary>
    /// Force refresh all pages after save file is reloaded from disk
    /// This is needed when user exports save, then reimports it, or wants to discard changes
    /// </summary>
    public static void RefreshAfterSaveReload()
    {
        ClearCache();
        _hasUnsavedChanges = false;
    }

    /// <summary>
    /// Get current save file reference (for comparison)
    /// </summary>
    public static SaveFile? CurrentSaveFile => _currentSaveFile;
}
