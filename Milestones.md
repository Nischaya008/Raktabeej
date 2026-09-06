Let me verify the two install details I'd otherwise be guessing at — the Godot .NET download naming and gdUnit4's current C# install path.Two corrections to Plan.md first, both from checking current tooling — they simplify your day one:

**gdUnit4 changed significantly.** gdUnit4Net v5+ is now VSTest-compatible and installs as NuGet packages (`gdUnit4.api`, `gdUnit4.test.adapter`), runs through plain `dotnet test`, and only spins up the Godot runtime for tests marked `[RequireGodotRuntime]`. But its published support badges list Godot 4.3–4.4.1, not 4.5. So: **use xUnit only on day one** (zero Godot coupling, zero version risk) and add gdUnit4Net at M1 when you actually need to test the camera rig, pinning your Godot version to whatever it supports then.

**Don't put Godot export into CI on day one.** It needs the headless binary plus matching export templates downloaded in the runner — a genuine rabbit hole. Start CI as build-core + run-tests. Add the export job at M1.

---

# Part 1 — Windows bootstrap, step by step

## Step 0: Pre-flight (do this before installing anything)

Three Windows-specific traps that will cost you days if you hit them later.

**Put the project outside OneDrive.** If your repo lives under a OneDrive-synced `Documents`, OneDrive will lock files mid-build and corrupt `.godot/` caches. Use `C:\dev\raktabeej`.

**Enable long paths.** Godot's `.godot/mono` paths plus NuGet nesting will exceed 260 characters. In an **Administrator** PowerShell:

```powershell
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" `
  -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
```

**Add Defender exclusions for your dev folder.** Real-time scanning of MSBuild output makes builds 2–3× slower. Admin PowerShell:

```powershell
Add-MpPreference -ExclusionPath "C:\dev"
Add-MpPreference -ExclusionProcess "dotnet.exe"
Add-MpPreference -ExclusionProcess "MSBuild.exe"
```

That does reduce your scanning coverage on `C:\dev` — reasonable on a dev box you control, but it is a deliberate security tradeoff, not a free win. Skip it if you'd rather not.

Also: confirm you have **16 GB RAM** and **~60 GB free disk**, and update your NVIDIA driver before installing Godot. Vulkan issues on stale drivers are the single most common "Godot won't start" cause.

## Step 1: Install the toolchain

```powershell
winget install --id Git.Git -e
winget install --id Microsoft.DotNet.SDK.8 -e
winget install --id Microsoft.VisualStudioCode -e
```

.NET **8 LTS** is the right choice — Godot 4.5 needs 8 or later, and .NET 9 is only required for Android export, which is post-1.0. Skip Blender and the art tools for now; you don't need them until M1 Task 7, and installing eleven programs on day one is how motivation dies before code exists.

If any package ID fails, find it with `winget search <name>` rather than guessing.

## Step 2: Install Godot .NET

Godot isn't reliably packaged on winget, and you need the **.NET build specifically** — the standard build cannot run C# at all.

1. Go to `godotengine.org/download/windows`
2. Download **Godot Engine – .NET** (not the plain version)
3. Extract to `C:\dev\tools\godot\`
4. Rename the executable to `godot.exe`
5. Add `C:\dev\tools\godot` to your user PATH
6. Set a `GODOT4` environment variable to `C:\dev\tools\godot\godot.exe` — the VS Code debug config uses it

## Step 3: Verify before proceeding

```powershell
dotnet --version    # expect 8.0.x
git --version
godot --version     # must say "mono" or ".NET" in the string
```

If `godot --version` doesn't mention mono/.NET, you downloaded the wrong build. Stop and fix it — everything downstream depends on this.

## Step 4: Configure Git

```powershell
git config --global user.name "Your Name"
git config --global user.email "you@example.com"
git config --global init.defaultBranch main
git config --global core.autocrlf false
git config --global core.longpaths true
git lfs install
```

`core.autocrlf false` matters: combined with the `.gitattributes` below it keeps line endings LF everywhere, so the repo stays sane if you ever touch it from the Mac or a Linux CI runner.

## Step 5: Pick your repo host now

Per Plan.md §23.3, GitHub's free LFS quota is 1 GB storage / 1 GB monthly bandwidth, which a 3D project burns through in weeks.

**Recommended: Azure DevOps** — free unlimited private repos, no separate LFS billing. Create an organization and project at `dev.azure.com`, then use its repo as `origin`. Keep a GitHub mirror later if you want Actions CI (you can push to both).

**Alternative:** GitHub with the lean-repo discipline — source assets (`.blend`, raw `.wav`) live in separate cloud storage, only exported game-ready files (`.glb`, `.ogg`, `.png`) go in the repo.

Decide today. Migrating LFS history later is genuinely painful.

## Step 6: Scaffold the solution

```powershell
cd C:\dev
mkdir raktabeej
cd raktabeej
git init

dotnet new sln -n Raktabeej
dotnet new classlib -o RaktabeejCore -f net8.0
dotnet new xunit -o RaktabeejCore.Tests -f net8.0
dotnet sln add RaktabeejCore RaktabeejCore.Tests
dotnet add RaktabeejCore.Tests reference RaktabeejCore

mkdir game, docs, tools, .github\workflows

dotnet test
```

`dotnet test` must pass with the template's placeholder test before you continue. That's your first green build.

## Step 7: Add the two Git config files

**`.gitattributes`** — the `.tres` / `.tscn` lines are important, they keep your content diffable:

```gitattributes
* text=auto eol=lf

*.cs      text diff=csharp
*.tres    text
*.tscn    text
*.godot   text
*.gd      text
*.md      text

*.png  filter=lfs diff=lfs merge=lfs -text
*.jpg  filter=lfs diff=lfs merge=lfs -text
*.exr  filter=lfs diff=lfs merge=lfs -text
*.hdr  filter=lfs diff=lfs merge=lfs -text
*.glb  filter=lfs diff=lfs merge=lfs -text
*.gltf filter=lfs diff=lfs merge=lfs -text
*.blend filter=lfs diff=lfs merge=lfs -text
*.wav  filter=lfs diff=lfs merge=lfs -text
*.ogg  filter=lfs diff=lfs merge=lfs -text
*.ttf  filter=lfs diff=lfs merge=lfs -text
```

**`.gitignore`**:

```gitignore
# Godot
.godot/
game/.godot/
*.translation

# .NET
bin/
obj/
*.user
*.suo
[Dd]ebug/
[Rr]elease/

# Builds
builds/
*.pck
*.exe

# OS
Thumbs.db
.DS_Store
```

Note on `export_presets.cfg`: **commit it** (CI needs it), but never put an Android keystore password in it — those go in environment variables. Desktop presets contain no secrets.

## Step 8: Create the Godot project and link the core library

1. Launch Godot .NET → **New Project** → path `C:\dev\raktabeej\game` → renderer **Forward+**
2. In the editor, attach a C# script to any node. This is what generates `game/Raktabeej.csproj`. Nothing C#-related works until you do it once.
3. Back in the terminal:

```powershell
dotnet sln add game\Raktabeej.csproj
dotnet add game\Raktabeej.csproj reference RaktabeejCore
dotnet build
```

Now your Godot project can call into `RaktabeejCore`, and one root solution holds all three projects. Godot won't clobber the reference — it only regenerates the csproj if it's missing.

4. In Godot: **Project Settings → Physics → 3D → Physics Engine → Jolt Physics**, then Save & Restart. (New 4.4+ projects default to Jolt, but verify — it's the one setting you don't want wrong at month six.)

## Step 9: Configure VS Code

Install extensions: **C# Dev Kit**, **godot-tools**, **EditorConfig for VS Code**.

In godot-tools settings, set the editor path to `C:\dev\tools\godot\godot.exe`.

Create `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Play",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${env:GODOT4}",
      "args": ["--path", "${workspaceFolder}/game"],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false
    }
  ]
}
```

And `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build"],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

F5 now launches the game with C# breakpoints working. Verify that before moving on — debugging is the difference between vibe-coding that converges and vibe-coding that flails.

## Step 10: Minimal CI

`.github/workflows/ci.yml` (or the Azure Pipelines equivalent):

```yaml
name: ci
on: [push, pull_request]

jobs:
  core:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet build RaktabeejCore/RaktabeejCore.csproj
      - run: dotnet test RaktabeejCore.Tests/RaktabeejCore.Tests.csproj --verbosity normal
```

Note it builds only the core projects, not `game/` — the Godot csproj needs the engine's SDK present, which the runner doesn't have yet. Add the Godot job at M1.

## Step 11: Set up the vibe-coding harness

This is the step people skip and then wonder why the agent drifts.

```powershell
mkdir .kiro\steering
```

Move your two planning documents into the repo:

```
docs/Plan.md          ← from your Mac; this is the constitution
docs/Milestones.md    ← from Part 2 below; this is the schedule
Agent_History.md      ← running log, newest entry on top
Changes.md            ← what changed and why, per session
```

Then create three always-loaded steering files. Keep them short — steering that's too long gets diluted:

**`.kiro/steering/product.md`** — the six design pillars verbatim from Plan.md §2, plus a hard statement of what is out of scope for the current milestone.

**`.kiro/steering/tech.md`** — the stack, and these non-negotiables:
- `RaktabeejCore` must never reference Godot. If a change needs a Godot type in core, the design is wrong.
- Every core rule ships with xUnit tests in the same commit.
- All content is a `[GlobalClass] Resource` in `game/data/`, never hardcoded.
- All file and folder names are `lowercase_snake_case` (Linux export is case-sensitive).
- `dotnet build` and `dotnet test` must pass before any commit.

**`.kiro/steering/structure.md`** — the folder map from Plan.md §21.3.

## Step 12: First commit and smoke test

```powershell
godot --headless --path game --quit
echo $LASTEXITCODE        # must be 0

git add .
git commit -m "chore: Scaffold Godot .NET project and core library"
git remote add origin <your-repo-url>
git push -u origin main
```

## Verification checklist

Do not start M0 Task 2 until every row is true:

| Check | Command / action | Expected |
|---|---|---|
| .NET SDK | `dotnet --version` | `8.0.x` |
| Godot is the .NET build | `godot --version` | contains `mono` |
| Core builds | `dotnet build` | 0 errors |
| Tests run | `dotnet test` | 1+ passing |
| Godot opens headless | `godot --headless --path game --quit` | exit code 0 |
| F5 debugging | VS Code F5, breakpoint in `_Ready()` | breakpoint hits |
| Jolt active | Project Settings → Physics → 3D | Jolt Physics |
| Repo pushed | `git log --oneline -1` on remote | your commit |
| CI green | Push and check | ✅ |
| Not in OneDrive | Path is `C:\dev\...` | ✅ |

---

# Part 2 — Milestones.md

Paste this into `/Users/gargnisc/Downloads/Career/Projects/DEV/Milestones.md`, then move it to `docs/Milestones.md` in the Windows repo.

I've used FDD adapted for solo work: FDD's real value here is the *feature list as the unit of planning and tracking*, with a strict "each feature is client-valued and demoable" rule. What I've dropped is FDD's team apparatus (chief programmers, feature teams, class ownership) since it's meaningless for one person. The estimating unit is a **session** — one focused ~3-hour block — because that's the honest unit for part-time solo work, and it makes slippage visible within a week instead of within a quarter.

````markdown
# RAKTABEEJ — Milestone & Feature Delivery Plan

> **Companion to `docs/Plan.md`.**
> Plan.md is the **constitution** — what the game is and why. It changes rarely and deliberately.
> This document is the **schedule** — what gets built, in what order, and how you know it's done. It changes weekly.
>
> **Status:** v1.0
> **Delivery model:** Feature-Driven Development, adapted for solo AI-assisted work
> **Estimating unit:** 1 session = one focused ~3-hour block
> **Assumed capacity:** 7 sessions/week (~21 h) → ~336 sessions to Early Access
> **Target:** Early Access, Windows + Steam Deck, week 48

---

## 0. How to Use This Document

1. **Never work without an active Feature Card.** Pick the next feature from the current milestone table, write its card (§3), work it, demo it, commit it, tick it off.
2. **One feature = one branch = one commit = one demo.** If a feature can't be demoed, it isn't a feature — it's a task hiding inside one. Merge it into its parent.
3. **Gates are hard stops.** A gate is not a checkpoint you note and pass. If a gate fails, you stop and fix or stop and rethink. The M3 gate can end the project; that is its job.
4. **Update the tracking board (§7) at the end of every session.** Two minutes. It is the only defence against the month-eight realisation that you're twelve weeks behind.
5. **When this document and Plan.md disagree, Plan.md wins** — unless you deliberately amend Plan.md and note it in `Changes.md`.

---

## 1. Delivery Model

### Why FDD

FDD fits this project for three reasons:

- **Features are client-valued and demoable.** For a solo dev with no team to hold them accountable, a demoable increment every 1–3 sessions is the only reliable motivation engine. You always have something to show.
- **It front-loads the domain model.** Plan.md *is* the overall model (FDD process #1). The `RaktabeejCore` library is that model made executable. This is exactly FDD's shape.
- **It plans by feature, not by layer.** You will never spend six weeks building "the UI system" with nothing playable. Every feature cuts vertically through core → engine → UI.

### What is deliberately dropped

Chief programmers, feature teams, class ownership, and inspections. Irrelevant for one person. Their function — independent review — is replaced by the **agent/reviewer separation** in §4.

### Hierarchy

```
Major Feature Set   = Milestone (M0–M8)
  Feature Set       = a coherent subsystem within the milestone
    Feature         = 1–3 sessions, demoable, ID'd, tracked
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

A feature is Done only when **all nine** are true. No partial credit.

1. `dotnet build` passes with **zero warnings** in `RaktabeejCore` (warnings-as-errors on core only).
2. `dotnet test` passes. New core logic has xUnit tests **in the same commit**.
3. The feature is reachable in a running build without a debugger attached.
4. It is exercisable from the **debug menu** (set the state, trigger the effect).
5. No `TODO`, no `NotImplementedException`, no commented-out code, no stub returning a fixed value.
6. Frame time still inside budget (Plan.md §21.8). Check the overlay, don't assume.
7. A **demo artifact** exists: a 10–30 s GIF or clip in `docs/demos/<feature-id>.gif`.
8. Committed on a feature branch, squashed to **1–2 commits**, merged to `main`.
9. `Changes.md` has a one-line entry. Tracking board updated.

### Core architecture rules — non-negotiable

- `RaktabeejCore` **never** references Godot. If you think it must, the design is wrong.
- Game rules live in core. Godot nodes only read core state and render it.
- All content is a `[GlobalClass] Resource` in `game/data/`. Nothing balance-related is hardcoded in C#.
- All filenames `lowercase_snake_case`. Linux export is case-sensitive; Windows will not warn you.
- Save serialization is POCO + `System.Text.Json`. Never serialize a Godot node.

### Content rules

- Every third-party asset is logged in `docs/ASSET_LICENSES.md` **in the commit that adds it**. No exceptions, ever.
- All player-facing strings go through the localization CSV from M1 onward. Never concatenate translated strings.
- Every visual effect gets its photosensitivity-safe variant **in the same feature**, not in a later accessibility pass.

---

## 3. Feature Card Template

Write this before you write code. It takes four minutes and it is what makes the agent useful instead of chaotic.

```markdown
## Feature Card — M4-SUN-02

**Feature:** Project the moving shadow map for the Sun Map overlay
**Feature Set:** M4-SUN (Sunlight & Sun Map)
**Plan.md reference:** §10.1, §21.5
**Estimate:** 2 sessions

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
- Weather effects on the projection (that is M4-WTH-01)

### Core changes (RaktabeejCore)
- `SunExposure.GetSafetyAt(Vector3Like, GameTime)` — pure, testable

### Engine changes (game/)
- `World/SunMap.cs` — reads core, renders overlay
- `Ui/SunMapScreen.tscn`

### Tests
- xUnit: exposure state correct for 20 parameterized positions × 6 times of day
- xUnit: projection is deterministic for a given clock value
- Manual: overlay-reported safe zone matches actual runtime damage at 5 spots

### Demo
Stand in the open at 05:20, open the Sun Map, walk the shadow corridor home,
arrive at 05:38 alive. Record the clip.

### Done when
All nine DoD conditions met.
```

---

## 4. Vibe-Coding Protocol

Full AI-assisted development works on a project this size **only** with a verification harness. Without one it produces four months of plausible code that doesn't hold together. These rules are that harness.

### 4.1 The document hierarchy

| Document | Role | Change frequency |
|---|---|---|
| `docs/Plan.md` | Constitution. What the game is. | Rarely, deliberately |
| `docs/Milestones.md` | Schedule. This file. | Weekly |
| Feature Card | Work order. One at a time. | Per feature |
| `.kiro/steering/*.md` | Standing constraints the agent always loads | Rarely |
| `Changes.md` | What changed, one line per feature | Per feature |
| `Agent_History.md` | Decisions and rationale, newest on top | Per session |

### 4.2 The session loop

Every session, without deviation:

1. **Orient (5 min).** Read the tracking board and the top of `Agent_History.md`. Confirm which feature is active.
2. **Card (5 min).** If starting a new feature, write its card. If you cannot write the OUT-of-scope list, you do not understand the feature yet.
3. **Branch.** `git checkout -b feat/m4-sun-02`
4. **Core first.** Have the agent implement the core logic + tests. Run `dotnet test`. This step is fully verifiable — the tests either pass or they don't. Do not proceed while red.
5. **Engine second.** Wire it into Godot. Verify by playing, not by reading code.
6. **Debug hook.** Add the debug-menu command for this feature.
7. **Demo.** Record the clip. If you cannot produce a compelling 15 seconds, the feature is not done.
8. **Review.** Run your reviewer pass on the diff (§4.4).
9. **Land.** Squash, merge, update `Changes.md`, `Agent_History.md`, and the board.

**One feature per session block.** If a feature needs three sessions, it stays on its branch across them — but every session still ends on a green build.

### 4.3 The agent contract

Put this in `.kiro/steering/tech.md` so it loads every time:

```
- Read docs/Plan.md and the active Feature Card before writing code.
- Implement ONLY what the Feature Card's IN scope lists. If the work seems to
  require something in the OUT list, stop and say so. Do not build it.
- RaktabeejCore must never reference Godot.
- Every core rule change ships with xUnit tests in the same commit.
- Run `dotnet build` and `dotnet test` and report the actual output before
  claiming a feature works. Never claim a passing build you did not run.
- Never delete, skip, or weaken an existing test to make a build green.
- Never leave a stub, TODO, or NotImplementedException in a feature you report
  as complete.
- Balance numbers belong in .tres resources, not in C#.
```

### 4.4 Independent review

You are the only gate on the agent's work, so scrutinise it. Never accept a "done" claim at face value.

For every feature, before merging:

- **Read the diff yourself.** Every file. Not the summary.
- **Confirm the tests are real.** A test that asserts the implementation rather than the requirement is worse than no test. Ask: would this test fail if the feature were wrong?
- **Hunt the four failure modes:** silent stubs; tests deleted or weakened to go green; scope creep beyond the card; results reported but never actually run.
- **Run a separate reviewer session** on non-trivial diffs. A reviewer must never be the same session that wrote the code.
- **Check the pillars.** Does this feature serve one of the six in Plan.md §2? If not, why does it exist?

### 4.5 Where the agent excels vs where you must drive

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

### Legend

- **Est.** = sessions (~3 h each)
- 🔴 = gate feature; the milestone cannot exit without it
- ⚠️ = high risk; schedule it early in the milestone, not late

---

## M0 — Literacy & Foundations

> **Weeks 1–3 · 21 sessions**
> **Goal:** You can build, run, test, debug, and ship a Godot C# project, and you know the engine well enough that the real project isn't your teaching sandbox.

**Do not skip this milestone.** Learning an engine on your actual project is the single most common way first games die. Three weeks here buys back three months later.

### Feature Set M0-ENV — Environment

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M0-ENV-01 🔴 | Provision the Windows toolchain | Every row of the Part 1 verification checklist is green | 2 |
| M0-ENV-02 🔴 | Establish the repository and LFS host | Repo pushed; LFS host decided and configured; `.gitattributes` + `.gitignore` in place | 1 |
| M0-ENV-03 | Scaffold the solution and Godot project | Root solution holds core + tests + game; game references core; Jolt enabled | 1 |
| M0-ENV-04 | Establish continuous integration | Push triggers build + test; badge green | 1 |
| M0-ENV-05 | Establish the AI harness | `.kiro/steering/` populated; Plan.md and Milestones.md in `docs/`; `Changes.md` and `Agent_History.md` created | 1 |

### Feature Set M0-LRN — Engine literacy

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M0-LRN-01 | Ship a complete 2D Pong | Start screen, win/lose, sound, exported build runs standalone | 2 |
| M0-LRN-02 | Ship a complete top-down 3D arena shooter | One enemy type, aiming, health, win/lose, sound | 4 |
| M0-LRN-03 | Ship a complete 3D platformer with follow camera | Jump feel tuned, moving platforms, a camera you wrote yourself | 4 |
| M0-LRN-04 🔴 | Record the engine learnings | `docs/LEARNINGS.md` covers node lifecycle, signals, `_Process` vs `_PhysicsProcess`, resource loading, the C# struct-copy trap, export gotchas | 1 |

Build these in a throwaway `sandbox/` repo, not this one. **The artifact is your competence, not the code.** Delete them after.

### Feature Set M0-DBG — Debug infrastructure

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M0-DBG-01 🔴 | Provide the debug menu and dev overlay | F1 panel with frame-time overlay, free-camera toggle, clock scrubber, extensible command registry; stripped from release exports | 3 |
| M0-DBG-02 | Provide a frame-time and draw-call overlay | Live ms breakdown and draw-call count against Plan.md §21.8 budgets | 1 |

The debug menu is infrastructure, not a luxury. Every later milestone assumes you can force any state on demand. Build it now, grow it forever.

### 🚦 Gate M0

- [ ] All ten verification-checklist rows green
- [ ] Three learning games finished and playable end to end
- [ ] `docs/LEARNINGS.md` written
- [ ] Debug menu working in an exported build
- [ ] CI green on a push
- [ ] **You can answer without looking it up:** how do I load a resource, how do I connect a signal, how do I export a Windows build?

**Prompt seed:**
> "Read `docs/Plan.md` §21.9 and `.kiro/steering/tech.md`. Implement feature M0-DBG-01: a debug overlay autoload with an F1-toggled panel, a frame-time readout, and a command registry other systems call `DebugService.Register(name, callback)` on. It must compile out of release exports. Only what the card lists — no game-specific commands yet. Show me the actual `dotnet build` output."

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
| M1-CAM-04 🔴 | Resolve the aim-versus-rotate conflict | While RMB is held, aim freezes at last vector and abilities remain castable in that direction (Plan.md §6.3) | 2 |
| M1-CAM-05 | Expose camera tuning parameters | All values `[Export]`ed and live-tunable from the debug menu | 1 |

### Feature Set M1-MOV — Movement

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-MOV-01 🔴 | Provide camera-relative character movement | Correct at all 360° of yaw; verified by parameterized test at 8 yaw values | 3 |
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
| M1-RND-05 🔴 | Render the UI at native resolution | Separate `CanvasLayer` above the viewport; text crisp at 1080p / 1440p / 4K | 2 |
| M1-RND-06 | Author the three sub-palettes | 2070 Sprawl, 1861 Calcutta, Sanguine Sight — each with a colourblind-safe variant | 2 |
| M1-RND-07 | Verify the pipeline by golden image | Fixed test scene rendered and compared per stage; post chain under 2 ms | 2 |

### Feature Set M1-KIT — Modular art kit

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-KIT-01 | Establish the Blender→glTF→Godot pipeline | Repeatable from clean checkout; export script in `tools/` | 2 |
| M1-KIT-02 | Author the first 20 building modules | Wall, floor, roof, stair, balcony, awning, railing, signage on a shared 256² atlas, within poly budget | 3 |
| M1-KIT-03 🔴 | Assemble the first Stack chunk | One 48×48 m block, baked `LightmapGI`, emissive neon, inside draw-call budget | 3 |

### Feature Set M1-TST — Test infrastructure

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M1-TST-01 | Adopt gdUnit4Net for engine tests | NuGet packages added; Godot version pinned to a supported release; camera tests run under `dotnet test` | 2 |
| M1-TST-02 | Add the Godot export job to CI | Headless build + Windows export artifact on every push | 2 |

### 🚦 Gate M1 — the Feel Gate

- [ ] Camera and movement behave exactly to Plan.md §6 and §7
- [ ] Pixel pipeline running; UI text crisp
- [ ] One real-looking neon street block exists
- [ ] Frame time inside budget with the post chain active
- [ ] **The subjective gate: play it for 10 minutes with no objective, no enemies, no content. Is moving around genuinely pleasurable?**

**If the answer is no, stop and fix it.** Do not proceed hoping content will compensate. It will not. Every one of the next 280 sessions sits on top of this.

**Prompt seed:**
> "Implement M1-CAM-01 per `docs/Plan.md` §6.1–6.2. Build the `CameraPivot → YawGimbal → SpringArm3D → Camera3D` hierarchy. Pitch is locked at −52° and must be unchangeable by input. Yaw rotates 360° continuously while RMB (mouse) or L2+right-stick (pad) is held, at 140°/s mouse-scaled and 180°/s stick. Position damping 0.12 s. Expose every value as `[Export]`. Do NOT implement zoom or occlusion — those are separate features. Write gdUnit4 tests asserting pitch never changes and yaw wraps correctly across the 0/360 boundary."

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
| M2-FED-03 | Provide the combat grapple feed | Staggered humans can be grappled for a risky in-combat Drain | 2 |
| M2-FED-04 | Provide Rend as a combat finisher | 40% blood, always-Cold corrupted echo, loud, high Reek | 2 |
| M2-FED-05 | Flag surviving Sip victims as witnesses | Every Sip creates a persistent witness record | 1 |

### 🚦 Gate M2

- [ ] Core blood suite passes in under 2 s with ≥95% coverage
- [ ] Combat is readable and weighty at the fixed camera angle
- [ ] Sip vs Drain is a decision you actually feel while playing
- [ ] The Memory Core preview appears mid-feed and is legible
- [ ] Frame time in budget with 8 enemies active

---

## M3 — THE HOOK ⚠️🔴

> **Weeks 15–22 · 56 sessions**
> **Goal:** The Sanguine Ledger works, hauntings intrude systemically, and progression runs on biography.

**This is the make-or-break milestone.** Everything before it is scaffolding; everything after it assumes this system is compelling. Do not let it slip and do not soften its gate.

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
| M3-HNT-02 🔴⚠️ | Bleed the 1861 overlay across the world | Full material + palette + ambient-audio swap; **collision provably unchanged**; 20–60 s; photosensitivity-safe variant in the same feature | 6 |
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

This is the most important gate in the project. Treat it as a real decision point, not a formality.

1. Cut a standalone **45-minute playtest build**.
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
| M4-STR-01 🔴 | Stream chunks in a 3×3 ring | Async load; no frame spike above 4 ms; memory flat across 100 transitions | 5 |
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
- [ ] Frame time in budget in the densest Stack chunk at night

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

---

## M6 — The Story

> **Weeks 37–42 · 42 sessions**
> **Goal:** Prologue and Act I are complete, voiced-in-style, accessible, and controller-verified.

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
| M7-SHP-06 🔴 | Verify stability | 2 hours crash-free; no leaks over a long session; all performance budgets met | 3 |
| M7-SHP-07 | Register for Next Fest | Slot booked, assets submitted | 1 |
| M7-SHP-08 | Complete the licence audit | `ASSET_LICENSES.md` covers every third-party asset in the build | 2 |

### 🚦 Gate M7

- [ ] Steam page live and taking wishlists
- [ ] Demo stable for 20 strangers
- [ ] Zero P0/P1 bugs open
- [ ] 2 hours crash-free confirmed
- [ ] Every asset's licence documented
- [ ] Sensitivity read completed on all Kalighat / Kali / Bengali content

---

## M8 — Early Access Launch

> **Weeks 47–48 · 14 sessions**

| ID | Feature | Done when | Est. |
|---|---|---|---|
| M8-LCH-01 🔴 | Ship the release build | Windows + Steam Deck; signed; depot uploaded | 3 |
| M8-LCH-02 🔴 | Publish the EA roadmap | Public, honest, dated; matches Plan.md §24.3 | 2 |
| M8-LCH-03 | Open the community channels | Discord, bug reporting, feedback intake | 2 |
| M8-LCH-04 🔴 | Provide crash and telemetry reporting | Opt-in, privacy-respecting, actually actionable | 3 |
| M8-LCH-05 | Prepare the hotfix pipeline | You can ship a fix within 24 h of a launch-blocking report | 2 |
| M8-LCH-06 | Write the launch retrospective | In `Agent_History.md`: what the estimates got wrong and by how much | 2 |

---

## 6. Cadence

### Weekly rhythm

| Day | Focus |
|---|---|
| Mon–Fri | One feature per session block. Core → engine → demo → commit. |
| Sat | Longer session: the milestone's hardest feature, when you have runway |
| Sun | 30 min: update the board, review the week's demos, plan next week's features. **No code.** |

### Monthly

- Re-estimate the current milestone against actual velocity. If you're more than 20% over, cut from §8 *now* rather than at month nine.
- Post a devlog clip. Building an audience from month two costs almost nothing and is worth more than any single feature.

### Velocity tracking

After M1, compute actual sessions per feature and rescale the whole plan. **Your first three milestones will overrun.** That is normal and it is information, not failure. What matters is that you find out in month three.

---

## 7. Tracking Board

Update at the end of every session.

| Milestone | Features | Done | Sessions est. | Sessions actual | Status |
|---|---|---|---|---|---|
| M0 Literacy | 11 | 0 | 21 | 0 | ⬜ Not started |
| M1 Feel | 21 | 0 | 35 | 0 | ⬜ |
| M2 Predation | 14 | 0 | 42 | 0 | ⬜ |
| M3 The Hook ⚠️ | 20 | 0 | 56 | 0 | ⬜ |
| M4 The City | 21 | 0 | 49 | 0 | ⬜ |
| M5 Power & Pressure | 21 | 0 | 49 | 0 | ⬜ |
| M6 The Story | 17 | 0 | 42 | 0 | ⬜ |
| M7 Shipping | 8 | 0 | 28 | 0 | ⬜ |
| M8 Launch | 6 | 0 | 14 | 0 | ⬜ |
| **Total** | **139** | **0** | **336** | **0** | |

**Active feature:** _none_
**Blocked on:** _nothing_
**Last demo recorded:** _none_

---

## 8. Cut Triggers

Pre-agreed so the decision is unemotional when you're tired and behind. Cut **in this order**, and only when you're >20% over budget on a milestone:

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
| Godot 3D perf on open world | Medium | Severe | Chunk streaming + directors from M4; profile from M2, not M10 |
| Pixel-3D illegibility | Medium | Severe | Golden-image tests; readability over aesthetic purity (Pillar 6) |
| AI-generated code rot | Medium | Severe | Core/presentation split; tests as the contract; independent reviewer sessions |
| gdUnit4 / Godot version drift | Medium | Moderate | Pin Godot to a gdUnit4Net-supported release; core tests are pure xUnit and immune |
| LFS cost surprise | Low | Moderate | Azure DevOps decided in M0-ENV-02 |
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
> "Implement `<ID>`, the `<effect>` stage of the post chain in `docs/Plan.md` §18.2. Godot Shading Language. It must compose with the existing stages in the documented order. Add a debug toggle for this stage alone. Include the photosensitivity-safe variant in this same feature. Keep the whole chain under 2 ms."

**Bulk content**
> "Author `<N>` `EchoDefinition` `.tres` resources for the `<archetype>` archetype following `docs/Plan.md` §3.2. Each needs id, name template, valence, dissonance weight, tags from the canonical tag list, anchor site, unfinished act, and a 2–3 sentence vignette. Tags must be consistent with the archetype's real life. Do not add new tags to the canonical list without asking."

**Review**
> "Review the diff on branch `<branch>` against Feature Card `<ID>`. Report: (1) anything implemented outside the IN scope, (2) any stub, TODO, or fixed-value return, (3) any test that asserts the implementation rather than the requirement, (4) any core file that now references Godot, (5) any hardcoded balance number that belongs in a `.tres`. Do not fix anything — report only."
````

---

Two things before you start:

**Move Plan.md and Milestones.md into the Windows repo at `docs/`.** They're your source of truth and they should be versioned alongside the code they describe, not sitting in a Downloads folder on a different machine.

**Your first three features are M0-ENV-01 through M0-ENV-03** — which is exactly Part 1 above. Work down that verification checklist, and don't touch M0-LRN until every row is green.

Does this plan look good, or would you like me to adjust anything?