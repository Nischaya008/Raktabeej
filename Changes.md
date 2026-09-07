# Changes

One line per landed change, newest section on top.
Format: `<feature-id> — <what changed>`

Rationale lives in `Agent_History.md`. This file answers "what changed", not "why".

---

## Unreleased — M0 Literacy & Foundations

### Foundation audit and hardening (session 004) — closes M0-ENV-03/04/05

- **Moved `Plan.md` and `Milestones.md` into `docs/`** — every steering file and both
  documents already referenced `docs/Plan.md`, but the move had never happened. `git mv`,
  so history is preserved
- **D-16:** C# scripts under `game/` are now `PascalCase.cs` with an exactly-matching class
  name; assets, scenes, and resources stay `lowercase_snake_case`. Godot resolves scripts by
  case-sensitive filename match, so the old blanket-lowercase rule was unkeepable
- Renamed `game/src/main.cs` → `game/src/Main.cs` (`class main` → `class Main`), updated the
  `.uid` and the `ExtResource` path in `game/scenes/main.tscn`
- **Replaced the empty placeholder test** with `ArchitectureTests` — a reflection guard
  asserting `RaktabeejCore` never references Godot (decision D-06). Verified it goes red when
  the rule is violated, so CI now enforces D-06 on every push
- Deleted the `RaktabeejCore/Class1.cs` scaffold; core stays empty until M2-BLD-01
- `RaktabeejCore.csproj`: added `TreatWarningsAsErrors` (DoD condition #1)
- `.gitignore`: removed `[Dd]ebug/` and `[Rr]elease/` — redundant beside `bin/`/`obj/`, and
  they would have silently ignored `game/src/debug/`
- `ci.yml`: lowercase guard now uses `git ls-files` instead of `find`, so it matches what is
  actually committed. Exempts only the *filename* of `.cs` / `.cs.uid`, and gained a second
  pass for uppercase **directory** segments — `git ls-files` lists no directory entries, so
  the end-anchored filename regex alone silently let `game/Scenes/` through
- `ci.yml`: push trigger broadened from `[main, 'feat/**']` to `['**']` — a `chore/` or `fix/`
  branch matched nothing, so DoD #8 ("CI green") was only reachable after landing on `main`
- `.gitignore`: added `build/` (local code-review report output)
- `docs/TOOLCHAIN.md` is now the single home for versions; the duplicate tables in
  `Agent_History.md` and the machine/risk restatements were reduced to pointers
- `docs/Plan.md` Task 1: dropped the stale "one placeholder xUnit test passes" instruction
- `.kiro/steering/structure.md`: labelled explicitly as the **target** layout
- Corrected the rendering-driver documentation in three places. `project.godot` pins
  `rendering_device/driver.windows="d3d12"` and Godot writes no driver key itself, so
  `Milestones.md` A.3's "leave default" was false and would have produced a Vulkan Windows
  build diverging from the committed project. **The D3D12-vs-Vulkan choice itself is now logged
  as an open decision for M1-RND-07** — no rendering behaviour was changed
- `docs/Milestones.md` M1-TST-02: now also requires automating the `_Ready()` liveness probe in
  CI, so a broken C#↔Godot binding fails a build instead of waiting on a manual checklist row
- Added `docs/TOOLCHAIN.md`; added `.gitkeep` to `tools/` and `docs/demos/`
- Updated `.kiro/steering/tech.md` and `structure.md` for D-16 and the `docs/` layout

- Corrected the repository-visibility claim in three documents: it is **public**, not private.
  The visibility decision itself is logged as open — nothing was changed on GitHub

### M0-ENV-01 — Provision the macOS development toolchain — **IN PROGRESS**

- Installed Xcode Command Line Tools, Homebrew (`/opt/homebrew`, added to `~/.zprofile`)
- Installed Godot **4.7.2.stable.mono.official** to `/Applications`; exported `GODOT4` and
  a `godot` alias in `~/.zshrc`
- Installed **.NET SDK 8.0.x (arm64)** side-by-side with a pre-existing 10.0.400
- Installed VS Code with C# Dev Kit, godot-tools, EditorConfig
- Installed `git`, `git-lfs`, `gh`; authenticated to GitHub as `Nischaya008`
- Configured git: LF line endings, `main` default branch, LFS initialised
- **Verified:** Godot runs on the native **Metal** driver, Forward+ renderer, M4 Pro GPU
- **Verified:** `dotnet build` clean — 0 warnings, 0 errors
- **Verified:** `RaktabeejCore.dll` builds and is consumed by `Raktabeej.dll`
- **Verified:** C# `_Ready()` executes in-engine (`hello` printed)
- **Verified (session 004):** `dotnet test` passes; .NET SDK is 8.0.424 arm64
- **Verified (session 004):** first push landed (`42f1e09`) and both CI jobs are green
- ⛔ **Outstanding:** checklist row #9 — F5 debugger attach. 13 of 14 rows green

### M0-ENV-02 — Establish the repository and asset discipline — **IN PROGRESS**

- Cloned `github.com/Nischaya008/Raktabeej` to `~/dev/raktabeej` (outside iCloud sync)
- Added `.gitattributes` — LF normalisation, `.tres`/`.tscn` as diffable text, LFS
  patterns for binary asset types
- Added `.gitignore` — Godot caches, .NET build output, macOS junk, and an explicit block
  excluding **source assets** (`.blend`, `.vox`, `.aseprite`, `.kra`, `raw_audio/`)
- Created `~/dev/raktabeej-assets/` outside the repo for source assets

### M0-ENV-03 — Scaffold the solution and Godot project — **IN PROGRESS**

- Added `global.json` pinning .NET SDK 8.0.0 with `rollForward: latestFeature`
- Created `Raktabeej.sln` with `RaktabeejCore` (classlib, net8.0) and
  `RaktabeejCore.Tests` (xUnit, net8.0); test project references core
- Created the Godot project in `game/` — Forward+ renderer, Jolt 3D physics
- Attached the first C# script, generating `game/Raktabeej.csproj`
- Added a `ProjectReference` from the Godot project to `RaktabeejCore`
- Set **Main Scene** in Project Settings
- Added `.vscode/launch.json` (hardcoded Godot binary path) and `.vscode/tasks.json`

### M0-ENV-04 — Establish continuous integration — **NOT VERIFIED**

- Added `.github/workflows/ci.yml` with two jobs: core build + xUnit tests, and a
  **lowercase-path guard** that fails on any uppercase path under `game/` (both dev
  machines are case-insensitive and will not warn; the Linux/Steam Deck export will fail)
- Godot export job deliberately deferred to M1-TST-02

### M0-ENV-05 — Establish the AI harness — **IN PROGRESS**

- Added `.kiro/steering/product.md` — design pillars and out-of-scope list
- Added `.kiro/steering/tech.md` — stack, architecture rules, agent contract
- Added `.kiro/steering/structure.md` — repository layout
- Moved `Plan.md` and `Milestones.md` into `docs/`
- Created `Agent_History.md`, `Changes.md`, `docs/ASSET_LICENSES.md`

---

## Pending — not yet applied

- [x] `.vscode/settings.json` — was already committed in `42f1e09`
- [x] `docs/TOOLCHAIN.md` with pinned versions and the no-upgrade-mid-milestone policy
- [x] `docs/Plan.md` — Godot version 4.5 → 4.7.2 (6 occurrences)
- [x] `docs/Milestones.md` Appendix A.1 — explicit .NET 8 download URL
- [x] `docs/Milestones.md` Appendix A.3 — add the Main Scene setting row
- [x] `docs/Milestones.md` Appendix A.4 — add the F5 troubleshooting note to row 9
- [x] `docs/Milestones.md` §9 — gdUnit4 version-drift risk to Likelihood High
- [ ] `docs/Plan.md` §22.1 — reconsider macOS for v1.0 *(scope decision, still open)*
- [ ] M0-LRN — decide whether the three throwaway learning games are still the right spend
      under an AI-orchestrated workflow *(scope decision, still open)*

---

## Documents

| Date | Document | Version | Summary |
|---|---|---|---|
| [DATE] | `docs/Plan.md` | v1.0 | Full GDD/PRD — 30 sections. Design pillars, the Sanguine Ledger, three-act story with four endings, camera spec, all systems, technical architecture, engine decision record, $100 cost budget, 12-month roadmap |
| [DATE] | `docs/Milestones.md` | v1.1 | FDD delivery plan — 141 features across M0–M8, ~340 sessions, gates, Definition of Done (10 conditions), cut triggers, risk register, prompt seeds, self-contained Appendix A setup |
| [DATE] | `docs/TOOLCHAIN.md` | v1.0 | Pinned versions, no-upgrade-mid-milestone policy, machine roles, gdUnit4 drift risk, and the D-16 naming convention |
