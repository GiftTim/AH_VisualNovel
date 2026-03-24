# ACTk DOTS ECS Example

This example demonstrates how to integrate ACTk (Anti-Cheat Toolkit) with Unity's DOTS (Data-Oriented Technology Stack) and ECS (Entity Component System).

## Structure

```
Scripts/
├── Components/          # Core ECS components
│   ├── PlayerData.cs    # Player data with ObscuredTypes
│   ├── CheatState.cs    # Cheat detection state
│   └── CheatDetected.cs # Cheat detection events
├── Systems/             # Core ECS systems
│   ├── CheatResponseSystem.cs  # Handles cheat detection
│   └── PlayerBootstrapSystem.cs # Initializes player data
├── Authoring/           # MonoBehaviour authoring components
│   ├── PlayerAuthoring.cs      # Player data authoring
│   └── CheatStateAuthoring.cs  # Cheat state authoring
├── MonoBehaviors/       # Core MonoBehaviour components
│   └── AntiCheatHost.cs # ACTk detector management
├── UI/                  # UI-specific code (separated)
│   ├── Components/      # UI ECS components
│   ├── Systems/         # UI ECS systems
│   └── MonoBehaviors/   # UI presentation layer
└── README.md           # This file
```

## Core Example Features

- **ObscuredTypes Integration**: Uses `ObscuredInt` and `ObscuredFloat` for player data
- **Cheat Detection**: Integrates with ACTk detectors (SpeedHack, WallHack, etc.)
- **ECS Architecture**: Proper DOTS patterns with systems and components
- **Event-Driven**: Cheat detection events flow through ECS

## UI Example Features

The `UI/` folder contains a complete user interface example that demonstrates:
- DOTS-based UI data management
- User interaction handling through ECS
- Storage system integration (ObscuredPrefs, ObscuredFile, etc.)
- Real-time detector status display

## Usage

1. **Core Example**: Use the components and systems in your own DOTS project
2. **UI Example**: Reference the `UI/` folder for UI implementation patterns
3. **Combined**: Use both together for a complete ACTk + DOTS + UI solution

## Best Practices Demonstrated

- Separation of concerns (Core vs UI)
- Proper ECS data flow
- ACTk integration patterns
- DOTS system architecture
- Clean code organization
