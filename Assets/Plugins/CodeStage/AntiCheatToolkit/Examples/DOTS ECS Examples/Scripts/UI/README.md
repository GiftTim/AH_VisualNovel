# DOTS UI Example

This folder contains UI-specific code for the ACTk DOTS example. It's separated from the core DOTS example code to follow best practices and maintain clean separation of concerns.

## Structure

```
UI/
├── Components/          # UI-specific ECS components
│   ├── UIAction.cs     # UI action commands
│   └── UIData.cs       # UI display data
├── Systems/            # UI-specific ECS systems
│   ├── UIActionSystem.cs  # Processes UI actions
│   └── UIDataSystem.cs    # Updates UI data
├── MonoBehaviors/      # UI presentation layer
│   ├── ActkDotsUI.cs     # Main UI controller
│   └── UIHelpers.cs       # UI utility functions
└── README.md           # This file
```

## Purpose

- **Separation of Concerns**: UI code is isolated from core DOTS example logic
- **Maintainability**: Easier to modify UI without affecting core functionality
- **Reusability**: UI components can be reused in other examples
- **Clean Architecture**: Follows DOTS best practices for code organization

## Core vs UI

- **Core DOTS Example**: `PlayerData`, `CheatState`, `CheatDetected`, `CheatResponseSystem`, `PlayerBootstrapSystem`, `AntiCheatHost`
- **UI Example**: All files in this `UI/` folder

The core example demonstrates ACTk integration with DOTS, while the UI example shows how to build a user interface on top of it.
