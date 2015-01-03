﻿namespace AI_2048
{
    interface IMoveMaker2048
    {
        Board2048 MakePlayerMove(Board2048 board,
                                 Direction dir,
                                 out int scoreDelta,
                                 out bool nextMovePossible,
                                 out bool boardChanged);

        Board2048 MakeSpecificGameMove(Board2048 board, int row, int col, long tile);

        Board2048 MakeGameMove(Board2048 board);
    }
}