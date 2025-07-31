using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using PKHeX.MAUI.UITests.Utilities;

namespace PKHeX.MAUI.UITests.PageObjects;

public class MainPage
{
    private readonly AppiumDriver _driver;
    private readonly WaitHelper _waitHelper;

    public MainPage(AppiumDriver driver)
    {
        _driver = driver;
        _waitHelper = new WaitHelper();
        _waitHelper.Initialize(driver);
    }

    // Locators for main page elements
    private By DemoModeButton => By.XPath("//Button[contains(@Text, '🎮 Demo Mode')]");
    private By BoxEditorButton => By.XPath("//Button[contains(@Text, '📦') or contains(@Text, 'Box Editor')]");
    private By InventoryEditorButton => By.XPath("//Button[contains(@Text, '🎒') or contains(@Text, 'Inventory Editor')]");
    private By PartyEditorButton => By.XPath("//Button[contains(@Text, '🐾') or contains(@Text, 'Party Editor')]");
    private By LoadSaveButton => By.XPath("//Button[contains(@Text, '📁') or contains(@Text, 'Load Save')]");
    private By SaveButton => By.XPath("//Button[contains(@Text, '💾') or contains(@Text, 'Save File')]");
    
    // Status and information labels
    private By StatusLabel => By.XPath("//Label[@x:Name='StatusLabel']");
    private By CurrentSaveLabel => By.XPath("//Label[@x:Name='CurrentSaveLabel']");
    private By GameVersionLabel => By.XPath("//Label[@x:Name='GameVersionLabel']");
    private By TrainerNameLabel => By.XPath("//Label[@x:Name='TrainerNameLabel']");
    private By TrainerIdLabel => By.XPath("//Label[@x:Name='TrainerIdLabel']");
    private By PlayTimeLabel => By.XPath("//Label[@x:Name='PlayTimeLabel']");
    
    // Page title
    private By PageTitle => By.XPath("//Label[contains(@Text, 'PKHeX')]");

    // Alternative locators using accessibility identifiers (more reliable for automation)
    private By DemoModeButtonAlt => By.Id("DemoModeButton");
    private By BoxEditorButtonAlt => By.Id("BoxEditorButton");
    private By InventoryEditorButtonAlt => By.Id("ItemEditorButton");
    private By PartyEditorButtonAlt => By.Id("PartyEditorButton");

    /// <summary>
    /// Waits for the main page to load completely
    /// </summary>
    public async Task WaitForPageToLoad()
    {
        _waitHelper.WaitForElement(PageTitle, TestDataHelper.Timeouts.AppStartup);
        
        // Wait for all main UI elements to be present
        _waitHelper.WaitForElement(DemoModeButton, TestDataHelper.Timeouts.MediumWait);
        _waitHelper.WaitForElement(LoadSaveButton, TestDataHelper.Timeouts.MediumWait);
        
        // Give the page time to fully initialize
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Clicks the Demo Mode button to enable demo mode
    /// </summary>
    public async Task ClickDemoMode()
    {
        var demoButton = _waitHelper.WaitForElementToBeClickable(DemoModeButton, TestDataHelper.Timeouts.MediumWait);
        demoButton.Click();
        
        // Wait for confirmation dialog and click "Enable"
        await HandleDemoModeConfirmationDialog();
        
        // Wait for demo mode to be enabled
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Handles the demo mode confirmation dialog
    /// </summary>
    private async Task HandleDemoModeConfirmationDialog()
    {
        try
        {
            // Look for "Enable" button in the confirmation dialog
            var enableButton = _waitHelper.WaitForElementToBeClickable(
                By.XPath("//Button[contains(@Text, 'Enable')]"), 
                TestDataHelper.Timeouts.ShortWait);
            
            enableButton.Click();
            await _waitHelper.WaitAsync(1000);
            
            // Look for and dismiss the success dialog
            var okButton = _waitHelper.WaitForElementToBeClickable(
                By.XPath("//Button[contains(@Text, 'OK')]"), 
                TestDataHelper.Timeouts.ShortWait);
            
            okButton.Click();
            await _waitHelper.WaitAsync(1000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not handle demo mode dialog: {ex.Message}");
            // Continue anyway as the dialog handling might vary between platforms
        }
    }

    /// <summary>
    /// Navigates to the Box Editor page
    /// </summary>
    public async Task NavigateToBoxEditor()
    {
        var boxButton = _waitHelper.WaitForElementToBeClickable(BoxEditorButton, TestDataHelper.Timeouts.MediumWait);
        boxButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Navigates to the Inventory Editor page
    /// </summary>
    public async Task NavigateToInventoryEditor()
    {
        var inventoryButton = _waitHelper.WaitForElementToBeClickable(InventoryEditorButton, TestDataHelper.Timeouts.MediumWait);
        inventoryButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Navigates to the Party Editor page
    /// </summary>
    public async Task NavigateToPartyEditor()
    {
        var partyButton = _waitHelper.WaitForElementToBeClickable(PartyEditorButton, TestDataHelper.Timeouts.MediumWait);
        partyButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
    }

    /// <summary>
    /// Clicks the Load Save button
    /// </summary>
    public async Task ClickLoadSave()
    {
        var loadButton = _waitHelper.WaitForElementToBeClickable(LoadSaveButton, TestDataHelper.Timeouts.MediumWait);
        loadButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    /// <summary>
    /// Clicks the Save button
    /// </summary>
    public async Task ClickSave()
    {
        var saveButton = _waitHelper.WaitForElementToBeClickable(SaveButton, TestDataHelper.Timeouts.MediumWait);
        saveButton.Click();
        await _waitHelper.WaitAsync(TestDataHelper.Timeouts.ClickDelay);
    }

    // Property accessors for verification
    public string GetStatusText()
    {
        var element = _waitHelper.WaitForElement(StatusLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetCurrentSaveText()
    {
        var element = _waitHelper.WaitForElement(CurrentSaveLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetGameVersionText()
    {
        var element = _waitHelper.WaitForElement(GameVersionLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetTrainerNameText()
    {
        var element = _waitHelper.WaitForElement(TrainerNameLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetTrainerIdText()
    {
        var element = _waitHelper.WaitForElement(TrainerIdLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public string GetPlayTimeText()
    {
        var element = _waitHelper.WaitForElement(PlayTimeLabel, TestDataHelper.Timeouts.ShortWait);
        return element.Text;
    }

    public bool IsDemoModeEnabled()
    {
        try
        {
            // Check if demo mode button text has changed to indicate it's enabled
            var demoButton = _waitHelper.WaitForElement(DemoModeButton, TestDataHelper.Timeouts.ShortWait);
            var buttonText = demoButton.Text;
            return buttonText.Contains("✅") || buttonText.Contains("Enabled");
        }
        catch
        {
            return false;
        }
    }

    public bool AreEditorButtonsEnabled()
    {
        try
        {
            var boxButton = _waitHelper.WaitForElement(BoxEditorButton, TestDataHelper.Timeouts.ShortWait);
            var inventoryButton = _waitHelper.WaitForElement(InventoryEditorButton, TestDataHelper.Timeouts.ShortWait);
            var saveButton = _waitHelper.WaitForElement(SaveButton, TestDataHelper.Timeouts.ShortWait);
            
            return boxButton.Enabled && inventoryButton.Enabled && saveButton.Enabled;
        }
        catch
        {
            return false;
        }
    }

    public bool IsPageLoaded()
    {
        return _waitHelper.IsElementVisible(PageTitle, TestDataHelper.Timeouts.ShortWait) &&
               _waitHelper.IsElementVisible(DemoModeButton, TestDataHelper.Timeouts.ShortWait);
    }
}