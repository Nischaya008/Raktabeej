# Changes

One line per landed change, newest section on top.
Format: `<feature-id> — <what changed>`

Rationale lives in `Agent_History.md`. This file answers "what changed", not "why".

---

## Unreleased — M0 Literacy & Foundations

### M0-DBG-01 carded; decisions D-21 to D-23 (session 006)

- Added `docs/cards/m0-dbg-01.md` — the approved Feature Card for the debug menu, and the first
  card written under §3. `docs/cards/` is a new home, now listed in `structure.md`
- **D-21 — the time-of-day scrubber moves from M0-DBG-01 to M4-SUN.** There is no `GameClock` at
  M0, and `Plan.md` Task 3's own demo line called it a *"(placeholder) clock"*, which DoD #5
  forbids. `Plan.md` §25 Task 3 and the `Milestones.md` M0-DBG-01 row amended under §0.5. The
  same amendment records that the "free-camera toggle" is a flyable debug camera at M0 —
  `main.tscn` has no gameplay camera to toggle away from until M1-CAM
- **D-22 — demo clips live outside the repo** at `~/dev/raktabeej-assets/demos/<feature-id>.mp4`.
  `*.mp4` was never in the LFS patterns, so the first clip would have landed as a plain git blob.
  DoD #7 amended: the clip requirement is unchanged, only its location, plus an index row
- Added `docs/demos/README.md` as that index; deleted the redundant `docs/demos/.gitkeep`, whose
  text ("Feature demo clips land here") D-22 makes false
- `.gitignore`: `/docs/demos/*` with a `!README.md` negation. Scoped to that directory rather
  than a global `*.mp4`, so a future in-game video asset cannot be silently swallowed — the same
  mistake `[Dd]ebug/` made against `game/src/debug/` in session 004. Verified by writing a dummy
  clip and running `git add -A --dry-run`, because `git check-ignore -v` exits 0 on a negation
  and wrongly suggested the README was ignored
- **D-23 — M0-ENV-06 is resequenced ahead of M0-DBG-01.** The two were mutually blocking:
  ENV-06 required a frame-time overlay that only DBG-01 builds, while DBG-01's DoD #10,
  release-exclusion check, and Gate M0's exported-build row all required ENV-06's export
  templates. ENV-06 now lands everything provable against `Main.cs` alone, and its final row is
  co-verified with DBG-01's Windows check. Neither feature split or renumbered
- `.kiro/steering/structure.md`: added `docs/cards/`, `docs/LEARNINGS.md`, `docs/demos/README.md`
  as index-only, and the demo-clip rule under Source Assets
- `docs/Milestones.md`: tracking-board footer now shows M0-ENV-06 active and the revised order

### M0-ENV-01 closed at 14/14 (session 006)

- **Verified by the developer:** checklist row #9 — F5 debugger attach, breakpoint hit in
  `game/src/Main.cs` `_Ready()`. It was the last open row, so **M0-ENV-01 is complete at 14
  of 14** and the 🔴 gate feature is cleared
- **M0-ENV-01 through M0-ENV-05 are all Done. M0 stands at 5 of 10 features**
- `docs/Milestones.md`: tracking board M0 Done 4 → 5, Gate M0's Appendix A row ticked, and the
  status footer rewritten — it still claimed "13 of 14" and "Blocked on: checklist row #9",
  both false once row #9 passed
- Deleted the two squash-merged branches, local and remote: `chore/m0-foundation-hardening`
  (content landed identically as `3f92697` — verified by an empty `git diff fffc692 3f92697`)
  and `docs/m0-decisions-d17-d20` (was at `a099651`, i.e. `main` itself)

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

### M0-ENV-01 — Provision the macOS development toolchain — **DONE**

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
- **Verified (session 006, by the developer):** checklist row #9 — F5 debugger attach hits a
  breakpoint in `Main.cs` `_Ready()`. **14 of 14 rows green; feature complete**

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

### Decisions D-17 to D-20 (session 005)

- **D-17 — repo stays public**, deliberately. Docs corrected; spoiler exposure accepted as a cost
- **D-18 — dropped the three throwaway learning games.** M0-LRN is now a living
  `docs/LEARNINGS.md` plus a guided editor-literacy pass on the real project. M0: 12 → 10
  features, 23 → 15 sessions. Project total: 141 → 139 features, 340 → 332 sessions
- **D-19 — Windows renders through D3D12, explicitly pinned.** Reverses the session-004
  assumption that Vulkan was safer: D3D12 became Godot's Windows default in 4.6 because Windows
  Vulkan drivers are poorly maintained. `M1-RND-07` must now profile both backends
- **D-20 — macOS ships in v1.0** alongside Windows (primary). `M8-LCH-07` promoted from optional
  to required; Metal becomes a shipping backend; notarization is now a pre-M7 budget decision
- Corrected a session-004 error: `Plan.md` never mentioned the rendering driver, so the claimed
  constitution conflict did not exist

---

## Pending — not yet applied

- [x] `.vscode/settings.json` — was already committed in `42f1e09`
- [x] `docs/TOOLCHAIN.md` with pinned versions and the no-upgrade-mid-milestone policy
- [x] `docs/Plan.md` — Godot version 4.5 → 4.7.2 (6 occurrences)
- [x] `docs/Milestones.md` Appendix A.1 — explicit .NET 8 download URL
- [x] `docs/Milestones.md` Appendix A.3 — add the Main Scene setting row
- [x] `docs/Milestones.md` Appendix A.4 — add the F5 troubleshooting note to row 9
- [x] `docs/Milestones.md` §9 — gdUnit4 version-drift risk to Likelihood High
- [x] `docs/Plan.md` §22.1 — macOS pulled into v1.0 (**D-20**)
- [x] M0-LRN — throwaway learning games dropped (**D-18**)
- [ ] Steam Deck: still a v1.0 launch platform, or moved out? Decide before M6-ACC-04
- [ ] macOS signing: notarize at $99/yr, or ship unsigned with Gatekeeper instructions? Before M7
- [ ] `actions/checkout@v4` / `actions/setup-dotnet@v4` target the deprecated Node 20 — own change
- [ ] `.kiro/steering/product.md` still says "Windows + Steam Deck", which **D-20** superseded.
      Deliberately not half-corrected — bundle it with the Steam Deck decision above

---

## Documents

| Date | Document | Version | Summary |
|---|---|---|---|
| [DATE] | `docs/Plan.md` | v1.0 | Full GDD/PRD — 30 sections. Design pillars, the Sanguine Ledger, three-act story with four endings, camera spec, all systems, technical architecture, engine decision record, $100 cost budget, 12-month roadmap |
| [DATE] | `docs/Milestones.md` | v1.1 | FDD delivery plan — 141 features across M0–M8, ~340 sessions, gates, Definition of Done (10 conditions), cut triggers, risk register, prompt seeds, self-contained Appendix A setup |
| [DATE] | `docs/TOOLCHAIN.md` | v1.0 | Pinned versions, no-upgrade-mid-milestone policy, machine roles, gdUnit4 drift risk, and the D-16 naming convention |
