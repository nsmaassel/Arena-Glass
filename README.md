# ArenaGlass - Live Deck Tracking Overlay

ArenaGlass is a real-time overlay application for Magic: The Gathering Arena that tracks your library state during gameplay. It provides a non-intrusive display of remaining cards in your library, updating live as you draw cards.

## Features

- **Real-time Game Monitoring**: Automatically detects when MTG Arena is running
- **Live Log Parsing**: Monitors MTG Arena's Player.log file for game events
- **Card Library Tracking**: Maintains and displays your current library state
- **Event-driven Architecture**: Modular, service-oriented design with dependency injection
- **Non-intrusive Overlay**: Designed to be transparent and click-through (in WinUI 3 implementation)

## Project Structure

```
ArenaGlass/
├── ArenaGlass.Core/           # Backend services and core logic
│   ├── Models/                # Data models (Card, GameState, GameEvent)
│   ├── Services/              # Core services (GameMonitor, LogParser, CardTracker)
│   └── Extensions/            # Dependency injection extensions
├── ArenaGlass.UI/             # Console application demonstrating functionality
├── ArenaGlass.Tests/          # Unit tests for core functionality
└── WINUI3_IMPLEMENTATION.md   # Complete WinUI 3 overlay implementation guide
```

## Current Implementation

The current implementation includes:

### Backend Services (ArenaGlass.Core)

1. **GameMonitorService**: Monitors for MTGA.exe process
2. **LogParserService**: Parses MTG Arena's Player.log file for events
3. **CardTrackingService**: Manages library state and card counts

### Console Application (ArenaGlass.UI)

A fully functional console application that demonstrates all the backend functionality:
- Real-time card tracking simulation
- Interactive card drawing simulation
- Service integration with dependency injection
- Event-driven updates

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Windows operating system (for MTG Arena integration)

### Building and Running

1. Clone the repository:
```bash
git clone <repository-url>
cd Arena-Glass
```

2. Build the solution:
```bash
dotnet build
```

3. Run the console application:
```bash
dotnet run --project ArenaGlass.UI
```

4. Run tests:
```bash
dotnet test
```

### Using the Console Application

When you run the application, you can:
- Press 'd' to simulate drawing a card
- Press 'r' to reset the library to initial state
- Press 'q' to quit the application

The application will display:
- Current game status (ACTIVE/INACTIVE)
- Total cards remaining in library
- Detailed list of all cards with counts
- Real-time updates as cards are "drawn"

## WinUI 3 Implementation

For the complete overlay implementation using WinUI 3, see `WINUI3_IMPLEMENTATION.md`. This document provides:

- Complete project structure for WinUI 3
- Win32 interop code for overlay functionality
- XAML definitions for the overlay UI
- Data binding and MVVM implementation
- All necessary code for transparent, click-through overlay

## Technical Implementation

### Service Architecture

The application uses a service-oriented architecture with dependency injection:

```csharp
// Register services
services.AddArenaGlassCore();

// Services available:
IGameMonitorService    // Process monitoring
ILogParserService      // Log file parsing
ICardTrackingService   // Library state management
```

### Event-Driven Updates

All services communicate through events:

```csharp
// Game monitoring events
gameMonitor.GameStateChanged += OnGameStateChanged;

// Log parsing events
logParser.GameEventParsed += OnGameEventParsed;

// Card tracking events
cardTracker.GameStateUpdated += OnGameStateUpdated;
```

### Models

Core data models include:

- **Card**: Represents individual cards with name, count, mana cost, and type
- **GameState**: Current state of the game and library
- **GameEvent**: Log events parsed from MTG Arena

## Testing

The project includes comprehensive unit tests for:
- Card tracking functionality
- Event handling
- State management
- Service integration

Run all tests with:
```bash
dotnet test
```

## Future Enhancements

The WinUI 3 implementation (detailed in `WINUI3_IMPLEMENTATION.md`) would add:

1. **Transparent Overlay Window**: Non-intrusive visual overlay
2. **Always-on-Top Display**: Stays visible over fullscreen games
3. **Click-through Functionality**: Mouse events pass through to game
4. **Dynamic Positioning**: Automatically positions relative to MTG Arena window
5. **Real-time Visual Updates**: Live card list with smooth updates

## License

This project is provided as-is for educational and personal use.

## Contributing

Contributions are welcome! Please ensure all tests pass and follow the existing code style.

---

**Note**: This implementation provides a complete backend architecture and console demonstration. For the full overlay experience, implement the WinUI 3 version using the provided implementation guide.
