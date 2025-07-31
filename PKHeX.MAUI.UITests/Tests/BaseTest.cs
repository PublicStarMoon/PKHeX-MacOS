using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using PKHeX.MAUI.UITests.Utilities;
using PKHeX.MAUI.UITests.PageObjects;

namespace PKHeX.MAUI.UITests.Tests;

// Custom AppiumDriver implementation
// Note: This may have compilation issues on non-macOS platforms due to Appium driver dependencies
// The framework is designed to run on macOS CI runners where proper drivers are available
public class MacOSAppiumDriver : AppiumDriver
{
    public MacOSAppiumDriver(AppiumLocalService service, AppiumOptions options) : base(service, options)
    {
    }

    public MacOSAppiumDriver(AppiumLocalService service, AppiumOptions options, TimeSpan commandTimeout) : base(service, options, commandTimeout)
    {
    }
}

public abstract class BaseTest : IDisposable
{
    protected AppiumDriver? Driver;
    protected AppiumLocalService? Service;
    protected ScreenshotHelper ScreenshotHelper;
    protected WaitHelper WaitHelper;

    // Page Objects
    protected MainPage MainPage => new(Driver!);
    protected PokemonBoxPage PokemonBoxPage => new(Driver!);
    protected PokemonEditorPage PokemonEditorPage => new(Driver!);
    protected InventoryEditorPage InventoryEditorPage => new(Driver!);

    protected BaseTest()
    {
        ScreenshotHelper = new ScreenshotHelper();
        WaitHelper = new WaitHelper();
    }

    protected virtual async Task SetupAppiumDriver()
    {
        // Start Appium service
        var builder = new AppiumServiceBuilder()
            .UsingPort(4723)
            .WithIPAddress("127.0.0.1");

        Service = builder.Build();
        Service.Start();

        // Configure for macOS MAUI application
        var options = new AppiumOptions();
        options.AddAdditionalAppiumOption("platformName", "macOS");
        options.AddAdditionalAppiumOption("automationName", "Mac2");
        options.AddAdditionalAppiumOption("deviceName", "macOS");
        
        // Path to the MAUI app bundle - this will be set by CI or local test setup
        var appPath = Environment.GetEnvironmentVariable("PKHEX_APP_PATH") 
            ?? "/path/to/PKHeX.MAUI.app"; // Default path, should be overridden
        options.AddAdditionalAppiumOption("bundleId", "com.pkhex.macos");
        options.AddAdditionalAppiumOption("app", appPath);
        
        // Timeouts
        options.AddAdditionalAppiumOption("newCommandTimeout", 300);
        options.AddAdditionalAppiumOption("commandTimeouts", "120000");

        try
        {
            // Create driver instance using the service directly
            Driver = new MacOSAppiumDriver(Service, options, TimeSpan.FromSeconds(60));
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            
            WaitHelper.Initialize(Driver);
            
            // Take initial screenshot
            await ScreenshotHelper.TakeScreenshot(Driver, "app_startup");
            
            // Enable demo mode for all tests - this allows access to editors without real save files
            await EnableDemoMode();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Appium driver: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Enables demo mode for all tests to allow access to editors without real save files
    /// </summary>
    protected virtual async Task EnableDemoMode()
    {
        try
        {
            // Wait for main page to load
            await MainPage.WaitForPageToLoad();
            await ScreenshotHelper.TakeScreenshot(Driver!, "main_page_loaded_for_demo");
            
            // Click demo mode button to enable demo mode
            await MainPage.ClickDemoMode();
            await ScreenshotHelper.TakeScreenshot(Driver!, "demo_mode_enabled");
            
            // Wait for demo mode to be enabled
            await WaitHelper.WaitAsync(TestDataHelper.Timeouts.NavigationDelay);
            
            // Verify demo mode is enabled
            if (!MainPage.IsDemoModeEnabled())
            {
                throw new InvalidOperationException("Failed to enable demo mode - editors may not be accessible");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to enable demo mode: {ex.Message}. All tests require demo mode to access editors.", ex);
        }
    }

    protected virtual async Task TearDown()
    {
        if (Driver != null)
        {
            try
            {
                // Take final screenshot before closing
                await ScreenshotHelper.TakeScreenshot(Driver, "test_cleanup");
                Driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during driver cleanup: {ex.Message}");
            }
            finally
            {
                Driver = null;
            }
        }

        if (Service != null)
        {
            try
            {
                Service.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during service cleanup: {ex.Message}");
            }
            finally
            {
                Service = null;
            }
        }
    }

    public virtual void Dispose()
    {
        TearDown().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}