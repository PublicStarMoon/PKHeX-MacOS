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
        
        // Start background initialization of game data
        // This will load species/natures/moves/items data in a background thread
        // to improve performance when opening editors later
        CachedDataService.StartBackgroundInitialization();
        
        MainPage = new AppShell();
    }
    
    private static void InitializePKHeXCore()
    {
        // Initialize game strings and data
        // Note: Some initialization may not be needed for this basic demo
    }
    
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        window.Title = "PKHeX for macOS";
        
        return window;
    }
}
