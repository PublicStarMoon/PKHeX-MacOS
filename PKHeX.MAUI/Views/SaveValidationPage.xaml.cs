using PKHeX.Core;
using PKHeX.MAUI.Utilities;
using System.Text;

namespace PKHeX.MAUI.Views;

public partial class SaveValidationPage : ContentPage
{
    private SaveFile? _saveFile;
    private int _totalPokemonCount;
    private int _issuesFound;
    private int _issuesFixed;

    public SaveValidationPage()
    {
        InitializeComponent();
    }

    public SaveValidationPage(SaveFile saveFile) : this()
    {
        _saveFile = saveFile;
        UpdateGameInfo();
        UpdateStatistics();
    }

    private void UpdateGameInfo()
    {
        if (_saveFile == null)
        {
            GameInfoLabel.Text = "No save file loaded";
            return;
        }

        var info = new StringBuilder();
        info.AppendLine($"Game: {_saveFile.Version} (Generation {_saveFile.Generation})");
        info.AppendLine($"OT: {_saveFile.OT} (TID: {_saveFile.DisplayTID})");
        info.AppendLine($"Language: {_saveFile.Language}");
        
        if (_saveFile.Generation >= 9)
        {
            info.AppendLine();
            info.AppendLine("⚠️ SCARLET/VIOLET WARNINGS:");
            info.AppendLine("• Pokemon with incorrect trainer info will disobey");
            info.AppendLine("• Legacy items (Z-Crystals, Mega Stones) will be invisible");
            info.AppendLine("• Use validation tools to prevent these issues");
        }
        else if (_saveFile.Generation >= 8)
        {
            info.AppendLine();
            info.AppendLine("ℹ️ SWORD/SHIELD/BDSP NOTES:");
            info.AppendLine("• Some legacy items may not be available");
            info.AppendLine("• Validation recommended for transferred Pokemon");
        }

        GameInfoLabel.Text = info.ToString();
    }

    private void UpdateStatistics()
    {
        TotalPokemonLabel.Text = _totalPokemonCount.ToString();
        IssuesFoundLabel.Text = _issuesFound.ToString();
        IssuesFixedLabel.Text = _issuesFixed.ToString();
    }

    private async void OnScanAllClicked(object sender, EventArgs e)
    {
        if (_saveFile == null)
        {
            await DisplayAlert("Error", "No save file loaded", "OK");
            return;
        }

        try
        {
            var results = new StringBuilder();
            _totalPokemonCount = 0;
            _issuesFound = 0;

            results.AppendLine("🔍 COMPREHENSIVE SCAN RESULTS\n");

            // Scan trainer info issues
            var trainerIssues = await ScanTrainerIssues(false);
            if (trainerIssues > 0)
            {
                results.AppendLine($"⚠️ Trainer Info Issues: {trainerIssues} Pokemon with incorrect trainer data");
                _issuesFound += trainerIssues;
            }

            // Scan item issues
            var itemIssues = ItemHelper.ValidateAllPokemonItems(_saveFile);
            if (itemIssues.Count > 0)
            {
                results.AppendLine($"⚠️ Item Issues: {itemIssues.Count} Pokemon with unsafe items");
                foreach (var issue in itemIssues.Take(5)) // Show first 5
                {
                    results.AppendLine($"   • Box {issue.Box + 1}, Slot {issue.Slot + 1}: {issue.PokemonName} - {issue.Issue}");
                }
                if (itemIssues.Count > 5)
                {
                    results.AppendLine($"   • ... and {itemIssues.Count - 5} more items");
                }
                _issuesFound += itemIssues.Count;
            }

            // Scan legality issues
            var legalityIssues = await ScanLegalityIssues(false);
            if (legalityIssues > 0)
            {
                results.AppendLine($"⚠️ Legality Issues: {legalityIssues} Pokemon with validation problems");
                _issuesFound += legalityIssues;
            }

            if (_issuesFound == 0)
            {
                results.AppendLine("✅ No issues found! Your save file appears to be safe for current game.");
            }
            else
            {
                results.AppendLine($"\n💡 Total issues found: {_issuesFound}");
                results.AppendLine("Use the 'Fix All Issues' button to automatically resolve these problems.");
            }

            ResultsLabel.Text = results.ToString();
            UpdateStatistics();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Scan failed: {ex.Message}", "OK");
        }
    }

    private async void OnFixAllClicked(object sender, EventArgs e)
    {
        if (_saveFile == null)
        {
            await DisplayAlert("Error", "No save file loaded", "OK");
            return;
        }

        var shouldContinue = await DisplayAlert("Confirm", 
            "This will automatically fix all detected issues:\n\n" +
            "• Fix trainer info to prevent disobedience\n" +
            "• Remove unsafe items that could be invisible\n" +
            "• Apply basic legality fixes\n\n" +
            "Continue?", "Yes", "No");

        if (!shouldContinue) return;

        try
        {
            _issuesFixed = 0;
            var results = new StringBuilder();
            results.AppendLine("🔧 AUTOMATIC FIX RESULTS\n");

            // Fix trainer info
            var trainerFixed = await FixTrainerIssues();
            if (trainerFixed > 0)
            {
                results.AppendLine($"✅ Fixed trainer info for {trainerFixed} Pokemon");
                _issuesFixed += trainerFixed;
            }

            // Fix items
            var itemsFixed = ItemHelper.FixAllUnsafeItems(_saveFile);
            if (itemsFixed > 0)
            {
                results.AppendLine($"✅ Removed {itemsFixed} unsafe items");
                _issuesFixed += itemsFixed;
            }

            // Apply basic legality fixes
            var legalityFixed = await ApplyBasicLegalityFixes();
            if (legalityFixed > 0)
            {
                results.AppendLine($"✅ Applied basic fixes to {legalityFixed} Pokemon");
                _issuesFixed += legalityFixed;
            }

            if (_issuesFixed == 0)
            {
                results.AppendLine("ℹ️ No fixes were needed.");
            }
            else
            {
                results.AppendLine($"\n🎉 Successfully fixed {_issuesFixed} issues!");
                results.AppendLine("Your Pokemon should now work properly in the target game.");
            }

            ResultsLabel.Text = results.ToString();
            UpdateStatistics();

            await DisplayAlert("Success", $"Fixed {_issuesFixed} issues successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Fix failed: {ex.Message}", "OK");
        }
    }

    private async void OnCheckTrainerClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var issues = await ScanTrainerIssues(true);
            TrainerStatusLabel.Text = issues == 0 
                ? "✅ All trainer info is correct" 
                : $"⚠️ {issues} Pokemon have incorrect trainer info";
        }
        catch (Exception ex)
        {
            TrainerStatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }

    private async void OnFixTrainerClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var fixedCount = await FixTrainerIssues();
            TrainerStatusLabel.Text = fixedCount == 0 
                ? "✅ No trainer fixes needed" 
                : $"✅ Fixed trainer info for {fixedCount} Pokemon";
        }
        catch (Exception ex)
        {
            TrainerStatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }

    private async void OnCheckItemsClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var issues = ItemHelper.ValidateAllPokemonItems(_saveFile);
            ItemStatusLabel.Text = issues.Count == 0 
                ? "✅ All items are safe" 
                : $"⚠️ {issues.Count} Pokemon have unsafe items";
        }
        catch (Exception ex)
        {
            ItemStatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }

    private async void OnFixItemsClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var fixedCount = ItemHelper.FixAllUnsafeItems(_saveFile);
            ItemStatusLabel.Text = fixedCount == 0 
                ? "✅ No item fixes needed" 
                : $"✅ Removed {fixedCount} unsafe items";
        }
        catch (Exception ex)
        {
            ItemStatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }

    private async void OnCheckLegalityClicked(object sender, EventArgs e)
    {
        if (_saveFile == null) return;

        try
        {
            var issues = await ScanLegalityIssues(true);
            LegalityStatusLabel.Text = issues == 0 
                ? "✅ All Pokemon are legal" 
                : $"⚠️ {issues} Pokemon have legality issues";
        }
        catch (Exception ex)
        {
            LegalityStatusLabel.Text = $"❌ Error: {ex.Message}";
        }
    }

    private async Task<int> ScanTrainerIssues(bool updateDisplay)
    {
        int issueCount = 0;
        var results = new StringBuilder();
        
        if (_saveFile.Generation >= 9)
        {
            results.AppendLine("🔍 TRAINER INFO VALIDATION (Scarlet/Violet)\n");
        }

        for (int box = 0; box < _saveFile.BoxCount; box++)
        {
            for (int slot = 0; slot < _saveFile.BoxSlotCount; slot++)
            {
                var pokemon = _saveFile.GetBoxSlotAtIndex(box, slot);
                if (pokemon?.Species > 0)
                {
                    _totalPokemonCount++;
                    
                    if (!PokemonHelper.ValidateTrainerInfo(pokemon, _saveFile))
                    {
                        issueCount++;
                        
                        if (updateDisplay && issueCount <= 10) // Show first 10
                        {
                            var speciesName = GameInfo.GetStrings(_saveFile.Language).specieslist[pokemon.Species];
                            results.AppendLine($"⚠️ Box {box + 1}, Slot {slot + 1}: {speciesName}");
                            results.AppendLine($"   OT: {pokemon.OT_Name} (Expected: {_saveFile.OT})");
                            results.AppendLine($"   TID: {pokemon.TID16} (Expected: {_saveFile.TID16})");
                        }
                    }
                }
            }
        }

        if (updateDisplay)
        {
            if (issueCount == 0)
            {
                results.AppendLine("✅ All Pokemon have correct trainer info!");
            }
            else
            {
                if (issueCount > 10)
                {
                    results.AppendLine($"... and {issueCount - 10} more Pokemon with trainer issues");
                }
                results.AppendLine($"\n💡 These Pokemon may disobey in Scarlet/Violet due to trainer mismatch.");
            }
            
            ResultsLabel.Text = results.ToString();
        }

        return issueCount;
    }

    private async Task<int> FixTrainerIssues()
    {
        int fixedCount = 0;

        for (int box = 0; box < _saveFile.BoxCount; box++)
        {
            for (int slot = 0; slot < _saveFile.BoxSlotCount; slot++)
            {
                var pokemon = _saveFile.GetBoxSlotAtIndex(box, slot);
                if (pokemon?.Species > 0)
                {
                    if (!PokemonHelper.ValidateTrainerInfo(pokemon, _saveFile))
                    {
                        PokemonHelper.FixTrainerInfo(pokemon, _saveFile);
                        _saveFile.SetBoxSlotAtIndex(pokemon, box, slot);
                        fixedCount++;
                    }
                }
            }
        }

        return fixedCount;
    }

    private async Task<int> ScanLegalityIssues(bool updateDisplay)
    {
        int issueCount = 0;
        var results = new StringBuilder();
        
        if (updateDisplay)
        {
            results.AppendLine("🔍 LEGALITY VALIDATION\n");
        }

        for (int box = 0; box < _saveFile.BoxCount; box++)
        {
            for (int slot = 0; slot < _saveFile.BoxSlotCount; slot++)
            {
                var pokemon = _saveFile.GetBoxSlotAtIndex(box, slot);
                if (pokemon?.Species > 0)
                {
                    if (!PokemonHelper.IsLegal(pokemon, _saveFile))
                    {
                        issueCount++;
                        
                        if (updateDisplay && issueCount <= 5) // Show first 5
                        {
                            var speciesName = GameInfo.GetStrings(_saveFile.Language).specieslist[pokemon.Species];
                            var summary = PokemonHelper.GetLegalitySummary(pokemon, _saveFile);
                            results.AppendLine($"⚠️ Box {box + 1}, Slot {slot + 1}: {speciesName}");
                            results.AppendLine($"   Issue: {summary}");
                        }
                    }
                }
            }
        }

        if (updateDisplay)
        {
            if (issueCount == 0)
            {
                results.AppendLine("✅ All Pokemon pass legality validation!");
            }
            else
            {
                if (issueCount > 5)
                {
                    results.AppendLine($"... and {issueCount - 5} more Pokemon with legality issues");
                }
            }
            
            ResultsLabel.Text = results.ToString();
        }

        return issueCount;
    }

    private async Task<int> ApplyBasicLegalityFixes()
    {
        int fixedCount = 0;

        for (int box = 0; box < _saveFile.BoxCount; box++)
        {
            for (int slot = 0; slot < _saveFile.BoxSlotCount; slot++)
            {
                var pokemon = _saveFile.GetBoxSlotAtIndex(box, slot);
                if (pokemon?.Species > 0)
                {
                    var wasLegal = PokemonHelper.IsLegal(pokemon, _saveFile);
                    
                    if (!wasLegal)
                    {
                        // Apply basic fixes
                        pokemon.Heal(); // Fix stats
                        
                        // Fix moves using basic approach since batch API is internal
                        // For now, just heal PP which is basic legality
                        pokemon.Heal(); // This includes HealPP()
                        
                        // Could add basic move validation here if needed
                        // var la = new LegalityAnalysis(pokemon);
                        // var info = new BatchInfo(pokemon, la);
                        // PKHeX.Core.BatchModifications.SetSuggestedMoveset(info);
                        
                        // Re-check if it's now legal
                        if (PokemonHelper.IsLegal(pokemon, _saveFile))
                        {
                            _saveFile.SetBoxSlotAtIndex(pokemon, box, slot);
                            fixedCount++;
                        }
                    }
                }
            }
        }

        return fixedCount;
    }
}
