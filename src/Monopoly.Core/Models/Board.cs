using Monopoly.Core.Interfaces;

namespace Monopoly.Core.Models
{
    public class Board : IBoard
    {
        // public ISpace[,] Grid { get; }
        // public List<ISpace> Path { get; } = new List<ISpace>();
        // public int Size { get; }

        // public Board(int size)
        // {
        //     Size = size;
        //     Grid = new ISpace[size, size];
        // }

        /// <summary>The grid is always 11 x 11: 40 squares around the edge, 9 on each side between the corners.</summary>
        public const int GridSize = 11;

        public ISpace[,] Grid { get; } = new ISpace[GridSize, GridSize];
        public List<ISpace> Path { get; } = new List<ISpace>();
        public int Size => GridSize;

        public Board()
        {
        }
    }
}