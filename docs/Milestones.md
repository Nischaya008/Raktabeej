# RAKTABEEJ — Milestone & Feature Delivery Plan

> **Companion to `docs/Plan.md`.**
> Plan.md is the **constitution** — what the game is and why. It changes rarely and deliberately.
> This document is the **schedule** — what gets built, in what order, and how you know it's done. It changes weekly.
>
> **Status:** v1.1
> **Delivery model:** Feature-Driven Development, adapted for solo AI-assisted work
> **Estimating unit:** 1 session = one focused ~3-hour block
> **Assumed capacity:** 7 sessions/week (~21 h) → ~338 sessions to Early Access
> **Target:** Early Access, Windows + Steam Deck, week 48
> **Repository:** `github.com/Nischaya008/Raktabeej` — **currently public** (verified session 004; the docs had said private). Decision pending

---

## Environment

| Role | Machine | Purpose |
|---|---|---|
| **Primary development** | MacBook Pro 16" M4 Pro, 48 GB, macOS Tahoe | All authoring, coding, art, iteration. Godot runs on the native **Metal** driver. |
| **Verification target** | Windows desktop, AMD CPU + NVIDIA RTX | Every gate build runs here. Vulkan/D3D12 parity, performance profiling against a median-Steam-spec GPU, release exports. |

**Rule:** develop on the Mac, **profile and gate on Windows**. The M4 Pro is fast enough to hide budget overruns; the RTX box is the honest number. Full setup in **Appendix A**.

---

## Table of Contents

0. [How to Use This Document](#0-how-to-use-this-document)
1. [Delivery Model](#1-delivery-model)
2. [Ground Rules & Definition of Done](#2-ground-rules--definition-of-done)
3. [Feature Card Template](#3-feature-card-template)
4. [Vibe-Coding Protocol](#4-vibe-coding-protocol)
5. [Milestones M0–M8](#5-milestones)
6. [Cadence](#6-cadence)
7. [Tracking Board](#7-tracking-board)
8. [Cut Triggers](#8-cut-triggers)
9. [Risk Register](#9-risk-register)
10. [Prompt Seed Library](#10-prompt-seed-library)
11. [Appendix A — Environment Setup](#appendix-a--environment-setup)

---

## 0. How to Use This Document

1. **Never work without an active Feature Card.** Pick the next feature from the current milestone table, write its card (§3), work it, demo it, commit it, tick it off.
2. **One feature = one branch = one demo.** If a feature can't be demoed, it isn't a feature — it's a task hiding inside one. Merge it into its parent.
3. **Gates are hard stops.** A gate is not a checkpoint you note and pass. If a gate fails, you stop and fix or stop and rethink. The M3 gate can end the project; that is its job.
4. **Update the tracking board (§7) at the end of every session.** Two minutes. It is the only defence against the month-eight realisation that you're twelve weeks behind.
5. **When this document and Plan.md disagree, Plan.md wins** — unless you deliberately amend Plan.md and note it in `Changes.md`.

---

## 1. Delivery Model

### Why FDD

- **Features are client-valued and demoable.** For a solo dev with nobody to hold them accountable, a demoable increment every 1–3 sessions is the only reliable motivation engine.
- **It front-loads the domain model.** Plan.md *is* the overall model (FDD process #1). `RaktabeejCore` is that model made executable.
- **It plans by feature, not by layer.** You will never spend six weeks building "the UI system" with nothing playable. Every feature cuts vertically through core → engine → UI.

### What is deliberately dropped

Chief programmers, feature teams, class ownership, formal inspections. Irrelevant for one person. Their function — independent review — is replaced by the agent/reviewer separation in §4.4.

### Hierarchy

```
Major Feature Set   = Milestone (M0–M8)
  Feature Set       = a coherent subsystem within the milestone
    Feature         = 1–6 sessions, demoable, ID'd, tracked
```

### Feature ID scheme

```
M4-SUN-02
│  │   └── sequence within the feature set
│  └────── feature set code
└───────── milestone
```

---

## 2. Ground Rules & Definition of Done

### Definition of Done — every feature

A feature is Done only when **all ten** are true. No partial credit.

1. `dotnet build` passes with **zero warnings** in `RaktabeejCore` (warnings-as-errors on core only).
2. `dotnet test` passes. New core logic has xUnit tests **in the same commit**.
3. The feature is reachable in a running build without a debugger attached.
4. It is exercisable from the **debug menu** — you can set the state and trigger the effect on demand.
5. No `TODO`, no `NotImplementedException`, no commented-out code, no stub returning a fixed value.
6. Frame time still inside budget (Plan.md §21.8). Check the overlay, don't assume.
7. A **demo artifact** exists: a 10–30 s clip in `docs/demos/<feature-id>.mp4`.
8. Committed on a feature branch, squashed to **1–2 commits**, merged to `main`, CI green.
9. `Changes.md` has a one-line entry. Tracking board updated.
10. **If the feature touches rendering, shaders, physics, or performance: verified running correctly on the Windows target**, not only on the Mac.

### Core architecture rules — non-negotiable

- `RaktabeejCore` **never** references Godot. If you think it must, the design is wrong.
- Game rules live in core. Godot nodes only read core state and render it.
- All content is a `[GlobalClass] Resource` in `game/data/`. Nothing balance-related is hardcoded in C#.
- Asset and resource paths under `game/` are `lowercase_snake_case` — directories, scenes, `.tres`, art, audio, shaders. They load via hand-written `res://` strings, and macOS and Windows won't warn you; the Linux/Steam Deck export will fail. **CI enforces this.**
- **C# script files under `game/` are `PascalCase.cs`, and the class name must match the file name exactly** (decision D-16). Godot resolves scripts by that match, case-sensitively, and a mismatch fails at *runtime*, not at build time. CI exempts only the *filename*, and only for `.cs`, `.cs.uid`, `.csproj`, `.sln`; **directory segments are never exempt**, so `game/src/Autoload/GameClock.cs` still fails the guard on its `Autoload/` segment.
- Save serialization is POCO + `System.Text.Json` with a `SchemaVersion`. Never serialize a Godot node.

### Content and asset rules

- **Source assets never enter this repo.** `.blend`, `.vox`, `.aseprite`, raw `.wav` live in `~/dev/raktabeej-assets/`, backed up separately. Only exported game-ready files (`.glb`, `.ogg`, `.png`) are committed. This is what keeps you inside GitHub's free 1 GB LFS quota.
- Every third-party asset is logged in `docs/ASSET_LICENSES.md` **in the commit that adds it**. No exceptions, ever.
- All player-facing strings go through the localization CSV from M1 onward. Never concatenate translated strings.
- Every visual effect gets its photosensitivity-safe variant **in the same feature**, not in a later accessibility pass.

---

## 3. Feature Card Template

Write this before you write code. Four minutes, and it is what makes the agent useful instead of chaotic.

```markdown
## Feature Card — M4-SUN-03

**Feature:** Project the moving shadow map for the Sun Map overlay
**Feature Set:** M4-SUN (Time and sunlight)
**Plan.md reference:** §10.1, §21.5
**Estimate:** 4 sessions

### Client value
The player can plan a dawn route through narrowing shadow corridors and survive
sunrise by reading the map instead of by memorisation.

### Scope — IN
- Real-time shadow projection layered on the world map screen
- Projection updates with GameClock sun position
- Safe/unsafe zones visually distinct, colourblind-safe

### Scope — OUT (do not build)
- Fast travel
- Route auto-planning or pathfinding hints
- Weather effects on the projection

### Core changes (RaktabeejCore)
- `SunExposure.GetSafetyAt(WorldPoint, GameTime)` — pure, testable

### Engine changes (game/)
- `world/sun_map.cs` — reads core, renders overlay
- `scenes/ui/sun_map_screen.tscn`

### Tests
- xUnit: exposure state correct for 20 parameterized positions × 6 times of day
- xUnit: projection deterministic for a given clock value
- Manual: overlay-reported safe zone matches actual runtime damage at 5 spots
- Windows: overlay renders identically under the Windows backend (currently D3D12)

### Demo
Stand in the open at 05:20, open the Sun Map, walk the shadow corridor home,
arrive at 05:38 alive.

### Done when
All ten DoD conditions met.
```

---

## 4. Vibe-Coding Protocol

Full AI-assisted development works on a project this size **only** with a verification harness. Without one it produces four months of plausible code that doesn't hold together. These rules are that harness.

### 4.1 Document hierarchy

| Document | Role | Change frequency |
|---|---|---|
| `docs/Plan.md` | Constitution. What the game is. | Rarely, deliberately |
| `docs/Milestones.md` | Schedule. This file. | Weekly |
| Feature Card | Work order. One at a time. | Per feature |
| `.kiro/steering/*.md` | Standing constraints, always loaded | Rarely |
| `Changes.md` | One line per completed feature | Per feature |
| `Agent_History.md` | Decisions and rationale, newest on top | Per session |

### 4.2 The session loop

1. **Orient (5 min).** Read the tracking board and the top of `Agent_History.md`. Confirm the active feature.
2. **Card (5 min).** If starting a new feature, write its card. **If you cannot write the OUT list, you do not understand the feature yet.**
3. **Branch.** `git checkout -b feat/m4-sun-03`
4. **Core first.** Agent implements core logic + tests. Run `dotnet test`. Fully verifiable — do not proceed while red.
5. **Engine second.** Wire into Godot. Verify by playing, not by reading code.
6. **Debug hook.** Add the debug-menu command for this feature.
7. **Demo.** Record the clip. If you cannot produce a compelling 15 seconds, it isn't done.
8. **Review.** Reviewer pass on the diff (§4.4).
9. **Land.** Squash, merge, CI green, update `Changes.md`, `Agent_History.md`, board.

**One feature per session block.** A multi-session feature stays on its branch — but every session still ends on a green build.

### 4.3 The agent contract

Lives in `.kiro/steering/tech.md` so it loads every time:

```
- Read docs/Plan.md and the active Feature Card before writing code.
- Implement ONLY what the Feature Card's IN scope lists. If the work seems to
  require something on the OUT list, stop and say so. Do not build it.
- RaktabeejCore must never reference Godot.
- Every core rule change ships with xUnit tests in the same commit.
- Run `dotnet build` and `dotnet test` and report the ACTUAL output before
  claiming a feature works. Never claim a passing build you did not run.
- Never delete, skip, or weaken an existing test to make a build green.
- Never leave a stub, TODO, or NotImplementedException in work reported complete.
- Balance numbers belong in .tres resources, not in C#.
```

### 4.4 Independent review

You are the only gate on the agent's work. Never accept a "done" claim at face value.

Before merging any feature:

- **Read the diff yourself.** Every file. Not the summary.
- **Confirm the tests are real.** Would this test fail if the feature were wrong? A test that asserts the implementation rather than the requirement is worse than no test.
- **Hunt the four failure modes:** silent stubs; tests deleted or weakened to go green; scope creep past the card; results reported but never actually run.
- **Run a separate reviewer session** on non-trivial diffs. A reviewer must never be the session that wrote the code.
- **Check the pillars.** Does this serve one of the six in Plan.md §2? If not, why does it exist?

### 4.5 Delegate vs drive

| Delegate freely | Drive yourself |
|---|---|
| Core logic with a clear spec + tests | Anything about **feel** — camera, movement, combat timing |
| Bulk content authoring (300 `EchoDefinition` resources) | Difficulty and balance tuning |
| Boilerplate: resources, UI plumbing, save models | Art direction judgement calls |
| Shader implementation from a described effect | Whether the hook is actually fun (M3 gate) |
| Test suites from a written requirement | Scope decisions and cuts |
| Refactors with test coverage behind them | Anything a playtester reacted badly to |

**Feel cannot be delegated.** M1's gate — "is this fun for 10 minutes with no content" — is a judgement only you can make, and it is the substrate everything else sits on.

---

## 5. Milestones

**Legend:** **Est.** = sessions (~3 h) · 🔴 = gate feature, milestone cannot exit without it · ⚠️ = high risk, schedule early

---

## M0 — Literacy & Foundations

> **Weeks 1–3 · 15 sessions** (was 23; −8 from D-18 dropping the throwaway learning games)
> **Goal:** You can build, run, test, debug, and ship a Godot C# project on both machines, and you know the engine well enough that the real project isn't your teaching sandbox.

**Do not skip this milestone** — but note its literacy approach changed under **D-18**. The three throwaway learning games are gone; editor literacy is now acquired on the real project, guided. The infrastructure features (M0-ENV, M0-DBG) are unchanged and still gate everything after.

### Feature Set M0-ENV — Environment

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M0-ENV-01 🔴 | Provision the macOS development toolchain | All 14 rows of the Appendix A checklist green — including F5 debugging with a live breakpoint | 2 |
| M0-ENV-02 🔴 | Establish the repository and asset discipline | Cloned, `.gitattributes` + `.gitignore` in place, LFS initialised, `~/dev/raktabeej-assets/` created, first push green | 1 |
| M0-ENV-03 | Scaffold the solution and Godot project | Root solution holds core + tests + game; game references core; Jolt enabled; `global.json` pins .NET 8 | 1 |
| M0-ENV-04 | Establish continuous integration | Push runs core build + tests **and** the lowercase-path guard; both green | 1 |
| M0-ENV-05 | Establish the AI harness | Three `.kiro/steering/` files written; `Plan.md` and `Milestones.md` in `docs/`; `Changes.md`, `Agent_History.md`, `ASSET_LICENSES.md` created | 1 |
| M0-ENV-06 🔴 | Establish the Windows verification target | Export templates installed; Windows Desktop preset configured; an exported build launches on the Windows machine; frame-time overlay readable there | 2 |

`M0-ENV-06` is what makes every later gate honest. Without it you will not discover a Metal-versus-Vulkan divergence until M7.

### Feature Set M0-LRN — Engine literacy

| ID | Feature | Done when | Est. |
|---|---|---|---|
**Restructured by decision D-18.** This set originally built three throwaway games — Pong, a top-down arena shooter, a 3D platformer — for ~11 sessions, on the reasoning that learning an engine on your real project is how first games die. That reasoning assumed a developer hand-writing the code. This project is AI-orchestrated, so hand-building Pong buys little that transfers. Dropped, saving ~8 sessions.

**What is deliberately not dropped.** The plan's argument was not purely about typing practice, and the surviving part is load-bearing:

- §4.5 lists **feel** as undelegatable, and the **M1 gate** is *"play it for 10 minutes with no content — is moving around genuinely pleasurable?"* No agent can answer that for you.
- The **M3 gate** is reading five strangers and deciding whether the hook works. It can end the project.
- §4.4 makes you the only reviewer of agent output — and in the very cycle that produced this decision, the agent shipped a CI regression that only review caught.

None of that needs Pong. It needs editor fluency and the ability to read a diff. Hence:

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M0-LRN-04 🔴 | Keep a living engine-learnings log | `docs/LEARNINGS.md` started now and appended **every session** a trap is hit — node lifecycle, signals, `_Process` vs `_PhysicsProcess`, resource loading, the C# struct-copy trap, export gotchas. A log, not a one-off write-up | 1 |
| M0-LRN-05 🔴 | Complete a guided editor-literacy pass | Unaided in the Godot editor **on the real project**: build a node hierarchy, attach a C# script, expose an `[Export]` and tune it live while running, use the remote inspector on a running game, read the debugger and profiler panels, export a build. Agent writes the step-by-step; you drive the editor | 2 |

The old sandbox instruction is void — there is no throwaway repo. `M0-LRN-05` happens in this project, and its artifacts are kept.

### Feature Set M0-DBG — Debug infrastructure

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M0-DBG-01 🔴 | Provide the debug menu and dev overlay | F1 panel with frame-time overlay, free-camera toggle, clock scrubber, extensible command registry; compiled out of release exports | 3 |
| M0-DBG-02 | Provide a frame-time and draw-call overlay | Live ms breakdown and draw-call count against Plan.md §21.8 budgets, readable on both machines | 1 |

The debug menu is infrastructure, not a luxury. Every later milestone assumes you can force any state on demand. Build it now, grow it forever.

### 🚦 Gate M0

- [x] All 14 Appendix A checklist rows green — closed in session 006 when row #9 (F5 debugger
      attach) passed
- [ ] Guided editor-literacy pass complete (M0-LRN-05) — you can drive the Godot editor unaided
- [ ] `docs/LEARNINGS.md` started and being appended
- [ ] Debug menu working in an exported build
- [ ] CI green, including the lowercase-path guard
- [ ] **An exported build runs on the Windows target**
- [ ] **You can answer without looking it up:** how do I load a resource, how do I connect a signal, how do I export a build?

**Prompt seed:**
> "Read `docs/Plan.md` §21.9 and `.kiro/steering/tech.md`. Implement M0-DBG-01: a debug overlay autoload with an F1-toggled panel, a frame-time readout, and a command registry other systems call `DebugService.Register(name, callback)` on. It must compile out of release exports. Only what the card lists — no game-specific commands yet. Show me the actual `dotnet build` output."

---

## M1 — Feel

> **Weeks 4–8 · 35 sessions**
> **Goal:** A grey-box block that is genuinely enjoyable to move a character around in, rendered in the final art style.

⚠️ **This milestone determines whether the game feels good.** Nothing later can rescue bad movement. Take the extra sessions if you need them.

### Feature Set M1-CAM — Camera

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-CAM-01 🔴 | Provide the pitch-locked orbiting camera rig | Rig per Plan.md §6.1; pitch locked −52°; yaw 360° on held RMB / held L2 + right stick; 0.12 s damping | 3 |
| M1-CAM-02 | Provide interpolated zoom | 5 steps, 9–22 m, smooth, clamped | 1 |
| M1-CAM-03 | Resolve camera occlusion by dither-fade | Geometry between camera and player fades; camera never moves or clips | 2 |
| M1-CAM-04 🔴 | Resolve the aim-versus-rotate conflict | While RMB held, aim freezes at last vector and abilities stay castable in that direction (Plan.md §6.3) | 2 |
| M1-CAM-05 | Expose camera tuning parameters | All values `[Export]`ed and live-tunable from the debug menu | 1 |

### Feature Set M1-MOV — Movement

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-MOV-01 🔴 | Provide camera-relative character movement | Correct at all 360° of yaw; parameterized test at 8 yaw values | 3 |
| M1-MOV-02 | Provide sprint and crouch | Distinct speeds, animation-ready state machine, no state deadlocks | 1 |
| M1-MOV-03 🔴 | Provide the dodge with i-frames | Exactly 0.4 s invulnerability, 0.9 s cooldown, no wall-clipping at max speed | 2 |
| M1-MOV-04 | Provide the input remapping layer | All actions in the input map; nothing reads raw keys; hold-to-toggle option plumbed | 2 |

### Feature Set M1-RND — Render pipeline

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-RND-01 🔴 | Render the world at low internal resolution | 640×360 `SubViewport`, nearest filter, integer upscale, no vertex snapping | 3 |
| M1-RND-02 🔴 | Quantize the frame to the fixed palette | 56-colour palette + Bayer 8×8 ordered dither | 3 |
| M1-RND-03 | Outline silhouettes by edge detection | Depth+normal Roberts cross, 1 px dark outline | 2 |
| M1-RND-04 | Bloom the neon | Threshold-gated, low radius, does not wash out the palette | 1 |
| M1-RND-05 🔴 | Render the UI at native resolution | Separate `CanvasLayer` above the viewport; text crisp at 1080p / 1440p / the Mac's 3456×2234 | 2 |
| M1-RND-06 | Author the three sub-palettes | 2070 Sprawl, 1861 Calcutta, Sanguine Sight — each with a colourblind-safe variant | 2 |
| M1-RND-07 🔴 | Verify the pipeline on both backends | Golden-image comparison per stage; post chain under 2 ms; **renders identically on Metal (macOS) and D3D12 (Windows)** — both are shipping backends under D-20. **Also profile Vulkan on Windows for comparison**, not just confirm D3D12 works: D3D12 has known slower startup and has trailed Vulkan in some scenes, so the fallback needs a real number behind it (D-19) | 2 |

`M1-RND-07` is the feature that de-risks the Metal-vs-Vulkan divergence for the whole project. Do not defer it.

### Feature Set M1-KIT — Modular art kit

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-KIT-01 | Establish the Blender→glTF→Godot pipeline | Repeatable from clean checkout; export script in `tools/`; `.blend` sources stay outside the repo | 2 |
| M1-KIT-02 | Author the first 20 building modules | Wall, floor, roof, stair, balcony, awning, railing, signage on a shared 256² atlas, within poly budget | 3 |
| M1-KIT-03 🔴 | Assemble the first Stack chunk | One 48×48 m block, baked `LightmapGI`, emissive neon, inside draw-call budget | 3 |

### Feature Set M1-TST — Test infrastructure

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-TST-01 | Adopt gdUnit4Net for engine tests | NuGet packages added; **Godot version pinned to a gdUnit4Net-supported release**; camera tests run under `dotnet test` | 2 |
| M1-TST-02 | Add the Godot export job to CI | Headless build + Windows export artifact on every push to `main`. **Also automate the liveness probe:** run `godot --headless --path game --quit` and assert it prints the `_Ready()` probe string, so a broken C#↔Godot binding fails CI instead of surviving until someone reads Appendix A.4 row 8 by hand | 2 |

### 🚦 Gate M1 — the Feel Gate

- [ ] Camera and movement behave exactly to Plan.md §6 and §7
- [ ] Pixel pipeline running; UI text crisp
- [ ] One real-looking neon street block exists
- [ ] **Renders identically on Metal and on the Windows target**
- [ ] Frame time inside budget **measured on Windows**, with the post chain active
- [ ] **The subjective gate: play it for 10 minutes with no objective, no enemies, no content. Is moving around genuinely pleasurable?**

**If the answer is no, stop and fix it.** Do not proceed hoping content will compensate. It will not. Every one of the next 280 sessions sits on top of this.

**Prompt seed:**
> "Implement M1-CAM-01 per `docs/Plan.md` §6.1–6.2. Build the `CameraPivot → YawGimbal → SpringArm3D → Camera3D` hierarchy. Pitch is locked at −52° and must be unchangeable by input. Yaw rotates 360° continuously while RMB (mouse) or L2+right-stick (pad) is held, at 140°/s mouse-scaled and 180°/s stick. Position damping 0.12 s. Expose every value as `[Export]`. Do NOT implement zoom or occlusion — separate features. Write tests asserting pitch never changes and yaw wraps correctly across the 0/360 boundary."

---

## M2 — Predation

> **Weeks 9–14 · 42 sessions**
> **Goal:** You can hunt, fight, and feed. The blood economy is real and tested.

### Feature Set M2-BLD — Blood model

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M2-BLD-01 🔴 | Model the blood pool in core | `BloodPool` with decay, combat drain, Withering threshold; ≥95% xUnit coverage | 3 |
| M2-BLD-02 | Model the eight blood types | Types, potency ranges, potency-scaled buff sets per Plan.md §9.2; all as `.tres` | 3 |
| M2-BLD-03 | Model Chrome Sickness | Stacking debuff → Chrome Fever at 3 stacks; cures work | 2 |
| M2-BLD-04 🔴 | Present the blood vial | Vertical vial HUD reflecting the model exactly; Withering desaturation + movement penalty | 3 |
| M2-BLD-05 | Resolve torpor and its three outcomes | Unfound / found / daylight branches all reachable and correct (Plan.md §9.6) | 3 |

### Feature Set M2-CBT — Combat

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M2-CBT-01 🔴 | Provide the claw combo | 3 hits, 0.35 s each, third staggers; hitboxes active only on correct frames | 4 |
| M2-CBT-02 | Provide hit feedback | Hit-stop, screen-shake budget, toggleable damage numbers, impact VFX | 2 |
| M2-CBT-03 🔴 | Provide the first enemy with readable telegraphs | Sprawl Ganger; 0.6 s wind-up with ground decal; utility-AI action selection | 5 |
| M2-CBT-04 | Provide the enemy death and corpse state | Lootable, persistent, draggable corpse | 2 |
| M2-CBT-05 | Provide environmental kill hooks | At least two: transformer discharge, falling cargo | 3 |

### Feature Set M2-FED — Feeding

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M2-FED-01 🔴 | Provide the feeding sequence with release window | 3 s hold; release early = Sip, hold to term = Drain; interrupts on damage | 4 |
| M2-FED-02 🔴 | Present the Memory Core preview during the feed | Victim's core surfaces in HUD *while* drinking (Plan.md §9.5) | 3 |
| M2-FED-03 | Provide the combat grapple feed | Staggered humans grappled for a risky in-combat Drain | 2 |
| M2-FED-04 | Provide Rend as a combat finisher | 40% blood, always-Cold corrupted echo, loud, high Reek | 2 |
| M2-FED-05 | Flag surviving Sip victims as witnesses | Every Sip creates a persistent witness record | 1 |

### 🚦 Gate M2

- [ ] Core blood suite passes in under 2 s with ≥95% coverage
- [ ] Combat is readable and weighty at the fixed camera angle
- [ ] Sip vs Drain is a decision you actually feel while playing
- [ ] The Memory Core preview appears mid-feed and is legible
- [ ] Frame time in budget **on Windows** with 8 enemies active
- [ ] **Start profiling on Windows from this milestone forward.** Not at M10.

---

## M3 — THE HOOK ⚠️🔴

> **Weeks 15–22 · 56 sessions**
> **Goal:** The Sanguine Ledger works, hauntings intrude systemically, and progression runs on biography.

**This is the make-or-break milestone.** Everything before it is scaffolding; everything after assumes this system is compelling. Do not let it slip and do not soften its gate.

### Feature Set M3-LDG — The Ledger

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M3-LDG-01 🔴 | Model the Sanguine Ledger in core | `SanguineLedger`, `EchoDefinition`, echo acquisition; heavy xUnit | 4 |
| M3-LDG-02 🔴 | Model Dissonance and its conflict multiplier | Accumulation, opposed-tag multiplier, thresholds at 25/50/75/90, Frenzy at 100 | 4 |
| M3-LDG-03 🔴 | Model Resonance | Man↔Beast axis; all consumer hooks defined; clamping tested | 3 |
| M3-LDG-04 | Model Warm capacity and its permanent reduction | Devour reduces max Warm; floor behaviour correct after repeated Devours | 2 |
| M3-LDG-05 🔴 | Present the Ledger as an 1861 book | Paged, handwritten, per-echo entries; Confessed signed / Devoured scratched out / Enshrined sealed; performant at 250 entries | 6 |

### Feature Set M3-MEM — Memory Cores

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M3-MEM-01 🔴 | Provide the `EchoDefinition` and `NpcArchetype` resources | `[GlobalClass]` resources editable in the Godot inspector | 2 |
| M3-MEM-02 🔴 | Assemble Memory Cores lazily per NPC | Dominant + 0–2 minors, tag-consistent, generated on first inspect/feed, stable thereafter, under 0.1 ms | 3 |
| M3-MEM-03 | Author three archetypes × 15 echoes | Labour, Courier, Enforcer; 45 echoes with tags, anchor sites, unfinished acts, vignettes | 4 |

### Feature Set M3-HNT — Hauntings

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M3-HNT-01 🔴 | Direct haunting selection from the Ledger | `HauntingDirector` triggers on thresholds, weighted by the triggering echo's preferences | 3 |
| M3-HNT-02 🔴⚠️ | Bleed the 1861 overlay across the world | Full material + palette + ambient-audio swap; **collision provably unchanged**; 20–60 s; photosensitivity-safe variant in the same feature; verified on both graphics backends | 6 |
| M3-HNT-03 | Populate the world with phantom crowds | Dead victims spawn among the living; no blood signature; no shadow; attacking air spikes Suspicion | 4 |
| M3-HNT-04 | Degrade the HUD through Name Loss | Icons scramble, markers detach, minimap reverts to the 1861 grid | 3 |
| M3-HNT-05 | Verify hauntings are photosensitivity-safe | Automated luminance-delta check on every safe variant; global "Reduce Visual Intrusion" toggle | 2 |

### Feature Set M3-RES — Resolution

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M3-RES-01 🔴 | Resolve an echo by Confession | Travel to anchor site, perform the unfinished act, echo becomes a permanent Boon | 4 |
| M3-RES-02 🔴 | Resolve an echo by Devouring | Coffin ritual with unmistakable permanent-cost warning; Beast+; Warm capacity down | 3 |
| M3-RES-03 🔴 | Resolve an echo by Enshrining | Reliquary placement; registers a Lair Event; removed from Ledger | 3 |
| M3-RES-04 | Author ten Confession micro-quests | Each 30 s–4 min, hand-written, tied to a real world location | 4 |

### Feature Set M3-PWR — Recipe progression

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M3-PWR-01 🔴 | Match power recipes against the Ledger | Tag counting (`height×2`) correct; partial matches report accurate missing tags | 3 |
| M3-PWR-02 🔴 | Unlock three powers by recipe | Sanguine Sight, Veil of Ash, Rat Form — each earned from specific biographies | 4 |
| M3-PWR-03 | Hint completable recipes via Sanguine Sight | Inspecting a target shows which recipe they would advance | 2 |

### 🚦🔴 GATE M3 — THE HOOK GATE

The most important gate in the project. A real decision point, not a formality.

1. Cut a standalone **45-minute playtest build** — Windows, since that's what your testers have.
2. Give it to **at least five people who are not you**.
3. Watch without helping, hinting, or explaining.
4. Afterwards ask exactly one open question: **"What was that game about?"**

- [ ] Do they talk about the memories, the Ledger, or the 1861 bleed — unprompted?
- [ ] Did anyone hesitate before a kill?
- [ ] Did anyone go hunting for a *specific kind of person*?
- [ ] Did anyone describe a haunting as interesting rather than annoying?

**If the memory system is not what they talk about, the hook has failed.** Do not proceed to M4. Either redesign the hook and re-gate, or stop the project. The nine months after this gate all assume the answer was yes.

**Prompt seed:**
> "Implement M3-LDG-02 in `RaktabeejCore` per `docs/Plan.md` §3.4. Pure C#, no Godot. `DissonanceCalculator` accumulates 8–20 per echo by weight, applies a multiplier up to ×2.0 when echoes with opposed tags are absorbed in the same in-game night, exposes threshold crossings at 25/50/75/90, and triggers Frenzy at 100. Write xUnit tests covering each threshold boundary, the multiplier at 1.0/1.5/2.0, and clamping at both ends. Do NOT implement haunting selection — that is M3-HNT-01. Report the real `dotnet test` output."

---

## M4 — The City

> **Weeks 23–29 · 49 sessions**
> **Goal:** Two streamed districts, a day/night cycle that can kill you, and a lair worth coming home to.

### Feature Set M4-STR — Streaming

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M4-STR-01 🔴 | Stream chunks in a 3×3 ring | Async load; no frame spike above 4 ms **on Windows**; memory flat across 100 transitions | 5 |
| M4-STR-02 🔴 | Persist chunk state without keeping nodes alive | Corpses, loot, broken lights, evidence survive unload/reload | 3 |
| M4-STR-03 | Render the far field as baked impostors | One impostor mesh per district with emissive windows | 3 |
| M4-STR-04 🔴 | Assemble The Stack | 12 chunks from the modular kit, lightmapped, in budget | 6 |
| M4-STR-05 🔴 | Assemble The Interment | 8 chunks; flooded colonial ruins; permanently sunless | 5 |
| M4-STR-06 | Cull the vertical floor bands | Above-player floors dither out; below-player floors drop detail | 2 |

### Feature Set M4-SUN — Time and sunlight

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M4-SUN-01 🔴 | Run the 48-minute day/night cycle | 32 night / 6 dusk / 6 dawn / 4 day; deterministic sun position; ambient transitions | 3 |
| M4-SUN-02 🔴 | Detect sun exposure accurately | Four damage states correct under awnings, overpasses, building shadows; 20 parameterized positions | 4 |
| M4-SUN-03 🔴 | Project the Sun Map | Real-time moving shadow overlay; matches actual runtime damage; colourblind-safe | 4 |
| M4-SUN-04 | Warn of dawn | Sun-clock arc HUD; turns red at 8 minutes | 1 |

### Feature Set M4-REK — Reek

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M4-REK-01 🔴 | Model Reek in core | All accumulation and reduction sources per Plan.md §10.2 | 2 |
| M4-REK-02 | Communicate Reek diegetically | Flies, NPC recoil, visible haze — no meter | 2 |
| M4-REK-03 | Scale NPC detection by Reek | Radius scales at the four documented thresholds | 2 |
| M4-REK-04 | Track garment Reek independently | Bloodstained clothes hold Reek separately; laundering works | 2 |

### Feature Set M4-LAR — The lair

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M4-LAR-01 🔴 | Provide the coffin | Save, day-skip with overnight resolution, respawn anchor, Devour site | 3 |
| M4-LAR-02 | Provide the cistern | Full Reek reset | 1 |
| M4-LAR-03 🔴 | Provide the Reliquary and Lair Events | Enshrinement counts drive events; hostile above 20 echoes | 3 |
| M4-LAR-04 | Provide the blood cellar | Typed, potency-preserving blood storage | 2 |
| M4-LAR-05 | Provide room construction | Placement UI with resource costs | 2 |

### Feature Set M4-SAV — Persistence

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M4-SAV-01 🔴 | Serialize the save model | POCO + `System.Text.Json`; round-trips a 250-echo Ledger with full fidelity | 3 |
| M4-SAV-02 🔴 | Migrate save schema versions | Migration chain exists; a simulated v1→v2 migration is written **and tested now** | 2 |
| M4-SAV-03 | Write saves atomically | Interrupted write leaves the previous save intact; corruption degrades gracefully | 2 |
| M4-SAV-04 | Autosave at the right moments | Coffin rest, district transition, quest completion; 3 slots + autosave | 1 |

### 🚦 Gate M4

- [ ] Run across a full district with no loading screen and no hitching
- [ ] Leave a corpse, walk two districts away, come back, it's there
- [ ] Get caught at 05:20 and survive by reading the Sun Map
- [ ] Save, quit, relaunch — world state identical
- [ ] The v1→v2 migration test passes
- [ ] Frame time in budget **on Windows** in the densest Stack chunk at night

---

## M5 — Power & Pressure

> **Weeks 30–36 · 49 sessions**
> **Goal:** Twelve powers, three forms, and a city that hunts you with two independent memories.

### Feature Set M5-AXS — Progression axes

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M5-AXS-01 🔴 | Wire Resonance to every consumer | Tech gating, Guise duration, combat scaling, merchant access, servant loyalty | 3 |
| M5-AXS-02 🔴 | Refuse technology to the Beast | Touchscreens do not register below Resonance 30; diegetic feedback, not an error popup | 2 |
| M5-AXS-03 🔴 | Model Comprehension | Gain from echoes, observation, tutoring; gates defined | 2 |
| M5-AXS-04 🔴⚠️ | Garble and resolve text by Comprehension | Deterministic per value; signage and item text resolve glyph→legible; exempt from accessibility text options; one-time explanatory tooltip | 4 |

### Feature Set M5-FRM — Forms

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M5-FRM-01 🔴 | Provide the shapeshift framework | Transitions cannot enter an invalid state; per-form movement, abilities, upkeep | 4 |
| M5-FRM-02 🔴 | Provide Guise | Full NPC social response; degrades on Resonance and Reek | 3 |
| M5-FRM-03 | Provide Rat Form and its traversal layer | Drains, vents, gaps reachable only as a rat | 3 |
| M5-FRM-04 | Provide Pariah Form | +45% speed; socially invisible as a street dog | 2 |

### Feature Set M5-THR — Threat

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M5-THR-01 🔴 | Model Suspicion and Heat | Per-witness Suspicion decaying in minutes; district Heat 1–5 decaying in hours | 3 |
| M5-THR-02 🔴 | Escalate Corpsec response by Heat tier | All five tiers spawn correct pressure via the director pattern | 4 |
| M5-THR-03 🔴⚠️ | Build the persistent Case File | All six evidence types generated by the right actions; never decays; survives save/load | 5 |
| M5-THR-04 🔴 | Counter the Case File | Every documented counterplay works: dispose, Memory Edit, intimidate, vary diet, roam, wash | 4 |
| M5-THR-05 | Present the Case File on the Study board | Inspectable evidence board showing your own habits as a pattern | 3 |
| M5-THR-06 | Close the case into a manhunt | 100% triggers permanent city-wide manhunt state | 2 |

### Feature Set M5-PWR — Powers

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M5-PWR-01 🔴 | Provide the four-slot ability bar | Cooldowns, blood costs, lair-only loadout swapping | 3 |
| M5-PWR-02 | Provide the six Sanguine and Ferine powers | Blood Mend, Blood Rite, Sanguine Coil, Predator's Lunge, Bat Sonar, Rend upgrades | 4 |
| M5-PWR-03 | Provide the three Umbral and Dominion powers | Veil of Ash, Shadow Step, Mesmerize | 3 |
| M5-PWR-04 🔴 | Provide Dominate and Servants | Conversion, servant decay, break-free-and-testify | 3 |
| M5-PWR-05 🔴 | Provide servant day-orders | Fetch blood, buy goods, scout, intimidate, launder; resolve overnight | 3 |

### Feature Set M5-BOS — Bosses

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M5-BOS-01 🔴 | Provide the PARJANYA squad | Multi-phase; UV lances, drone net, thermal; every authored environmental solution completable | 6 |
| M5-BOS-02 🔴 | Provide the Kennel Scout | Reek-scent tracking across chunks, pounce, regeneration, Dominate immunity | 5 |
| M5-BOS-03 | Provide the Kennel Hunt night event | Howl, music drop-out, 90 s scent-break window | 3 |

### 🚦 Gate M5

- [ ] Hunt sloppily four nights, then read a coherent case built from your own habits
- [ ] Beat PARJANYA using the environment, not raw damage
- [ ] Survive a Kennel Hunt by breaking scent in floodwater
- [ ] Reach Comprehension 40 and watch the city's language resolve
- [ ] Send a servant out and wake up fed
- [ ] Neither boss softlocks on any edge case (killed mid-transition, player torpor mid-fight)
- [ ] Full gate playthrough completed **on Windows**

---

## M6 — The Story

> **Weeks 37–42 · 42 sessions**
> **Goal:** Prologue and Act I complete, voiced-in-style, accessible, and controller-verified.

### Feature Set M6-NAR — Narrative

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M6-NAR-01 🔴 | Deliver the Prologue | All five scenes per Plan.md §5.2; Ranajoy's Warm echo and the labourer's Cold echo planted; first Overlay Bleed at Dissonance 24 | 6 |
| M6-NAR-02 🔴 | Deliver Act I's eight beats | All beats per Plan.md §5.3, ending on the Kennel Scout's line | 8 |
| M6-NAR-03 🔴 | Deliver Pixel | Companion presence, tutoring, dialogue, the Comprehension tutorial | 5 |
| M6-NAR-04 | Provide the dialogue system | Data-driven, localization-ready, skippable, accessible | 3 |
| M6-NAR-05 | Author 40 Confession micro-quests | Across all archetypes, tied to real world locations | 5 |

### Feature Set M6-AUD — Audio

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M6-AUD-01 🔴 | Provide the 2070 ambient bed | Layered, dense; clarifies as Comprehension rises | 3 |
| M6-AUD-02 🔴 | Provide the 1861 ambient bed | Complete parallel set; cross-fades on Overlay Bleed | 3 |
| M6-AUD-03 | Provide combat and feeding audio | Dry, weighty, close | 2 |
| M6-AUD-04 | Provide echo whisper signatures | ~10 s per authored echo; loop under Sensory Inversion | 3 |
| M6-AUD-05 | Provide the music system | Sparse; silence during hunts; enters on hauntings and bosses | 2 |

### Feature Set M6-HNT — Remaining hauntings

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M6-HNT-01 | Impose Compulsion directives | Real reward/penalty; resolves or expires at dawn | 3 |
| M6-HNT-02 | Invert the senses | Victim VO masks the audio cues you rely on; Bat Sonar counters it | 2 |
| M6-HNT-03 | Puppet an ability slot | Slot loses input authority; the echo casts instead | 2 |
| M6-HNT-04 | Refuse the mirror | Reflections and screens show the victim; biometric scanners misidentify you | 3 |

### Feature Set M6-ACC — Accessibility and localization

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M6-ACC-01 🔴 | Complete the accessibility pass | Photosensitivity toggle, camera pitch ±10° unlock, colourblind palettes, 3 text sizes, dyslexia font, hold-to-toggle everywhere, full remapping, subtitles | 5 |
| M6-ACC-02 | Provide difficulty sliders | Independent combat lethality / survival pressure / haunting frequency | 2 |
| M6-ACC-03 🔴 | Plumb localization | Every string through CSV; no concatenation; no baked text | 3 |
| M6-ACC-04 🔴 | Verify controller and Steam Deck | Full pad layout per Plan.md §7.2; playable start to finish on Deck | 3 |

### 🚦 Gate M6

- [ ] Prologue → Act I climax playable start to finish, no debug commands needed
- [ ] Both ambient beds working; Overlay Bleed cross-fades audio as well as visuals
- [ ] All seven hauntings implemented with safe variants
- [ ] Full accessibility pass complete and verified
- [ ] Completable on a controller and on Steam Deck
- [ ] Zero placeholder strings outside the localization CSV

---

## M7 — Shipping

> **Weeks 43–46 · 28 sessions**
> **Goal:** A Steam page, a demo, a trailer, and twenty real playtesters' worth of fixes.

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M7-SHP-01 🔴 | Publish the Steam page | Steam Direct paid ($100), page live, capsule art, description, tags, wishlist open | 4 |
| M7-SHP-02 🔴 | Cut the announce trailer | 60–90 s, built around Overlay Bleed footage — the shot nobody else has | 4 |
| M7-SHP-03 🔴 | Ship the demo build | Prologue + ~45 min; separate branch; independently stable | 4 |
| M7-SHP-04 🔴⚠️ | Run external playtesting | **20+ players**, structured feedback capture, prioritized fix list | 5 |
| M7-SHP-05 🔴 | Fix the playtest findings | Every P0 and P1 resolved | 5 |
| M7-SHP-06 🔴 | Verify stability | 2 hours crash-free on **both** machines; no leaks over a long session; all budgets met on Windows | 3 |
| M7-SHP-07 | Register for Next Fest | Slot booked, assets submitted | 1 |
| M7-SHP-08 | Complete the licence audit | `ASSET_LICENSES.md` covers every third-party asset in the build | 2 |

### 🚦 Gate M7

- [ ] Steam page live and taking wishlists
- [ ] Demo stable for 20 strangers
- [ ] Zero P0/P1 bugs open
- [ ] 2 hours crash-free confirmed on Windows
- [ ] Every asset's licence documented
- [ ] Sensitivity read completed on all Kalighat / Kali / Bengali content

---

## M8 — Early Access Launch

> **Weeks 47–48 · 14 sessions**

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M8-LCH-01 🔴 | Ship the release build | **Windows (primary) + macOS** per D-20, plus Steam Deck if it stays in scope; depot uploaded; exe metadata correct (export from the Windows target, or use Wine on macOS) | 3 |
| M8-LCH-02 🔴 | Publish the EA roadmap | Public, honest, dated; matches Plan.md §24.3 | 2 |
| M8-LCH-03 | Open the community channels | Discord, bug reporting, feedback intake | 2 |
| M8-LCH-04 🔴 | Provide crash and telemetry reporting | Opt-in, privacy-respecting, actually actionable | 3 |
| M8-LCH-05 | Prepare the hotfix pipeline | You can ship a fix within 24 h of a launch-blocking report | 2 |
| M8-LCH-06 | Write the launch retrospective | In `Agent_History.md`: what the estimates got wrong and by how much | 2 |
| M8-LCH-07 🔴 | Ship the macOS build | **Required by D-20 — no longer optional.** Apple Silicon build tested natively; either notarized ($99/yr Apple Developer) or shipped unsigned with clear Gatekeeper instructions. That signing choice is an open question and must be settled before M7, not at launch | 2 |

**On M8-LCH-07 — decided (D-20): macOS ships in v1.0.** Plan.md §22.1 had it at v1.1; that is superseded. Developing on Apple Silicon means the build is exercised daily for free, on the slice most Mac players actually run. Two consequences worth carrying forward rather than rediscovering at M7: Metal becomes a **shipping** backend, not a dev convenience, so `M1-RND-07`'s dual-backend verification now protects real players on both sides; and the notarization certificate stops being deferrable — either budget $99/yr or commit to shipping unsigned with Gatekeeper instructions, and decide which before M7.

---

## 6. Cadence

### Weekly rhythm

| Day | Focus |
|---|---|
| Mon–Fri | One feature per session block. Core → engine → demo → commit. |
| Sat | Longer session: the milestone's hardest feature, when you have runway |
| Sun | 30 min: update the board, review the week's demos, plan next week's features. **No code.** |

### Monthly

- Re-estimate the current milestone against actual velocity. If more than 20% over, cut from §8 **now** rather than at month nine.
- Run the gate build on the Windows target even mid-milestone. Divergence found early is a bug; found late it's a rewrite.
- Post a devlog clip. Building an audience from month two costs almost nothing and is worth more than any single feature.

### Velocity tracking

After M1, compute actual sessions per feature and rescale the whole plan. **Your first three milestones will overrun.** That is normal and it is information, not failure. What matters is finding out in month three.

---

## 7. Tracking Board

Update at the end of every session.

| Milestone | Features | Done | Sessions est. | Sessions actual | Status |
|---|---|---|---|---|---|
| M0 Literacy | 10 | 5 | 15 | ~5 | 🟨 In progress |
| M1 Feel | 21 | 0 | 35 | 0 | ⬜ |
| M2 Predation | 14 | 0 | 42 | 0 | ⬜ |
| M3 The Hook ⚠️ | 20 | 0 | 56 | 0 | ⬜ |
| M4 The City | 21 | 0 | 49 | 0 | ⬜ |
| M5 Power & Pressure | 21 | 0 | 49 | 0 | ⬜ |
| M6 The Story | 17 | 0 | 42 | 0 | ⬜ |
| M7 Shipping | 8 | 0 | 28 | 0 | ⬜ |
| M8 Launch | 7 | 0 | 16 | 0 | ⬜ |
| **Total** | **139** | **5** | **332** | **~5** | |

Sessions actual for M0 is approximate — sessions 002–004 mixed design authoring with
environment work. Track it precisely from M1, where the velocity rescale depends on it.

**Done:** M0-ENV-01 (all 14 Appendix A rows green), M0-ENV-02, M0-ENV-03, M0-ENV-04, M0-ENV-05
**Active feature:** none — M0-DBG-01 is next and its Feature Card is awaiting approval
**Blocked on:** nothing
**Next:** M0-DBG-01 (debug menu — first real code feature, first demoable one) → M0-LRN-05 → M0-ENV-06
**Last demo recorded:** _none_ — M0-ENV work is not filmable; M0-DBG-01 is the first feature with a demo artifact

---

## 8. Cut Triggers

Pre-agreed so the decision is unemotional when you're tired and behind. Cut **in this order**, and only when >20% over budget on a milestone:

1. All minigames except vending machine and transit terminal
2. All vehicles except tram-hopping
3. Confession micro-quests: 40 → 20
4. Echo archetypes: 3 → 2
5. Crowd NPC count: 250 → 120
6. Lair rooms: ship only Coffin, Cistern, Reliquary, Blood Cellar
7. Weather: reduce to a rain visual plus the sun-damage modifier
8. Powers: 12 → 8

**Never cut:** the Ledger, the seven Hauntings (or their safe variants), the Sun Map, Sip vs Drain, the Case File, the accessibility pass. These are the game, and the last one is a duty of care.

---

## 9. Risk Register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| The hook isn't fun | Medium | **Fatal** | M3 gate with five external testers. Honour it. |
| Scope creep | **High** | Severe | Feature Cards with explicit OUT lists; agent contract forbids unrequested work |
| Solo burnout | **High** | Severe | Demoable increment every 1–3 sessions; Sunday no-code rule; devlog for external validation |
| Velocity overrun | **Very high** | Moderate | Re-estimate monthly; cut list pre-agreed |
| **Metal vs D3D12 divergence** | Medium | **Severe → now unavoidable** | Both are *shipping* backends under D-20, so this stopped being a dev-vs-verify risk and became a two-platform correctness requirement. Develop on Metal, gate on Windows from M0-ENV-06; M1-RND-07 verifies the post chain on both and profiles Vulkan as the fallback; run gate builds on Windows every milestone |
| **Over-budget because M4 Pro is too fast** | **High** | Moderate | All performance numbers measured on the Windows target, never on the Mac |
| Godot 3D perf on open world | Medium | Severe | Chunk streaming + directors from M4; profile from M2, not M10 |
| Pixel-3D illegibility | Medium | Severe | Golden-image tests; readability over aesthetic purity (Pillar 6) |
| AI-generated code rot | Medium | Severe | Core/presentation split; tests as the contract; independent reviewer sessions |
| gdUnit4 / Godot version drift | **High** | Moderate | Engine is pinned at 4.7.2, three minor versions past gdUnit4Net's published 4.3–4.4.1 support matrix. If it hasn't caught up by M1-TST-01, drop it and rely on the xUnit core suite plus a headless smoke test and the debug menu. Core tests are pure xUnit and immune |
| **GitHub LFS quota exceeded** | Medium | Low | Source assets stay outside the repo (§2); only exported files committed; fallback is a $5/mo data pack |
| Case-sensitivity break on Linux export | Medium | Moderate | CI lowercase-path guard from M0-ENV-04; both dev machines are case-insensitive and will not warn you |
| Cultural insensitivity | Medium | **Reputational** | Fictionalized shrine and order; sensitivity read before M7 ships |
| Save corruption in EA | Medium | Severe | Atomic writes + migration chain written and tested at M4, not when needed |

---

## 10. Prompt Seed Library

Reusable shapes. Adapt per feature; never skip the OUT list.

**Core logic**
> "Implement `<ID>` in `RaktabeejCore` per `docs/Plan.md` §X. Pure C#, no Godot reference. `<behaviour spec with exact numbers>`. Write xUnit tests covering `<boundaries>`. Do NOT implement `<OUT list>`. Report the real `dotnet test` output."

**Engine wiring**
> "Wire the existing `RaktabeejCore.<Type>` into the game for `<ID>`. Read core state, render it, do not duplicate rules in the node. Register a debug-menu command to set the state directly. Do NOT change core logic — if the feature seems to need a core change, stop and tell me."

**Shader**
> "Implement `<ID>`, the `<effect>` stage of the post chain in `docs/Plan.md` §18.2. Godot Shading Language. It must compose with the existing stages in the documented order. Add a debug toggle for this stage alone. Include the photosensitivity-safe variant in this same feature. Keep the whole chain under 2 ms. Note anything that might behave differently on Vulkan/D3D12 than on Metal."

**Bulk content**
> "Author `<N>` `EchoDefinition` `.tres` resources for the `<archetype>` archetype following `docs/Plan.md` §3.2. Each needs id, name template, valence, dissonance weight, tags from the canonical tag list, anchor site, unfinished act, and a 2–3 sentence vignette. Tags must be consistent with the archetype's real life. Do not add new tags to the canonical list without asking."

**Review**
> "Review the diff on branch `<branch>` against Feature Card `<ID>`. Report: (1) anything implemented outside the IN scope, (2) any stub, TODO, or fixed-value return, (3) any test that asserts the implementation rather than the requirement, (4) any core file that now references Godot, (5) any hardcoded balance number that belongs in a `.tres`, (6) any asset path under `game/` that is not lowercase_snake_case, or any `.cs` script under `game/` whose class name does not match its PascalCase file name exactly. Do not fix anything — report only."

---

## Appendix A — Environment Setup

Complete sequence to rebuild the dev environment from a fresh macOS install. Detailed rationale for each step is in the project chat log; this is the executable version.

### A.1 macOS toolchain

```bash
# 1 — Command line tools (GUI dialog: click Install)
xcode-select --install

# 2 — Homebrew, then put it on PATH (Apple Silicon installs to /opt/homebrew)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
echo >> ~/.zprofile
echo 'eval "$(/opt/homebrew/bin/brew shellenv)"' >> ~/.zprofile
eval "$(/opt/homebrew/bin/brew shellenv)"

# 3 — .NET 8 SDK: download the Arm64 .pkg from
#     dotnet.microsoft.com/download/dotnet/8.0  — NOT the x64 build.
#     Verify: dotnet --info must show Architecture: arm64

# 4 — Godot .NET: download the ".NET" macOS build from
#     godotengine.org/download/macos, unzip, drag the .app to /Applications.
#     Right-click → Open if Gatekeeper objects.
ls /Applications | grep -i godot     # confirm the exact app name
cat >> ~/.zshrc << 'EOF'

# Godot
export GODOT4="/Applications/Godot_mono.app/Contents/MacOS/Godot"
alias godot="$GODOT4"
EOF
source ~/.zshrc
godot --version                      # MUST contain "mono"

# 5 — VS Code + tooling
brew install --cask visual-studio-code
#   Cmd+Shift+P → "Shell Command: Install 'code' command in PATH"
#   Extensions: C# Dev Kit, godot-tools, EditorConfig
#   Setting godotTools.editorPath.godot4 = the path above

# 6 — Git, LFS, GitHub CLI
brew install git git-lfs gh
git config --global user.name "Nischaya Agarg"
git config --global user.email "nischayagarg008@gmail.com"
git config --global init.defaultBranch main
git config --global core.autocrlf false
git config --global pull.rebase false
git lfs install
gh auth login                        # GitHub.com → HTTPS → Yes → web browser
```

### A.2 Project layout

```bash
mkdir -p ~/dev
cd ~/dev
git clone https://github.com/Nischaya008/Raktabeej.git raktabeej
cd raktabeej

# Source assets live OUTSIDE the repo
mkdir -p ~/dev/raktabeej-assets/{blender,voxel,audio_raw,reference}
```

Then: `global.json` (pin .NET 8) → `dotnet new sln` + core + tests → `.gitattributes` / `.gitignore` → create the Godot project in `game/` via the GUI (Forward+, then attach a C# script to generate the csproj) → `dotnet add game/Raktabeej.csproj reference RaktabeejCore/RaktabeejCore.csproj` → `.vscode/launch.json` and `tasks.json` (hardcode the Godot path; VS Code does not inherit `~/.zshrc`) → `.github/workflows/ci.yml` → `.kiro/steering/` × 3.

### A.3 Godot project settings

| Setting | Value |
|---|---|
| Rendering → Renderer | **Forward+** |
| Rendering → Rendering Device → Driver | **macOS: leave default** (resolves to Metal). **Windows: keep the explicit pin `rendering_device/driver.windows="d3d12"` in `project.godot`** — decision **D-19**. D3D12 is Godot's own Windows default from 4.6 onward, because Windows Vulkan drivers are poorly maintained and have broken shipped Godot games. Do **not** "leave it default": the default changed once already between 4.5 and 4.6, so only an explicit pin is stable across engine versions |
| Physics → 3D → Physics Engine | **Jolt Physics** |
| **Application → Run → Main Scene** | **`res://scenes/main.tscn` — MUST be set.** Without it F5 builds successfully and then opens no window, with no error explaining why |
| Version Control Metadata | None (git is configured separately) |

### A.4 Verification checklist

| # | Check | Command | Expected |
|---|---|---|---|
| 1 | Command line tools | `xcode-select -p` | `/Library/Developer/CommandLineTools` |
| 2 | Homebrew on PATH | `which brew` | `/opt/homebrew/bin/brew` |
| 3 | .NET SDK version | `dotnet --list-sdks` | `8.0.x` |
| 4 | .NET is native arm64 | `dotnet --info \| grep Architecture` | `arm64` |
| 5 | Godot is the .NET build | `godot --version` | contains `mono` |
| 6 | Core builds | `dotnet build` | 0 errors |
| 7 | Tests pass | `dotnet test` | 1+ passing |
| 8 | Godot headless | `godot --headless --path game --quit` | exit 0 **and prints `boot: C# assembly loaded`**. That line is the liveness probe in `src/Main.cs` — without it, exit 0 only proves the engine started, not that the C# assembly loaded and `_Ready()` ran |
| 9 | **F5 debugging** | breakpoint in `src/Main.cs` `_Ready()` | breakpoint hits. **Select the "Play" config in the Run and Debug dropdown first.** Pressing F5 with a `.cs` file focused lets C# Dev Kit hijack it and report `does not support debugging. No launchable target found` — a Godot game assembly is a *library* with no entry point. Build output goes to **Terminal**; `GD.Print` and debugger status go to **Debug Console** (Cmd+Shift+Y) |
| 10 | Jolt enabled | Project Settings → Physics → 3D | Jolt Physics |
| 11 | Repo outside iCloud | `pwd` | `/Users/gargnisc/dev/raktabeej` |
| 12 | GitHub auth | `gh auth status` | logged in as Nischaya008 |
| 13 | CI green | Actions tab on GitHub | both jobs pass |
| 14 | No junk committed | `git ls-files \| grep -E '\.blend\|bin/\|\.godot'` | no output |

### A.5 Windows verification target (M0-ENV-06)

On the Mac: **Editor → Manage Export Templates → Download and Install**, then **Project → Export → Add → Windows Desktop**. Export to `builds/windows/`.

Copy the `.exe` and `.pck` to the Windows machine and run. Confirm the debug overlay is readable and note the frame time — that number, not the Mac's, is your budget baseline.

For polished exe metadata (icon, version info) Godot needs `rcedit`, which requires Wine on macOS (`brew install --cask wine-stable`). Alternatively do release exports on the Windows machine. Functional debug builds do not need this.
