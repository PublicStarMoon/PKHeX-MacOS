using PKHeX.Core;

namespace PKHeX.MAUI.Utilities;

/// <summary>
/// Helper class for managing Pokemon items with Scarlet/Violet compatibility
/// Prevents invisible item issues by validating item availability
/// </summary>
public static class ItemHelper
{
    /// <summary>
    /// Items that are known to cause issues in Scarlet/Violet
    /// These items may be invisible or cause other problems
    /// </summary>
    private static readonly HashSet<int> ProblematicItemsSV = new()
    {
        // Z-Crystals (not available in SV)
        777, 778, 779, 780, 781, 782, 783, 784, 785, 786, 787, 788, 789, 790, 791, 792, 793, 794, 795, 796, 797, 798, 799, 800, 801, 802, 803, 804, 805, 806, 807, 808, 809, 810, 811, 812, 813, 814, 815, 816, 817, 818, 819, 820, 821, 822, 823, 824, 825, 826, 827, 828, 829, 830, 831, 832, 833, 834, 835,
        
        // Mega Stones (not available in SV)
        658, 659, 660, 661, 662, 663, 664, 665, 666, 667, 668, 669, 670, 671, 672, 673, 674, 675, 676, 677, 678, 679, 680, 681, 682, 683, 684, 685, 686, 687, 688, 689, 690, 691, 692, 693, 694, 695, 696, 697, 698, 699, 700, 701, 702, 703, 704, 705, 706, 707, 708, 709, 710, 711, 712, 713, 714, 715, 716, 717, 718, 719, 720, 721, 722, 723, 724, 725, 726, 727, 728, 729, 730, 731, 732, 733, 734, 735, 736, 737, 738, 739, 740, 741, 742, 743, 744, 745, 746, 747, 748, 749, 750, 751, 752, 753, 754, 755, 756, 757, 758, 759, 760, 761, 762, 763, 764, 765, 766, 767, 768, 769, 770, 771, 772, 773, 774, 775, 776,
        
        // Other legacy items that may cause issues
        // This list would be expanded based on testing
    };

    /// <summary>
    /// Validates if an item is safe to use in the target save file
    /// </summary>
    public static bool IsItemSafe(int itemId, SaveFile save)
    {
        // No item is always safe
        if (itemId == 0) return true;
        
        // Check basic bounds
        if (itemId < 0 || itemId > save.MaxItemID) return false;
        
        // Scarlet/Violet specific checks
        if (save.Generation >= 9)
        {
            return IsItemSafeForSV(itemId);
        }
        
        return true;
    }

    /// <summary>
    /// Checks if an item is safe for Scarlet/Violet specifically
    /// </summary>
    private static bool IsItemSafeForSV(int itemId)
    {
        // Check against known problematic items
        if (ProblematicItemsSV.Contains(itemId)) return false;
        
        // Items beyond reasonable SV range
        if (itemId > 2400) return false; // Approximate SV upper limit
        
        return true;
    }

    /// <summary>
    /// Safely applies a held item to a Pokemon, with validation
    /// </summary>
    public static bool SafelyApplyHeldItem(PKM pokemon, SaveFile save, int itemId)
    {
        // Validate the item first
        if (!IsItemSafe(itemId, save))
        {
            pokemon.HeldItem = 0; // Clear invalid item
            return false;
        }
        
        // Convert item between formats if needed
        var convertedItem = ItemConverter.GetItemForFormat(itemId, pokemon.Context, save.Context);
        
        // Final validation on converted item
        if (convertedItem <= 0 || convertedItem > save.MaxItemID)
        {
            pokemon.HeldItem = 0;
            return false;
        }
        
        pokemon.HeldItem = convertedItem;
        return true;
    }

    /// <summary>
    /// Gets a list of safe held items for a specific save file
    /// </summary>
    public static List<(int Id, string Name)> GetSafeHeldItems(SaveFile save)
    {
        var items = new List<(int Id, string Name)>();
        var itemNames = GameInfo.GetStrings(save.Language).itemlist;
        
        // Add "None" option
        items.Add((0, "None"));
        
        // Common safe items that exist in most games
        var commonSafeItems = new[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, // Basic items
            45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, // Berries
            169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, // More berries
            229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, // Type-enhancing items
        };

        foreach (var itemId in commonSafeItems)
        {
            if (IsItemSafe(itemId, save) && itemId < itemNames.Length)
            {
                var name = itemNames[itemId];
                if (!string.IsNullOrEmpty(name) && name != "???")
                {
                    items.Add((itemId, name));
                }
            }
        }
        
        return items.OrderBy(x => x.Name).ToList();
    }

    /// <summary>
    /// Validates all Pokemon in a save file for item safety
    /// </summary>
    public static List<(int Box, int Slot, string PokemonName, string Issue)> ValidateAllPokemonItems(SaveFile save)
    {
        var issues = new List<(int Box, int Slot, string PokemonName, string Issue)>();
        
        for (int box = 0; box < save.BoxCount; box++)
        {
            for (int slot = 0; slot < save.BoxSlotCount; slot++)
            {
                var pokemon = save.GetBoxSlotAtIndex(box, slot);
                if (pokemon?.Species > 0)
                {
                    if (pokemon.HeldItem > 0 && !IsItemSafe(pokemon.HeldItem, save))
                    {
                        var speciesName = GameInfo.GetStrings(save.Language).specieslist[pokemon.Species];
                        var itemName = pokemon.HeldItem < GameInfo.GetStrings(save.Language).itemlist.Length 
                            ? GameInfo.GetStrings(save.Language).itemlist[pokemon.HeldItem] 
                            : $"Item #{pokemon.HeldItem}";
                        
                        issues.Add((box, slot, speciesName, $"Unsafe item: {itemName}"));
                    }
                }
            }
        }
        
        return issues;
    }

    /// <summary>
    /// Fixes all unsafe items in a save file
    /// </summary>
    public static int FixAllUnsafeItems(SaveFile save)
    {
        int fixedCount = 0;
        
        for (int box = 0; box < save.BoxCount; box++)
        {
            for (int slot = 0; slot < save.BoxSlotCount; slot++)
            {
                var pokemon = save.GetBoxSlotAtIndex(box, slot);
                if (pokemon?.Species > 0 && pokemon.HeldItem > 0)
                {
                    if (!IsItemSafe(pokemon.HeldItem, save))
                    {
                        pokemon.HeldItem = 0; // Remove unsafe item
                        save.SetBoxSlotAtIndex(pokemon, box, slot);
                        fixedCount++;
                    }
                }
            }
        }
        
        return fixedCount;
    }

    /// <summary>
    /// Gets item information safely
    /// </summary>
    public static string GetItemName(int itemId, SaveFile save)
    {
        try
        {
            if (itemId == 0) return "None";
            
            var itemNames = GameInfo.GetStrings(save.Language).itemlist;
            if (itemId < itemNames.Length)
            {
                var name = itemNames[itemId];
                return string.IsNullOrEmpty(name) || name == "???" ? $"Item #{itemId}" : name;
            }
            
            return $"Item #{itemId}";
        }
        catch
        {
            return $"Item #{itemId}";
        }
    }

    /// <summary>
    /// Converts an item safely between different game contexts
    /// </summary>
    public static int ConvertItemSafely(int itemId, EntityContext fromContext, EntityContext toContext, SaveFile targetSave)
    {
        try
        {
            var convertedItem = ItemConverter.GetItemForFormat(itemId, fromContext, toContext);
            
            if (IsItemSafe(convertedItem, targetSave))
            {
                return convertedItem;
            }
            
            return 0; // Return no item if conversion results in unsafe item
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets a suggested replacement for an unsafe item
    /// </summary>
    public static int GetSafeItemReplacement(int unsafeItemId, SaveFile save)
    {
        // Try to find a similar safe item
        // This is a basic implementation - could be more sophisticated
        
        var safeItems = GetSafeHeldItems(save);
        
        // For now, just return a basic safe item
        // Could be enhanced to suggest similar items based on type/effect
        if (safeItems.Count > 1) // Skip "None" option
        {
            return safeItems[1].Id; // Return first safe item
        }
        
        return 0; // No item
    }
}
