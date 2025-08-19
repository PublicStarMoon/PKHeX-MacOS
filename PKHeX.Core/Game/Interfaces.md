# PKHeX.Core.Game Public Interfaces

This document lists the public interfaces found in the `PKHeX.Core/Game/` directory and its subdirectories.

---

## `IBasicStrings`

**Description:**

String providing interface with minimal support for game strings.

**Definition:**

```csharp
public interface IBasicStrings
{
    IReadOnlyList<string> Species { get; }
    IReadOnlyList<string> Item { get; }
    IReadOnlyList<string> Move { get; }
    IReadOnlyList<string> Ability { get; }
    IReadOnlyList<string> Types { get; }
    IReadOnlyList<string> Natures { get; }

    /// <summary>
    /// Name an Egg has when obtained on this language.
    /// </summary>
    string EggName { get; }
}
```

---

## `ILocationSet`

**Description:**

Stores location names for a given game group.

**Definition:**

```csharp
public interface ILocationSet
{
    /// <summary>
    /// Gets the location name group for the requested location bank group ID.
    /// </summary>
    ReadOnlySpan<string> GetLocationNames(int bankID);

    /// <summary>
    /// Gets the location name for the requested location ID.
    /// </summary>
    string GetLocationName(int locationID);

    /// <summary>
    /// Gets all groups -- ONLY USE FOR UNIT TESTING.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    IEnumerable<(int Bank, string[] Names)> GetAll();
}
```
