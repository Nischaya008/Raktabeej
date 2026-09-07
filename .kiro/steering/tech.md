---
inclusion: always
---

# Technical Constraints

## Stack
Godot 4.x (.NET build) · C# / .NET 8 · Jolt 3D physics · xUnit
Dev machine: macOS Apple Silicon (Metal, default). Verify target: Windows (D3D12 —
`project.godot` pins `rendering_device/driver.windows`, it is not Godot's default).

## Architecture — non-negotiable
- `RaktabeejCore` is a plain .NET library and MUST NEVER reference Godot.
  If a change seems to need a Godot type in core, the design is wrong — say so.
- Game rules live in core. Godot nodes read core state and render it. Nodes
  never own rules.
- All content is a `[GlobalClass] Resource` under `game/data/`. No balance
  numbers hardcoded in C#.
- Save data is a POCO serialized with System.Text.Json, with a SchemaVersion.
  Never serialize a Godot node.

## Naming (decision D-16 — rationale in Agent_History.md)
- Under `game/`, C# script files are `PascalCase.cs` and the class name MUST
  match the file name exactly. Godot resolves scripts by that match and is
  case-sensitive about it; a mismatch fails at runtime, not at build time.
- Everything else under `game/` is lowercase_snake_case: directories, scenes,
  resources, art, audio, shaders. These load via hand-written `res://` strings,
  where a case mismatch survives macOS and Windows but breaks the Linux and
  Steam Deck export. CI enforces this on every committed path. The exemption
  covers only the *filename*, and only for `.cs`, `.cs.uid`, `.csproj`, `.sln`.
  **Directory segments are never exempt** — `game/src/Autoload/GameClock.cs`
  fails the guard on its `Autoload/` segment despite being a `.cs` file.
- C# types PascalCase, private fields _camelCase.

## Working rules
- Read `docs/Plan.md` and the active Feature Card before writing code.
- Implement ONLY the Feature Card's IN scope. Never build something on its
  OUT list — stop and report instead.
- Every core rule change ships with xUnit tests in the same commit.
- Run `dotnet build` and `dotnet test` and report the ACTUAL output. Never
  claim a passing build you did not run.
- Never delete, skip, or weaken a test to make a build green.
- Never leave a stub, TODO, or NotImplementedException in work reported complete.
- Every visual effect ships with its photosensitivity-safe variant in the same
  feature.
- Log every third-party asset in `docs/ASSET_LICENSES.md` in the same commit.
