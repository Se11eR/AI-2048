using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_2048
{
    interface I2048MoveMaker
    {
        Board2048 MakePlayerMove(Direction dir, Board2048 board);

        Board2048 MakeGameMove(Board2048 board);
    }
}
