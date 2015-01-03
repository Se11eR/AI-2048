using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_2048
{
    interface IAi2048
    {
        Direction CalculateNextMove(Board2048 board2048);
    }
}
