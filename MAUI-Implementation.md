# PKHeX MAUI - Pokemon Box Editor

This document describes the newly implemented Pokemon Box Editor functionality in the PKHeX MAUI application for macOS.

## Features Implemented

### 1. Pokemon Box Editor (`PokemonBoxPage`)
- **Box Navigation**: Switch between different Pokemon storage boxes
- **Grid Layout**: 6x5 grid showing all 30 Pokemon slots per box
- **Pokemon Display**: Shows Pokemon name, level, and visual indicators (shiny = gold, egg = pink, normal = blue)
- **Slot Management**: Click on any slot to view, edit, or delete Pokemon
- **Quick Actions**: 
  - Add new Pokemon to empty slots
  - Clear entire boxes
  - View Pokemon count per box

### 2. Pokemon Detail Editor (`PokemonEditorPage`)
- **Basic Information**: Edit species, nickname, level, and nature
- **IV Editor**: Modify Individual Values (IVs) for all 6 stats with sliders
- **Move Editor**: Change Pokemon's 4 moves with searchable dropdowns
- **Special Properties**: Toggle shiny status, egg status
- **Utility Functions**: Heal Pokemon, randomize IVs, suggest legal moves
- **Real-time Stats**: View calculated battle stats as you edit

### 3. Pokemon Database Browser (`PokemonDatabasePage`)
- **Complete Pokedex**: Browse all Pokemon species (1-1010)
- **Search & Filter**: Search by name/number and filter by generation
- **Species Information**: View basic details and stats
- **Pokemon Creation**: Create new Pokemon directly from the database
- **Random Selection**: Generate random Pokemon for variety

### 4. Utility Classes (`PokemonHelper`)
- **Legal Pokemon Creation**: Automatically creates legal Pokemon with proper OT info
- **Legality Checking**: Validate Pokemon for competitive play
- **Base Stats Lookup**: Get species base stats and types
- **Box Management**: Count and retrieve Pokemon from boxes
- **Competitive Pokemon**: Create Pokemon with perfect IVs for competitive play

## How to Use

### Loading a Save File
1. Launch the MAUI app
2. Click "Load Save" or "Open Save File"
3. Select your Pokemon save file (.sav, .dat, .bin)
4. View save file information once loaded

### Editing Pokemon Boxes
1. Click "Open Pokemon Box Editor" after loading a save
2. Use the box dropdown to switch between different boxes
3. Click on any Pokemon slot to:
   - View detailed stats
   - Open the full detail editor
   - Quick edit level or shiny status
   - Delete the Pokemon
4. Click "Add Pokemon" to add new Pokemon to empty slots

### Using the Detail Editor
1. Access via "Edit in Detail Editor" from the box view
2. Modify any aspect of the Pokemon:
   - Change species, nickname, level, nature
   - Adjust IVs using the sliders
   - Set moves from comprehensive move lists
   - Toggle special properties
3. Save changes and return to box view

### Browse Pokemon Database
1. Click "Open PKM Database" from the main menu
2. Search for specific Pokemon or browse by generation
3. Select a Pokemon to view its details
4. Create new Pokemon directly from the database (if save file loaded)

## Technical Implementation

### Project Structure
```
PKHeX.MAUI/
├── Views/
│   ├── MainPage.xaml/cs          # Main application interface
│   ├── PokemonBoxPage.xaml/cs    # Box editor with 30-slot grid
│   ├── PokemonEditorPage.xaml/cs # Detailed Pokemon editor
│   └── PokemonDatabasePage.xaml/cs # Pokemon species browser
└── Utilities/
    └── PokemonHelper.cs          # Pokemon manipulation utilities
```

### Key Dependencies
- **PKHeX.Core**: Core Pokemon data structures and save file handling
- **PKHeX.Drawing**: Pokemon sprite and image utilities  
- **PKHeX.Drawing.Misc**: Additional drawing utilities
- **PKHeX.Drawing.PokeSprite**: Pokemon sprite generation

### Data Flow
1. Save files are loaded using `SaveUtil.GetVariantSAV()`
2. Pokemon are retrieved/stored using `SaveFile.GetBoxSlotAtIndex()` and `SaveFile.SetBoxSlotAtIndex()`
3. Pokemon properties are edited directly on PKM objects
4. Changes are automatically saved back to the SaveFile object
5. Users can export the modified save file to disk

## Building and Running

### Prerequisites
- .NET 8 SDK
- macOS development environment
- MAUI workload installed

### Build Commands
```bash
# Build the project
dotnet build PKHeX.MAUI/PKHeX.MAUI.csproj

# Run the project
dotnet run --project PKHeX.MAUI/PKHeX.MAUI.csproj
```

### VS Code Tasks
Two tasks are available in VS Code:
- **Build MAUI App**: Builds the project and shows compilation errors
- **Run MAUI App**: Runs the application in the background

## Features for Future Enhancement

### Immediate Improvements
- **Visual Pokemon Sprites**: Display actual Pokemon images in the box grid
- **Type Information**: Show Pokemon types in the editor
- **Ability Editor**: Allow changing Pokemon abilities
- **Item Management**: Assign held items to Pokemon
- **Batch Operations**: Mass edit multiple Pokemon at once

### Advanced Features
- **Team Builder**: Create and save competitive teams
- **Import/Export**: Import Pokemon from Showdown format
- **Breeding Calculator**: IV inheritance and breeding tools
- **Wonder Trade Simulation**: Random Pokemon exchange
- **Save File Comparison**: Compare different save files

### UI/UX Improvements
- **Dark Mode**: Add dark theme support
- **Keyboard Shortcuts**: Hotkeys for common operations
- **Drag & Drop**: Move Pokemon between slots via drag and drop
- **Undo/Redo**: Revert unwanted changes
- **Auto-save**: Periodic backup of changes

## Error Handling

The application includes comprehensive error handling for:
- Invalid save file formats
- Corrupted Pokemon data
- Missing game resources
- File I/O operations
- Network connectivity (for future online features)

All errors are displayed to the user with clear, actionable messages.

## Legality Validation

The Pokemon editor includes basic legality checking to ensure created Pokemon are valid for:
- Online trading
- Competitive play
- Game compatibility

Invalid Pokemon will show warnings with suggestions for fixes.
