# Agent History — RAKTABEEJ

Running log of decisions, rationale, and learnings. **Newest entry on top.**
Never rewrite a prior entry — only add above it. Corrections go in a new entry
that references the old one.

The **Standing Reference** section below is the exception: it is an index, and it
is updated in place so there is always one place to look for the current answer.

---

## Standing Reference (updated in place)

### Decision index

| # | Decision | Rationale | Session |
|---|---|---|---|
| D-01 | **Godot 4.x (.NET) + C#** as engine and language | $0 forever, MIT, no royalties; C# plays to existing skill; Unity costs money above thresholds; Unreal too heavy for pixel art and slowest learning curve; custom C++ engine would consume the entire year before a game existed | 001 |
| D-02 | **3D low-poly geometry + pixelation post-process**, not 2D sprites | Forced by the camera spec: a freely-rotating 360° camera cannot be served by hand-drawn sprites. Same approach as *V Rising* | 001 |
| D-03 | **Windows + Steam Deck for v1.0.** Web ruled out permanently; consoles out of scope; Android/iOS post-1.0 | C# in Godot 4 cannot target web at all. Godot has no official console support — would need a paid third-party porting partner. Mobile needs a control redesign, not a port | 001 |
| D-04 | **v1.0 scope = Prologue + Act I, two districts** (The Interment, The Stack) | The full feature list is a decade of work. This is the smallest complete, shippable game that proves the hook | 001 |
| D-05 | **Setting: 2070 Kalighat Sprawl (Kolkata)**, with the Raktabeej myth as the vampire spine | Kolkata was the capital of British India until 1911, so an 1861 interment is historically coherent rather than hand-waved. The 1861↔2070 overlay becomes thematically loaded. Distinctive versus the Neo-Tokyo-saturated genre | 001 |
| D-06 | **`RaktabeejCore` is a pure .NET library with zero Godot references** | The single most important technical decision. Makes game rules unit-testable in seconds, plays to existing software skill, and — critically — makes AI-assisted development *verifiable* rather than eyeballed | 001 |
| D-07 | **Comprehension axis doubles as the scope-management tool** | The vampire cannot use modern technology until he learns it. So vehicles, firearms, terminals, and minigames can ship post-launch with perfect narrative justification. The EA roadmap is literally the character's education | 001 |
| D-08 | **All content as `[GlobalClass] Resource` `.tres` files**, no hardcoded balance | Free editor GUI, text-diffable in git, and bulk content authoring becomes a data task an agent can do safely without touching code | 001 |
| D-09 | **Feature-Driven Development**, estimated in ~3-hour sessions | FDD's demoable-feature unit is the only reliable motivation engine for solo work; sessions make slippage visible within a week instead of a quarter. FDD's team apparatus (chief programmers, feature teams) dropped as meaningless for one person | 001 |
| D-10 | ~~Develop on Windows, Mac is EOL Intel hardware~~ **SUPERSEDED by D-11** | — | 001 |
| D-11 | **Develop on macOS (M4 Pro). Windows box is the verification target.** | Reverses D-10. See Session 002 for the full reasoning — both premises behind D-10 were false | 002 |
| D-12 | **GitHub, not Azure DevOps**, with strict source-asset exclusion | Repo already existed. Free LFS quota is 1 GB storage / 1 GB monthly bandwidth, so `.blend`/`.vox`/raw `.wav` live outside the repo in `~/dev/raktabeej-assets/`. Only exported `.glb`/`.ogg`/`.png` are committed. Fallback if exceeded is a $5/mo data pack | 002 |
| D-13 | **.NET 8 LTS, pinned via `global.json`** | Godot 4.x targets `net8.0`; gdUnit4Net supports 8/9 not 10; LTS is supported past the EA date. .NET 10 remains installed side-by-side but unused in this repo | 003 |
| D-14 | **Pin Godot to 4.7.2.stable.mono. No upgrades mid-milestone.** | Minor Godot releases change rendering and physics behaviour. An accidental upgrade mid-project would silently shift the post-process chain | 003 |
| D-15 | **xUnit only for now. gdUnit4Net deferred to M1 and may be dropped.** | gdUnit4Net's published support matrix covers Godot 4.3–4.4.1; actual engine is 4.7.2. ~90% of test value lives in the engine-agnostic core anyway, which is unaffected | 001, 003 |
| D-16 | **C# scripts under `game/` are `PascalCase.cs` with an exactly-matching class name. Everything else stays `lowercase_snake_case`.** | Godot resolves a C# script by matching class name to file name, case-sensitively — verified empirically on 4.7.2. The old blanket-lowercase rule would have forced `class debug_service` across ~200 files. The lowercase guard's real value is protecting `res://` asset strings from breaking the Linux export, which is untouched by this | 004 |
| D-17 | **The repository stays public.** | Deliberate now, rather than the accident it was until session 004. Accepts that `Plan.md` publishes the complete story and all four endings pre-launch. Aligns with the §6 build-an-audience-from-month-two stance, and public repos get unlimited Actions minutes. **Consequence: the spoiler exposure is now a chosen cost, so stop treating `Plan.md` as internal** | 005 |
| D-18 | **Drop the three throwaway learning games. Acquire engine literacy on the real project, guided.** | The plan assumed hand-coding; this project is AI-orchestrated, so hand-building Pong teaches little that transfers. Saves ~8 sessions. **What is not dropped:** the M1 feel gate and the M3 playtest reading are subjective and explicitly undelegatable (§4.5), and §4.4 makes the developer the only reviewer of agent output. Replaced by a living `LEARNINGS.md` and a guided editor-literacy pass on the real project | 005 |
| D-19 | **Windows renders through D3D12, explicitly pinned. Not "leave default".** | Reverses the session-004 assumption that Vulkan was the safer default. The Godot Foundation sponsored work to make **D3D12 the default Windows driver from 4.6** because Windows Vulkan GPU drivers are poorly maintained and have repeatedly broken shipped Godot games. Pinning explicitly also survives the default changing again between engine versions, which it already did once | 005 |
| D-20 | **macOS ships in v1.0 alongside Windows. Windows is the primary target.** | Supersedes `Plan.md` §22.1, which had macOS at v1.1 and `M8-LCH-07` as optional. Development happens on Apple Silicon, so the macOS build is tested daily for free. Consequence: Metal is now a *shipping* backend rather than a dev convenience, which makes `M1-RND-07`'s dual-backend verification load-bearing, and notarization becomes a real budget question | 005 |

### Pinned toolchain

**Versions live in `docs/TOOLCHAIN.md`** — one authoritative copy, so they cannot
drift. Machine roles are in `docs/Milestones.md` § Environment. Repo:
`github.com/Nischaya008/Raktabeej` — **currently PUBLIC**, though three documents claimed
private until session 004. Unresolved; see the open questions below.

### Current position

- **Active milestone:** M0 — Literacy & Foundations (4 of **10** features done; was 12 before D-18)
- **Active feature:** M0-ENV-01 (Provision the macOS development toolchain) — 13 of 14 rows green
- **Blocked on:** Verification checklist row #9 — F5 debugger attach in VS Code. Manual step; only a human can press F5
- **Done:** M0-ENV-02, M0-ENV-03, M0-ENV-04, M0-ENV-05 — merged as `3f92697`, CI green
- **Not started:** M0-ENV-06 (Windows verification target), M0-LRN-04/05, M0-DBG-01/02
- **Next up:** M0-DBG-01 (debug menu) is the first real code feature and the first demoable one
- **Resolved in session 005:** repo visibility (D-17), M0-LRN (D-18), Windows driver (D-19),
  macOS in v1.0 (D-20).
- **Open question:** does **Steam Deck** stay a v1.0 launch platform? `Plan.md` §22.1 says yes,
  but with macOS promoted that makes three at launch. Decide before M6-ACC-04.
- **Open question:** **notarize the macOS build ($99/yr) or ship unsigned** with Gatekeeper
  instructions? Now a launch-blocking question rather than a deferrable one. Decide before M7.
- **Tracked follow-up:** CI warns that `actions/checkout@v4` and `actions/setup-dotnet@v4`
  target the deprecated Node 20 and are being forced onto Node 24. Not a failure. Deliberately
  not bundled into M0 foundation work — it wants its own small change.
- **Tracked follow-up:** `M1-RND-07` must **profile both D3D12 and Vulkan** on the Windows box,
  not merely confirm D3D12 renders. See D-19 for why.

---

## Session 005 — [DATE] — Four decisions settled; the Windows driver reversed on evidence

### Goal
Land the M0 foundation work, then resolve the three open questions session 004 surfaced.

### Landed first
`3f92697` squash-merged to `main`, CI green on both jobs — including the path guard whose
directory-detection regression this cycle introduced and fixed. The trigger fix is validated
by existence: a `chore/**` branch produced a CI run, which it could not have done before.

### D-19 — I was wrong about the Windows driver, and the reversal matters

Session 004 left this open and framed Vulkan as the safer choice, reasoning it was Godot's
default and therefore its better-exercised Windows path. **That reasoning was a version behind.**

The Godot Foundation sponsored work making **Direct3D 12 the default RenderingDevice driver on
Windows from 4.6**, because Windows Vulkan drivers are poorly maintained relative to their
Direct3D 12 counterparts, and driver regressions from Nvidia and AMD have outright broken Godot
and games shipped with it ([Godot 4.6 dev 5 release
notes](https://godotengine.org/article/dev-snapshot-godot-4-6-dev-5/), and [maintainer comment
on the crash thread](https://forum.godotengine.org/t/godot-crashing-to-the-point-where-its-unusable/129333/14)).

So the `rendering_device/driver.windows="d3d12"` sitting in `project.godot` since `42f1e09` was
correct all along. The stale guidance was `Milestones.md` Appendix A.3 saying *"leave default"* —
written when Vulkan was the Windows default. Amended.

**Correction to Session 004.** That entry claimed `Plan.md` §21.1 mandated leaving the driver to
Godot, and therefore that `project.godot` contradicted the constitution and Plan.md should win
under §0.5. Both claims were false: `Plan.md` does not mention the rendering driver anywhere —
verified by grep across the whole file. The only stale statements were in `Milestones.md` A.3 and
the `Agent_History` Standing Reference table. There was no constitution conflict to resolve.
Cited from memory of the document's structure rather than from the document. Grep first.

Two things worth holding onto:

- **The explicit pin is the point, not just the value.** The Windows default changed between 4.5
  and 4.6. Any instruction of the form "leave it default" is unstable across engine versions,
  which is exactly the class of silent drift D-14 exists to prevent.
- **D3D12 is not free.** It has reported slower startup than Vulkan
  ([#103844](https://github.com/godotengine/godot/issues/103844)), the original driver landed
  measurably behind Vulkan in some scenes
  ([PR #64304](https://github.com/godotengine/godot/pull/64304)), and post-4.6 perf differences
  are still being filed ([#115431](https://github.com/godotengine/godot/issues/115431)). So
  **M1-RND-07 must measure both backends on the Windows box, not just confirm D3D12 renders.**
  Vulkan stays a one-line fallback if the post-process chain profiles badly.

Also note the *stability* argument cuts our way harder than most: this game's look is a
`SubViewport` plus a five-stage post-process chain, which is precisely where driver bugs surface.

### D-20 — macOS joins v1.0

Windows stays primary; macOS ships with it. This supersedes `Plan.md` §22.1 and promotes
`M8-LCH-07` from optional to required. It is close to free — development happens on Apple
Silicon, so that build is exercised daily — but it is not *actually* free, and the honest costs
are: notarization is $99/yr, or ship unsigned and talk users through Gatekeeper; and Metal stops
being a development convenience and becomes a shipping backend, so `M1-RND-07`'s two-backend
verification is now protecting real players on both sides rather than de-risking one.

**Left unresolved deliberately:** Steam Deck was in v1.0 per `Plan.md` §22.1. Three launch
platforms is a lot for a first game. Not assumed either way — see open questions.

### D-18 — the learning games are dropped, and what that actually costs

The plan budgeted ~11 sessions for three throwaway games and argued hard against skipping them.
That argument was written for a developer hand-writing the code. It is not this project.

Accepted, with ~8 sessions saved. But the plan's reasoning was not *entirely* about typing
practice, and the part that survives is worth stating plainly rather than quietly dropping:

- `Milestones.md` §4.5 lists **feel** as undelegatable, and the **M1 gate** is literally "play it
  for 10 minutes with no content — is moving around genuinely pleasurable?" No agent can answer
  that.
- The **M3 gate** is reading five strangers' reactions and deciding whether the hook works. That
  gate can end the project.
- §4.4 makes the developer the only reviewer of agent output. This very cycle is the argument
  for it: the agent introduced a CI regression, and it was caught by review, not by the agent.

None of that requires having built Pong. It requires being fluent in the editor and able to read
a diff. So M0-LRN-01/02/03 are replaced by `M0-LRN-05`, a guided editor-literacy pass performed
on the real project, and `M0-LRN-04` becomes a living log appended each session rather than a
one-off write-up.

### D-17 — public, deliberately

Now a decision rather than an accident. The cost is real and should not be restated as a
non-issue: `Plan.md` publishes the whole three-act story, all four endings, the secret ending's
exact unlock conditions, and the Pixel reveal, before anyone plays it. Accepted, and it fits the
§6 devlog-from-month-two stance. The follow-on obligation is to stop writing `Plan.md` as if it
were an internal document.

### Plan amendments made under §0.5

`Milestones.md` §0.5 says Plan.md wins unless deliberately amended. These are the deliberate
amendments: §22.1 platform matrix and recommendation, the v1.0 platform line, and §23.2
notarization (all D-20); §23.1 VCS row (D-17); §18.2 driver pointer (D-19). In `Milestones.md`:
the M0-LRN feature set, Gate M0, and the M0 session budget (D-18); Appendix A.3's driver row,
M1-RND-07, and the divergence risk row (D-19); M8-LCH-01 and M8-LCH-07 (D-20).

---

## Session 004 — [DATE] — Foundation audit: three doc claims were false, one ground rule was impossible

### Goal
Read the plan, establish where the project actually stands, and continue. This turned
into an audit, because the documents and the repository disagreed.

### Session 003's outstanding list was wrong in both directions

Verified against the machine rather than the notes:

| Session 003 claimed | Reality |
|---|---|
| ⛔ `dotnet test` unconfirmed | **Passes.** But it was one test asserting nothing — see below |
| ⛔ First push and CI unconfirmed | **Both green.** `42f1e09` pushed; both CI jobs passed ~2 days ago |
| Pending: `.vscode/settings.json` | **Already existed and was committed** |
| "Moved `Plan.md` and `Milestones.md` into `docs/`" | **Never happened.** Both were still at the repo root |

That last one mattered. Every steering file, and both documents, point at `docs/Plan.md`.
The agent contract's first instruction — *read `docs/Plan.md`* — referenced a file that did
not exist, and would have silently failed on every future session. Now actually moved, with
`git mv` so history survives.

**Learning:** `Changes.md` recorded an intent as an accomplished fact. Verify against the
filesystem before writing a line in it, not after.

### D-16 — the lowercase rule was impossible to keep

The ground rule said *all* paths under `game/` are `lowercase_snake_case`, CI-enforced. But
`tech.md` also said C# types are PascalCase. Those cannot both hold, because Godot resolves a
C# script by matching the class name to the file name **exactly and case-sensitively**.

Proven, not assumed — renamed `class main` to `class Main` in `main.cs`, built clean, and the
engine refused it:

```
ERROR: Cannot instantiate C# script because the associated class could not be found.
Script: 'res://src/main.cs'. Make sure the script exists and contains a class definition
with a name that matches the filename of the script exactly (it's case-sensitive).
```

So the blanket rule would have produced `class debug_service`, `class camera_rig`, and
`class chunk_streamer` across roughly 200 files, permanently at odds with `RaktabeejCore`.

Resolved by splitting the rule along the line where it actually earns its keep. The guard
exists to stop the Linux/Steam Deck export breaking on case-mismatched `res://` strings —
and those strings address *assets*, which stay lowercase. Compiled scripts are bound by the
editor via `ExtResource`, so they carry no such risk. C# files therefore go PascalCase, which
is also [Godot's own C# style guide](https://docs.godotengine.org/en/4.4/tutorials/scripting/c_sharp/c_sharp_style_guide.html).

Caught with exactly one `.cs` file in the tree. Cost about ten minutes. At M4 it would have
been a 200-file refactor touching every `.tscn`.

### The one passing test was asserting nothing

`UnitTest1.Test1()` had an empty body. It could never fail, so row #7 of the checklist and
DoD condition #2 were both being satisfied by a test with no content — precisely the failure
mode §4.4 exists to catch, in its purest form.

Replaced with a guard on **D-06**, the decision the whole testing strategy rests on:
`RaktabeejCore` must never reference Godot. It reads the assembly reference table by
reflection, so it trips the moment a Godot type is used in core.

**Falsified before trusting it.** Temporarily added a `GodotSharp` reference plus a
`Godot.Vector3` usage to core and confirmed red with an actionable message:

```
Failed RaktabeejCore.Tests.ArchitectureTests.CoreAssemblyDoesNotReferenceGodot
  RaktabeejCore must not reference Godot. Found: GodotSharp
Failed! - Failed: 1, Passed: 0
```

Then reverted and confirmed green. CI now enforces the project's most important
architectural decision on every push, instead of relying on vigilance.

Known limit, noted in the test: Roslyn drops assembly references nothing consumes, so a
Godot reference sitting unused in the csproj would not surface. That is the harmless case.

### Two latent traps closed

**`.gitignore` would have swallowed the debug menu.** It carried the legacy Visual Studio
patterns `[Dd]ebug/` and `[Rr]elease/`. `bin/` and `obj/` already cover .NET output, so those
two added nothing — but they would have silently ignored `game/src/debug/`, which is exactly
where M0-DBG-01 was about to land. Removed, with a comment saying why so nobody adds them back.

**The CI guard used `find`, so it didn't match the repository.** `find` ignores `.gitignore`,
so it flagged `game/.DS_Store` locally while CI never saw the file. Switched to `git ls-files`:
now it checks precisely what is committed, and the same one-liner reproduces the CI result
locally.

### Also landed

- `RaktabeejCore.csproj` gained `TreatWarningsAsErrors` — DoD condition #1 required zero
  warnings on core and nothing was enforcing it
- `docs/TOOLCHAIN.md` created: pinned versions, the no-upgrade-mid-milestone policy, the
  gdUnit4 risk, and D-16
- Deleted the `Class1.cs` scaffold. Core is intentionally empty until M2-BLD-01 gives it real
  types
- `tools/` and `docs/demos/` were in `structure.md` but absent from git, which cannot track
  empty directories — `.gitkeep` added to both
- Cleared the Session 002 pending-edit list: Godot 4.5 → 4.7.2 across `Plan.md` (6 places),
  Appendix A.3 gained the **Main Scene** row, A.4 row 9 gained the F5 troubleshooting note,
  and the gdUnit4 drift risk went to Likelihood **High**

### Evidence

```
Build succeeded.  0 Warning(s)  0 Error(s)
Passed!  - Failed: 0, Passed: 1
Godot Engine v4.7.2.stable.mono.official — hello
CI lowercase guard: clean
```

### External review round — one real regression, caught

Two independent reviewer sessions. The valuable finding: **my CI guard rewrite was a
regression, not the improvement I thought it was.**

Switching `find` to `git ls-files` was meant to make the guard match what is actually
committed. It did — but `find … -print` emits *directory entries as their own lines*, and
`git ls-files` emits only files. The regex `/[^/]*[A-Z][^/]*$` is anchored to the end of the
path, so it only ever inspects the final segment. Combined, uppercase **directory** names
stopped being caught entirely.

Reproduced on a scratch tree. Old guard flagged 5 paths, mine flagged 1:

| Path | old `find` | my `git ls-files` | fixed |
|---|---|---|---|
| `game/Scenes/main.tscn` | flagged | **passed** | flagged |
| `game/audio/SFX/hit.wav` | flagged | **passed** | flagged |
| `game/src/Autoload/GameClock.cs` | flagged | **passed** | flagged |
| `game/art/textures/Wall.png` | flagged | flagged | flagged |

The lesson generalises: this gap hid because the regex was unchanged, so it *looked* untouched.
The input to it had changed, and that was the whole defect. When two explanations are on offer,
verify rather than take the reassuring one.

Fixed with a second pass, `grep -E '[A-Z][^/]*/'`, which matches an uppercase character in
any directory segment. Two passes with one clear intent each, rather than a shell loop.

**`game/src/Main.cs.uid` had been deleted from disk.** Both reviewers flagged it; I had
recorded the tree as clean. It was — at commit time. The file went missing afterwards, almost
certainly Godot cleaning up what a case-insensitive filesystem let it see as the stale
`main.cs.uid`. HEAD still held the correct `uid://bcpm70vaoxx2f`, so restoring from git was
enough. Had it been committed as a deletion, Godot would have generated a *fresh* uid and
`main.tscn` would have failed to resolve its script. A genuine macOS case-rename trap: after
any case-only rename here, re-check `git status` once the engine has run again.

**CI would not have run on this branch at all** — a defect neither reviewer caught, found
while verifying that the guard fix would actually be exercised. The push trigger was
`[main, 'feat/**']`, and the branch is `chore/m0-foundation-hardening`. Nothing matched. Since
the workflow otherwise only fires on a push to `main` or a pull request, and this workflow
merges locally rather than through PRs, DoD condition #8 — *CI green* — was only reachable
**after** landing on `main`, which is precisely too late. Broadened to `branches: ['**']`.

Worth naming as a pattern: both reviewers audited the guard's *logic* thoroughly and neither
asked whether the workflow containing it would ever execute. Reviewers check the code in front
of them; whether it runs at all is still mine to verify.

Also fixed: the reviewers' own output directory `build/` was untracked and one `git add -A`
away from being committed — now in `.gitignore`. Documentation duplication cut so that
versions have exactly one home (`docs/TOOLCHAIN.md`), which is the drift that put Godot 4.5
in `Plan.md` for two sessions. Removed a stale instruction in `Plan.md` Task 1 telling a
future reader that "one placeholder xUnit test passes" is acceptable — the thing this session
had just removed. Labelled `structure.md` as the *target* layout, since it lists ~30 files
that do not exist yet and is always-on steering.

### Round two — the anti-drift file was the only one telling the truth

Round two passed the guard fix cleanly: 19 adversarial paths, correct under
`bash -eo pipefail` (GitHub's actual run-step shell, where `pipefail` would otherwise turn a
no-match `grep` into a failed step — the `|| true` binds to the whole pipeline, so it does
not), and correct for filenames containing spaces.

The interesting finding was a **rendering-driver disagreement**, reported as
`docs/TOOLCHAIN.md` being the outlier that should be changed to "Vulkan/D3D12" to match the
other documents. Checked before acting, and the direction was backwards:

- `game/project.godot` contains `rendering_device/driver.windows="d3d12"`, and has since
  `42f1e09`. That is the *actual* configuration.
- Godot writes no driver key on its own — verified by opening a bare 4.7.2 project in the
  editor and diffing `project.godot`. So D3D12 is a deliberate override.
- `TOOLCHAIN.md` said D3D12 and was therefore **correct**. `Milestones.md` A.3 said the
  setting should be *"leave default"*, which is false and actively harmful: anyone rebuilding
  the environment from Appendix A would leave it unset, get Vulkan on the Windows target, and
  diverge from the committed project on the one axis the risk register calls out.

Fixed the three documents to state the real configuration. Had the suggestion been applied as
written, the file created to stop drift would have been edited into disagreeing with the
engine.

**This leaves a real unrecorded decision**, logged below rather than settled here.

### Kept against review, with reasons

- **`GD.Print` in `Main.cs`** — flagged as a leftover placeholder. It is the M0 liveness
  probe: the only signal that the C# assembly loaded and `_Ready()` ran, and the breakpoint
  target for checklist row #9. `--quit` exits 0 whether or not C# works. Kept, renamed from
  `hello` to `boot: C# assembly loaded` so it reads as a probe, and checklist row #8 now
  asserts that string instead of only the exit code.
- **`name is not null` in the architecture test** — not defensive padding. `AssemblyName.Name`
  is genuinely `string?`. Removing the guard emits `warning CS8602: Dereference of a possibly
  null reference`. Note the build still *succeeds*: `TreatWarningsAsErrors` is on
  `RaktabeejCore` only, per the ground rule, so the test project can accumulate warnings
  silently. Worth considering whether the verification harness should be held to the same bar
  as the code it verifies.

### Declined

- **A CI grep of `RaktabeejCore.csproj`** to also catch a Godot reference that is *declared
  but unused*. Declined: the reflection guard fires exactly when harm occurs, the moment a
  Godot type is actually used. An unused reference is inert. A second mechanism for a case
  that cannot hurt is machinery, and the limitation is already stated in the test.
- **`actions/checkout@v4` pinning** — pre-existing, not in this change. Self-rejected by the
  reviewer that raised it.
- **Changing the Windows rendering driver to Vulkan** to match the prose. Rejected: the prose
  was wrong, not the configuration. See the driver finding below.

### Deliberately not decided

- **M0-LRN.** The plan mandates three throwaway learning games and says not to skip them,
  because the artifact is competence rather than code. An agent cannot acquire that on the
  developer's behalf, so delegating them defeats their purpose entirely. Whether to spend
  ~11 sessions on them is a judgement call for the developer, not the agent. Surfaced, not
  resolved.
- **The repository is public, and three documents said it was private.** Found in session 004
  by checking `gh repo view` rather than trusting the docs — the same failure mode as the
  `docs/` move that was recorded as done but never happened. The exposure is not the source
  code, which hardly matters, but `Plan.md`: the complete three-act story, all four endings,
  the exact unlock conditions for the secret ending, and the reveal that Pixel's father is the
  prologue victim are all world-readable and search-indexable before the game ships. The
  business plan and the M3 kill-criteria are in there too.
  Working in the open is a legitimate strategy, and `Milestones.md` §6 already encourages
  devlogging from month two — but *devlogging in public* and *publishing the spoiler bible* are
  separate choices, and only one of them was made deliberately. Three options: make the repo
  private (the docs already assumed this); stay public and move the narrative sections of
  `Plan.md` out of the repo; or stay public and accept it. Note public repos get unlimited free
  Actions minutes, and private ones get 2,000/month, which is ample for a ~1-minute CI.
  Not changed unilaterally: repository visibility is the developer's call, and the content is
  already published and possibly cached or forked.
- **D3D12 or Vulkan on the Windows target.** `project.godot` pins D3D12. `Plan.md` §21.1 says
  the driver should be left to Godot per platform, which would mean Vulkan on Windows — so the
  project currently contradicts the constitution, and by `Milestones.md` §0.5 Plan.md wins
  unless deliberately amended. Nobody appears to have chosen this; it was set during M0-ENV-03
  and never recorded. It matters because M1-RND-07 verifies the post-process chain against the
  Windows backend, and the risk register rates Metal-vs-Windows divergence Medium/Severe.
  Vulkan is Godot's default and therefore its better-exercised Windows path; D3D12 is more
  native but less travelled in Godot. **Decide before M1-RND-07, not after.** Left as-is for
  now: silently flipping a rendering backend mid-milestone is exactly what D-14 forbids.
- **macOS in v1.0** (`Plan.md` §22.1 vs `M8-LCH-07`). Still open from Session 002.
- **`game/Raktabeej.sln`** is redundant beside the root solution, but Godot generates it.
  Left alone — churning engine-generated files buys nothing.

---

## Session 003 — [DATE] — Environment bootstrap and three toolchain corrections

### Goal
Install the full toolchain on a fresh M4 Pro and get checklist rows 1–14 green.

### What happened
Worked the Appendix A sequence. Three failures surfaced, all environmental, none
architectural. Two were errors in my own written instructions.

### Decisions

**D-13 — .NET 8 LTS, pinned.** The `.pkg` that got installed was **.NET 10.0.400**,
not 8. `global.json` correctly refused to build against it: `rollForward: latestFeature`
only rolls forward *within* the 8.0 band, so it requires an actual 8.0.x SDK.

Installed .NET 8 arm64 side-by-side rather than switching the pin to 10. Reasoning:
Godot's generated csproj targets `net8.0` and `Godot.NET.Sdk` is validated against 8;
gdUnit4Net supports 8 and 9 but not 10; .NET 8 is LTS through late 2026, covering the
whole run to EA. Building `net8.0` with a 10 SDK would *probably* work, but it is the
least-tested combination in the stack, and a month-four failure would be
indistinguishable from a code bug.

**Root cause of the mistake:** my instruction said "download the Arm64 installer" without
specifying the `/8.0` URL. The .NET download page leads with the newest release.
Instruction corrected.

**D-14 — Pin Godot 4.7.2.** The installed engine is **4.7.2.stable.mono.official**, not
the 4.5 my documents assumed. It works, including with .NET 8 — proven by a clean build.
Docs updated and the version pinned in `docs/TOOLCHAIN.md`.

**D-15 amended — gdUnit4 risk raised to High.** 4.7.2 is three minor versions past
gdUnit4Net's published support matrix (4.3–4.4.1). Fallback if it hasn't caught up by
M1-TST-01: drop it entirely and rely on the xUnit core suite plus a headless smoke-test
scene and the debug menu. Less elegant, blocks nothing. This is precisely the
insulation D-06 was designed to provide.

### Learnings (all now folded into Appendix A)

| Trap | Symptom | Fix |
|---|---|---|
| Homebrew on Apple Silicon | `brew` not found after install | Installs to `/opt/homebrew`; must add `brew shellenv` to `~/.zprofile` |
| Wrong .NET major | `A compatible .NET SDK was not found. Requested SDK version: 8.0.0` | Use the `/download/dotnet/8.0` page specifically; SDK not Runtime; Arm64 not x64 |
| **Main Scene not set** | F5 builds but no window; `--path` does nothing useful | Project Settings → Application → Run → **Main Scene**. Was missing from my Step 11 |
| VS Code env inheritance | `${env:GODOT4}` resolves empty when VS Code is launched from the Dock | Hardcode the absolute Godot binary path in `launch.json` |
| **C# Dev Kit hijacks F5** | `'Raktabeej.csproj' does not support debugging. No launchable target found.` | A Godot game assembly is a **library** with no entry point. Must explicitly select the **"Play"** config in the Run and Debug dropdown; F5 with a `.cs` file focused lets C# Dev Kit inject its own launcher. Also set `dotnet.defaultSolution` |
| Wrong output panel | "Nothing happens" after F5 | Build output goes to **Terminal**; `GD.Print` and debugger status go to **Debug Console** (Cmd+Shift+Y) |
| Case sensitivity | Nothing locally; Linux/Steam Deck export fails to find assets | Both dev machines are case-insensitive and will never warn. CI lowercase-path guard added in `ci.yml` |

### Evidence

**Confirmed by pasted output:**

```
Godot Engine v4.7.2.stable.mono.official.ed1daf0bf
Metal 4.0 - Forward+ - Using Device #0: Apple - Apple M4 Pro (Apple9)
hello
```
→ Engine is the .NET build; **native Metal driver on Forward+**; C# assembly loads and
`_Ready()` executes; Main Scene is set. Grey viewport is correct for an empty `Node3D`
with no camera or environment.

```
RaktabeejCore -> .../RaktabeejCore/bin/Debug/net8.0/RaktabeejCore.dll
Raktabeej     -> .../game/.godot/mono/temp/bin/Debug/Raktabeej.dll
Build succeeded.  0 Warning(s)  0 Error(s)
```
→ **The D-06 architecture is working end to end**: the engine-agnostic core compiles and
is consumed by the Godot project. This is the most important confirmation of the session.

**Not yet confirmed — do not assume:**
- Row #9 — F5 debugger attach and breakpoint hit
- Row #7 — `dotnet test` green (scaffold ran but output not verified)
- Row #13 — first push and CI green

### Follow-ups
1. Finish row #9: select "Play" in the Run and Debug dropdown, then F5.
2. Add `.vscode/settings.json` with `dotnet.defaultSolution: Raktabeej.sln`.
3. Create `docs/TOOLCHAIN.md` with the pinned versions.
4. Apply the pending doc edits listed under Session 002 follow-ups.
5. Then M0-ENV-06 (Windows verification target) before any M0-LRN work.

---

## Session 002 — [DATE] — Dev machine decision reversed; delivery plan authored

### Goal
Decide the primary development machine, then produce a followable milestone plan.

### The reversal (D-10 → D-11)

I first recommended **Windows**, on two premises:
1. The Mac was described as having an Intel CPU — a platform Apple has moved past, with a
   weak integrated GPU.
2. Godot's Forward+ renderer is Vulkan-based, so on macOS it would run through MoltenVK
   translating to Metal — the worst possible situation for a project whose entire art
   direction is a `SubViewport` plus a five-stage post-process shader chain.

**Both premises were false.**

The actual machine is a **MacBook Pro 16" (Nov 2024), Apple M4 Pro, 48 GB RAM**. And
Godot's Forward+ renderer supports **Metal as a native rendering driver** alongside Vulkan
and Direct3D 12 — there is no translation layer on Apple Silicon. That removed the entire
technical objection.

**Reversed to: develop on the Mac, use the Windows box as a verification target.** The M4
Pro wins on RAM headroom (Godot + Blender + VS Code + a running build simultaneously),
compile speed, and — decisively for a 12-month solo project — it is the machine already
in daily use. Forcing a context-switch to a second computer is a friction tax that
compounds over 340 sessions.

Confirmed empirically in Session 003: `Metal 4.0 - Forward+ - Apple M4 Pro`.

### Consequences accepted

**The Windows box gets a real job, not idleness.** Rule: *a milestone gate is not passed
until the build runs on the Windows target.* Three reasons it earns its keep:
- **Backend divergence.** The Metal driver is newer and less battle-tested than the Vulkan
  path, and the post-process chain is exactly where differences surface.
- **Performance calibration.** An M4 Pro will hide frame-budget overruns. A 3050-class RTX
  is the honest proxy for the median Steam GPU. **Develop on Mac, profile on Windows.**
- It is the release-export machine.

**Windows exe metadata** (icon, version info) needs `rcedit`, which requires Wine on
macOS. Functional debug builds don't care; release exports either use Wine or happen on
the Windows machine.

**macOS build became nearly free** — and testable on the Apple Silicon slice most Mac
players actually run. Recorded as optional feature `M8-LCH-07`; worth considering pulling
macOS into v1.0 from v1.1.

### Also decided

**D-12 — GitHub with source-asset exclusion.** Repo already existed at
`github.com/Nischaya008/Raktabeej`, so the earlier Azure DevOps recommendation is void and
the free LFS quota applies. Discipline: source assets never enter the repo. For this art
style (low-poly meshes, 128–256 px textures) the exported files are small enough that 1 GB
is realistically sufficient through M4.

### Delivered
`Milestones.md` v1.1 — FDD plan, 141 features across M0–M8, ~340 sessions, gates,
cut-trigger list, risk register, prompt-seed library, and a self-contained Appendix A
environment setup so the plan does not depend on chat history.

### Follow-ups — pending doc edits
- [ ] `Plan.md` §21.1, §22: Godot version → 4.7.2.stable.mono
- [ ] `Plan.md` §22.1: reconsider macOS for v1.0 rather than v1.1
- [ ] `Milestones.md` Appendix A.1 step 3: explicit `/download/dotnet/8.0` instruction
- [ ] `Milestones.md` Appendix A.3: add the **Main Scene** row
- [ ] `Milestones.md` Appendix A.4 row 9: add the F5 troubleshooting note
- [ ] `Milestones.md` §9: gdUnit4 version-drift risk → Likelihood **High**

---

## Session 001 — [DATE] — Game design and PRD authored

### Goal
Turn a one-paragraph game idea into a complete, unambiguous source-of-truth design
document, for a solo developer with zero prior game-development experience.

### Two tensions in the original idea, both resolved

**The camera fought the art style.** A 360°-swivel camera requires actual 3D geometry —
*V Rising* is a 3D game with an angle-locked camera, not a 2D game. Hand-drawn sprites
cannot rotate. Resolved as **D-02**: low-poly 3D rendered into a 640×360 `SubViewport`
with palette quantization and Bayer dithering. Achievable, distinctive, and it makes 3D
asset cost real — mitigated by the modular-kit strategy and CC0 sources.

**The feature list was AAA-scale.** Drivable helicopters and boats, playable basketball
and table tennis, a full police AI, ~20 powers, six shapeshift forms, survival systems,
and a revenge campaign — Rockstar territory. Resolved two ways:
- **D-04**: v1.0 is Prologue + Act I in two districts.
- **D-07**: the "vampire doesn't understand modern things" fiction becomes a *diegetic
  feature gate*, so the entire modern feature set ships post-launch with narrative
  justification rather than being cut.

### Core design

The unique mechanic is the **Sanguine Ledger**: every drained victim's core memory enters
the player character permanently, as simultaneously the progression currency, the debuff
source, and the narrative content. Powers are not bought with XP — they are assembled
from *specific biographies* (flight requires someone who knew falling; Dominate requires
someone who knew obedience). Hunting becomes about biography, not blood.

Seven **Hauntings** make it mechanical rather than cosmetic — the signature one re-renders
2070 Kolkata as 1861 Calcutta while leaving collision unchanged, so the tram you can no
longer see will still kill you.

**This is designated the MVP.** The M3 gate exists to kill the project if five external
playtesters don't talk about it unprompted.

### Setting (D-05)
Moved from generic cyberpunk to **2070 Kalighat Sprawl**. Antagonist Colonel Ambrose
Thorne is the werewolf who killed the protagonist's family in 1861 and is now CEO of the
corporation whose drill exhumed him — which makes the police/corporate pressure system
*the villain's body* rather than a boss at the end.

**Recorded obligation:** Kali and Kalighat are actively worshipped. The in-game shrine and
the Ashen Order are fictionalized composites, not depictions of the real temple or a real
sect. A sensitivity read is a hard gate before M7 ships.

### Architecture (D-06)
`RaktabeejCore` as a pure .NET library with zero Godot references, with Godot nodes as a
thin presentation layer. Four reasons: unit-testable in seconds; plays to existing
software-engineering skill; **makes AI-assisted development verifiable against tests
rather than eyeballed**; and survives an engine change if Godot proves inadequate.

### Delivered
`Plan.md` — 30 sections covering the hook, world, full three-act story with four endings,
camera spec, controls, core loop, blood and survival systems, powers, combat, threat
systems, lair, vehicles, art and audio pipelines, accessibility, technical architecture,
engine decision record, a $100 cost budget, and a 12-month roadmap.

### Notable verified facts
- C# in Godot 4 **cannot** export to web. Ruled out permanently.
- Godot has **no official console support**; Switch would need a paid third-party partner.
- Android/iOS C# support is experimental (since 4.2); Android export needs .NET 9+.
- Jolt Physics is built in and the default since Godot 4.4.
- gdUnit4Net v5+ is VSTest-compatible and runs via `dotnet test`.
- Minimum cost to ship on Steam: **$100** (Steam Direct, recoupable after $1,000 revenue).
