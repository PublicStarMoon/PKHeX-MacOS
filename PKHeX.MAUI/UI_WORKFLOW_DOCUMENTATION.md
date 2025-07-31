# PKHeX macOS - UI Workflow Documentation

## Overview

PKHeX for macOS is a comprehensive Pokémon save file editor built using .NET MAUI. This documentation provides a detailed understanding of the user interface workflow, page interactions, and navigation patterns that enable users to edit their Pokémon save files effectively.

## Application Architecture

### Core Components

1. **MainPage** - Central hub and entry point
2. **InventoryEditorPage** - Item and pouch management
3. **PokemonBoxPage** - Box Pokémon management  
4. **PokemonEditorPage** - Individual Pokémon editing
5. **PartyEditorPage** - Active party management
6. **PageManager** - State management service

### State Management Philosophy

The application uses a sophisticated state management system that:
- **Preserves user work** across navigation
- **Maintains changes in memory** until explicitly saved to disk
- **Provides clear feedback** about saved vs. unsaved state
- **Prevents data loss** through navigation warnings

## Complete User Workflow

### 1. Application Startup

**Entry Point: MainPage**
- Users are greeted with a clean, modern interface
- Save file status shows "No save file loaded"
- Editor buttons are initially disabled
- Two primary options are available:
  - Load an existing save file
  - Enable demo mode for testing

### 2. Loading Save Files

#### Option A: Load Real Save File
```
User clicks "📁 Load Save" → File picker opens → User selects save file
→ Application validates file → Save file information displays → Editors enabled
```

**Validation Process:**
- File size validation (must be > 100 bytes)
- Format detection using PKHeX.Core.SaveUtil
- Game version identification
- Trainer information extraction

**Displayed Information:**
- Current save file name
- Game version and generation
- Trainer name and ID
- Play time (when available)

#### Option B: Demo Mode
```
User clicks "🎮 Demo Mode" → Confirmation dialog → Demo save created (SAV8SWSH)
→ UI updates with demo data → All editors enabled
```

**Demo Mode Features:**
- Creates a temporary Sword/Shield save file
- Enables full editor functionality for testing
- Changes are not persistent between sessions
- Clear indication that demo mode is active

### 3. Editor Navigation and State Management

#### Navigation Pattern
```
MainPage → Editor Page → [Navigate between editors] → Return to MainPage → Save
```

**Key Principles:**
- **Page Caching**: Editor pages are cached and reused via PageManager
- **State Persistence**: Changes persist across navigation sessions
- **Memory vs. Disk**: Clear distinction between memory edits and saved files

#### State Management Flow
```
Load Save File → Clear PageManager Cache → Create Editor Instances (cached)
→ Make Changes → Mark as Unsaved → Navigate freely → Export to Disk → Mark as Saved
```

## Detailed Page Workflows

### MainPage - Central Hub

**Purpose**: Primary navigation and file management center

**Key Features:**
- **Save File Management**: Load, display info, and export save files
- **Editor Access**: Navigate to all specialized editors
- **Status Tracking**: Monitor saved/unsaved state
- **Demo Mode**: Test functionality without real save files

**User Actions:**
1. **Load Save File**
   - File picker with supported formats (.sav, .dat, .bin, etc.)
   - Automatic validation and error handling
   - Immediate feedback on success/failure

2. **View Save Information**
   - Current file name and location
   - Game version and generation details
   - Trainer information and play time

3. **Access Editors**
   - Inventory Editor (🎒 button)
   - Party Editor (⚡ button)  
   - Box Editor (📦 button)
   - Each redirects to demo mode if no save loaded

4. **Save Changes**
   - Export current state to disk
   - File picker for save location
   - Success confirmation and status update

**Navigation Safeguards:**
- Warns users about unsaved changes when loading new files
- Provides option to export before losing work
- Clear status indicators for current state

### InventoryEditorPage - Item Management

**Purpose**: Comprehensive item and pouch editing interface

**Key Features:**
- **Pouch Selection**: Navigate between different item pouches/bags
- **Item Editing**: Modify item quantities and types
- **Generation Support**: Adapts to different game generations
- **Memory Persistence**: Changes saved to memory, not disk

**User Workflow:**
1. **Pouch Navigation**
   ```
   Enter Page → View Available Pouches → Select Pouch → View Items
   ```

2. **Item Editing Process**
   ```
   Select Item → Modify Quantity → Update in Memory → Navigate to Other Pouches
   ```

3. **Save to Memory**
   ```
   Make Changes → Click "💾 Save" → Changes Written to SaveFile Object
   → PageManager.MarkChangesUnsaved() → Return or Continue Editing
   ```

**Technical Details:**
- Pouches are loaded based on save file generation
- Different generations support different item types
- Validation ensures items are compatible with game version
- All changes remain in memory until main page export

### PokemonBoxPage - Box Management

**Purpose**: Grid-based interface for managing boxed Pokémon

**Key Features:**
- **Box Selection**: Switch between multiple PC boxes
- **Grid Layout**: Visual 6x5 grid representing box slots
- **Pokémon Overview**: Quick view of species and basic info
- **Individual Editing**: Open specific Pokémon for detailed editing

**User Workflow:**
1. **Box Navigation**
   ```
   Enter Page → Select Box from Dropdown → View 30 Pokémon Slots
   ```

2. **Pokémon Interaction**
   ```
   Click Empty Slot → [Create New Pokémon] OR
   Click Occupied Slot → Open PokemonEditorPage for Detailed Editing
   ```

3. **Box Management**
   ```
   Switch Boxes → View Different Pokémon → Make Changes → Save to Memory
   ```

**Visual Design:**
- 6 columns × 5 rows = 30 slots per box
- Empty slots clearly indicated
- Occupied slots show Pokémon species/nickname
- Current box name and capacity displayed

**Integration Points:**
- Seamlessly opens PokemonEditorPage for individual Pokémon
- Returns to box view after individual edits
- Maintains position and context when navigating

### PokemonEditorPage - Individual Pokémon Editing

**Purpose**: Comprehensive single Pokémon editing interface

**Key Features:**
- **Basic Information**: Species, nickname, level, nature
- **Advanced Stats**: IVs, EVs, abilities, moves
- **Visual Elements**: Forms, gender, shininess
- **Game Metadata**: Original trainer, ball type, location

**User Workflow:**
1. **Data Loading**
   ```
   Receive Pokémon from Box/Party → Load All Attributes → Display in Form
   ```

2. **Editing Process**
   ```
   Modify Basic Info → Edit Stats → Change Moves → Update Abilities
   → Adjust Visual Appearance → Save Changes
   ```

3. **Save and Return**
   ```
   Click "💾 Save" → Validate Changes → Update SaveFile Object
   → Mark as Unsaved → Return to Source (Box/Party)
   ```

**Editing Categories:**
- **Basic**: Species, nickname, level, experience
- **Stats**: Current HP, Attack, Defense, Special stats, Speed
- **Advanced**: IVs (Individual Values), EVs (Effort Values)
- **Moves**: Four move slots with move selection
- **Abilities**: Primary and hidden abilities
- **Appearance**: Form variations, gender, shiny status
- **Metadata**: OT, ball, met location, met level

**Validation Features:**
- Legal move combinations for species/level
- Stat ranges within valid limits
- Form compatibility with abilities/moves
- Generation-specific restrictions

### PartyEditorPage - Active Party Management

**Purpose**: Manage the active party of up to 6 Pokémon

**Key Features:**
- **Party Slots**: Visual representation of 6 party positions
- **Quick Overview**: Basic info for each party member
- **Individual Editing**: Open detailed editor for party Pokémon
- **Party Composition**: Drag/drop or selection-based organization

**User Workflow:**
1. **Party Overview**
   ```
   Enter Page → View 6 Party Slots → See Current Party Composition
   ```

2. **Pokémon Management**
   ```
   Click Slot → [Edit Existing] OR [Add from Box] OR [Create New]
   ```

3. **Party Organization**
   ```
   Reorder Pokémon → Set Party Leader → Ensure Valid Composition
   ```

## Advanced Navigation Patterns

### Cross-Editor Workflow

**Typical User Session:**
```
MainPage → Load Save → InventoryEditor → Add Items
→ BoxEditor → Select Pokémon → PokemonEditor → Modify Stats
→ PartyEditor → Organize Team → MainPage → Export Save
```

**State Preservation:**
- Each editor maintains its state independently
- Changes accumulate across multiple editor sessions
- No data loss when switching between editors
- All changes visible immediately upon return

### Navigation Safety Features

**Unsaved Changes Protection:**
```
User Attempts New Save Load → Check PageManager.HasUnsavedChanges
→ [If True] Show Warning Dialog → [Export First] OR [Continue and Lose]
```

**Session Management:**
- Clear indication of unsaved state
- Warning dialogs for destructive actions
- Export reminders before major operations
- Graceful handling of navigation interruptions

## Technical Implementation Details

### PageManager Service

**Singleton Pattern Implementation:**
```csharp
public static class PageManager
{
    private static Dictionary<string, ContentPage> _pageCache;
    private static SaveFile? _currentSaveFile;
    private static bool _hasUnsavedChanges;
}
```

**Key Methods:**
- `GetInventoryEditorPage(SaveFile)` - Returns cached or new instance
- `MarkChangesUnsaved()` - Called when editors save to memory
- `MarkChangesSaved()` - Called after successful disk export
- `ClearCache()` - Called when loading new save files

**Caching Strategy:**
- One instance per editor type per save file
- Automatic cache clearing on save file changes
- Memory management through explicit disposal
- Consistent state across navigation sessions

### Data Flow Architecture

**Save File Loading:**
```
File Selection → Validation → SaveUtil.GetVariantSAV() → PageManager.ClearCache()
→ Set Current Save → Enable Editors → Display Information
```

**Change Management:**
```
Editor Changes → Save to Memory → SaveFile.State.Edited = true
→ PageManager.MarkChangesUnsaved() → UI State Updates
```

**Export Process:**
```
Export Request → SaveFile.Write() → File.WriteAllBytesAsync()
→ PageManager.MarkChangesSaved() → Success Confirmation
```

### Error Handling Patterns

**File Loading Errors:**
- Invalid file format detection
- Size validation (minimum thresholds)
- Corruption detection and reporting
- User-friendly error messages with guidance

**Edit Validation Errors:**
- Real-time validation during editing
- Prevention of invalid combinations
- Clear error messages with correction hints
- Graceful fallbacks for edge cases

**Save Operation Errors:**
- Disk space validation
- Permission checking
- File lock detection
- Recovery suggestions for failures

## User Experience Design Principles

### Visual Consistency

**Color Scheme:**
- Primary: #2C3E50 (Dark blue-gray headers)
- Success: #27AE60 (Green for save actions)
- Warning: #F39C12 (Orange for important actions)
- Info: #3498DB (Blue for navigation)
- Background: #F8F9FA (Light gray for readability)

**Typography:**
- Headers: Bold, larger fonts for clear hierarchy
- Body: Readable fonts with appropriate line spacing
- Labels: Descriptive text with emoji icons for visual appeal
- Status: Color-coded text for immediate recognition

**Layout Patterns:**
- Card-based design for content organization
- Consistent padding and margins throughout
- Grid layouts for data-heavy sections
- Scrollable content areas for long lists

### Accessibility Features

**Navigation:**
- Clear button labels with descriptive text
- Logical tab order for keyboard navigation
- Consistent placement of common actions
- Visual feedback for interactive elements

**Feedback:**
- Immediate response to user actions
- Progress indicators for long operations
- Success/error messages with clear next steps
- Status indicators for current application state

### Performance Optimizations

**Memory Management:**
- Page caching to reduce recreation overhead
- Lazy loading of editor data
- Efficient image handling for Pokémon sprites
- Garbage collection awareness in navigation

**Responsiveness:**
- Asynchronous file operations
- Background processing for validations
- UI thread safety for all updates
- Smooth transitions between pages

## Best Practices for Users

### Recommended Workflow

1. **Always Backup First**
   - Create copies of save files before editing
   - Test with demo mode before using real saves
   - Verify exports before overwriting originals

2. **Efficient Editing Sessions**
   - Make related changes in single sessions
   - Use multiple editors in sequence without saving
   - Export only when satisfied with all changes
   - Verify results after major modifications

3. **Data Safety**
   - Pay attention to unsaved change warnings
   - Don't ignore validation error messages
   - Keep original save files as backups
   - Test edited saves in games before extensive play

### Troubleshooting Common Issues

**Save File Won't Load:**
- Verify file is actual Pokémon save data
- Check file isn't corrupted or truncated
- Ensure file format is supported by PKHeX
- Try demo mode to verify application functionality

**Changes Not Saving:**
- Confirm you clicked "Save" in individual editors
- Return to MainPage and use "Export Save" 
- Check file permissions for export location
- Verify export process completed successfully

**Editor Features Missing:**
- Some features depend on game generation
- Demo mode may have limitations
- Older saves may not support newer features
- Check PKHeX documentation for compatibility

## Conclusion

The PKHeX macOS UI workflow is designed around preserving user work and providing intuitive navigation between different editing capabilities. The combination of the PageManager state system, comprehensive editor pages, and careful attention to user experience creates a powerful yet approachable save editing environment.

The workflow ensures that users can safely edit their Pokémon save files with confidence, knowing that their changes are preserved across navigation sessions and that they have multiple opportunities to save or discard their work as needed.