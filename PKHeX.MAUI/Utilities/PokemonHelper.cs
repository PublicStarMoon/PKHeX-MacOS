using PKHeX.Core;

namespace PKHeX.MAUI.Utilities;

public static class PokemonHelper
{
    /// <summary>
    /// Creates a legal Pokemon with strict trainer info compliance for Scarlet/Violet
    /// This prevents "不听话" (disobedience) issues by ensuring proper trainer data
    /// </summary>
    public static PKM CreateLegalPokemon(SaveFile save, int species, int level = 5)
    {
        var pk = save.BlankPKM;
        pk.Species = (ushort)species;
        pk.CurrentLevel = level;
        
        // CRITICAL: Set trainer info correctly to prevent disobedience
        ApplyCorrectTrainerInfo(pk, save);
        
        // Set basic legal values
        // For now, skip move suggestion as API is internal - moves will be set elsewhere
        // var la = new LegalityAnalysis(pk);
        // var info = new BatchInfo(pk, la);
        // PKHeX.Core.BatchModifications.SetSuggestedMoveset(info);
        pk.SetRandomIVs();
        pk.Heal();
        
        // Ensure proper legality for modern games
        if (save.Generation >= 9) // Scarlet/Violet and later
        {
            ApplyScarletVioletCompliance(pk, save);
        }
        
        return pk;
    }

    /// <summary>
    /// Applies correct trainer information to prevent disobedience in Scarlet/Violet
    /// This is crucial for proper Pokemon recognition by the game
    /// </summary>
    public static void ApplyCorrectTrainerInfo(PKM pokemon, SaveFile save)
    {
        // Core trainer info - MUST match save file exactly
        pokemon.OT_Name = save.OT;
        pokemon.TID16 = save.TID16;
        pokemon.SID16 = save.SID16;
        pokemon.Language = save.Language;
        pokemon.OT_Gender = save.Gender;
        
        // Set current handler to Original Trainer (critical for SV)
        pokemon.CurrentHandler = 0;
        
        // Clear any previous handler data to prevent conflicts
        pokemon.HT_Name = "";
        pokemon.HT_Gender = 0;
        
        // Set proper version for compatibility
        if (save.Version > 0)
        {
            pokemon.Version = (int)save.Version;
        }
        
        // Apply region/country info for Gen 6+ saves
        if (save is IRegionOrigin region && pokemon is IRegionOrigin pkRegion)
        {
            pkRegion.Country = region.Country;
            pkRegion.Region = region.Region;
            pkRegion.ConsoleRegion = region.ConsoleRegion;
        }
        
        // Note: Context is read-only and determined by the PKM type
        // Cannot modify context after creation
    }

    /// <summary>
    /// Applies Scarlet/Violet specific compliance to prevent issues
    /// </summary>
    private static void ApplyScarletVioletCompliance(PKM pokemon, SaveFile save)
    {
        // Ensure proper Tera Type for SV
        if (pokemon is ITeraType tera)
        {
            var types = pokemon.PersonalInfo;
            tera.SetTeraType((MoveType)types.Type1); // Default to primary type
        }
        
        // Clear any incompatible data
        if (pokemon is PK9 pk9)
        {
            // Reset any problematic fields that could cause disobedience
            pk9.Met_Location = save.Context == EntityContext.Gen9 ? 6 : pk9.Met_Location; // Paldea if SV
            pk9.Met_Level = Math.Min(pokemon.CurrentLevel, pk9.Met_Level);
            
            // Ensure proper ball for legality
            if (pk9.Ball == 0)
                pk9.Ball = 4; // Poke Ball default
        }
    }

    /// <summary>
    /// Creates a competitive Pokemon with perfect stats and proper trainer info
    /// </summary>
    public static PKM CreateCompetitivePokemon(SaveFile save, int species, int level = 50, Nature nature = Nature.Hardy)
    {
        var pk = CreateLegalPokemon(save, species, level);
        
        // Perfect IVs
        pk.IV_HP = 31;
        pk.IV_ATK = 31;
        pk.IV_DEF = 31;
        pk.IV_SPA = 31;
        pk.IV_SPD = 31;
        pk.IV_SPE = 31;
        
        // Set nature
        pk.Nature = (int)nature;
        
        // Refresh stats and ensure legality
        pk.Heal();
        
        return pk;
    }

    /// <summary>
    /// Validates Pokemon trainer info to prevent disobedience
    /// </summary>
    public static bool ValidateTrainerInfo(PKM pokemon, SaveFile save)
    {
        // Critical checks for trainer compliance
        if (pokemon.OT_Name != save.OT) return false;
        if (pokemon.TID16 != save.TID16) return false;
        if (pokemon.SID16 != save.SID16) return false;
        if (pokemon.Language != save.Language) return false;
        if (pokemon.OT_Gender != save.Gender) return false;
        if (pokemon.CurrentHandler != 0) return false; // Must be OT for no disobedience
        
        return true;
    }

    /// <summary>
    /// Applies a held item safely, ensuring it exists in the target game
    /// This prevents invisible item issues in Scarlet/Violet
    /// </summary>
    public static bool ApplySafeHeldItem(PKM pokemon, SaveFile save, int itemId)
    {
        return ItemHelper.SafelyApplyHeldItem(pokemon, save, itemId);
    }

    /// <summary>
    /// Gets the Pokemon's type names safely
    /// </summary>
    public static (string Type1, string Type2) GetPokemonTypes(int species)
    {
        try
        {
            var pi = PersonalTable.SV.GetFormEntry((ushort)species, 0);
            var types = GameInfo.GetStrings(1).types;
            
            var type1 = (int)pi.Type1 < types.Length ? types[pi.Type1] : "Unknown";
            var type2 = pi.Type1 != pi.Type2 && (int)pi.Type2 < types.Length ? types[pi.Type2] : "";
            
            return (type1, type2);
        }
        catch
        {
            return ("Unknown", "");
        }
    }

    /// <summary>
    /// Gets base stats for a Pokemon species
    /// </summary>
    public static (int HP, int ATK, int DEF, int SPA, int SPD, int SPE) GetBaseStats(int species)
    {
        try
        {
            var pi = PersonalTable.SV.GetFormEntry((ushort)species, 0);
            return (pi.HP, pi.ATK, pi.DEF, pi.SPA, pi.SPD, pi.SPE);
        }
        catch
        {
            return (0, 0, 0, 0, 0, 0);
        }
    }

    /// <summary>
    /// Validates if a Pokemon is legal with strict Scarlet/Violet standards
    /// </summary>
    public static bool IsLegal(PKM pokemon, SaveFile save)
    {
        try
        {
            var analysis = new LegalityAnalysis(pokemon);
            
            // Basic legality check
            if (!analysis.Valid) return false;
            
            // Additional trainer info validation for SV
            if (save.Generation >= 9)
            {
                if (!ValidateTrainerInfo(pokemon, save)) return false;
                
                // Check for unsafe items that could cause visibility issues
                if (pokemon.HeldItem > 0 && !ItemHelper.IsItemSafe(pokemon.HeldItem, save))
                {
                    return false;
                }
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a comprehensive legality summary
    /// </summary>
    public static string GetLegalitySummary(PKM pokemon, SaveFile save)
    {
        try
        {
            var analysis = new LegalityAnalysis(pokemon);
            var issues = new List<string>();
            
            if (!analysis.Valid)
            {
                var legalityIssues = analysis.Results.Where(r => !r.Valid).Select(r => r.Comment).ToList();
                issues.AddRange(legalityIssues.Take(2)); // Limit to avoid overwhelming
            }
            
            // Check trainer info for SV
            if (save.Generation >= 9 && !ValidateTrainerInfo(pokemon, save))
            {
                issues.Add("Trainer info mismatch - may cause disobedience");
            }
            
            // Check item safety
            if (pokemon.HeldItem > 0 && !ItemHelper.IsItemSafe(pokemon.HeldItem, save))
            {
                var itemName = ItemHelper.GetItemName(pokemon.HeldItem, save);
                issues.Add($"Unsafe item: {itemName} - may be invisible");
            }
            
            if (issues.Count == 0)
            {
                return "✅ Legal and safe for current game";
            }
            
            return "❌ Issues: " + string.Join(", ", issues);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Fixes trainer info to prevent disobedience
    /// </summary>
    public static void FixTrainerInfo(PKM pokemon, SaveFile save)
    {
        ApplyCorrectTrainerInfo(pokemon, save);
        
        // Apply any additional fixes based on game generation
        if (save.Generation >= 9)
        {
            ApplyScarletVioletCompliance(pokemon, save);
            
            // Fix any unsafe items
            if (pokemon.HeldItem > 0 && !ItemHelper.IsItemSafe(pokemon.HeldItem, save))
            {
                pokemon.HeldItem = 0; // Remove unsafe item
            }
        }
        
        // Refresh Pokemon data
        pokemon.Heal();
        pokemon.RefreshChecksum();
    }

    /// <summary>
    /// Gets the generation range for Pokemon species
    /// </summary>
    public static (int Start, int End) GetGenerationRange(int generation)
    {
        return generation switch
        {
            1 => (1, 151),
            2 => (152, 251),
            3 => (252, 386),
            4 => (387, 493),
            5 => (494, 649),
            6 => (650, 721),
            7 => (722, 809),
            8 => (810, 905),
            9 => (906, 1010),
            _ => (1, 1010)
        };
    }

    /// <summary>
    /// Gets all Pokemon in a box safely
    /// </summary>
    public static List<PKM> GetBoxPokemon(SaveFile save, int box)
    {
        var pokemon = new List<PKM>();
        
        for (int slot = 0; slot < save.BoxSlotCount; slot++)
        {
            var pkm = save.GetBoxSlotAtIndex(box, slot);
            if (pkm != null && pkm.Species != 0)
            {
                pokemon.Add(pkm);
            }
        }
        
        return pokemon;
    }

    /// <summary>
    /// Counts Pokemon in a box
    /// </summary>
    public static int CountBoxPokemon(SaveFile save, int box)
    {
        int count = 0;
        
        for (int slot = 0; slot < save.BoxSlotCount; slot++)
        {
            var pkm = save.GetBoxSlotAtIndex(box, slot);
            if (pkm != null && pkm.Species != 0)
            {
                count++;
            }
        }
        
        return count;
    }

    /// <summary>
    /// Creates a safe blank Pokemon for clearing slots
    /// </summary>
    public static PKM CreateBlankPokemon(SaveFile save)
    {
        var blank = save.BlankPKM;
        
        // Ensure it's completely blank
        blank.Species = 0;
        blank.ClearNickname();
        blank.HeldItem = 0;
        
        return blank;
    }
}
