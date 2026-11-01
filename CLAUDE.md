# CollegeEducatedSpouts - notes for Claude Code

A mod for Railroader's built-in mod loader (Unity/Mono, game 0.6.x; entry point `ModEntryPoint` in Main.cs, asset-pack Info.json, settings in Settings.xml): water columns and towers whose spouts turn to the tender hatch. Namespace `CollegeEducatedSpouts`, author Exp232. Companion to AutoEngineerBrakeFix (optional; it calls `SpoutApi.CanReach` by reflection).

## Build
- `dotnet build -c Release /p:DeployToGame=false` compiles against the game's DLLs (`RailroaderDir` in the csproj, or `/p:RailroaderDir=...`).
- Do not deploy into the game's Mods folder unless asked.
- Your usual job here: build, read compile errors, fix them minimally without changing behaviour, repeat until clean.

## Conventions
- Harmony patches; every patch target and reflected member must be listed in `SelfCheck.cs`.
- Do not touch the game's meshes: they are not readable, and changing `vertexBufferTarget` / `indexBufferTarget` empties them (the pole vanishes).
- `SpoutApi` (end of SpoutSwing.cs) is public API: keep its signatures stable (Auto Engineer Brake Fix finds it by reflection).
- When changing behaviour or a setting, bump the version in `Info.json`, `BUILDING.md` and the csproj together and add a `CHANGELOG.md` entry (newest first, under `# Changelog`). Update `README.md` for user-visible changes.
- Settings window: `SettingsUi.cs` (Ctrl+F9). `BuildPage` draws the page without a scroll view so the Mod Settings Hub can host it; `HubLink.cs` registers the page with the hub by reflection (copy of the hub's `HubLink.Template.cs`; do not list the hub in Info.json).
