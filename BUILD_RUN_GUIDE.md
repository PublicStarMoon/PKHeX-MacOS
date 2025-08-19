# PKHeX macOS - Build & Run Scripts Guide

This guide explains how to use the unified build and run scripts for PKHeX macOS.

## Quick Start

### Building the Project
```bash
# Build in Debug mode (default)
./build.sh

# Build in Release mode
./build.sh release
```

### Running the Application
```bash
# Run in Debug mode (default)
./run.sh

# Run in Release mode  
./run.sh release
```

## Script Reference

### build.sh - Unified Build Script

**Usage:** `./build.sh [debug|release]`

**Parameters:**
- `debug` or `d` - Build in Debug configuration (default)
- `release`, `rel`, or `r` - Build in Release configuration

**What it does:**
1. Validates the configuration parameter
2. Checks for required dependencies (.NET SDK)
3. Cleans previous builds
4. Restores NuGet packages
5. Builds the PKHeX.MAUI project
6. Reports build status and output location

**Example:**
```bash
# Debug build (default)
./build.sh
./build.sh debug
./build.sh d

# Release build
./build.sh release
./build.sh rel
./build.sh r
```

### run.sh - Unified Run Script

**Usage:** `./run.sh [debug|release]`

**Parameters:**
- `debug` or `d` - Run Debug configuration (default)
- `release`, `rel`, or `r` - Run Release configuration

**What it does:**
1. Validates the configuration parameter
2. Checks if the project has been built
3. Automatically builds if necessary
4. Runs the application using `dotnet run`
5. Handles framework targeting (net8.0-maccatalyst)

**Example:**
```bash
# Run debug (default)
./run.sh
./run.sh debug
./run.sh d

# Run release
./run.sh release
./run.sh rel
./run.sh r
```

## Legacy Scripts (Deprecated)

The following scripts are kept for backward compatibility but redirect to the new unified scripts:

- `launch-pkhex.sh` → `./run.sh release`
- `launch-pkhex-release.sh` → `./run.sh release`

## Configuration Differences

### Debug Configuration
- **Purpose:** Development and debugging
- **Optimizations:** Disabled for better debugging experience
- **Symbol Information:** Full debug symbols included
- **Performance:** Slower execution, larger binaries
- **Use Case:** Development, testing, troubleshooting

### Release Configuration
- **Purpose:** Production use
- **Optimizations:** Enabled for best performance
- **Symbol Information:** Minimal debug symbols
- **Performance:** Faster execution, smaller binaries
- **Use Case:** Distribution, final testing, end-user deployment

## Error Handling

Both scripts include comprehensive error handling:

- **Missing .NET SDK:** Clear instructions to install .NET 8.0 SDK
- **Invalid parameters:** Usage help with examples
- **Build failures:** Detailed error reporting
- **Missing files:** Automatic dependency checking

## Development Workflow

### Standard Development
```bash
# Build and run in debug mode for development
./build.sh debug
./run.sh debug

# Or simply (debug is default)
./build.sh
./run.sh
```

### Testing Release Builds
```bash
# Build and test release version
./build.sh release
./run.sh release
```

### Continuous Development
```bash
# The run script will auto-build if needed
./run.sh  # Automatically builds if not already built
```

## Output Locations

After building, you can find the compiled applications at:

**Debug:**
- `PKHeX.MAUI/bin/Debug/net8.0-maccatalyst/`

**Release:**
- `PKHeX.MAUI/bin/Release/net8.0-maccatalyst/`

## System Requirements

- **macOS:** 11.0 (Big Sur) or later
- **.NET SDK:** 8.0 or later
- **MAUI Workload:** Installed via `dotnet workload install maui`
- **Xcode:** Latest stable version (for macOS development)

## Troubleshooting

### Build Issues
```bash
# Clean and rebuild
./build.sh clean  # If implemented
./build.sh release
```

### Runtime Issues
```bash
# Check .NET installation
dotnet --version
dotnet --list-sdks

# Verify MAUI workload
dotnet workload list
```

### Permission Issues
```bash
# Make scripts executable if needed
chmod +x build.sh run.sh
```

## Advanced Usage

### Integration with IDEs
You can configure your IDE to use these scripts:

**VS Code tasks.json example:**
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "Build Debug",
            "type": "shell",
            "command": "./build.sh",
            "args": ["debug"],
            "group": "build"
        },
        {
            "label": "Build Release",
            "type": "shell", 
            "command": "./build.sh",
            "args": ["release"],
            "group": "build"
        }
    ]
}
```

### CI/CD Integration
```yaml
# GitHub Actions example
- name: Build Debug
  run: ./build.sh debug

- name: Build Release  
  run: ./build.sh release
```

## Benefits of Unified Scripts

✅ **Consistency** - Same interface for all configurations  
✅ **Simplicity** - Single script to remember  
✅ **Flexibility** - Easy parameter-based configuration  
✅ **Error Handling** - Comprehensive validation and feedback  
✅ **Documentation** - Self-documenting with help text  
✅ **Maintenance** - Easier to update and maintain  

## Migration from Legacy Scripts

If you were using the old scripts:

```bash
# Old way
./launch-pkhex.sh
./launch-pkhex-release.sh

# New way (equivalent)
./run.sh debug
./run.sh release
```

The legacy scripts will continue to work but will show deprecation warnings and redirect to the new unified scripts.
