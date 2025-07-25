using PKHeX.Core;

namespace PKHeX.MAUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Initialize PKHeX Core
        InitializePKHeXCore();
        
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
        
        if (window != null)
        {
            window.Title = "PKHeX for macOS";
        }
        
        return window;
    }
}
