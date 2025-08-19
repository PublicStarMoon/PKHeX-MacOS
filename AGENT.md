PKHeX-MacOS — Agent Handbook

Purpose
- Longer-format guide for an AI coding agent working on PKHeX-MacOS. Focuses on engineering workflow, precise edit patterns, and safe change rules.

Quick contract for changes
- Inputs: modified C# sources, test changes, or new assets.
- Outputs: builds/tests pass, no regressions in `PKHeX.Core` semantics.
- Error modes: compilation errors, failing tests, behavioral regression in save parsing.
- Success: `dotnet build` + `dotnet test` pass for affected projects; no changes to public `PKHeX.Core` APIs unless explicit.

Checklist before editing
1. Locate the minimal set of files to change (prefer SaveFile subclasses or specific utilities).
2. Run targeted unit tests locally: `dotnet test Tests/PKHeX.Core.Tests/PKHeX.Core.Tests.csproj --filter "FullyQualifiedName~<TestClassOrMethod>" --no-build`.
3. Make small commits that change one concern only (parsing, RNG, UI wiring).
4. Add or update tests that cover the new behavior (happy path + 1 edge case).
5. Run `dotnet build PKHeX.sln --configuration Debug` and `dotnet test` for the affected projects.

Edge cases to watch
- Endianness: many reads/writes use `BinaryPrimitives` or explicit big/little endian helpers. Match existing patterns in the nearby file.
- Off-by-one offsets: when adding or changing offsets, re-run packing/unpacking tests for affected generations.
- RNG determinism: random algorithms (xoroshiro/xorshift/LCRNG) are used for PKM generation and legality checks — altering them changes reproducibility.
- Decryption/Encryption: SwishCrypto and other encryption helpers are seed/byte-order sensitive. Validate using `Tests/PKHeX.Core.Tests/Saves/*`.

PKM derived values reminder
- When changing `PKM` fields like `IV`, `EV`, `Nature`, `Ability` or trainer data, call `pk.ResetPartyStats()` (for party-format fields) or use the `SaveFile` helper flow which calls `UpdatePKM(...)` and `SetPartyValues(...)` before writing the slot. Example safe flow:
  1. Modify `pk` fields.
  2. Call `UpdatePKM(pk, isParty, PKMImportSetting.Update, PKMImportSetting.Update)` (or rely on `SetBoxSlot` / `SetPartySlot` which call this).
  3. Call `SetPartyValues(pk, isParty)` if not already called — this triggers `pk.ResetPartyStats()` when needed.

Recommended smoke test (add to Tests when modifying parsing/generator code)
1. Load a small sample save or construct a `SaveFile` instance.
2. Read a stored PKM, change `IV`/`EV`/`Nature`.
3. Call the safe write flow (`SetBoxSlot`/`SetPartySlot`) to persist changes.
4. Round-trip: write the save to bytes, load into a new `SaveFile` instance, read back PKM and assert derived fields (e.g., recalculated stats, CurrentHP bounds) match expectations.


Examples (copy-paste patterns)
- Create/extend an Offsets struct near `SAV*.cs` rather than using raw numbers. Example usage in `SAV1`:
  - `Offsets = Japanese ? SAV1Offsets.JPN : SAV1Offsets.INT;`
- Use `GetString(Data.AsSpan(offset, length))` and `SetString(Data.AsSpan(offset, length+1), value, maxLength, StringConverterOption.ClearZero)` for name fields.
- Use `BinaryPrimitives.WriteUInt32LittleEndian(span, value)` for writing seeds and `ReadUInt32LittleEndian(span)` for reading.

Gen7/8/9 concrete snippets
- Compute group seed (xoroshiro offset) and verification (SpawnerUtil8a.cs)

```csharp
private static ulong ComputeGroupSeed(ulong seed) => unchecked(seed - Xoroshiro128Plus.XOROSHIRO_CONST);
var rand = new Xoroshiro128Plus(groupSeed);
if (entry.GenerateSeed != rand.Next()) /* mismatch */;
```

- Per-entry seed and alpha move index via xoroshiro (SpawnerEntry8a.cs)

```csharp
public ulong GenerateSeed { get => ReadUInt64LittleEndian(Data[0x20..]); set => WriteUInt64LittleEndian(Data[0x20..], value); }
var rand = new Xoroshiro128Plus(AlphaSeed);
return (int)rand.NextInt((ulong)count);
```

- Raid seed storage and comment (RaidSpawnList8.cs)

```csharp
[Category(General), Description("RNG Seed for generating the Raid's content (64bit).")]
public ulong Seed { get => ReadUInt64LittleEndian(Data.AsSpan(Offset + 8)); set => WriteUInt64LittleEndian(Data.AsSpan(Offset + 8), value); }
// The games use a xoroshiro RNG to create the PKM from the stored seed.
```

- Gen8 SetPKM flow note (SAV8SWSH.cs)

```csharp
PK8 pk8 = (PK8)pk;
pk8.Trade(this, Date.Day, Date.Month, Date.Year);
pk8.RefreshChecksum();
AddCountAcquired(pk8);
```

Testing guidance
- Focused test runs are fast. Use `--filter` to run specific test classes when iterating.
- Tests to consult when touching RNG: `Tests/PKHeX.Core.Tests/General/Xoroshiro128Tests.cs`, `Xoroshiro128bTests.cs`, and `Tests/PKHeX.Core.Tests/Legality/Wild8aRNGTests.cs`.
- Tests to consult when touching parsing: `Tests/PKHeX.Core.Tests/Saves/*` and `PKM/*`.

Quality gates
- Build: `dotnet build PKHeX.sln --configuration Debug` (or build specific projects).
- Lint/Typecheck: rely on compiler warnings; maintain `Nullable` correctness.
- Unit tests: `dotnet test Tests/PKHeX.Core.Tests/PKHeX.Core.Tests.csproj`.
- Smoke: attempt a small roundtrip serialization of an edited save class (create an instance, `Write()`, create a new instance from written bytes, and compare key fields).

When to ask the human
- If a public API in `PKHeX.Core` needs to change (signature/behavior), ask for approval.
- If you need test fixtures (sample saves) to validate a change but they are not present, request them.

Delivery note
- Commit messages should be short and descriptive: `PKHeX.Core: Fix <area> - <reason>`.

Feedback
Tell me which module you want more examples from (e.g., `Saves/SAV8*` raid generation, or `Legality/*` analyzers) and I will expand the handbook with concrete snippets and tests.
