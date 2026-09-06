---
inclusion: always
---

# Repository Structure

    raktabeej/
    ├── RaktabeejCore/
    │   └── Pure C# rules library — MUST NOT reference Godot or Godot assemblies
    ├── RaktabeejCore.Tests/
    │   └── xUnit — runs in seconds; most tests live here
    ├── game/
    │   └── Godot project
    │       ├── src/
    │       │   ├── autoload/
    │       │   │   ├── game_clock
    │       │   │   ├── event_bus
    │       │   │   ├── save_service
    │       │   │   └── debug_service
    │       │   ├── player/
    │       │   │   ├── controller
    │       │   │   ├── camera_rig
    │       │   │   ├── ability_caster
    │       │   │   └── feeding
    │       │   ├── npc/
    │       │   │   ├── agent
    │       │   │   ├── crowd_director
    │       │   │   └── memory_core_factory
    │       │   ├── world/
    │       │   │   ├── chunk_streamer
    │       │   │   ├── sun_map
    │       │   │   └── weather_director
    │       │   ├── threat/
    │       │   │   ├── heat_director
    │       │   │   └── investigation_director
    │       │   ├── haunting/
    │       │   │   └── one class per H1..H7
    │       │   ├── lair/
    │       │   │   ├── rooms
    │       │   │   ├── servants
    │       │   │   └── reliquary
    │       │   └── ui/
    │       │       ├── hud
    │       │       ├── ledger_screen
    │       │       └── menus
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
    │   ├── Plan.md
    │   ├── Milestones.md
    │   ├── ASSET_LICENSES.md
    │   └── demos/
    └── .github/
        └── workflows/
            └── ci.yml

## Source Assets

Source assets (.blend, .vox, raw .wav) live OUTSIDE this repository in:

    ~/dev/raktabeej-assets/

Only exported, game-ready files are committed to this repository.
