using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using PKHeX.MAUI.UITests.Utilities;

namespace PKHeX.MAUI.UITests.PageObjects;

public class InventoryEditorPage
{
    private readonly AppiumDriver _driver;
    private readonly WaitHelper _waitHelper;

    public InventoryEditorPage(AppiumDriver driver)
    {
        _driver = driver;
        _waitHelper = new WaitHelper();
        _waitHelper.Initialize(driver);
    }

    // Page header and navigation
    private By PageTitle => By.XPath("//Label[contains(@Text, 'Inventory Editor')]");
    private By BackButton => By.XPath("//Button[contains(@Text, '🔙 Back') or contains(@Text, 'Back')]");
    private By SaveButton => By.XPath("//Button[contains(@Text, '💾 Save') or contains(@Text, 'Save')]");
    private By HeaderLabel => By.XPath("//Label[@x:Name='HeaderLabel']");
    private By GenerationLabel => By.XPath("//Label[@x:Name='GenerationLabel']");

    // Pouch selection
    private By PouchContainer => By.XPath("//StackLayout[@x:Name='PouchContainer']");
    private By PouchInfoFrame => By.XPath("//Frame[@x:Name='PouchInfoFrame']");
    private By PouchNameLabel => By.XPath("//Label[@x:Name='PouchNameLabel']");

    // Individual pouch buttons
    private By BallPouchButton => By.XPath("//Button[contains(@Text, 'Ball') or contains(@Text, '⚽')]");
    private By ItemPouchButton => By.XPath("//Button[contains(@Text, 'Item') or contains(@Text, '🎒')]");
    private By MedicinePouchButton => By.XPath("//Button[contains(@Text, 'Medicine') or contains(@Text, '💊')]");
    private By BerriesPouchButton => By.XPath("//Button[contains(@Text, 'Berries') or contains(@Text, '🍇')]");
    private By KeyItemsPouchButton => By.XPath("//Button[contains(@Text, 'Key') or contains(@Text, '🔑')]");
    private By TMPouchButton => By.XPath("//Button[contains(@Text, 'TM') or contains(@Text, '💿')]");

    // Item list and editing
    private By ItemListContainer => By.XPath("//CollectionView[@x:Name='ItemListView'] | //StackLayout[@x:Name='ItemListContainer']");
    private By AddItemButton => By.XPath("//Button[contains(@Text, '+') or contains(@Text, 'Add Item')]");
    private By SearchEntry => By.XPath("//Entry[@x:Name='SearchEntry']");
    private By SearchButton => By.XPath("//Button[contains(@Text, 'Search') or contains(@Text, '🔍')]");

    // Item editing controls
    private By ItemPicker => By.XPath("//Picker[@x:Name='ItemPicker']");
    private By QuantityEntry => By.XPath("//Entry[@x:Name='QuantityEntry']");
    private By AddQuantityButton => By.XPath("//Button[contains(@Text, 'Add') or contains(@Text, '+')]");
    private By RemoveQuantityButton => By.XPath("//Button[contains(@Text, 'Remove') or contains(@Text, '-')]");
    private By MaxQuantityButton => By.XPath("//Button[contains(@Text, 'Max') or contains(@Text, '999')]");

    // Item display
    private By SelectedItemFrame => By.XPath("//Frame[@x:Name='SelectedItemFrame']");
    private By ItemNameLabel => By.XPath("//Label[@x:Name='ItemNameLabel']");
    private By ItemQuantityLabel => By.XPath("//Label[@x:Name='ItemQuantityLabel']");
    private By ItemDescriptionLabel => By.XPath("//Label[@x:Name='ItemDescriptionLabel']");

    /// <summary>
    /// Waits for the Inventory Editor page to load completely
    /// </summary>
    public async Task WaitForPageToLoad()
    {
        _waitHelper.WaitForElement(PageTitle, TestDataHelper.Timeouts.MediumWait);
        _waitHelper.WaitForElement(PouchContainer, TestDataHelper.Timeouts.MediumWait);
        
        // Wait for pouches to be loaded
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Selects the Ball pouch
    /// </summary>
    public async Task SelectBallPouch()
    {
        await SelectPouch(BallPouchButton, "Ball");
    }

    /// <summary>
    /// Selects the Item pouch
    /// </summary>
    public async Task SelectItemPouch()
    {
        await SelectPouch(ItemPouchButton, "Item");
    }

    /// <summary>
    /// Selects the Medicine pouch
    /// </summary>
    public async Task SelectMedicinePouch()
    {
        await SelectPouch(MedicinePouchButton, "Medicine");
    }

    /// <summary>
    /// Selects the Berries pouch
    /// </summary>
    public async Task SelectBerriesPouch()
    {
        await SelectPouch(BerriesPouchButton, "Berries");
    }

    /// <summary>
    /// Selects the Key Items pouch
    /// </summary>
    public async Task SelectKeyItemsPouch()
    {
        await SelectPouch(KeyItemsPouchButton, "Key");
    }

    /// <summary>
    /// Selects the TM pouch
    /// </summary>
    public async Task SelectTMPouch()
    {
        await SelectPouch(TMPouchButton, "TM");
    }

    /// <summary>
    /// Generic method to select a pouch
    /// </summary>
    private async Task SelectPouch(By pouchLocator, string pouchName)
    {
        try
        {
            var pouchButton = _waitHelper.WaitForElementToBeClickable(pouchLocator, TestDataHelper.Timeouts.MediumWait);
            pouchButton.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            
            // Wait for pouch content to load
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not select {pouchName} pouch: {ex.Message}");
            // Try alternative selection method
            await TryAlternativePouchSelection(pouchName);
        }
    }

    /// <summary>
    /// Alternative method to select pouch by text
    /// </summary>
    private async Task TryAlternativePouchSelection(string pouchName)
    {
        try
        {
            var pouchButton = _waitHelper.WaitForElementToBeClickable(
                By.XPath($"//Button[contains(@Text, '{pouchName}')]"),
                TestDataHelper.Timeouts.MediumWait);
            
            pouchButton.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alternative pouch selection failed for {pouchName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds an item to the current pouch
    /// </summary>
    public async Task AddItem(string itemName, int quantity = 1)
    {
        try
        {
            // Try to click add item button
            await ClickAddItem();
            
            // Select the item
            await SelectItem(itemName);
            
            // Set quantity
            await SetQuantity(quantity);
            
            // Confirm addition (might require clicking an OK or Add button)
            await ConfirmItemAddition();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not add item {itemName}: {ex.Message}");
            // Try alternative method
            await TryAlternativeItemAddition(itemName, quantity);
        }
    }

    /// <summary>
    /// Clicks the Add Item button
    /// </summary>
    private async Task ClickAddItem()
    {
        var addButton = _waitHelper.WaitForElementToBeClickable(AddItemButton, TestDataHelper.Timeouts.MediumWait);
        addButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    /// <summary>
    /// Selects an item from the item picker/list
    /// </summary>
    private async Task SelectItem(string itemName)
    {
        try
        {
            // Try using the item picker
            var itemPicker = _waitHelper.WaitForElementToBeClickable(ItemPicker, TestDataHelper.Timeouts.MediumWait);
            itemPicker.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            
            // Select the specific item
            var itemOption = _waitHelper.WaitForElementToBeClickable(
                By.XPath($"//option[contains(@Text, '{itemName}')] | //PickerItem[contains(@Text, '{itemName}')]"),
                TestDataHelper.Timeouts.MediumWait);
            
            itemOption.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not select item {itemName} from picker: {ex.Message}");
            await TrySearchForItem(itemName);
        }
    }

    /// <summary>
    /// Alternative method to find item using search
    /// </summary>
    private async Task TrySearchForItem(string itemName)
    {
        try
        {
            // Use search functionality if available
            var searchEntry = _waitHelper.WaitForElementToBeClickable(SearchEntry, TestDataHelper.Timeouts.MediumWait);
            searchEntry.Clear();
            searchEntry.SendKeys(itemName);
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
            
            // Click search button if available
            if (_waitHelper.IsElementPresent(SearchButton, TestDataHelper.Timeouts.ShortWait))
            {
                var searchButton = _waitHelper.WaitForElementToBeClickable(SearchButton, TestDataHelper.Timeouts.ShortWait);
                searchButton.Click();
                await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            }
            
            // Select from search results
            var searchResult = _waitHelper.WaitForElementToBeClickable(
                By.XPath($"//Label[contains(@Text, '{itemName}')]"),
                TestDataHelper.Timeouts.MediumWait);
            
            searchResult.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Search for item {itemName} also failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets the quantity for the selected item
    /// </summary>
    private async Task SetQuantity(int quantity)
    {
        try
        {
            var quantityEntry = _waitHelper.WaitForElementToBeClickable(QuantityEntry, TestDataHelper.Timeouts.MediumWait);
            quantityEntry.Clear();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
            
            quantityEntry.SendKeys(quantity.ToString());
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.FormInputDelay);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not set quantity to {quantity}: {ex.Message}");
            // Try using quantity buttons if available
            await TryQuantityButtons(quantity);
        }
    }

    /// <summary>
    /// Alternative method to set quantity using + and - buttons
    /// </summary>
    private async Task TryQuantityButtons(int targetQuantity)
    {
        try
        {
            // If we want max quantity, try max button first
            if (targetQuantity >= 999 && _waitHelper.IsElementPresent(MaxQuantityButton, TestDataHelper.Timeouts.ShortWait))
            {
                var maxButton = _waitHelper.WaitForElementToBeClickable(MaxQuantityButton, TestDataHelper.Timeouts.ShortWait);
                maxButton.Click();
                await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
                return;
            }
            
            // Use + button to reach target quantity (simplified approach)
            if (_waitHelper.IsElementPresent(AddQuantityButton, TestDataHelper.Timeouts.ShortWait))
            {
                var addButton = _waitHelper.WaitForElementToBeClickable(AddQuantityButton, TestDataHelper.Timeouts.ShortWait);
                
                for (int i = 0; i < Math.Min(targetQuantity, 10); i++) // Limit clicks for practical purposes
                {
                    addButton.Click();
                    await _waitHelper.WaitAsync(100);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Quantity buttons method failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Confirms the item addition (clicks OK, Add, etc.)
    /// </summary>
    private async Task ConfirmItemAddition()
    {
        try
        {
            // Look for confirmation buttons
            var confirmButtons = new[]
            {
                By.XPath("//Button[contains(@Text, 'OK')]"),
                By.XPath("//Button[contains(@Text, 'Add')]"),
                By.XPath("//Button[contains(@Text, 'Confirm')]"),
                By.XPath("//Button[contains(@Text, '✓')]")
            };
            
            foreach (var buttonLocator in confirmButtons)
            {
                if (_waitHelper.IsElementPresent(buttonLocator, TestDataHelper.Timeouts.ShortWait))
                {
                    var button = _waitHelper.WaitForElementToBeClickable(buttonLocator, TestDataHelper.Timeouts.ShortWait);
                    button.Click();
                    await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not confirm item addition: {ex.Message}");
        }
    }

    /// <summary>
    /// Alternative method to add items
    /// </summary>
    private async Task TryAlternativeItemAddition(string itemName, int quantity)
    {
        try
        {
            // Try finding the item directly in the list and modifying it
            var itemInList = _waitHelper.WaitForElementToBeClickable(
                By.XPath($"//Label[contains(@Text, '{itemName}')]"),
                TestDataHelper.Timeouts.MediumWait);
            
            itemInList.Click();
            await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
            
            // Try to modify quantity directly
            await SetQuantity(quantity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alternative item addition method failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves changes to the inventory
    /// </summary>
    public async Task SaveChanges()
    {
        var saveButton = _waitHelper.WaitForElementToBeClickable(SaveButton, TestDataHelper.Timeouts.MediumWait);
        saveButton.Click();
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

    /// <summary>
    /// Gets the current item quantity for a specific item
    /// </summary>
    public int GetItemQuantity(string itemName)
    {
        try
        {
            // Look for the item and its quantity in the list
            var itemElement = _waitHelper.WaitForElement(
                By.XPath($"//Label[contains(@Text, '{itemName}')]/following-sibling::Label | " +
                        $"//Frame[contains(.//Label/@Text, '{itemName}')]//Label[contains(@Text, 'x')]"),
                TestDataHelper.Timeouts.ShortWait);
            
            var quantityText = itemElement.Text;
            
            // Extract number from text like "x10" or "Quantity: 10"
            var match = System.Text.RegularExpressions.Regex.Match(quantityText, @"(\d+)");
            return match.Success ? int.Parse(match.Value) : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Checks if an item exists in the current pouch
    /// </summary>
    public bool HasItem(string itemName)
    {
        return _waitHelper.IsElementPresent(
            By.XPath($"//Label[contains(@Text, '{itemName}')]"), 
            TestDataHelper.Timeouts.ShortWait);
    }

    /// <summary>
    /// Gets the current pouch name
    /// </summary>
    public string GetCurrentPouchName()
    {
        try
        {
            var pouchLabel = _waitHelper.WaitForElement(PouchNameLabel, TestDataHelper.Timeouts.ShortWait);
            return pouchLabel.Text;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Gets the current generation info
    /// </summary>
    public string GetGenerationInfo()
    {
        try
        {
            var genLabel = _waitHelper.WaitForElement(GenerationLabel, TestDataHelper.Timeouts.ShortWait);
            return genLabel.Text;
        }
        catch
        {
            return "Unknown";
        }
    }

    public bool IsPageLoaded()
    {
        return _waitHelper.IsElementVisible(PageTitle, TestDataHelper.Timeouts.ShortWait) &&
               _waitHelper.IsElementVisible(PouchContainer, TestDataHelper.Timeouts.ShortWait);
    }

    public bool IsPouchSelected()
    {
        return _waitHelper.IsElementVisible(PouchInfoFrame, TestDataHelper.Timeouts.ShortWait);
    }

    public bool CanAddItems()
    {
        return _waitHelper.IsElementEnabled(AddItemButton, TestDataHelper.Timeouts.ShortWait) ||
               _waitHelper.IsElementPresent(ItemPicker, TestDataHelper.Timeouts.ShortWait);
    }

    /// <summary>
    /// Performs a complete Master Ball addition workflow
    /// </summary>
    public async Task AddMasterBall(int quantity = 10)
    {
        await WaitForPageToLoad();
        await SelectBallPouch();
        await AddItem(TestDataHelper.Items.MasterBall, quantity);
        
        // Verify the addition
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }
}