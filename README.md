# ArenaGlass - MTG Arena Deck Tracker MVP

ArenaGlass is a Windows-native deck tracker for Magic: The Gathering Arena. This MVP implementation provides real-time tracking of cards remaining in your library during gameplay.

## Features

### Core MVP Features ✅
- **Game State Detection**: Automatically detects when MTG Arena is running
- **Log File Monitoring**: Monitors the MTG Arena Player.log file for real-time game events
- **Deck Parsing**: Extracts starting deck composition from game logs
- **Library Tracking**: Tracks cards drawn from library and maintains accurate remaining counts
- **Real-Time Updates**: Provides instant feedback when cards are drawn or played

### Console Interface
- Interactive command-line interface for testing and demonstration
- Real-time status updates and library display
- Demo mode with sample cards for testing
- Manual card draw simulation for validation

## Requirements

- Windows 10/11
- .NET 8.0
- MTG Arena (for live monitoring)

## Quick Start

### Building the Application
```bash
cd ArenaGlass
dotnet restore
dotnet build
```

### Running the Application
```bash
dotnet run
```

### Available Commands
- `start` - Start monitoring MTG Arena process and log file
- `demo` - Run demonstration with sample cards and simulated draws
- `draw <cardname>` - Simulate drawing a specific card
- `status` - Show current library status
- `clear` - Clear current deck
- `quit` - Exit application

## Architecture

The application is built with a modular service-oriented architecture:

### Core Services
- **GameMonitorService**: Monitors MTG Arena process and initiates log watching
- **LogParserService**: Parses MTG Arena log files using regex patterns
- **CardTrackingService**: Manages library state and card counting

### Models
- **Card**: Represents individual MTG cards with quantity tracking
- **EventArgs**: Custom event arguments for game state and library changes

## Technical Implementation

### Process Monitoring
- Periodically scans for `MTGA.exe` process
- Locates Player.log file at standard Windows location
- Handles process start/stop events gracefully

### Log Parsing
- Uses `FileSystemWatcher` for efficient real-time file monitoring
- Regex-based parsing for deck lists and card events
- Incremental reading to handle large log files efficiently

### Card Database
- Sample card ID mapping for demonstration
- Extensible design for full card database integration

## Future Enhancements (Post-MVP)

- **GUI Overlay**: Transparent, always-on-top overlay window
- **Win32 Integration**: Click-through overlay with proper window positioning
- **Full Card Database**: Complete MTG Arena card ID to name mapping
- **Draft Support**: Draft pack and pick tracking
- **Statistics**: Match history and performance analytics
- **Customization**: User preferences and overlay positioning

## Development Notes

This MVP demonstrates all core functionality required for a deck tracker:
1. ✅ Game detection and log monitoring
2. ✅ Real-time deck parsing and card tracking
3. ✅ Event-driven architecture with proper separation of concerns
4. ✅ Robust error handling and logging
5. ✅ Extensible design for future enhancements

The console interface provides a complete testing environment and proves that all core tracking logic is functional and ready for GUI integration.

## License

This project is intended for educational and personal use. MTG Arena is a trademark of Wizards of the Coast LLC.
