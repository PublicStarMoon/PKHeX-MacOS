namespace PKHeX.MAUI.Models;

/// <summary>
/// Interface for picker items used in SearchablePickerPage
/// </summary>
public interface IPickerItem
{
    int Id { get; set; }
    string DisplayName { get; set; }
}

/// <summary>
/// Move item for move selection pickers
/// </summary>
public class MoveItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// Ability item for ability selection pickers
/// </summary>
public class AbilityItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// Nature item for nature selection pickers
/// </summary>
public class NatureItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// Item for general item/inventory selection pickers
/// </summary>
public class ItemItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// Ball item for Pokéball selection pickers
/// </summary>
public class BallItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// Form item for Pokémon form selection pickers
/// </summary>
public class FormItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}

/// <summary>
/// Species item for Pokémon species selection pickers
/// </summary>
public class SpeciesItem : IPickerItem
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";
}
