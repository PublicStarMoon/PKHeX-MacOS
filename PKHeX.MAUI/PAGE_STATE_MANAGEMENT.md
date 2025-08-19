# Page State Management System

## Overview

The PKHeX macOS app now uses a **PageManager** system to maintain in-memory state across page navigation, providing a much better user experience.

## How It Works

### Before (The Problem)
- Each time you opened an editor page, a new instance was created
- Constructor reloaded all data from the save file
- Any changes made in the UI were lost when navigating away and back
- Users had to redo their work constantly

### After (The Solution)
- Editor pages are cached and reused via `PageManager`
- Data is loaded once and maintained in memory
- UI state persists across navigation
- Changes accumulate until explicitly saved to disk

## User Workflow

### Making Changes
1. Load a save file or use Demo Mode
2. Open any editor (Inventory, Party, Box)
3. Make changes (modify items, edit Pokemon, etc.)
4. Click "Save to Memory" in the editor
5. Navigate to other editors and make more changes
6. All changes are preserved in memory

### Persisting Changes
1. Go back to Main Page
2. Click "Export Save" button
3. Choose where to save the file
4. All changes are written to disk

### Loading New Save Files
- If you have unsaved changes, you'll get a warning
- Choose to export first or lose changes
- New save file clears all cached pages

## Technical Architecture

### PageManager.cs
- **Caches page instances** by type (Inventory, Party, Box)
- **Tracks save file changes** and clears cache when needed
- **Monitors unsaved state** to warn users
- **Provides state management** across the app

### Key Methods
- `GetInventoryEditorPage(saveFile)` - Returns cached or new instance
- `MarkChangesUnsaved()` - Called when editors save to memory
- `MarkChangesSaved()` - Called after successful export
- `ClearCache()` - Called when loading new save files

### Integration Points

#### MainPage Navigation
```csharp
// OLD: Always creates new instance
var inventoryPage = new InventoryEditorPage(_currentSave);

// NEW: Reuses cached instance
var inventoryPage = PageManager.GetInventoryEditorPage(_currentSave);
```

#### Editor Save Methods
```csharp
// Mark changes as unsaved after saving to memory
_saveFile.State.Edited = true;
PageManager.MarkChangesUnsaved();
```

#### Export Save Method
```csharp
// Mark changes as saved after writing to disk
PageManager.MarkChangesSaved();
```

## Benefits

### For Users
- ✅ **No lost work** - Changes persist across navigation
- ✅ **Better workflow** - Make changes in multiple editors before saving
- ✅ **Clear feedback** - Understand memory vs. disk state
- ✅ **Safety warnings** - Prevent accidental data loss

### For Developers
- ✅ **Centralized state** - All page management in one place
- ✅ **Consistent behavior** - All editors work the same way
- ✅ **Easy to extend** - Add new editors with same pattern
- ✅ **Maintainable** - Clear separation of concerns

## Important Notes

### When Pages Are Recreated
- Loading a different save file
- Calling `PageManager.ClearCache()` explicitly
- Calling `PageManager.RefreshAfterSaveReload()`

### When Pages Are Reused
- Normal navigation between editors
- Going back to Main Page and reopening editors
- Multiple edit sessions before export

### Memory Management
- Pages are kept in memory until cache is cleared
- Only one instance per editor type
- Garbage collected when cache is cleared

## Migration Guide

### For New Editors
1. Add method to `PageManager` (follow existing pattern)
2. Update navigation code to use `PageManager.GetXXXPage()`
3. Add `PageManager.MarkChangesUnsaved()` to save methods
4. Include using `PKHeX.MAUI.Services;`

### For Bug Fixes
- Remove `OnAppearing` data reloading (if present)
- Ensure save methods call `PageManager.MarkChangesUnsaved()`
- Test navigation preserves state
- Verify export clears unsaved flag
