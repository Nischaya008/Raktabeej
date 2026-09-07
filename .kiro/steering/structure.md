---
inclusion: always
---

# Repository Structure — TARGET layout

This is where things **go**, not what exists today. Most files below are not
written yet (as of M0, `game/src/` holds only `Main.cs`). Use it to decide where a
new file belongs; do not assume a path here is real. Check the filesystem first.

    raktabeej/
    ├── RaktabeejCore/
    │   └── Pure C# rules library — MUST NOT reference Godot or Godot assemblies
    ├── RaktabeejCore.Tests/
    │   └── xUnit — runs in seconds; most tests live here
    ├── game/
    │   └── Godot project
    │       ├── src/            ← lowercase dirs, PascalCase.cs files (D-16)
    │       │   ├── autoload/
    │       │   │   ├── GameClock.cs
    │       │   │   ├── EventBus.cs
    │       │   │   ├── SaveService.cs
    │       │   │   └── DebugService.cs
    │       │   ├── player/
    │       │   │   ├── PlayerController.cs
    │       │   │   ├── CameraRig.cs
    │       │   │   ├── AbilityCaster.cs
    │       │   │   └── FeedingController.cs
    │       │   ├── npc/
    │       │   │   ├── NpcAgent.cs
    │       │   │   ├── CrowdDirector.cs
    │       │   │   └── MemoryCoreFactory.cs
    │       │   ├── world/
    │       │   │   ├── ChunkStreamer.cs
    │       │   │   ├── SunMap.cs
    │       │   │   └── WeatherDirector.cs
    │       │   ├── threat/
    │       │   │   ├── HeatDirector.cs
    │       │   │   └── InvestigationDirector.cs
    │       │   ├── haunting/
    │       │   │   └── one class per H1..H7
    │       │   ├── lair/
    │       │   │   ├── RoomNodes.cs
    │       │   │   ├── ServantController.cs
    │       │   │   └── ReliquaryController.cs
    │       │   └── ui/
    │       │       ├── Hud.cs
    │       │       ├── LedgerScreen.cs
    │       │       └── Menus.cs
    │       ├── data/
    │       │   └── ALL content as .tres resources
    │       ├── scenes/
    │       │   ├── chunks
    │       │   ├── interiors
    │       │   ├── ui
    │       │   └── vfx
    │       ├── art/
    │       │   ├── models
    │       │   ├── textures
    │       │   ├── palettes
    │       │   └── sprites
    │       ├── audio/
    │       │   ├── ambient_2070
    │       │   ├── ambient_1861
    │       │   ├── sfx
    │       │   └── music
    │       └── shaders/
    │           ├── pixelate
    │           ├── palette
    │           ├── dither
    │           ├── outline
    │           └── haunting_*
    ├── tools/
    │   └── Blender export scripts, palette generator
    ├── docs/
    │   ├── Plan.md            ← constitution
    │   ├── Milestones.md      ← schedule
    │   ├── TOOLCHAIN.md       ← pinned versions, naming convention
    │   ├── ASSET_LICENSES.md
    │   └── demos/
    └── .github/
        └── workflows/
            └── ci.yml

## Source Assets

Source assets (.blend, .vox, raw .wav) live OUTSIDE this repository in:

    ~/dev/raktabeej-assets/

Only exported, game-ready files are committed to this repository.
