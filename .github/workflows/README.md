# GitHub Workflows

This directory contains GitHub Actions workflows for building and testing PKHeX for macOS.

## Workflows

### 1. MacOS Build (`macos-build.yml`)

**Purpose**: Builds PKHeX as a native macOS application using .NET MAUI.

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches  
- Manual workflow dispatch

**What it does**:
- Sets up .NET 8.0 SDK on macOS runner
- Installs MAUI workload required for macOS app development
- Builds PKHeX.MAUI project for macOS (maccatalyst)
- Creates native macOS `.app` bundle
- Uploads the built app as an artifact for download

**Artifacts**: 
- `PKHeX-MacOS-App` - Contains the built macOS application bundle

### 2. Build and Test (`build-and-test.yml`)

**Purpose**: Comprehensive build validation for all core projects and macOS UI.

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual workflow dispatch

**What it does**:
- **Job 1 (build-core-projects)**: Builds all core libraries on Ubuntu
  - PKHeX.Core
  - PKHeX.Drawing
  - PKHeX.Drawing.PokeSprite
  - PKHeX.Drawing.Misc
- **Job 2 (build-macos-ui)**: Builds the macOS UI on macOS runner
  - Depends on successful core builds
  - Sets up MAUI workload
  - Builds PKHeX.MAUI for macOS

**Artifacts**:
- `PKHeX-MacOS-Build` - Contains all build outputs for macOS

## Usage

### For Contributors

These workflows automatically run when you:
1. Push code to `main` or `develop` branches
2. Create pull requests targeting `main` or `develop`

### For Users

To get the latest PKHeX macOS build:
1. Go to the [Actions tab](../../actions)
2. Click on the latest successful "MacOS Build" workflow run
3. Download the `PKHeX-MacOS-App` artifact
4. Extract the downloaded zip file
5. Locate the `.app` file and drag it to your Applications folder
6. Launch PKHeX from Applications or Launchpad

## Development Notes

- All projects now target .NET 8.0 for compatibility
- PKHeX.Drawing.Misc was downgraded from .NET 9.0 to .NET 8.0
- Package dependencies updated to .NET 8.0 compatible versions
- The workflows use `macos-latest` runner for native macOS builds
- PKHeX.MAUI includes RuntimeIdentifiers for both Intel (x64) and Apple Silicon (arm64) Macs