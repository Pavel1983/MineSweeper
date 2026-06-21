# Minesweeper

A Minesweeper game built with Unity.

## Modules

**Board** — core game state: mine field, flags, and cell reveals.

**WinCondition** — win rule checks based on board state.

**BoardRevealHelper** — flood-fill logic when opening cells.

**BoardPresenter** — ties everything together during a match: board state, views, timer, input, and HUD updates.

**GameTimer** — elapsed time tracking.

**BoardLayout** — grid sizing and tile positions inside the play area.

**BoardInputHandler** — right-click input for placing flags.

**Navigation** — screen flow (start, game, settings, results) via a stack-based navigator.

**Screens** (`StartScreen`, `GameScreen`, `SettingsScreen`, `ResultsScreen`) — UI for each game state.

**Views** (`TileView`, `FlagView`, `ClockView`, `BoardViewport`) — visuals for the board, HUD, and play area bounds.

## Configuration

**Level settings** — `Assets/Resources/LevelConfig.asset`  
Edit grid size and mine count here (columns, rows, mines).

**Screen registry** — `Assets/Resources/ScreensRegistry.asset`  
Maps screen IDs to UI prefabs.

**UI & tile prefabs** — `Assets/Data/Prefabs/`

## Requirements

- Unity 6000.0.40f1 (or compatible)
