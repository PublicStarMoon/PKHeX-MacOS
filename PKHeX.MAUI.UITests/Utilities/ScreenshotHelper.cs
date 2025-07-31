using OpenQA.Selenium.Appium;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace PKHeX.MAUI.UITests.Utilities;

public class ScreenshotHelper
{
    private readonly string _screenshotDirectory;
    private int _screenshotCounter = 0;

    public ScreenshotHelper()
    {
        _screenshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        Directory.CreateDirectory(_screenshotDirectory);
    }

    public async Task<string> TakeScreenshot(AppiumDriver driver, string testName, string? step = null)
    {
        try
        {
            var screenshot = driver.GetScreenshot();
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var counter = Interlocked.Increment(ref _screenshotCounter);
            
            var fileName = step != null 
                ? $"{testName}_{step}_{timestamp}_{counter:D3}.png"
                : $"{testName}_{timestamp}_{counter:D3}.png";
            
            var filePath = Path.Combine(_screenshotDirectory, fileName);
            
            // Save as PNG using ImageSharp for better compression
            using var image = Image.Load(screenshot.AsByteArray);
            await image.SaveAsPngAsync(filePath);
            
            Console.WriteLine($"Screenshot saved: {filePath}");
            return filePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> TakeScreenshotWithElement(AppiumDriver driver, string testName, string elementId, string step)
    {
        try
        {
            // First take a full screenshot
            var fullScreenPath = await TakeScreenshot(driver, testName, $"{step}_full");
            
            // Try to highlight the element if possible (implementation may vary based on platform capabilities)
            try
            {
                var element = driver.FindElement(OpenQA.Selenium.By.Id(elementId));
                if (element != null)
                {
                    // Take screenshot focused on element
                    var elementScreenshot = element.GetScreenshot();
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    var counter = Interlocked.Increment(ref _screenshotCounter);
                    
                    var fileName = $"{testName}_{step}_element_{timestamp}_{counter:D3}.png";
                    var filePath = Path.Combine(_screenshotDirectory, fileName);
                    
                    using var image = Image.Load(elementScreenshot.AsByteArray);
                    await image.SaveAsPngAsync(filePath);
                    
                    Console.WriteLine($"Element screenshot saved: {filePath}");
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not take element screenshot, using full screen: {ex.Message}");
            }
            
            return fullScreenPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to take screenshot with element: {ex.Message}");
            return string.Empty;
        }
    }

    public string GetScreenshotDirectory() => _screenshotDirectory;

    public void CleanupOldScreenshots(int daysToKeep = 7)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_screenshotDirectory, "*.png");
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted old screenshot: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to cleanup old screenshots: {ex.Message}");
        }
    }
}