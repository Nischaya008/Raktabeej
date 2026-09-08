# Feature Card — M0-DBG-01

**Feature:** Provide the debug menu and dev overlay
**Feature Set:** M0-DBG (Debug infrastructure)
**Plan.md reference:** §21.9 (debug menu as the manual-test layer), §21.8 (frame budgets),
§25 Phase 0 Task 3 (prose spec, amended by **D-21**)
**Estimate:** 3 sessions, as planned. Deferring the clock scrubber (D-21) may bring it in under;
the M0 budget is not being rewritten on that speculation.
**Approved:** session 006, with decisions D-21 (scrubber deferred), D-22 (demo artifact
location), D-23 (sequencing behind M0-ENV-06).

## Client value

Not player-facing. This is the harness every later milestone assumes: DoD #4 requires every
feature to be exercisable from the debug menu on demand, and Plan.md §21.9 is blunt that
without it you spend more time reproducing states than fixing them. It is also the first
feature subject to DoD #7, so it establishes how demo artifacts are produced and indexed.

**Pillar check (§4.4).** This serves no player-facing pillar, and claiming otherwise would be
a fabrication. It is infrastructure mandated by Plan.md §21.9 and Milestones.md M0-DBG. The one
genuine pillar contact is **6 — readable first**: the overlay is built on a top-level
`CanvasLayer` outside any `SubViewport`, so it renders at native resolution and survives the
M1-RND pixelation chain unchanged.

## Scope — IN

- `F1`-toggled overlay panel, bound through an InputMap action so the key stays rebindable
- Frame-time readout: frame milliseconds and fps, live, against the §21.8 16.6 ms budget
- Extensible command registry — `DebugService.Register(name, callback)`, per the Gate M0
  prompt seed. The registry's logic lives in `RaktabeejCore` and is unit-tested there
- A flyable debug camera (**decision 2, session 006**): at M0 there is no gameplay camera to
  toggle away from, so this is a debug `Camera3D` that exists and flies. Toggling *between*
  gameplay and debug cameras arrives with M1-CAM and is explicitly out of scope here
- Excluded from release builds by conditional compilation, proven by asserting the debug types
  are absent from a Release build of the game assembly

## Scope — OUT (do not build)

- Any game-specific command: no blood, Dissonance, Heat, Echo, haunting, or teleport commands.
  Each registers itself from its own feature, per Plan.md §25 Task 6 onward
- Draw-call count and the per-stage millisecond breakdown — that is **M0-DBG-02**. The boundary
  confirmed in session 006: DBG-01 gives frame ms + fps; DBG-02 adds the breakdown and draw
  calls measured against §21.8, readable on both machines
- **A time-of-day scrubber.** Deferred to M4-SUN by **D-21** — there is no `GameClock` to scrub
  at M0, and Plan.md Task 3's own demo line called it a *placeholder*, which DoD #5 forbids
- Console text input, command history, or autocomplete. The registry is called from code
- Save-state, scene-reload, or time-scale commands
- Any change to `main.tscn` beyond what the debug camera requires
- Toggling between a gameplay camera and the debug camera (needs M1-CAM)

## Core changes (RaktabeejCore)

- `DebugCommandRegistry` — pure .NET, zero Godot references (D-06). `Register(string, Action)`,
  `TryInvoke(string)`, and an ordered read-only view for display. Rejects null, empty, and
  duplicate names rather than silently overwriting a previously registered command.

This is infrastructure rather than a game rule, and `tech.md` says game rules live in core. It
goes in core anyway, deliberately: it is the only part of this feature with branching logic, it
has no Godot dependency, and CI runs no engine — so core is the only place its behaviour can be
tested at all. The alternative is untested logic in an autoload.

## Engine changes (game/)

- `game/src/autoload/DebugService.cs` — autoload. Wraps the core registry, owns overlay
  lifetime, conditionally compiled out of release
- `game/src/debug/DebugOverlay.cs` — `CanvasLayer`, F1 toggle, frame-time label
- `game/src/debug/FreeCamera.cs` — flyable `Camera3D`
- `game/scenes/ui/debug_overlay.tscn`
- `game/project.godot` — `[autoload]` entry, and an `[input]` action `debug_toggle` bound to F1

Naming follows **D-16**: directories `lowercase_snake_case`, `.cs` files `PascalCase` with an
exactly-matching class name. The CI guard checks directory segments in a separate pass, so
`src/autoload/` and `src/debug/` must stay lowercase.

## Tests

- xUnit: duplicate name rejected; null name rejected; empty and whitespace name rejected
- xUnit: `TryInvoke` returns false for an unknown name, true for a registered one, and invokes
  the correct callback exactly once
- xUnit: registration order is stable in the display view
- xUnit: debug types are absent from a **Release** build of the game assembly, by reflection —
  the same technique as the existing D-06 architecture guard, and it must be **falsified first**
  (confirm it goes red against a Debug build) before it is trusted
- Manual: F1 opens and closes the panel with no effect on the running scene
- Manual: the frame-time readout tracks reality — confirm it moves under an artificial load,
  rather than only confirming that it displays a number
- Windows: overlay legible and frame-time readout correct on the Windows target (DoD #10).
  **This is the same act that closes M0-ENV-06's final row** — see D-23
- Exported build: menu present in a debug export, absent from a release export (Gate M0)

## Demo

`~/dev/raktabeej-assets/demos/m0-dbg-01.mp4`, indexed in `docs/demos/README.md` per **D-22**.
Press F1 in a running build, read live frame timings, fly the debug camera around the scene,
close the panel. 10–30 s.

## Done when

All ten DoD conditions met. Conditions #10 and Gate M0's exported-build row depend on
M0-ENV-06 landing first (**D-23**), which is why this feature is sequenced behind it.
