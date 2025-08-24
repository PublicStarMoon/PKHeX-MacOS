using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace PKHeX.MAUI.UITests.Utilities;

public class WaitHelper
{
    private AppiumDriver? _driver;
    private WebDriverWait? _wait;
    private const int DefaultTimeoutSeconds = 30;
    private const int DefaultPollingIntervalMs = 500;

    public void Initialize(AppiumDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutSeconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(DefaultPollingIntervalMs)
        };
    }

    public IWebElement WaitForElement(By locator, int timeoutSeconds = DefaultTimeoutSeconds)
    {
        EnsureInitialized();
        
        var wait = timeoutSeconds == DefaultTimeoutSeconds ? _wait! : 
            new WebDriverWait(_driver!, TimeSpan.FromSeconds(timeoutSeconds));
            
        return wait.Until(ExpectedConditions.ElementExists(locator));
    }

    public IWebElement WaitForElementToBeClickable(By locator, int timeoutSeconds = DefaultTimeoutSeconds)
    {
        EnsureInitialized();
        
        var wait = timeoutSeconds == DefaultTimeoutSeconds ? _wait! : 
            new WebDriverWait(_driver!, TimeSpan.FromSeconds(timeoutSeconds));
            
        return wait.Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    public bool WaitForElementToDisappear(By locator, int timeoutSeconds = DefaultTimeoutSeconds)
    {
        EnsureInitialized();
        
        try
        {
            var wait = timeoutSeconds == DefaultTimeoutSeconds ? _wait! : 
                new WebDriverWait(_driver!, TimeSpan.FromSeconds(timeoutSeconds));
                
            return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public bool WaitForTextToAppear(By locator, string expectedText, int timeoutSeconds = DefaultTimeoutSeconds)
    {
        EnsureInitialized();
        
        try
        {
            var wait = timeoutSeconds == DefaultTimeoutSeconds ? _wait! : 
                new WebDriverWait(_driver!, TimeSpan.FromSeconds(timeoutSeconds));
                
            return wait.Until(ExpectedConditions.TextToBePresentInElementLocated(locator, expectedText));
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public IWebElement WaitForElementWithText(By locator, string expectedText, int timeoutSeconds = DefaultTimeoutSeconds)
    {
        EnsureInitialized();
        
        var wait = timeoutSeconds == DefaultTimeoutSeconds ? _wait! : 
            new WebDriverWait(_driver!, TimeSpan.FromSeconds(timeoutSeconds));
            
        return wait.Until(driver => 
        {
            var element = driver.FindElement(locator);
            return element.Text.Contains(expectedText) ? element : null!;
        });
    }

    public async Task WaitAsync(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }

    public void WaitForPageToLoad(int timeoutSeconds = DefaultTimeoutSeconds)
    {
        EnsureInitialized();
        
        var wait = timeoutSeconds == DefaultTimeoutSeconds ? _wait! : 
            new WebDriverWait(_driver!, TimeSpan.FromSeconds(timeoutSeconds));
            
        // Wait for the document to be ready (if applicable to MAUI apps)
        wait.Until(driver => 
        {
            try
            {
                // Check if we can interact with the app by finding any visible element
                var elements = driver.FindElements(By.XPath("//*[@visible='true']"));
                return elements.Count > 0;
            }
            catch
            {
                return false;
            }
        });
    }

    public bool IsElementPresent(By locator, int timeoutSeconds = 5)
    {
        EnsureInitialized();
        
        try
        {
            var originalTimeout = _driver!.Manage().Timeouts().ImplicitWait;
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(timeoutSeconds);
            
            try
            {
                _driver.FindElement(locator);
                return true;
            }
            finally
            {
                _driver.Manage().Timeouts().ImplicitWait = originalTimeout;
            }
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool IsElementVisible(By locator, int timeoutSeconds = 5)
    {
        EnsureInitialized();
        
        try
        {
            var element = WaitForElement(locator, timeoutSeconds);
            return element.Displayed;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    public bool IsElementEnabled(By locator, int timeoutSeconds = 5)
    {
        EnsureInitialized();
        
        try
        {
            var element = WaitForElement(locator, timeoutSeconds);
            return element.Enabled;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    private void EnsureInitialized()
    {
        if (_driver == null || _wait == null)
        {
            throw new InvalidOperationException("WaitHelper must be initialized with a driver before use.");
        }
    }

    /// <summary>
    /// Waits for any of the provided conditions to be true
    /// </summary>
    public T WaitForAny<T>(params Func<IWebDriver, T>[] conditions) where T : class
    {
        EnsureInitialized();
        
        return _wait!.Until(driver =>
        {
            foreach (var condition in conditions)
            {
                try
                {
                    var result = condition(driver);
                    if (result != null)
                        return result;
                }
                catch (Exception)
                {
                    // Continue to next condition
                }
            }
            return null!;
        });
    }

    /// <summary>
    /// Retries an action until it succeeds or timeout
    /// </summary>
    public async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMs = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Retry {i + 1}/{maxRetries} failed: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }
        
        // Final attempt without catching exception
        return await action();
    }
}