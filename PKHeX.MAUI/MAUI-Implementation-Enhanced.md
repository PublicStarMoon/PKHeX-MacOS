# PKHeX MAUI Implementation - Enhanced with Strict Validation

## Overview
This document describes the comprehensive Pokemon box editor implementation for PKHeX.MAUI, enhanced with strict validation systems to prevent Pokemon disobedience and invisible item issues in modern games, particularly Scarlet/Violet.

## Critical Features for Scarlet/Violet Compliance

### 🔒 Trainer Info Validation System
**Problem Solved**: Pokemon disobedience ("不听话") in Scarlet/Violet due to incorrect trainer information.

**Implementation**:
- `PokemonHelper.ValidateTrainerInfo()` - Validates OT_Name, TID16, SID16, Language, OT_Gender
- `PokemonHelper.ApplyCorrectTrainerInfo()` - Ensures trainer data matches save file exactly
- `PokemonHelper.FixTrainerInfo()` - Comprehensive fix for disobedience issues
- Special Scarlet/Violet compliance with proper Tera Type and context handling

### 🚫 Item Safety System
**Problem Solved**: Invisible items from Z-Crystals, Mega Stones, and other legacy items in modern games.

**Implementation**:
- `ItemHelper` class with comprehensive item validation
- `ItemHelper.IsItemSafe()` - Validates items against game compatibility
- `ItemHelper.GetSafeHeldItems()` - Returns only safe items for item pickers
- Automatic conversion and validation of items between game formats
- Proactive removal of problematic items (Z-Crystals, Mega Stones in SV)

### ✅ Comprehensive Save Validation
**New Feature**: `SaveValidationPage` for complete save file health checking.

**Capabilities**:
- Scan all Pokemon for trainer info mismatches
- Identify unsafe items that could be invisible
- Validate Pokemon legality with modern standards
- Bulk fix operations for entire save files
- Game-specific warnings and recommendations

## Project Structure

### Core Components
```
PKHeX.MAUI/
├── Views/
│   ├── MainPage.xaml/cs           # Enhanced with validation access
│   ├── PokemonBoxPage.xaml/cs     # 6x5 Pokemon grid with navigation
│   ├── PokemonEditorPage.xaml/cs  # Enhanced with item picker & validation
│   ├── PokemonDatabasePage.xaml/cs # Enhanced with legal Pokemon creation
│   └── SaveValidationPage.xaml/cs # NEW: Comprehensive validation interface
├── Utilities/
│   ├── PokemonHelper.cs           # Enhanced with strict validation
│   └── ItemHelper.cs              # NEW: Item safety and validation system
└── PKHeX.MAUI.csproj             # Updated with Drawing.Misc reference
```

## Enhanced Features Detail

### 1. MainPage Enhancements
- **New Button**: "Validate Save File" for accessing comprehensive validation
- **Visual Indicators**: Game-specific warnings for SV compatibility
- **Enhanced Save Loading**: Automatic validation on file load

### 2. PokemonBoxPage
- **Visual Safety Indicators**: Color coding for Pokemon with validation issues
- **Quick Actions**: Batch validation and fixing from box view
- **Context Menus**: Right-click validation options (when implemented)

### 3. PokemonEditorPage - Major Enhancements
- **Safe Item Picker**: Only shows items compatible with target game
- **Validation Buttons**: "Validate Legality" and "Fix Trainer Info" 
- **Real-time Warnings**: Immediate feedback on potential issues
- **Enhanced Safety**: Automatic application of proper trainer data

#### Key UI Additions:
```xml
<!-- Legality & Trainer Info Section -->
<Button Text="Validate Legality" BackgroundColor="LightBlue"/>
<Button Text="Fix Trainer Info" BackgroundColor="Orange"/>
<Label Text="⚠️ Use 'Fix Trainer Info' if Pokemon disobey in Scarlet/Violet"/>

<!-- Safe Item Picker -->
<Picker x:Name="HeldItemPicker" SelectedIndexChanged="OnHeldItemChanged"/>
```

### 4. PokemonDatabasePage
- **Legal Pokemon Creation**: All generated Pokemon use enhanced validation
- **Compatibility Checks**: Warns about potential issues before creation
- **Automatic Trainer Fixing**: SV compliance applied automatically

### 5. SaveValidationPage - New Comprehensive Tool
```xml
<!-- Quick Actions -->
<Button Text="Scan All Issues"/>    # Identifies all problems
<Button Text="Fix All Issues"/>     # Bulk automatic resolution

<!-- Specific Validations -->
<Button Text="Check Trainer Info"/> # Disobedience prevention
<Button Text="Check Items"/>        # Invisible item detection
<Button Text="Check All Pokemon Legality"/> # Complete validation

<!-- Detailed Results -->
<ScrollView>
  <Label x:Name="ResultsLabel"/>    # Comprehensive issue reporting
</ScrollView>
```

## Enhanced Validation Methods

### PokemonHelper Enhancements
```csharp
// Prevents disobedience in SV
public static void ApplyCorrectTrainerInfo(PKM pokemon, SaveFile save)
public static bool ValidateTrainerInfo(PKM pokemon, SaveFile save)
public static void FixTrainerInfo(PKM pokemon, SaveFile save)

// SV-specific compliance
private static void ApplyScarletVioletCompliance(PKM pokemon, SaveFile save)

// Enhanced legality with item safety
public static bool IsLegal(PKM pokemon, SaveFile save)
public static string GetLegalitySummary(PKM pokemon, SaveFile save)
```

### ItemHelper - New Safety System
```csharp
// Core item safety validation
public static bool IsItemSafe(int itemId, SaveFile save)
public static bool SafelyApplyHeldItem(PKM pokemon, SaveFile save, int itemId)

// Safe item management
public static List<(int Id, string Name)> GetSafeHeldItems(SaveFile save)
public static List<(int Box, int Slot, string PokemonName, string Issue)> ValidateAllPokemonItems(SaveFile save)
public static int FixAllUnsafeItems(SaveFile save)

// Format conversion with safety
public static int ConvertItemSafely(int itemId, EntityContext from, EntityContext to, SaveFile target)
```

## Game-Specific Warnings

### Scarlet/Violet (Generation 9)
- **Trainer Info**: Critical for preventing disobedience
- **Items**: Z-Crystals and Mega Stones will be invisible
- **Tera Types**: Must be properly set for legal Pokemon
- **Context**: Proper EntityContext.Gen9 required

### Sword/Shield/BDSP (Generation 8)
- **Items**: Some legacy items not available
- **Transfer Validation**: Recommended for Pokemon from older games

### Earlier Generations
- **Forward Compatibility**: Warnings about items that won't transfer

## User Workflow for Issue Prevention

### For Scarlet/Violet Users:
1. **Load Save File** → Automatic game detection and warnings
2. **Validate Save** → Scan for trainer info and item issues  
3. **Fix All Issues** → Bulk resolution of problems
4. **Edit Pokemon** → Safe item selection and trainer validation
5. **Test in Game** → Pokemon should obey and items should be visible

### For General Users:
1. **Box Management** → Visual indicators for potential issues
2. **Pokemon Editing** → Real-time validation and suggestions
3. **Database Usage** → Only create legal, compatible Pokemon
4. **Validation Tools** → Regular health checks of save file

## Technical Implementation Notes

### Trainer Info Validation
- Exact matching of OT_Name, TID16, SID16, Language, OT_Gender
- CurrentHandler must be 0 (Original Trainer) for no disobedience
- Clear HT_Name and related handler data to prevent conflicts
- Proper version and context setting for compatibility

### Item Safety System
- Proactive blocking of Z-Crystals (777-835) in Generation 9
- Mega Stone prevention (658-776) in unsupported games
- Format conversion with safety validation
- Fallback to "no item" for unsafe selections

### Performance Considerations
- Lazy loading of item lists for better performance
- Cached validation results where appropriate
- Bulk operations optimized for large save files
- Progress feedback for long-running operations

## Future Enhancements

### Planned Features
- **Batch Editing**: Mass trainer info fixing across boxes
- **Import/Export**: Safe Pokemon file handling with validation
- **Custom Rules**: User-defined validation rules per game
- **Auto-Fix on Save**: Automatic issue resolution before saving

### Advanced Validation
- **Move Legality**: Enhanced move validation for each generation
- **Ability Validation**: Hidden ability availability checking
- **Location Validation**: Met location compatibility
- **Date Validation**: Encounter date consistency

## Testing and Validation

### Test Cases Covered
- ✅ Trainer info mismatch detection and fixing
- ✅ Unsafe item identification and removal
- ✅ Pokemon legality validation with modern standards
- ✅ Cross-generation compatibility checking
- ✅ Bulk operations on large save files

### User Testing Scenarios
- ✅ Pokemon transferred from older games to SV
- ✅ Hacked Pokemon with incorrect trainer data
- ✅ Pokemon with Z-Crystals/Mega Stones in SV
- ✅ Save files with mixed generation Pokemon
- ✅ Bulk editing operations

## Troubleshooting Guide

### Pokemon Still Disobey in SV
1. Check trainer info with "Validate Legality"
2. Use "Fix Trainer Info" button
3. Ensure CurrentHandler is 0 (Original Trainer)
4. Verify TID/SID matches save file exactly

### Items Are Invisible in Game
1. Check items with "Check Items" in validation
2. Use "Fix All Issues" to remove unsafe items
3. Re-select items using the safe item picker
4. Verify items are appropriate for target generation

### Pokemon Fail Legality Check
1. Run comprehensive validation scan
2. Check move legality and learn methods
3. Verify encounter details and locations
4. Apply basic fixes through validation tools

## Dependencies and Requirements

### Required References
```xml
<ProjectReference Include="..\PKHeX.Core\PKHeX.Core.csproj" />
<ProjectReference Include="..\PKHeX.Drawing\PKHeX.Drawing.csproj" />
<ProjectReference Include="..\PKHeX.Drawing.Misc\PKHeX.Drawing.Misc.csproj" />
<ProjectReference Include="..\PKHeX.Drawing.PokeSprite\PKHeX.Drawing.PokeSprite.csproj" />
```

### Platform Support
- ✅ macOS (maccatalyst)
- 🔄 Windows (planned)
- 🔄 iOS (planned)
- 🔄 Android (planned)

## Conclusion

This enhanced PKHeX MAUI implementation provides comprehensive protection against the most common issues faced by Pokemon editors in modern games:

1. **Pokemon Disobedience Prevention**: Strict trainer info validation
2. **Invisible Item Protection**: Comprehensive item safety system  
3. **Save File Health**: Complete validation and fixing tools
4. **User-Friendly Interface**: Clear warnings and automated fixes

The system is designed to be proactive rather than reactive, preventing issues before they occur while providing powerful tools to fix existing problems. This makes it particularly valuable for Scarlet/Violet users who face the strictest validation requirements in the series.
