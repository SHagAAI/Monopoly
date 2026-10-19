namespace Monopoly.Core.Interfaces;

/// <summary>
/// Represents the Monopoly board consisting of 40 squares arranged in a loop (BRD-01 to BRD-21).
/// Takes ISpace Grid, List<ISpace> Path, int Size
/// </summary>
public interface IBoard
{
     /// <summary>
    /// The squares laid out in a grid, for drawing the board.
    /// The squares sit on the outer edge of the grid. Cells that are not a square are null.
    /// </summary>
    ISpace[,] Grid { get; }
 
    /// <summary>
    /// The squares in the order tokens walk, starting at GO.
    /// The index in this list is the square's position (0 = GO, 39 = Boardwalk).
    /// </summary>
    List<ISpace> Path { get; }
 
    /// <summary>The number of cells along one side of the grid (11 for the standard board).</summary>
    int Size { get; }
}