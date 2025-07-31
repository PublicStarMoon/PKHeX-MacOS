using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using PKHeX.MAUI.UITests.Utilities;

namespace PKHeX.MAUI.UITests.PageObjects;

public class PokemonEditorPage
{
    private readonly AppiumDriver _driver;
    private readonly WaitHelper _waitHelper;

    public PokemonEditorPage(AppiumDriver driver)
    {
        _driver = driver;
        _waitHelper = new WaitHelper();
        _waitHelper.Initialize(driver);
    }

    // Page header and navigation
    private By PageTitle => By.XPath("//Label[contains(@Text, 'Pokemon Editor')]");
    private By BackButton => By.XPath("//Button[contains(@Text, '← Back') or contains(@Text, 'Back')]");
    private By SaveButton => By.XPath("//Button[contains(@Text, '💾') or contains(@Text, 'Save')]");

    // Basic Pokemon information
    private By SpeciesPicker => By.XPath("//Picker[@x:Name='SpeciesPicker']");
    private By NicknameEntry => By.XPath("//Entry[@x:Name='NicknameEntry']");
    private By LevelEntry => By.XPath("//Entry[@x:Name='LevelEntry']");
    private By NaturePicker => By.XPath("//Picker[@x:Name='NaturePicker']");
    private By GenderPicker => By.XPath("//Picker[@x:Name='GenderPicker']");
    private By AbilityPicker => By.XPath("//Picker[@x:Name='AbilityPicker']");

    // Pokemon stats
    private By HpStat => By.XPath("//Entry[@x:Name='HpEntry']");
    private By AttackStat => By.XPath("//Entry[@x:Name='AttackEntry']");
    private By DefenseStat => By.XPath("//Entry[@x:Name='DefenseEntry']");
    private By SpAttackStat => By.XPath("//Entry[@x:Name='SpAttackEntry']");
    private By SpDefenseStat => By.XPath("//Entry[@x:Name='SpDefenseEntry']");
    private By SpeedStat => By.XPath("//Entry[@x:Name='SpeedEntry']");

    // Moves section
    private By Move1Picker => By.XPath("//Picker[@x:Name='Move1Picker']");
    private By Move2Picker => By.XPath("//Picker[@x:Name='Move2Picker']");
    private By Move3Picker => By.XPath("//Picker[@x:Name='Move3Picker']");
    private By Move4Picker => By.XPath("//Picker[@x:Name='Move4Picker']");

    // Other Pokemon details
    private By HeldItemPicker => By.XPath("//Picker[@x:Name='HeldItemPicker']");
    private By BallPicker => By.XPath("//Picker[@x:Name='BallPicker']");
    private By OriginalTrainerEntry => By.XPath("//Entry[@x:Name='OriginalTrainerEntry']");
    private By TrainerIdEntry => By.XPath("//Entry[@x:Name='TrainerIdEntry']");

    // Pokemon display
    private By PokemonSprite => By.XPath("//Image[@x:Name='PokemonSprite']");
    private By PokemonInfo => By.XPath("//Label[@x:Name='PokemonInfoLabel']");

    /// <summary>
    /// Waits for the Pokemon Editor page to load completely
    /// </summary>
    public async Task WaitForPageToLoad()
    {
        _waitHelper.WaitForElement(PageTitle, TestDataHelper.Timeouts.MediumWait);
        
        // Wait for main form elements to be available
        _waitHelper.WaitForElement(SpeciesPicker, TestDataHelper.Timeouts.MediumWait);
        _waitHelper.WaitForElement(NicknameEntry, TestDataHelper.Timeouts.MediumWait);
        
        // Allow time for page to fully initialize
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Sets the Pokemon species
    /// </summary>
    public async Task SetSpecies(string species)
    {
        await ClickPicker(SpeciesPicker);
        await SelectPickerOption(species);
    }

    /// <summary>
    /// Sets the Pokemon nickname
    /// </summary>
    public async Task SetNickname(string nickname)
    {
        var nicknameField = _waitHelper.WaitForElementToBeClickable(NicknameEntry, TestDataHelper.Timeouts.MediumWait);
        
        // Clear existing text and enter new nickname
        nicknameField.Clear();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
        
        nicknameField.SendKeys(nickname);
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
    }

    /// <summary>
    /// Sets the Pokemon level
    /// </summary>
    public async Task SetLevel(int level)
    {
        var levelField = _waitHelper.WaitForElementToBeClickable(LevelEntry, TestDataHelper.Timeouts.MediumWait);
        
        levelField.Clear();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
        
        levelField.SendKeys(level.ToString());
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
    }

    /// <summary>
    /// Sets the Pokemon nature
    /// </summary>
    public async Task SetNature(string nature)
    {
        await ClickPicker(NaturePicker);
        await SelectPickerOption(nature);
    }

    /// <summary>
    /// Sets the Pokemon gender
    /// </summary>
    public async Task SetGender(string gender)
    {
        await ClickPicker(GenderPicker);
        await SelectPickerOption(gender);
    }

    /// <summary>
    /// Sets the Pokemon ability
    /// </summary>
    public async Task SetAbility(string ability)
    {
        await ClickPicker(AbilityPicker);
        await SelectPickerOption(ability);
    }

    /// <summary>
    /// Sets Pokemon moves
    /// </summary>
    public async Task SetMoves(string[] moves)
    {
        var movePickers = new[] { Move1Picker, Move2Picker, Move3Picker, Move4Picker };
        
        for (int i = 0; i < Math.Min(moves.Length, 4); i++)
        {
            if (!string.IsNullOrEmpty(moves[i]))
            {
                await ClickPicker(movePickers[i]);
                await SelectPickerOption(moves[i]);
            }
        }
    }

    /// <summary>
    /// Sets a specific move slot
    /// </summary>
    public async Task SetMove(int moveSlot, string move)
    {
        var movePickers = new[] { Move1Picker, Move2Picker, Move3Picker, Move4Picker };
        
        if (moveSlot >= 1 && moveSlot <= 4 && !string.IsNullOrEmpty(move))
        {
            await ClickPicker(movePickers[moveSlot - 1]);
            await SelectPickerOption(move);
        }
    }

    /// <summary>
    /// Sets the held item
    /// </summary>
    public async Task SetHeldItem(string item)
    {
        await ClickPicker(HeldItemPicker);
        await SelectPickerOption(item);
    }

    /// <summary>
    /// Sets the Pokeball type
    /// </summary>
    public async Task SetPokeball(string ball)
    {
        await ClickPicker(BallPicker);
        await SelectPickerOption(ball);
    }

    /// <summary>
    /// Sets the original trainer name
    /// </summary>
    public async Task SetOriginalTrainer(string trainerName)
    {
        var trainerField = _waitHelper.WaitForElementToBeClickable(OriginalTrainerEntry, TestDataHelper.Timeouts.MediumWait);
        
        trainerField.Clear();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
        
        trainerField.SendKeys(trainerName);
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
    }

    /// <summary>
    /// Sets stats manually (for advanced editing)
    /// </summary>
    public async Task SetStats(Dictionary<string, int> stats)
    {
        if (stats.ContainsKey("HP"))
            await SetStat(HpStat, stats["HP"]);
        
        if (stats.ContainsKey("Attack"))
            await SetStat(AttackStat, stats["Attack"]);
            
        if (stats.ContainsKey("Defense"))
            await SetStat(DefenseStat, stats["Defense"]);
            
        if (stats.ContainsKey("SpAttack"))
            await SetStat(SpAttackStat, stats["SpAttack"]);
            
        if (stats.ContainsKey("SpDefense"))
            await SetStat(SpDefenseStat, stats["SpDefense"]);
            
        if (stats.ContainsKey("Speed"))
            await SetStat(SpeedStat, stats["Speed"]);
    }

    /// <summary>
    /// Helper method to set individual stat
    /// </summary>
    private async Task SetStat(By statLocator, int value)
    {
        try
        {
            var statField = _waitHelper.WaitForElementToBeClickable(statLocator, TestDataHelper.Timeouts.ShortWait);
            statField.Clear();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
            statField.SendKeys(value.ToString());
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not set stat: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic method to click a picker element
    /// </summary>
    private async Task ClickPicker(By pickerLocator)
    {
        var picker = _waitHelper.WaitForElementToBeClickable(pickerLocator, TestDataHelper.Timeouts.MediumWait);
        picker.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    /// <summary>
    /// Generic method to select an option from a picker
    /// </summary>
    private async Task SelectPickerOption(string optionText)
    {
        try
        {
            // Look for the option in the picker dropdown
            var option = _waitHelper.WaitForElementToBeClickable(
                By.XPath($"//option[contains(@Text, '{optionText}')] | //PickerItem[contains(@Text, '{optionText}')]"),
                TestDataHelper.Timeouts.MediumWait);
            
            option.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not select picker option '{optionText}': {ex.Message}");
            
            // Try alternative approach - use keyboard navigation
            await TryKeyboardSelection(optionText);
        }
    }

    /// <summary>
    /// Alternative method to select picker option using keyboard
    /// </summary>
    private async Task TryKeyboardSelection(string optionText)
    {
        try
        {
            // Send keys to search for the option
            var activeElement = _driver.SwitchTo().ActiveElement();
            activeElement.SendKeys(optionText.Substring(0, Math.Min(3, optionText.Length)));
            await _waitHelper.WaitAsync(500);
            
            activeElement.SendKeys(OpenQA.Selenium.Keys.Enter);
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Keyboard selection also failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves the Pokemon changes
    /// </summary>
    public async Task SaveChanges()
    {
        var saveButton = _waitHelper.WaitForElementToBeClickable(SaveButton, TestDataHelper.Timeouts.MediumWait);
        saveButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    /// <summary>
    /// Goes back to the previous page
    /// </summary>
    public async Task GoBack()
    {
        var backButton = _waitHelper.WaitForElementToBeClickable(BackButton, TestDataHelper.Timeouts.MediumWait);
        backButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    // Property accessors for verification
    public string GetCurrentSpecies()
    {
        try
        {
            var speciesPicker = _waitHelper.WaitForElement(SpeciesPicker, TestDataHelper.Timeouts.ShortWait);
            return speciesPicker.Text;
        }
        catch
        {
            return "Unknown";
        }
    }

    public string GetCurrentNickname()
    {
        try
        {
            var nicknameField = _waitHelper.WaitForElement(NicknameEntry, TestDataHelper.Timeouts.ShortWait);
            return nicknameField.GetAttribute("value") ?? nicknameField.Text;
        }
        catch
        {
            return "";
        }
    }

    public string GetCurrentLevel()
    {
        try
        {
            var levelField = _waitHelper.WaitForElement(LevelEntry, TestDataHelper.Timeouts.ShortWait);
            return levelField.GetAttribute("value") ?? levelField.Text;
        }
        catch
        {
            return "0";
        }
    }

    public string GetCurrentNature()
    {
        try
        {
            var naturePicker = _waitHelper.WaitForElement(NaturePicker, TestDataHelper.Timeouts.ShortWait);
            return naturePicker.Text;
        }
        catch
        {
            return "Unknown";
        }
    }

    public string[] GetCurrentMoves()
    {
        var moves = new string[4];
        var movePickers = new[] { Move1Picker, Move2Picker, Move3Picker, Move4Picker };
        
        for (int i = 0; i < 4; i++)
        {
            try
            {
                var movePicker = _waitHelper.WaitForElement(movePickers[i], TestDataHelper.Timeouts.ShortWait);
                moves[i] = movePicker.Text;
            }
            catch
            {
                moves[i] = "";
            }
        }
        
        return moves;
    }

    public bool IsPageLoaded()
    {
        return _waitHelper.IsElementVisible(PageTitle, TestDataHelper.Timeouts.ShortWait) &&
               _waitHelper.IsElementVisible(SpeciesPicker, TestDataHelper.Timeouts.ShortWait);
    }

    public bool CanSave()
    {
        return _waitHelper.IsElementEnabled(SaveButton, TestDataHelper.Timeouts.ShortWait);
    }

    /// <summary>
    /// Performs a complete Pokemon setup with provided data
    /// </summary>
    public async Task SetupPokemon(string species, string nickname, int level, string nature, string[] moves)
    {
        await WaitForPageToLoad();
        
        // Set basic information
        await SetSpecies(species);
        await SetNickname(nickname);
        await SetLevel(level);
        await SetNature(nature);
        
        // Set moves
        await SetMoves(moves);
        
        // Allow time for all changes to be processed
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }
}