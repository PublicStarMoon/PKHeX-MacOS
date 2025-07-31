using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using PKHeX.MAUI.UITests.Utilities;
using PKHeX.MAUI.UITests.PageObjects;

namespace PKHeX.MAUI.UITests.Tests;

// Custom AppiumDriver implementation to work around the abstract class issue
public class MacOSAppiumDriver : AppiumDriver
{
    public MacOSAppiumDriver(Uri remoteAddress, DriverOptions options) : base(remoteAddress, options)
    {
    }

    public MacOSAppiumDriver(Uri remoteAddress, DriverOptions options, TimeSpan commandTimeout) : base(remoteAddress, options, commandTimeout)
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
            // Create a custom driver instance 
            Driver = new MacOSAppiumDriver(Service.ServiceUrl, options, TimeSpan.FromSeconds(60));
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            
            WaitHelper.Initialize(Driver);
            
            // Take initial screenshot
            await ScreenshotHelper.TakeScreenshot(Driver, "app_startup");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Appium driver: {ex.Message}", ex);
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