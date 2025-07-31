using PKHeX.Core;
using PKHeX.MAUI.Services;

namespace PKHeX.MAUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Initialize PKHeX Core
        InitializePKHeXCore();
        
        // Remove background initialization - load on demand instead
        // CachedDataService.StartBackgroundInitialization();
        
        MainPage = new AppShell();
    }
    
    private static void InitializePKHeXCore()
    {
        try
        {
            // Initialize game strings and data
            // Force initialization of GameInfo strings for multiple languages
            var englishStrings = GameInfo.GetStrings("en");
            var chineseStrings = GameInfo.GetStrings("zh");

            // Set current language to Chinese as default
            GameInfo.CurrentLanguage = "zh";
            
            // Ensure strings are properly loaded
            if (englishStrings?.itemlist == null || chineseStrings?.itemlist == null)
            {
                throw new InvalidOperationException("Failed to load game strings");
            }
        }
        catch (Exception ex)
        {
            // If initialization fails, we'll handle it gracefully in CachedDataService
            Console.WriteLine($"PKHeX Core initialization warning: {ex.Message}");
        }
    }
    
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        window.Title = "PKHeX for macOS";
        
        return window;
    }
}

