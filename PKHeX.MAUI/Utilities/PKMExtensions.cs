using PKHeX.Core;

namespace PKHeX.MAUI.Utilities;

/// <summary>
/// Extension methods for PKM to provide compatibility with legacy array-based property access
/// </summary>
public static class PKMExtensions
{
    /// <summary>
    /// Gets PP values as an array for compatibility with legacy code
    /// </summary>
    public static int[] GetMovePPs(this PKM pkm)
    {
        return new[] { pkm.Move1_PP, pkm.Move2_PP, pkm.Move3_PP, pkm.Move4_PP };
    }

    /// <summary>
    /// Sets PP values from an array for compatibility with legacy code
    /// </summary>
    public static void SetMovePPs(this PKM pkm, int[] pps)
    {
        if (pps.Length >= 1) pkm.Move1_PP = pps[0];
        if (pps.Length >= 2) pkm.Move2_PP = pps[1];
        if (pps.Length >= 3) pkm.Move3_PP = pps[2];
        if (pps.Length >= 4) pkm.Move4_PP = pps[3];
    }

    /// <summary>
    /// Gets PP Up values as an array for compatibility with legacy code
    /// </summary>
    public static int[] GetMovePPUps(this PKM pkm)
    {
        return new[] { pkm.Move1_PPUps, pkm.Move2_PPUps, pkm.Move3_PPUps, pkm.Move4_PPUps };
    }

    /// <summary>
    /// Sets PP Up values from an array for compatibility with legacy code
    /// </summary>
    public static void SetMovePPUps(this PKM pkm, int[] ppUps)
    {
        if (ppUps.Length >= 1) pkm.Move1_PPUps = ppUps[0];
        if (ppUps.Length >= 2) pkm.Move2_PPUps = ppUps[1];
        if (ppUps.Length >= 3) pkm.Move3_PPUps = ppUps[2];
        if (ppUps.Length >= 4) pkm.Move4_PPUps = ppUps[3];
    }

    /// <summary>
    /// Gets a specific PP value by index
    /// </summary>
    public static int GetMovePP(this PKM pkm, int index) => index switch
    {
        0 => pkm.Move1_PP,
        1 => pkm.Move2_PP,
        2 => pkm.Move3_PP,
        3 => pkm.Move4_PP,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 3.")
    };

    /// <summary>
    /// Sets a specific PP value by index
    /// </summary>
    public static void SetMovePP(this PKM pkm, int index, int value)
    {
        switch (index)
        {
            case 0: pkm.Move1_PP = value; break;
            case 1: pkm.Move2_PP = value; break;
            case 2: pkm.Move3_PP = value; break;
            case 3: pkm.Move4_PP = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 3.");
        }
    }

    /// <summary>
    /// Gets a specific PP Up value by index
    /// </summary>
    public static int GetMovePPUp(this PKM pkm, int index) => index switch
    {
        0 => pkm.Move1_PPUps,
        1 => pkm.Move2_PPUps,
        2 => pkm.Move3_PPUps,
        3 => pkm.Move4_PPUps,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 3.")
    };

    /// <summary>
    /// Sets a specific PP Up value by index
    /// </summary>
    public static void SetMovePPUp(this PKM pkm, int index, int value)
    {
        switch (index)
        {
            case 0: pkm.Move1_PPUps = value; break;
            case 1: pkm.Move2_PPUps = value; break;
            case 2: pkm.Move3_PPUps = value; break;
            case 3: pkm.Move4_PPUps = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 3.");
        }
    }

    /// <summary>
    /// Gets HP Current for compatibility with legacy code
    /// </summary>
    public static int GetHPCurrent(this PKM pkm) => pkm.Stat_HPCurrent;

    /// <summary>
    /// Sets HP Current for compatibility with legacy code
    /// </summary>
    public static void SetHPCurrent(this PKM pkm, int value) => pkm.Stat_HPCurrent = value;

    /// <summary>
    /// Gets HP Max for compatibility with legacy code
    /// </summary>
    public static int GetStatHP(this PKM pkm) => pkm.Stat_HPMax;

    /// <summary>
    /// Sets HP Max for compatibility with legacy code
    /// </summary>
    public static void SetStatHP(this PKM pkm, int value) => pkm.Stat_HPMax = value;
}