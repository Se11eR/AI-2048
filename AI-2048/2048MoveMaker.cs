using System;

namespace AI_2048
{
    internal class MoveMaker2048 : IMoveMaker2048
    {
        private readonly Random __Rand = new Random(Int32.MaxValue / 2);
        private readonly ushort[] __SwipeLookup;
        private readonly uint[] __ScoreLookup;

        private readonly ushort[] __ReverseSwipeLookup;
        private readonly uint[] __ReverseScoreLookup;

        public MoveMaker2048()
        {
            Helper.GenerateLookupScoresTable(out __SwipeLookup, out __ScoreLookup);
            Helper.GenerateLookupScoresReverseTable(out __ReverseSwipeLookup, out __ReverseScoreLookup);
        }

        public Board2048 MakePlayerMove(Board2048 board,
                                        Direction dir,
                                        out long scoreDelta,
                                        out bool boardChanged)
        {
            scoreDelta = 0;
            var originalBoard = new Board2048(board);

            //http://stackoverflow.com/a/22498940
            switch (dir)
            {
                case Direction.Up:
                    board.Transpose();
                    for (var i = 0; i < Board2048.SIZE; i++)
                    {
                        var b = board.ExtractChunkBlock(i);
                        board.SetChunkBlock(__SwipeLookup[b], i);
                        scoreDelta += __ScoreLookup[b];
                    }
                    board.Transpose();
                    break;
                case Direction.Down:
                    board.Transpose();
                    for (var i = 0; i < Board2048.SIZE; i++)
                    {
                        var b = board.ExtractChunkBlock(i);
                        board.SetChunkBlock(__ReverseSwipeLookup[b], i);
                        scoreDelta += __ReverseScoreLookup[b];
                    }
                    board.Transpose();
                    break;
                case Direction.Right:
                    for (var i = 0; i < Board2048.SIZE; i++)
                    {
                        var b = board.ExtractChunkBlock(i);
                        board.SetChunkBlock(__ReverseSwipeLookup[b], i);
                        scoreDelta += __ReverseScoreLookup[b];
                    }
                    break;
                case Direction.Left:
                    for (var i = 0; i < Board2048.SIZE; i++)
                    {
                        var b = board.ExtractChunkBlock(i);
                        board.SetChunkBlock(__SwipeLookup[b], i);
                        scoreDelta += __ScoreLookup[b];
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dir");
            }

            boardChanged = !originalBoard.Equals(board);
            return board;
        }

        public bool IsGameOver(Board2048 board)
        {
            foreach (var dir in new[] {Direction.Up, Direction.Down, Direction.Left, Direction.Right})
            {
                long s;
                bool changed;
                MakePlayerMove(board, dir, out s, out changed);
                if (changed)
                    return false;
            }

            return true;
        }

        public Board2048 MakeSpecificGameMove(Board2048 board, int row, int col, long tile)
        {
            board[row, col] = tile;
            return board;
        }

        public Board2048 MakeGameMove(Board2048 board, out long scoreDelta)
        {
            int randRow;
            int randCol;
            do
            {
                randRow = __Rand.Next(Board2048.SIZE);
                randCol = __Rand.Next(Board2048.SIZE);
            } while (board[randRow, randCol] > 0);

            scoreDelta = 0;
            var prob = __Rand.NextDouble();
            long tile;
            if (prob > Board2048.CONTS2_PROB)
            {
                tile = Board2048.CONST4;
                scoreDelta += 4;
            }
            else
                tile = Board2048.CONST2;

            board[randRow, randCol] = tile;
            return board;
        }
    }
}
