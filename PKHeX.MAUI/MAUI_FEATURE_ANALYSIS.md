# PKHeX MAUI Feature Alignment Analysis

This document provides a detailed analysis of the feature parity between the PKHeX MAUI version and the WinForms version, specifically for Generation 8 and Generation 9 Pokémon and Inventory editing. The analysis is based on the requirement to exclude features related to Pokémon Legends: Arceus, Handling Trainer Language, Battle Version, and HomeTracker.

## Pokemon Editor (`PokemonEditorPage`) Analysis

The `PokemonEditorPage` in the MAUI application is missing several key fields required for complete and accurate editing of Pokémon from Generation 8 and Generation 9 games.

### Gen 8 (Sword/Shield & BDSP) Feature Gaps

The following essential Gen 8 properties are not implemented in the MAUI editor:

-   **`DynamaxLevel`**: This integer value (0-10) determines a Pokémon's Dynamax level. The MAUI editor does not have a UI element (like a slider or numeric entry) to view or modify this value.
    -   **WinForms Implementation**: A `ComboBox` named `CB_DynamaxLevel` is used, allowing the user to select a value from 0 to 10.

-   **`CanGigantamax`**: This boolean flag indicates if a Pokémon has the Gigantamax factor. The MAUI editor is missing a checkbox to toggle this state.
    -   **WinForms Implementation**: A `CheckBox` named `CHK_Gigantamax` is provided to toggle the Gigantamax factor on or off.

-   **`StatNature`**: In Gen 8, a Pokémon's stats can be affected by a "stat nature" (changed by mints) that is different from its original nature. The MAUI editor only allows editing the base `Nature`, not the `StatNature`, which can lead to incorrect stat calculations if not handled.
    -   **WinForms Implementation**: A separate `ComboBox` named `CB_StatNature` is available, distinct from the main `Nature` ComboBox, to set the stat-modifying nature.

### Gen 9 (Scarlet & Violet) Feature Gaps

The following essential Gen 9 properties are not implemented in the MAUI editor:

-   **`TeraTypeOriginal`**: This property stores the Pokémon's original Terastal type. The MAUI editor does not have a picker to view or set this.
    -   **WinForms Implementation**: A `ComboBox` named `CB_TeraTypeOriginal` is used to select the Pokémon's original Tera Type from a list of all valid types.

-   **`TeraTypeOverride`**: This property stores the Pokémon's current Terastal type if it has been changed. This is also missing a UI picker in the MAUI version.
    -   **WinForms Implementation**: A `ComboBox` named `CB_TeraTypeOverride` allows the user to set a new Tera Type, overriding the original one.

-   **`StatNature`**: Similar to Gen 8, Gen 9 uses a separate `StatNature` for stat calculations after using mints. This is not exposed in the MAUI editor.
    -   **WinForms Implementation**: The same `CB_StatNature` ComboBox is used for Gen 9 Pokémon to set the stat-modifying nature independently of the original nature.

## Inventory Editor (`InventoryEditorPage`) Analysis

The `InventoryEditorPage` in the MAUI application is functional but lacks support for at least one specific item flag from the core logic.

### Gen 8/9 Feature Gaps

-   **`IItemFreeSpace`**: Some inventory items in the core `PKHeX.Core` library implement the `IItemFreeSpace` interface. This is used to handle "free space" slots in the inventory, which is a distinct mechanic. The MAUI `InventoryEditorPage` does not appear to check for or provide UI to manage this flag, which could lead to improper inventory representation for games that use this feature. While `IItemFavorite` and `IItemNewFlag` are correctly implemented, the absence of `IItemFreeSpace` handling is a gap.
    -   **WinForms Implementation**: The `SAV_Inventory` form checks if items in a pouch implement `IItemFreeSpace`. If they do, it adds a `DataGridViewCheckBoxColumn` to the grid, allowing the user to toggle the "Free" flag for each item slot. This is handled dynamically based on the save file's inventory structure.

## Conclusion

The MAUI version of the Pokémon and Inventory editors are **not fully aligned** with the WinForms version for Gen 8 and Gen 9 editing, based on the specified requirements. To achieve feature parity, the following additions are necessary:

1.  **For the Pokemon Editor:**
    -   Add UI controls for `DynamaxLevel` and `CanGigantamax` for Gen 8 Pokémon.
    -   Add UI controls for `TeraTypeOriginal` and `TeraTypeOverride` for Gen 9 Pokémon.
    -   Add a UI control for `StatNature` for both Gen 8 and Gen 9 Pokémon.
2.  **For the Inventory Editor:**
    -   Implement logic and potentially UI to correctly handle the `IItemFreeSpace` flag for inventory items.

Without these features, users will not be able to perform complete or accurate edits for modern generation Pokémon and their inventories.
