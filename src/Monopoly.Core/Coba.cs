using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monopoly.Core
{
    public class Coba
    {
        public void InitializePath()
        {
           for (int i = 0; i < 40; i++)
           {
                var res = GetGridPosition(i);
                Console.WriteLine(res);
           }
        }

        private static (int row, int column) GetGridPosition(int index)
        {
            // GridSize = 11, Monopoly size
            int last = Models.Board.GridSize - 1;

            if (index <= last)
            {
                return (last, last - index);          // bottom edge, right to left
            }

            if (index <= 2 * last)
            {
                return (2 * last - index, 0);         // left edge, bottom to top
            }

            if (index <= 3 * last)
            {
                return (0, index - 2 * last);         // top edge, left to right
            }

            return (index - 3 * last, last);   
        }
    }
}