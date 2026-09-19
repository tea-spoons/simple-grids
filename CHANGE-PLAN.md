# Change plan

> Draft. This file tracks what changed on the way to this repo and what I plan to change next. Edit freely.

## Origin

Originally developed at Bigpoint. Published here with Bigpoint's permission for research, education and other noncommercial use. Copyright (c) 2026 Bigpoint; see [LICENSE.md](LICENSE.md).

## Changes made before publishing

- Namespaces are now `TeaSpoons.*` and the package id is `com.tea-spoons.simple-grids` (assemblies renamed to match).
- Internal build, registry and tracker references were removed; the repo uses GitHub Actions (`CI` and `Release`) built on `unity-ci-kit`.
- Added `LICENSE.md` (PolyForm Noncommercial 1.0.0), an install section in the README, and package metadata (author, license and documentation URLs).

## Planned changes

- [x] Tag and publish `v0.5.2` with the Release workflow.
- [ ] Run this package's tests in CI with `unity-ci-kit` (needs a small test-project helper in the kit).
- [ ] Make installs resolve dependencies automatically, for example through a registry such as OpenUPM.
<!-- review-items:start -->
- [ ] **P1** Add `GetNeighbors(cell, NeighborMode)` (4-way and 8-way, and hex for freeform grids) with tests.
- [ ] **P1** Extract the coordinate math into a plain struct or static class that does not need a `MonoBehaviour`. That makes it testable and Burst-friendly, and the grid delegates to it.
- [ ] **P1** Declares `unity: 2022.3`, but only Unity 6000.3.8f1 was tested. Add a Unity version matrix to CI once package tests run there (see the `unity-ci-kit` plan), or raise the minimum.
- [ ] **P2** Resolve the TODO by validating the serialized settings in `OnValidate` with clear messages.
- [ ] **P2** Provide an optional node interface for pathfinding, and a sample A* outside the core.
- [ ] **P2** Make the package-core dependency optional.
- [ ] **P2** Add a `CHANGELOG.md`. Unity's package layout lists one next to `README.md`, and the `unity-ci-kit` validator warns without it.
<!-- review-items:end -->

<!-- review:start -->
## Review (September 2026)

Reviewed as a senior Unity engineer would: I read the code and compared the package with similar open-source projects (September 2026). Those projects are listed for ideas only. Nothing was copied from them, and their licenses are noted in case code is ever reused. Priorities: **P0** correctness bug or broken metadata, **P1** should be done soon, **P2** nice to have.

### Compared with

| Project | License | Worth noting |
|---|---|---|
| [yasirkula/UnityGridFramework](https://github.com/yasirkula/UnityGridFramework) | not checked | Grid framework for building grid-based levels and placing prefabs. |
| [MarcoElz/unity-pathfinding-grid](https://github.com/MarcoElz/unity-pathfinding-grid) | not checked | Pathfinding algorithms on grids. Your node type implements `INode` (position, visitable, weight, neighbors). |

### Findings from reading the code

- **[Gap]** There is no neighbor enumeration (4, 8, hex) and no pathfinding hook. The only neighbor logic is inside `AdvancedGridResidentPlacement`.
- **[Design]** `SimpleGrid` is a `MonoBehaviour` that implements `IEnumerable<Vector2Int>`. The coordinate math (`WorldToCellPosition` and friends) cannot be used from jobs or tests without a GameObject.
- **[TODO]** `SimpleGridMeshGenerator.cs` line 21 suggests adding a validation package.
- **[Tests]** 6 tests, 186 lines, for about 1,000 lines of code.
<!-- review:end -->

## Notes and ideas

_Add your own here._
