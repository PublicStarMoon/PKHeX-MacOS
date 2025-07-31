# PKHeX macOS - Navigation Flow Diagram

## Visual Navigation Map

```
┌─────────────────────────────────────────────────────────────────┐
│                           MainPage                              │
│                     (Central Hub)                               │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │ Load Save    │  │ Demo Mode    │  │ Export Save  │         │
│  │ File         │  │              │  │              │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │🎒 Inventory  │  │⚡ Party      │  │📦 Box        │         │
│  │ Editor       │  │ Editor       │  │ Editor       │         │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘         │
└─────────┼──────────────────┼──────────────────┼─────────────────┘
          │                  │                  │
          ▼                  ▼                  ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ InventoryEditor │ │ PartyEditor     │ │ PokemonBoxPage  │
│ Page            │ │ Page            │ │                 │
│                 │ │                 │ │                 │
│ ┌─────────────┐ │ │ ┌─────────────┐ │ │ ┌─────────────┐ │
│ │ Pouch 1     │ │ │ │ Slot 1      │ │ │ │ Box 1       │ │
│ │ Pouch 2     │ │ │ │ Slot 2      │ │ │ │ Box 2       │ │
│ │ Pouch 3     │ │ │ │ Slot 3      │ │ │ │ Box N       │ │
│ │ ...         │ │ │ │ ...         │ │ │ │ (30 slots)  │ │
│ └─────────────┘ │ │ └─────┬───────┘ │ │ └─────┬───────┘ │
│                 │ │       │         │ │       │         │
│ ┌─────────────┐ │ │       ▼         │ │       ▼         │
│ │ 💾 Save     │ │ │ ┌─────────────┐ │ │ ┌─────────────┐ │
│ │ to Memory   │ │ │ │ Edit Pokemon│ │ │ │ Edit Pokemon│ │
│ └─────────────┘ │ │ │ (if exists) │ │ │ │ (if exists) │ │
└─────────────────┘ │ └─────┬───────┘ │ │ └─────┬───────┘ │
                    │       │         │ │       │         │
                    │       ▼         │ │       ▼         │
                    │ ┌─────────────┐ │ │ ┌─────────────┐ │
                    │ │ 💾 Save     │ │ │ │ 💾 Save     │ │
                    │ │ to Memory   │ │ │ │ to Memory   │ │
                    │ └─────────────┘ │ │ └─────────────┘ │
                    └─────────────────┘ └─────────────────┘
                              │                   │
                              ▼                   ▼
                    ┌─────────────────────────────────────┐
                    │        PokemonEditorPage            │
                    │                                     │
                    │ ┌─────────────┐ ┌─────────────┐     │
                    │ │ Basic Info  │ │ Stats       │     │
                    │ │ • Species   │ │ • Level     │     │
                    │ │ • Nickname  │ │ • HP/Atk/Def│     │
                    │ │ • Nature    │ │ • IVs/EVs   │     │
                    │ └─────────────┘ └─────────────┘     │
                    │                                     │
                    │ ┌─────────────┐ ┌─────────────┐     │
                    │ │ Moves       │ │ Appearance  │     │
                    │ │ • Move 1-4  │ │ • Form      │     │
                    │ │ • Abilities │ │ • Gender    │     │
                    │ │ • Items     │ │ • Shiny     │     │
                    │ └─────────────┘ └─────────────┘     │
                    │                                     │
                    │ ┌─────────────┐                     │
                    │ │ 💾 Save     │                     │
                    │ │ Changes     │                     │
                    │ └─────────────┘                     │
                    └─────────────────────────────────────┘
```

## Navigation Patterns

### Primary Navigation Flow
```
MainPage → [Editor Selection] → Editor Page → [Individual Pokemon] → PokemonEditorPage
    ↑                                ↓                                        ↓
    ←── [Back] ←←←←← [Back] ←←←←←←←←←←←←←←←←←←←← [Save & Back] ←←←←←←←←←←←←←←←←←←←←
```

### State Management Flow
```
Load Save File → Clear Cache → Create Editor Instances → Make Changes → Save to Memory
     ↓                                                                            ↓
Return to MainPage ←←←←←←←←←←←←←←←←←←←←←← Export to Disk ←←←←←←←←←←←←←←←←←←←←←←←←←←
```

### Cross-Editor Navigation
```
InventoryEditor ←→ MainPage ←→ PartyEditor
       ↑              ↓              ↑
       ←←←← BoxEditor ←←←←←→ PokemonEditor
```

## Detailed Navigation Rules

### From MainPage

| Button/Action | Destination | Condition | State Management |
|---------------|------------|-----------|------------------|
| 🎒 Inventory Editor | InventoryEditorPage | Save file loaded or Demo mode | PageManager.GetInventoryEditorPage() |
| ⚡ Party Editor | PartyEditorPage | Save file loaded or Demo mode | PageManager.GetPartyEditorPage() |
| 📦 Box Editor | PokemonBoxPage | Save file loaded or Demo mode | PageManager.GetPokemonBoxPage() |
| 📁 Load Save | File Picker Dialog | Always available | Clears cache if successful |
| 💾 Save File | File Picker Dialog | Save file loaded | Exports current state to disk |
| 🎮 Demo Mode | Creates demo save | Always available | Enables all editors |

### From InventoryEditorPage

| Button/Action | Destination | Condition | State Management |
|---------------|------------|-----------|------------------|
| 🔙 Back | MainPage | Always | Preserves page state |
| 💾 Save | Same page | Changes made | Marks changes as unsaved |
| [Pouch Selection] | Updates current view | Valid pouch | Updates displayed items |

### From PartyEditorPage

| Button/Action | Destination | Condition | State Management |
|---------------|------------|-----------|------------------|
| ← Back | MainPage | Always | Preserves party state |
| Save Changes | Same page | Changes made | Marks changes as unsaved |
| Heal All | Same page | Party has Pokemon | Updates all party HP |
| Clear Party | Same page | Confirmation | Removes all party Pokemon |
| Fill Empty Slots | Same page | Empty slots exist | Adds Pokemon to empty slots |
| [Pokemon Slot] | PokemonEditorPage | Slot has Pokemon | Opens individual editor |

### From PokemonBoxPage

| Button/Action | Destination | Condition | State Management |
|---------------|------------|-----------|------------------|
| ← Back | MainPage | Always | Preserves box state |
| Save Changes | Same page | Changes made | Marks changes as unsaved |
| Export Box | File operation | Current box | Exports current box data |
| [Box Selection] | Updates current view | Valid box | Loads different box |
| [Pokemon Slot] | PokemonEditorPage | Slot has Pokemon | Opens individual editor |

### From PokemonEditorPage

| Button/Action | Destination | Condition | State Management |
|---------------|------------|-----------|------------------|
| 🔙 Back | Source page (Party/Box) | Always | Updates Pokemon in source |
| 💾 Save | Same page | Changes made | Updates SaveFile object |
| [Form/Picker Changes] | Same page | Valid selection | Updates current Pokemon |

## State Persistence Rules

### Page Cache Lifecycle

1. **Creation**: First access creates new instance via PageManager
2. **Reuse**: Subsequent access returns cached instance
3. **Updates**: Changes accumulate in cached instances
4. **Clearing**: New save file or explicit clear removes cache

### Memory vs. Disk State

```
Editor Changes → SaveFile Object (Memory) → [User Export] → Disk File
      ↑                  ↑                       ↑
   Live UI          Cached State           Persistent Storage
```

### Unsaved Changes Tracking

```
Make Change → Mark Unsaved → Warning on New File Load → Export or Lose Changes
      ↓              ↓                    ↓                        ↓
   UI Update    Flag Set           Dialog Shown            State Reset
```

## Navigation Safety Features

### Data Loss Prevention

1. **New File Loading**: Warns if unsaved changes exist
2. **Application Exit**: Could warn about unsaved changes (platform dependent)
3. **Memory Preservation**: Changes persist across navigation sessions
4. **Clear Feedback**: UI shows saved/unsaved state

### User Experience Patterns

1. **Breadcrumb Navigation**: Always clear path back to MainPage
2. **Context Preservation**: Return to exact previous state
3. **Progressive Disclosure**: Complex editing happens in dedicated pages
4. **Consistent Actions**: Save buttons behave similarly across editors

## Technical Implementation Notes

### PageManager Integration

```csharp
// Standard navigation pattern
var editorPage = PageManager.GetInventoryEditorPage(saveFile);
await Navigation.PushAsync(editorPage);

// Change tracking
_saveFile.State.Edited = true;
PageManager.MarkChangesUnsaved();

// Cache management
if (_currentSaveFile != saveFile) {
    PageManager.ClearCache();
    _currentSaveFile = saveFile;
}
```

### Error Handling in Navigation

1. **Missing Save File**: Redirect to demo mode or main page
2. **Invalid Pokemon**: Graceful fallbacks in editor
3. **Navigation Failures**: User feedback and retry options
4. **State Corruption**: Cache clearing and restart

This navigation system ensures users can efficiently move between different editing contexts while maintaining their work and understanding their current state in the application.