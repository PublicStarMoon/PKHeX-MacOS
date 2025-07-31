using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using PKHeX.MAUI.UITests.Utilities;

namespace PKHeX.MAUI.UITests.PageObjects;

public class PokemonBoxPage
{
    private readonly AppiumDriver _driver;
    private readonly WaitHelper _waitHelper;

    public PokemonBoxPage(AppiumDriver driver)
    {
        _driver = driver;
        _waitHelper = new WaitHelper();
        _waitHelper.Initialize(driver);
    }

    // Locators for Pokemon Box page elements
    private By PageTitle => By.XPath("//Label[contains(@Text, 'Pokemon Box Editor')]");
    private By BackButton => By.XPath("//Button[contains(@Text, '← Back') or contains(@Text, 'Back')]");
    private By SaveButton => By.XPath("//Button[contains(@Text, 'Save Changes')]");
    private By ExportBoxButton => By.XPath("//Button[contains(@Text, 'Export Box')]");
    
    // Box selection and navigation
    private By BoxPicker => By.XPath("//Picker[@x:Name='BoxPicker']");
    private By BoxCountLabel => By.XPath("//Label[@x:Name='BoxCountLabel']");
    private By BoxNameLabel => By.XPath("//Label[@x:Name='BoxNameLabel']");
    
    // Pokemon slots in the box grid
    private By PokemonGrid => By.XPath("//CollectionView[@x:Name='PokemonGrid']");
    private By EmptySlot => By.XPath("//Frame[contains(@BackgroundColor, '#ECF0F1') or @x:Name='EmptySlotFrame']");
    private By PokemonSlot => By.XPath("//Frame[@x:Name='PokemonSlotFrame']");
    
    // Add/Edit Pokemon controls
    private By AddPokemonButton => By.XPath("//Button[contains(@Text, '+') or contains(@Text, 'Add')]");
    private By EditPokemonButton => By.XPath("//Button[contains(@Text, 'Edit') or contains(@Text, '✏️')]");
    private By DeletePokemonButton => By.XPath("//Button[contains(@Text, 'Delete') or contains(@Text, '🗑️')]");
    
    // Pokemon info display
    private By SelectedPokemonInfo => By.XPath("//Frame[@x:Name='SelectedPokemonFrame']");
    private By PokemonNameLabel => By.XPath("//Label[@x:Name='PokemonNameLabel']");
    private By PokemonLevelLabel => By.XPath("//Label[@x:Name='PokemonLevelLabel']");
    private By PokemonSpeciesLabel => By.XPath("//Label[@x:Name='PokemonSpeciesLabel']");

    /// <summary>
    /// Waits for the Pokemon Box page to load completely
    /// </summary>
    public async Task WaitForPageToLoad()
    {
        _waitHelper.WaitForElement(PageTitle, TestDataHelper.Timeouts.MediumWait);
        _waitHelper.WaitForElement(PokemonGrid, TestDataHelper.Timeouts.MediumWait);
        
        // Wait for the page to fully initialize
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Selects an empty slot in the Pokemon box
    /// </summary>
    public async Task SelectEmptySlot(int slotIndex = 0)
    {
        await WaitForPageToLoad();
        
        try
        {
            // Try to find empty slots
            var emptySlots = _driver.FindElements(EmptySlot);
            
            if (emptySlots.Count > slotIndex)
            {
                emptySlots[slotIndex].Click();
                await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            }
            else
            {
                // Alternative: click on first available slot in the grid
                var gridSlots = _driver.FindElements(By.XPath("//CollectionView[@x:Name='PokemonGrid']//Frame"));
                if (gridSlots.Count > slotIndex)
                {
                    gridSlots[slotIndex].Click();
                    await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
                }
                else
                {
                    throw new InvalidOperationException($"Could not find slot at index {slotIndex}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not select empty slot: {ex.Message}");
            // Try alternative approach - click on general grid area
            var grid = _waitHelper.WaitForElement(PokemonGrid, TestDataHelper.Timeouts.ShortWait);
            grid.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }
    }

    /// <summary>
    /// Clicks the Add Pokemon button to create a new Pokemon
    /// </summary>
    public async Task ClickAddPokemon()
    {
        try
        {
            var addButton = _waitHelper.WaitForElementToBeClickable(AddPokemonButton, TestDataHelper.Timeouts.MediumWait);
            addButton.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not find Add Pokemon button: {ex.Message}");
            // Try alternative - right-click on empty slot might bring up context menu
            await HandleAddPokemonAlternative();
        }
    }

    /// <summary>
    /// Alternative method to add Pokemon if direct button is not available
    /// </summary>
    private async Task HandleAddPokemonAlternative()
    {
        try
        {
            // Some implementations might require double-clicking empty slot
            var emptySlots = _driver.FindElements(EmptySlot);
            if (emptySlots.Count > 0)
            {
                var slot = emptySlots[0];
                
                // Double-click the empty slot
                slot.Click();
                await _waitHelper.WaitAsync(500);
                slot.Click();
                await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alternative add Pokemon method failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigates to Pokemon Editor for detailed editing
    /// </summary>
    public async Task NavigateToPokemonEditor()
    {
        try
        {
            var editButton = _waitHelper.WaitForElementToBeClickable(EditPokemonButton, TestDataHelper.Timeouts.MediumWait);
            editButton.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not find Edit button: {ex.Message}");
            // Try double-clicking on selected Pokemon slot
            await HandleNavigateToPokemonEditorAlternative();
        }
    }

    /// <summary>
    /// Alternative method to navigate to Pokemon editor
    /// </summary>
    private async Task HandleNavigateToPokemonEditorAlternative()
    {
        try
        {
            // Try double-clicking on a Pokemon slot that has a Pokemon
            var pokemonSlots = _driver.FindElements(PokemonSlot);
            if (pokemonSlots.Count > 0)
            {
                var slot = pokemonSlots[0];
                
                // Double-click to open editor
                slot.Click();
                await _waitHelper.WaitAsync(500);
                slot.Click();
                await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alternative navigate to editor method failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Selects a specific box by index
    /// </summary>
    public async Task SelectBox(int boxIndex)
    {
        try
        {
            var boxPicker = _waitHelper.WaitForElementToBeClickable(BoxPicker, TestDataHelper.Timeouts.MediumWait);
            boxPicker.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            
            // Select the box at the specified index
            // This might need adjustment based on the actual picker implementation
            var boxOptions = _driver.FindElements(By.XPath("//Picker//option | //PickerItem"));
            if (boxOptions.Count > boxIndex)
            {
                boxOptions[boxIndex].Click();
                await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not select box {boxIndex}: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves changes made in the box
    /// </summary>
    public async Task SaveChanges()
    {
        var saveButton = _waitHelper.WaitForElementToBeClickable(SaveButton, TestDataHelper.Timeouts.MediumWait);
        saveButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    /// <summary>
    /// Exports the current box
    /// </summary>
    public async Task ExportBox()
    {
        var exportButton = _waitHelper.WaitForElementToBeClickable(ExportBoxButton, TestDataHelper.Timeouts.MediumWait);
        exportButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    /// <summary>
    /// Goes back to the main page
    /// </summary>
    public async Task GoBack()
    {
        var backButton = _waitHelper.WaitForElementToBeClickable(BackButton, TestDataHelper.Timeouts.MediumWait);
        backButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    // Property accessors for verification
    public string GetPageTitle()
    {
        var element = _waitHelper.WaitForElement(PageTitle, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetBoxCount()
    {
        try
        {
            var element = _waitHelper.WaitForElement(BoxCountLabel, TestDataHelper.Timeouts.ShortWait);
            return element.Text;
        }
        catch
        {
            return "Unknown";
        }
    }

    public string GetSelectedPokemonInfo()
    {
        try
        {
            var pokemonName = GetSelectedPokemonName();
            var pokemonLevel = GetSelectedPokemonLevel();
            var pokemonSpecies = GetSelectedPokemonSpecies();
            
            return $"{pokemonName} - {pokemonSpecies} - Level {pokemonLevel}";
        }
        catch
        {
            return "No Pokemon selected";
        }
    }

    public string GetSelectedPokemonName()
    {
        var element = _waitHelper.WaitForElement(PokemonNameLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetSelectedPokemonLevel()
    {
        var element = _waitHelper.WaitForElement(PokemonLevelLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetSelectedPokemonSpecies()
    {
        var element = _waitHelper.WaitForElement(PokemonSpeciesLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public int GetEmptySlotCount()
    {
        try
        {
            var emptySlots = _driver.FindElements(EmptySlot);
            return emptySlots.Count;
        }
        catch
        {
            return 0;
        }
    }

    public int GetOccupiedSlotCount()
    {
        try
        {
            var pokemonSlots = _driver.FindElements(PokemonSlot);
            return pokemonSlots.Count;
        }
        catch
        {
            return 0;
        }
    }

    public bool IsPageLoaded()
    {
        return _waitHelper.IsElementVisible(PageTitle, TestDataHelper.Timeouts.ShortWait) &&
               _waitHelper.IsElementVisible(PokemonGrid, TestDataHelper.Timeouts.ShortWait);
    }

    public bool HasPokemonSelected()
    {
        return _waitHelper.IsElementVisible(SelectedPokemonInfo, TestDataHelper.Timeouts.ShortWait);
    }

    public bool CanAddPokemon()
    {
        return _waitHelper.IsElementEnabled(AddPokemonButton, TestDataHelper.Timeouts.ShortWait) ||
               GetEmptySlotCount() > 0;
    }
}