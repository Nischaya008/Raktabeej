# Pinned Toolchain

**This file is the single source of truth for versions.** If a version appears
anywhere else, that copy is wrong — point at this file instead. Godot 4.5 lived
in `Plan.md` for two sessions while the installed engine was 4.7.2; that is the
drift this file exists to prevent.

**Policy: no version upgrades mid-milestone.** Minor Godot releases change
rendering and physics behaviour. An accidental upgrade would silently shift the
post-process chain, and the resulting bug would be indistinguishable from a code
defect. Upgrades happen deliberately, between milestones, as their own change
with a gate build on the Windows target.

| Component | Pinned version | Notes |
|---|---|---|
| Godot | `4.7.2.stable.mono.official` | .NET build. `godot --version` must contain `mono` |
| .NET SDK | `8.0.x` (arm64) | Pinned in `global.json` (`8.0.0`, `rollForward: latestFeature`). 10.0.400 is installed side-by-side and unused here |
| Target framework | `net8.0` | What `Godot.NET.Sdk` is validated against |
| Renderer | Forward+ | |
| Rendering driver | Metal (macOS, default), **D3D12 (Windows, explicitly pinned)** | `project.godot` sets `rendering_device/driver.windows="d3d12"`. Godot writes no driver key on its own, so this is an override rather than a default. macOS is left alone |
| 3D physics | Jolt Physics | Default since Godot 4.4 |
| Test framework | xUnit | gdUnit4Net is deferred and may be dropped — decision D-15 |

## Elsewhere, deliberately not repeated here

| Topic | Owner |
|---|---|
| Machine roles, and why budgets are measured on Windows | `Milestones.md` § Environment |
| gdUnit4Net version-drift risk and its fallback | `Milestones.md` §9 risk register, decision D-15 |
| Why C# scripts are PascalCase while assets are lowercase | Decision D-16 in `../Agent_History.md` |
