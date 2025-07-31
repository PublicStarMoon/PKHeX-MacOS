namespace PKHeX.MAUI.UITests.Utilities;

public static class TestDataHelper
{
    public static class Pokemon
    {
        public const string WishiwashiSpecies = "Wishiwashi";
        public const int WishiwashiSpeciesId = 746;
        public const string DefaultNickname = "MyWishiwashi";
        public const int DefaultLevel = 25;
        public const string DefaultNature = "Adamant";
        
        public static readonly string[] DefaultMoves = new[]
        {
            "Water Gun",
            "Growl", 
            "Helping Hand",
            "Feint Attack"
        };
        
        public static readonly Dictionary<string, object> DefaultStats = new()
        {
            { "HP", 45 },
            { "Attack", 20 },
            { "Defense", 20 },
            { "SpAttack", 25 },
            { "SpDefense", 25 },
            { "Speed", 40 }
        };
    }

    public static class Items
    {
        public const string MasterBall = "Master Ball";
        public const int MasterBallId = 1;
        public const int DefaultQuantity = 10;
        public const string BallPouch = "Ball";
        
        public static readonly string[] CommonBalls = new[]
        {
            "Poké Ball",
            "Great Ball", 
            "Ultra Ball",
            "Master Ball",
            "Safari Ball",
            "Net Ball",
            "Dive Ball",
            "Nest Ball",
            "Repeat Ball",
            "Timer Ball"
        };
    }

    public static class UI
    {
        // Common UI element identifiers and text
        public const string DemoModeButtonText = "🎮 Demo Mode";
        public const string DemoModeEnabledText = "✅ Demo Mode Enabled";
        public const string BoxEditorButtonText = "📦 Box Editor";
        public const string InventoryEditorButtonText = "🎒 Inventory Editor";
        public const string PartyEditorButtonText = "🐾 Party Editor";
        
        // Expected demo mode status messages
        public const string DemoModeSuccessMessage = "Demo mode enabled successfully!";
        public const string DemoSaveFileText = "Demo Mode (SAV8SWSH)";
        public const string DemoTrainerName = "Demo";
        public const string DemoGameVersion = "Pokémon Sword/Shield (Generation 8)";
        
        // Common button texts
        public const string SaveButtonText = "💾 Save";
        public const string BackButtonText = "← Back";
        public const string ExportButtonText = "Export";
        
        // Page titles
        public const string MainPageTitle = "PKHeX - Pokémon Save Editor";
        public const string BoxEditorTitle = "Pokemon Box Editor";
        public const string InventoryEditorTitle = "Inventory Editor";
        public const string PokemonEditorTitle = "Pokemon Editor";
    }

    public static class Timeouts
    {
        public const int ShortWait = 5;
        public const int MediumWait = 15;
        public const int LongWait = 30;
        public const int AppStartup = 60;
        
        public const int ClickDelay = 1000;
        public const int NavigationDelay = 2000;
        public const int FormInputDelay = 500;
    }

    public static class TestData
    {
        public static readonly Dictionary<string, string> SampleTrainerData = new()
        {
            { "Name", "TestTrainer" },
            { "ID", "12345" },
            { "SecretID", "54321" }
        };
        
        public static string GenerateTestFileName(string baseName = "test")
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{baseName}_{timestamp}";
        }
        
        public static string GetTempSaveFilePath(string fileName = "test_save.sav")
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "PKHeX_UITests");
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, fileName);
        }
    }

    /// <summary>
    /// Gets random test data for more diverse testing
    /// </summary>
    public static class Random
    {
        private static readonly System.Random _random = new();
        
        public static string GetRandomPokemonName()
        {
            var names = new[] { "Pikachu", "Charizard", "Blastoise", "Venusaur", "Mewtwo", "Mew", "Celebi" };
            return names[_random.Next(names.Length)];
        }
        
        public static int GetRandomLevel() => _random.Next(1, 101);
        
        public static string GetRandomNature()
        {
            var natures = new[] { "Hardy", "Lonely", "Brave", "Adamant", "Naughty", "Bold", "Docile", "Relaxed", "Impish", "Lax" };
            return natures[_random.Next(natures.Length)];
        }
        
        public static int GetRandomQuantity() => _random.Next(1, 999);
    }
}