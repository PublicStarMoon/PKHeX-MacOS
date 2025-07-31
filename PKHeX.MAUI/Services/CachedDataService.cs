using PKHeX.Core;
using PKHeX.MAUI.Views;
using PKHeX.MAUI.Models;
using System.Collections.Concurrent;

namespace PKHeX.MAUI.Services;

/// <summary>
/// Simplified cached data service - loads data on demand only
/// </summary>
public static class CachedDataService
{
    private static readonly ConcurrentDictionary<string, List<IPickerItem>> _cache = new();
    private static readonly object _lockObject = new();

    /// <summary>
    /// Get moves list - loads on first access
    /// </summary>
    public static List<IPickerItem> GetMoves()
    {
        return _cache.GetOrAdd("moves", key =>
        {
            var items = new List<IPickerItem>();
            try
            {
                var chineseMoves = GameInfo.GetStrings("zh")?.movelist;
                var englishMoves = GameInfo.GetStrings("en")?.movelist;
                
                if (chineseMoves == null || englishMoves == null)
                {
                    // Fallback to basic move list
                    items.Add(new MoveItem { Id = 0, DisplayName = "无招式" });
                    for (int i = 1; i <= 100; i++)
                    {
                        items.Add(new MoveItem { Id = i, DisplayName = $"招式 {i}" });
                    }
                    return items;
                }
                
                items.Add(new MoveItem { Id = 0, DisplayName = "无招式" });
                
                // Load ALL moves with Chinese names as primary
                for (int i = 1; i < chineseMoves.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(chineseMoves[i]))
                    {
                        var englishName = i < englishMoves.Length ? englishMoves[i] : "";
                        var displayName = string.IsNullOrEmpty(englishName) || chineseMoves[i] == englishName 
                            ? chineseMoves[i] 
                            : $"{chineseMoves[i]} ({englishName})";
                        items.Add(new MoveItem { Id = i, DisplayName = displayName });
                    }
                }
            }
            catch 
            { 
                // Fallback to basic move list
                items.Clear();
                items.Add(new MoveItem { Id = 0, DisplayName = "无招式" });
                for (int i = 1; i <= 100; i++)
                {
                    items.Add(new MoveItem { Id = i, DisplayName = $"招式 {i}" });
                }
            }
            return items;
        });
    }

    /// <summary>
    /// Get abilities list - loads on first access
    /// </summary>
    public static List<IPickerItem> GetAbilities()
    {
        return _cache.GetOrAdd("abilities", key =>
        {
            var items = new List<IPickerItem>();
            try
            {
                var chineseAbilities = GameInfo.GetStrings("zh")?.abilitylist;
                var englishAbilities = GameInfo.GetStrings("en")?.abilitylist;
                
                if (chineseAbilities == null || englishAbilities == null)
                {
                    // Fallback to basic ability list
                    items.Add(new AbilityItem { Id = 0, DisplayName = "无特性" });
                    for (int i = 1; i <= 50; i++)
                    {
                        items.Add(new AbilityItem { Id = i, DisplayName = $"特性 {i}" });
                    }
                    return items;
                }
                
                items.Add(new AbilityItem { Id = 0, DisplayName = "无特性" });
                
                // Load ALL abilities with Chinese names as primary
                for (int i = 1; i < chineseAbilities.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(chineseAbilities[i]))
                    {
                        var englishName = i < englishAbilities.Length ? englishAbilities[i] : "";
                        var displayName = string.IsNullOrEmpty(englishName) || chineseAbilities[i] == englishName 
                            ? chineseAbilities[i] 
                            : $"{chineseAbilities[i]} ({englishName})";
                        items.Add(new AbilityItem { Id = i, DisplayName = displayName });
                    }
                }
            }
            catch 
            { 
                // Fallback to basic ability list
                items.Clear();
                items.Add(new AbilityItem { Id = 0, DisplayName = "无特性" });
                for (int i = 1; i <= 50; i++)
                {
                    items.Add(new AbilityItem { Id = i, DisplayName = $"特性 {i}" });
                }
            }
            return items;
        });
    }

    /// <summary>
    /// Get items list - loads on first access
    /// </summary>
    public static List<IPickerItem> GetItems()
    {
        return _cache.GetOrAdd("items", key =>
        {
            var items = new List<IPickerItem>();
            try
            {
                var chineseItems = GameInfo.GetStrings("zh")?.itemlist;
                var englishItems = GameInfo.GetStrings("en")?.itemlist;
                
                if (chineseItems == null || englishItems == null)
                {
                    // Return fallback item list
                    items.Add(new ItemItem { Id = 0, DisplayName = "无道具" });
                    for (int i = 1; i <= 100; i++)
                    {
                        items.Add(new ItemItem { Id = i, DisplayName = $"道具 {i}" });
                    }
                    return items;
                }
                
                items.Add(new ItemItem { Id = 0, DisplayName = "无道具" });
                
                // Load ALL items with Chinese names as primary
                for (int i = 1; i < chineseItems.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(chineseItems[i]))
                    {
                        var englishName = i < englishItems.Length ? englishItems[i] : "";
                        var displayName = string.IsNullOrEmpty(englishName) || chineseItems[i] == englishName 
                            ? chineseItems[i] 
                            : $"{chineseItems[i]} ({englishName})";
                        items.Add(new ItemItem { Id = i, DisplayName = displayName });
                    }
                }
            }
            catch 
            {
                // Return a minimal safe list if loading fails
                items.Clear();
                items.Add(new ItemItem { Id = 0, DisplayName = "无道具" });
                for (int i = 1; i <= 100; i++)
                {
                    items.Add(new ItemItem { Id = i, DisplayName = $"道具 {i}" });
                }
            }
            return items;
        });
    }

    /// <summary>
    /// Get species list - loads on first access
    /// </summary>
    public static List<IPickerItem> GetSpecies()
    {
        return _cache.GetOrAdd("species", key =>
        {
            var items = new List<IPickerItem>();
            try
            {
                var chineseSpecies = GameInfo.GetStrings("zh")?.specieslist;
                var englishSpecies = GameInfo.GetStrings("en")?.specieslist;
                
                if (chineseSpecies == null || englishSpecies == null)
                {
                    // Fallback to basic species list
                    items.Add(new SpeciesItem { Id = 0, DisplayName = "无宝可梦" });
                    for (int i = 1; i <= 150; i++)
                    {
                        items.Add(new SpeciesItem { Id = i, DisplayName = $"{i:000} - 宝可梦 {i}" });
                    }
                    return items;
                }
                
                items.Add(new SpeciesItem { Id = 0, DisplayName = "无宝可梦" });
                
                // Load ALL species with Chinese names as primary
                for (int i = 1; i < chineseSpecies.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(chineseSpecies[i]))
                    {
                        var englishName = i < englishSpecies.Length ? englishSpecies[i] : "";
                        var displayName = string.IsNullOrEmpty(englishName) || chineseSpecies[i] == englishName 
                            ? $"{i:000} - {chineseSpecies[i]}" 
                            : $"{i:000} - {chineseSpecies[i]} ({englishName})";
                        items.Add(new SpeciesItem { Id = i, DisplayName = displayName });
                    }
                }
            }
            catch 
            { 
                // Fallback to basic species list
                items.Clear();
                items.Add(new SpeciesItem { Id = 0, DisplayName = "无宝可梦" });
                for (int i = 1; i <= 150; i++)
                {
                    items.Add(new SpeciesItem { Id = i, DisplayName = $"{i:000} - 宝可梦 {i}" });
                }
            }
            return items;
        });
    }

    /// <summary>
    /// Get natures list - loads on first access
    /// </summary>
    public static List<IPickerItem> GetNatures()
    {
        return _cache.GetOrAdd("natures", key =>
        {
            var items = new List<IPickerItem>();
            try
            {
                var chineseNatures = GameInfo.GetStrings("zh")?.natures;
                var englishNatures = GameInfo.GetStrings("en")?.natures;
                
                if (chineseNatures == null || englishNatures == null)
                {
                    // Fallback to basic nature list
                    string[] fallbackNatures = { "勤奋", "怕寂寞", "勇敢", "固执", "顽皮", "大胆", "温和", "悠闲", "淘气", "无虑",
                                               "温顺", "慎重", "冷静", "狂妄", "开朗", "天真", "内敛", "急躁", "认真", "胆小",
                                               "孤僻", "马虎", "浮躁", "活泼", "实干" };
                    for (int i = 0; i < fallbackNatures.Length; i++)
                    {
                        items.Add(new NatureItem { Id = i, DisplayName = fallbackNatures[i] });
                    }
                    return items;
                }
                
                for (int i = 0; i < Math.Min(chineseNatures.Length, 25); i++)
                {
                    var englishName = i < englishNatures.Length ? englishNatures[i] : "";
                    var displayName = string.IsNullOrEmpty(englishName) || chineseNatures[i] == englishName 
                        ? chineseNatures[i] 
                        : $"{chineseNatures[i]} ({englishName})";
                    items.Add(new NatureItem { Id = i, DisplayName = displayName });
                }
            }
            catch 
            { 
                // Fallback to basic nature list
                string[] fallbackNatures = { "勤奋", "怕寂寞", "勇敢", "固执", "顽皮", "大胆", "温和", "悠闲", "淘气", "无虑",
                                           "温顺", "慎重", "冷静", "狂妄", "开朗", "天真", "内敛", "急躁", "认真", "胆小",
                                           "孤僻", "马虎", "浮躁", "活泼", "实干" };
                for (int i = 0; i < fallbackNatures.Length; i++)
                {
                    items.Add(new NatureItem { Id = i, DisplayName = fallbackNatures[i] });
                }
            }
            return items;
        });
    }

    /// <summary>
    /// Get balls list - small fixed list
    /// </summary>
    public static List<IPickerItem> GetBalls()
    {
        return _cache.GetOrAdd("balls", key =>
        {
            return new List<IPickerItem>
            {
                new BallItem { Id = 0, DisplayName = "无球" },
                new BallItem { Id = 1, DisplayName = "大师球 (Master Ball)" },
                new BallItem { Id = 2, DisplayName = "超级球 (Ultra Ball)" },
                new BallItem { Id = 3, DisplayName = "高级球 (Great Ball)" },
                new BallItem { Id = 4, DisplayName = "精灵球 (Poké Ball)" },
                new BallItem { Id = 5, DisplayName = "狩猎球 (Safari Ball)" },
                new BallItem { Id = 6, DisplayName = "捕网球 (Net Ball)" },
                new BallItem { Id = 7, DisplayName = "潜水球 (Dive Ball)" },
                new BallItem { Id = 8, DisplayName = "巢穴球 (Nest Ball)" },
                new BallItem { Id = 9, DisplayName = "重复球 (Repeat Ball)" },
                new BallItem { Id = 10, DisplayName = "计时球 (Timer Ball)" },
                new BallItem { Id = 11, DisplayName = "豪华球 (Luxury Ball)" },
                new BallItem { Id = 12, DisplayName = "纪念球 (Premier Ball)" }
            };
        });
    }

    /// <summary>
    /// Get forms for a specific species
    /// </summary>
    public static List<IPickerItem> GetForms(ushort species, EntityContext context)
    {
        var items = new List<IPickerItem>();
        try
        {
            var englishStrings = GameInfo.GetStrings("en");
            if (englishStrings?.forms != null && englishStrings?.types != null)
            {
                var forms = FormConverter.GetFormList(species, englishStrings.types, 
                    englishStrings.forms, GameInfo.GenderSymbolUnicode, context);
                
                for (int i = 0; i < forms.Length; i++)
                {
                    items.Add(new FormItem { Id = i, DisplayName = forms[i] });
                }
            }
            else
            {
                throw new InvalidOperationException("String data not available");
            }
        }
        catch
        {
            items.Add(new FormItem { Id = 0, DisplayName = "普通形态" });
        }
        return items;
    }

    /// <summary>
    /// Get items filtered by pouch type
    /// </summary>
    public static List<IPickerItem> GetItemsForPouch(InventoryType pouchType, SaveFile? saveFile = null)
    {
        var allItems = GetItems();
        
        if (saveFile == null)
            return allItems;
            
        // Use generation-based filtering instead of trying to call GetAllItems()
        var generation = saveFile.Generation;
        var filteredItems = new List<IPickerItem>();
        
        foreach (var item in allItems)
        {
            if (IsItemValidForPouch(item.Id, pouchType, generation))
            {
                filteredItems.Add(item);
            }
        }
        
        return filteredItems.Any() ? filteredItems : allItems;
    }
    
    /// <summary>
    /// Check if an item is valid for a specific pouch type based on game generation
    /// </summary>
    private static bool IsItemValidForPouch(int itemId, InventoryType pouchType, int generation)
    {
        switch (pouchType)
        {
            case InventoryType.Balls:
                if (itemId >= 1 && itemId <= 26) return true;
                if (itemId >= 492 && itemId <= 498) return true;
                if (itemId == 576 || itemId == 851) return true;
                if (generation >= 8 && itemId >= 1785 && itemId <= 1787) return true;
                return false;

            case InventoryType.Medicine:
                if (itemId >= 17 && itemId <= 54) return true;
                if (itemId >= 134 && itemId <= 135) return true;
                if (itemId >= 504 && itemId <= 591) return true;
                return false;

            case InventoryType.Berries:
                return itemId >= 149 && itemId <= 212;

            case InventoryType.KeyItems:
                if (generation >= 8)
                    return (itemId >= 628 && itemId <= 702) || (itemId >= 1103 && itemId <= 1279);
                return itemId >= 428 && itemId <= 484;

            case InventoryType.TMHMs:
                if (generation >= 8)
                    return (itemId >= 1130 && itemId <= 1229) || (itemId >= 328 && itemId <= 427);
                return itemId >= 328 && itemId <= 427;

            case InventoryType.Items:
            default:
                // Exclude items from other categories
                if (itemId >= 1 && itemId <= 26) return false; // Balls
                if (itemId >= 149 && itemId <= 212) return false; // Berries
                if (itemId >= 328 && itemId <= 427) return false; // TMs
                if (generation >= 8 && itemId >= 1130 && itemId <= 1229) return false; // TRs
                return true;
        }
    }

    // Remove all async methods and background initialization
    public static void StartBackgroundInitialization() { }
    public static void EnsureInitialized() { }
    public static Task<List<IPickerItem>> GetSpeciesAsync() => Task.FromResult(GetSpecies());
    public static Task<List<IPickerItem>> GetMovesAsync() => Task.FromResult(GetMoves());
    public static Task<List<IPickerItem>> GetAbilitiesAsync() => Task.FromResult(GetAbilities());
    public static Task<List<IPickerItem>> GetNaturesAsync() => Task.FromResult(GetNatures());
    public static Task<List<IPickerItem>> GetItemsAsync() => Task.FromResult(GetItems());
    public static Task<List<IPickerItem>> GetBallsAsync() => Task.FromResult(GetBalls());
}
