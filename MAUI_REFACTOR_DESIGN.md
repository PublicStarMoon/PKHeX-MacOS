# PKHeX MAUI UI Refactor - Design Documentation

## Overview
This document describes the refactored MAUI UI architecture for PKHeX-MacOS, focused on providing a minimal, maintainable editor for Gen 7-9 Pokémon and items only.

## Architecture Design

### Core Principles
1. **Separation of Concerns**: All business logic handled by PKHeX.Core, UI only for presentation
2. **Minimal Feature Set**: Only essential Pokémon and item editing functionality
3. **Generation Focus**: Support limited to Gen 7, 8, and 9 games only
4. **Clean Dependencies**: Clear dependency flow from UI → Core → Data

### Project Structure
```
PKHeX.Core/          - Core Pokémon logic, save file handling, validation
PKHeX.Drawing/       - Graphics utilities, sprite handling  
PKHeX.MAUI/         - Minimal MAUI UI (refactored)
  Views/
    MainPage.*       - Simple navigation and save file loading
    PokemonBoxPage.* - Box/party Pokémon management
    PokemonEditorPage.* - Individual Pokémon editing
    InventoryEditorPage.* - Item and Poké Ball management
  Utilities/
    PokemonHelper.cs - UI-specific Pokémon operations
    ItemHelper.cs    - UI-specific item operations
```

### Removed Components
The following overly complex or unnecessary components were removed:
- EventFlagsEditorPage - Game progression flags (not essential)
- MemoryAmieEditorPage - Affection/memory system (not essential)  
- MiscDataEditorPage - Miscellaneous data editing (not essential)
- MoveEditorPage - Advanced move editing (integrated into PokemonEditor)
- PokedexEditorPage - Pokédex completion (not essential)
- PokemonDatabasePage - Database browsing (not essential)
- SAVEditorPage - Advanced save editing (not essential)
- SaveValidationPage - Validation display (integrated into main flow)
- StatEditorPage - Advanced stats (integrated into PokemonEditor)
- TrainerIDEditorPage - Trainer ID editing (not essential)

## Feature Specification

### Core Features (Implemented)
1. **Save File Management**
   - Load Gen 7/8/9 save files
   - Basic save validation and info display
   - Export modified save files

2. **Pokémon Box Editor** 
   - View and navigate between boxes
   - Select and edit individual Pokémon
   - Clear boxes, export functionality

3. **Pokémon Editor**
   - Species, nickname, level editing
   - Nature, IVs, stats modification
   - Move selection and validation
   - Shiny toggle, held items
   - Inline legality validation
   - Trainer info fixing for Gen 9

4. **Item Editor**
   - Browse item categories (Normal, Medicine, Poké Balls, etc.)
   - Modify item quantities
   - Max/clear all items functionality
   - Proper Core inventory integration

### Integration with Core/Drawing
- **SaveFile Loading**: Uses `SaveUtil.GetVariantSAV()` for file parsing
- **Pokémon Data**: Direct manipulation of `PKM` objects via Core
- **Item Management**: Uses `InventoryPouch` system from Core
- **Validation**: Leverages Core legality checking
- **Sprites**: Ready for Drawing project integration (sprites not implemented due to platform constraints)

### Generation Support
- **Gen 7**: Sun/Moon, Ultra Sun/Moon
- **Gen 8**: Sword/Shield, Brilliant Diamond/Shining Pearl, Legends Arceus  
- **Gen 9**: Scarlet/Violet

Older generations are not supported to maintain focus and reduce complexity.

## Code Quality Improvements

### Removed Code
- Deleted ~4,500 lines of complex, unmaintainable UI code
- Eliminated 10 unnecessary editor pages and their dependencies
- Cleaned up all references to removed components

### Simplified Navigation
- MainPage now shows only essential editor options
- Clear, direct navigation to Box Editor and Item Editor
- Removed complex sub-navigation and feature overload

### Error Handling
- Consistent async/await patterns
- Proper exception handling with user feedback
- Graceful degradation for unsupported features

## Future Enhancements

### Potential Additions (if needed)
1. **Sprite Display**: Use PKHeX.Drawing for Pokémon sprites in box view
2. **Legality Indicators**: Visual feedback for legal/illegal Pokémon
3. **Batch Operations**: Simple batch editing for common operations
4. **Import/Export**: Individual Pokémon file import/export

### Architecture Notes
- The architecture supports easy addition of new features
- Core/Drawing dependencies are properly isolated
- UI components are reusable and well-separated

## Build and Testing

### Dependencies
- .NET 8.0
- Microsoft.Maui.Controls 8.0.100
- PKHeX.Core (local reference)
- PKHeX.Drawing projects (local reference)

### Validation Approach
Since full MAUI building requires macOS-specific workloads:
1. Core/Drawing projects build successfully in any environment
2. XAML and C# code is syntactically validated
3. All references to deleted components removed
4. Architecture ensures clean separation of concerns

This refactored UI provides a focused, maintainable foundation for Gen 7-9 Pokémon and item editing while maintaining clean architecture principles.