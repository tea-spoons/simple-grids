# Simple Grids
Lightweight general-purpose 2D grids in 3D space.

# Usage
## Creating grids
- Add a grid to the scene by using one of the provided **grid components** located in `TeaSpoons/Simple Grids/` in the add component menu.
- Edit the grid to provide the cells.
- Optional: Create a `SimpleGridGroup` asset to assign grids to. The grid group can be used as a proxy to find positions on a set of grids.

## Creating grid residents
A "grid resident" is an object with a shape that can go on a grid. Its shape is defined by **blocks** which are equivalent to grid cells.

You don't _need_ to use grid residents, but doing so allows you to
- check whether the object fits onto a grid at a given position.
- check each block for whether it is allowed to go on the cell underneath. This can be used to check whether a cell is blocked
or whether the "must be on water" part of a building is actually on water.

# Technical usage
## SimpleGrids
`SimpleGrid`s mainly work to convert between world position, "grid position" and "cell position".
- A **grid position** is a local position on the grid, but 2D and scaled with cell size. It's basically the cell position before rounding.
- A **cell position** is the two-dimensional index on a grid at which a cell _may or may not_ be.

### Find grid positions
- Use `SimpleGrid.Raycast` or `SimpleGridGroup.Raycast` to find a grid position on a grid.
  - `SimpleGrid.Raycast` returns true if the grid's plane was hit, it doesn't check whether there is actually a cell at the detected position.
  - `SimpleGridGroup.Raycast` only returns true when an actual cell was hit.
- You can also use `SimpleGrid.WorldToGridPosition(Vector3)` to translate a world position into a grid position along the grid's normal.

### Check for cells at a grid position
- Use `SimpleGrid.GridToCellPosition` to translate a grid position into a cell position.
- You can use `SimpleGrid.HasCellAt` to check whether the grid actually has a cell on a given cell position.
- `SimpleGrid.TryGetCell` is a shorthand for combining both of the above.

### Translate cell positions to world space
To position an object on a grid, you need to translate the position of the cell it is on back to a world position.
- Use `SimpleGrid.CellToWorldPosition` to get the position of _the corner_ of a cell in world space.

### Attach data to cells
- Use the `SimpleGridCellDictionary<T>` to attach data structs of type `T` to a combination of a grid reference and a cell position.
  It is your responsibility to make sure that the dictionary only contains valid cell positions.

## SimpleGridResidents
`SimpleGridResident` is an abstract base class for rid residents. You can either use one of its provided subclasses or inherit your own.

Placement of grid residents happens via the **origin cell position**, which is the cell position at the (min, min) corner of the object's bounds.
However, the center of the object is in the middle of its bounds.
So the code required from having a ray to knowing the validated position of a grid resident looks a bit like this:
```csharp
var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
if (gridGroup.Raycast(ray, out hit))
{
    // Find the origin position according to the resident's bounds.
    var originGridPosition = gridResident.CenterToOriginGridPosition(hit.GridPosition);
    // Find the cell position at the origin grid position.
    var originCellPosition = hit.Grid.GridToCellPosition(originGridPosition);
    
    // Checks whether all blocks can go on the cells underneath them.
    var placementCheckResult = gridResident.CheckGridPlacement(hit.Grid, originCellPosition);
    
    if (placementCheckResult == PlacementCheckResult.Valid)
    {
        gridResident.PlaceOnGrid(hit.Grid, originCellPosition);
    }
}
```

However, in most cases, it's easier and yielding better results to use the `AdvancedGridResidentPlacement` class.

### Cell Check Functions
The `SimpleGridResident` class has a delegate `CellCheckFunction` and a protected field `cellCheck` of that type.

This function is used during `CheckGridPlacement` calls. **For every block** of the resident, the method is called with
- The grid the resident is checking with.
- The resident-relative (as in: "ignoring orientation") position of the block.
- The cell position of the cell that is underneath the block in the current check.

Use a `SimpleGridCellDictionary` (see above) to get the data from the grid and cell position,
and use whatever logic you prefer to attach data to blocks in your grid resident class.
Then, you can use the data passed into the cell check function to implement logic that decides whether a block could be placed
on given cell or not.

The `CellCheckFunction` returns a `PlacementCheckResult` with the following semantics:
- `Valid`: The block can be placed on the cell.
- `Invalid`: The block is over a cell, but cannot be placed on it (example: The block must be placed on land, but the cell is an "ocean" cell).
A grid resident with invalid block placements will still move to the target position, but return `Invalid` to indicate that invalid placement should be
visualized - for example by turning the mesh red.
- `BlockedOrOffGrid`: Returned by the resident if the target position would make one or more block hover outside of the grid.
The `CellCheckFunction` can return this as well to define that a block is blocked from moving onto the cell.
As a result, the grid resident will not move to the target position at all.

### AdvancedGridResidentPlacement
This class features methods for complex grid resident placement, offering a smooth user experience through **sliding**.

When using this class, the player can leave the grid with their cursor/finger and have the resident "slide" along the edge of a grid.

It's also very easy to use:
```csharp
var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
var placementResult = AdvancedGridResidentPlacement.PlaceOnGridGroup(gridResident, gridGroup, ray);
```

## Grid Visualizaion
Visualize grids at runtime by following these steps:
- Create a child GameObject to the grid GameObject.
- Add a non-abstract subtype of the `SimpleGridMeshGenerator` class to the GameObject.
- Add a `MeshRenderer` and give it a material.

## Installation

In Unity: **Window > Package Manager > + > Add package from git URL**, then enter:

```
https://github.com/tea-spoons/simple-grids.git
```

Pin a release by appending a tag, for example `#v0.5.2`.

### Dependencies

Unity cannot resolve git dependencies automatically, so add these to your project first:

- `com.tea-spoons.package-core` 1.4.0

## Change plan

See [CHANGE-PLAN.md](CHANGE-PLAN.md) for what changed before publishing and what is planned next.

## License

Copyright (c) 2026 Bigpoint. Authored by Muhammad Tarek Abdou.

Available for research, education and other noncommercial use under the [PolyForm Noncommercial 1.0.0](LICENSE.md)
license. Commercial use is not permitted.
