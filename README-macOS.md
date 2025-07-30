# PKHeX for macOS 🍎

A native macOS port of PKHeX (Pokémon save file editor) built with .NET MAUI, specifically designed for Apple Silicon (M1/M2/M3/M4) Macs.

## What is PKHeX?

PKHeX is a Pokémon save file editor for most recent Pokémon games. It allows you to edit your save files to modify Pokémon, items, trainer information, and more. This macOS version brings the full power of PKHeX to Mac users without needing Windows or emulation.

## Features

- ✅ **Native macOS App**: Runs natively on Apple Silicon Macs (M1/M2/M3/M4)
- ✅ **Save File Loading**: Load and parse Gen 7-9 Pokémon save files
- ✅ **Pokémon Box Editor**: View and edit Pokémon in boxes and party
- ✅ **Pokémon Editor**: Comprehensive individual Pokémon editing
- ✅ **Item Editor**: Manage items, Poké Balls, and inventory
- ✅ **Cross-Platform**: Built with .NET MAUI for modern app experience
- ✅ **File Operations**: Import/export save files with macOS file picker
- ✅ **Minimal & Focused**: Streamlined UI for essential editing only

## Requirements

- macOS 14.0 or later
- Apple Silicon Mac (M1/M2/M3/M4 chips)
- .NET 8.0 SDK
- Xcode Command Line Tools

## Quick Start

### Option 1: Use the Launch Script (Recommended)

1. Open Terminal and navigate to this folder
2. Run the launch script:
   ```bash
   ./launch-pkhex.sh
   ```

The script will automatically build and launch PKHeX for you!

### Option 2: Manual Build

1. **Install Prerequisites**:
   ```bash
   # Install .NET 8.0 SDK if not already installed
   # Download from: https://dotnet.microsoft.com/download
   
   # Install MAUI workload
   sudo dotnet workload install maui
   ```

2. **Build the App**:
   ```bash
   cd PKHeX.MAUI
   dotnet build --configuration Release
   ```

3. **Run the App**:
   ```bash
   # Option A: Run directly
   dotnet run
   
   # Option B: Open the app bundle
   open bin/Release/net8.0-maccatalyst/maccatalyst-arm64/PKHeX.MAUI.app
   ```

## Installation

### Adding to Applications Folder

After building, you can add PKHeX to your Applications folder:

1. Build the app (using either method above)
2. Find the app bundle at:
   ```
   PKHeX.MAUI/bin/Release/net8.0-maccatalyst/maccatalyst-arm64/PKHeX.MAUI.app
   ```
3. Drag `PKHeX.MAUI.app` to your `/Applications` folder
4. Launch from Launchpad or Finder like any other Mac app

## Usage

1. **Launch PKHeX** using one of the methods above
2. **Load a Save File**:
   - Click "Load Save File" button
   - Select your Gen 7-9 Pokémon save file (.sav, .dat, .bin, etc.)
3. **Edit Your Game**:
   - **Pokémon Box Editor**: View boxes, select and edit individual Pokémon
   - **Item Editor**: Modify your item inventory, including Poké Balls
   - Use validation tools to ensure legal Pokémon
4. **Save Changes**:
   - Click "Export Save" to save your modifications
   - Import the modified save back to your console

## Supported Games

PKHeX for macOS focuses on modern Pokémon games (Gen 7-9):
- **Generation 7**: Sun/Moon, Ultra Sun/Ultra Moon
- **Generation 8**: Sword/Shield, Brilliant Diamond/Shining Pearl, Legends Arceus
- **Generation 9**: Scarlet/Violet

Supported file formats: .sav, .dat, .bin

## Troubleshooting

### Common Issues

1. **"Cannot be opened because it is from an unidentified developer"**:
   - Right-click the app and select "Open"
   - Click "Open" in the dialog
   - Or run: `sudo xattr -cr PKHeX.MAUI.app`

2. **Build Errors**:
   - Ensure you have .NET 8.0 SDK installed
   - Make sure MAUI workload is installed: `sudo dotnet workload install maui`
   - Verify Xcode Command Line Tools: `xcode-select --install`

3. **App Won't Launch**:
   - Try running from Terminal: `dotnet run` in the PKHeX.MAUI folder
   - Check Console.app for error messages

### Performance Notes

- Some image processing functions show warnings (System.Drawing.Common limitations on macOS)
- These warnings don't affect core functionality
- Sprite rendering may be limited compared to Windows version

## Development Status

This is a fully functional, focused editor for Gen 7-9 Pokémon games. Current status:

- ✅ **Core Engine**: PKHeX.Core library fully functional
- ✅ **Save Loading**: Gen 7-9 save formats fully supported
- ✅ **Pokémon Editing**: Complete individual Pokémon editor
- ✅ **Box Management**: Full box and party editing capabilities
- ✅ **Item Management**: Comprehensive item and Poké Ball editing
- ✅ **Minimal UI**: Streamlined, focused interface
- ✅ **Native Integration**: macOS file picker and app bundle
- 🔄 **Architecture**: Refactored for maintainability and focus
- 🚧 **Pokémon Sprites**: Image rendering improvements needed

### Recent Refactor (v2024.1)
The UI has been completely refactored to focus on essential functionality:
- Removed 10+ unnecessary editor pages (~4,500 lines of code)
- Streamlined navigation to Box Editor and Item Editor only
- Integrated advanced features into main editing flows
- Improved architecture with clean Core/Drawing separation

## Technical Details

### Architecture

- **PKHeX.Core**: The original PKHeX engine (Windows/cross-platform)
- **PKHeX.MAUI**: New macOS UI built with .NET MAUI
- **Target Framework**: .NET 8.0 with MacCatalyst
- **UI Framework**: Microsoft MAUI (Multi-platform App UI)

### Build Output

The build creates a native macOS app bundle:
```
PKHeX.MAUI.app/
├── Contents/
│   ├── Info.plist          # App metadata
│   ├── MacOS/              # Native executable
│   ├── MonoBundle/         # .NET runtime and assemblies
│   └── Resources/          # App resources
```

## Contributing

This port focuses on bringing PKHeX to macOS users. Contributions welcome for:

- UI/UX improvements for macOS
- Additional macOS-specific features
- Bug fixes and optimizations
- Documentation improvements

## Credits

- **PKHeX**: Original project by Kaphotics and contributors
- **macOS Port**: Built with .NET MAUI for Apple Silicon compatibility
- **Framework**: Microsoft .NET and MAUI teams

## License

This port maintains the same license as the original PKHeX project.

---

## Getting Help

- Check the original PKHeX documentation for gameplay-related questions
- For macOS-specific issues, check the troubleshooting section above
- Ensure your save files are backed up before editing

**Happy save editing on macOS! 🎮✨**
