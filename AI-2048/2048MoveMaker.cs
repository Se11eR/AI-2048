using System;

namespace AI_2048
{
    internal class MoveMaker2048 : IMoveMaker2048
    {
        private readonly Random __Rand = new Random(Int32.MaxValue / 2);
        private readonly ushort[] __SwipeLookup;
        private readonly sbyte[] __ScoreLookup;

        private readonly ushort[] __ReverseSwipeLookup;
        private readonly sbyte[] __ReverseScoreLookup;

        public MoveMaker2048()
        {
            Helper.GenerateLookupScoresTable(4, out __SwipeLookup, out __ScoreLookup);
            Helper.GenerateLookupScoresReverseTable(4, out __ReverseSwipeLookup, out __ReverseScoreLookup);
        }

        public Board2048 MakePlayerMove(Board2048 board,
                                        Direction dir,
                                        out int scoreDelta,
                                        out bool boardChanged)
        {
            scoreDelta = 0;
            var originalBoard = new Board2048(board);

            //http://stackoverflow.com/a/22498940
            switch (dir)
            {
                case Direction.Up:
                    for (int i = 0; i < board.Size; i++)
                    {
                        var r = board.ExtractColumn(i);
                        board.SetColumn(__SwipeLookup[r], i);
                        var sl = __ScoreLookup[r];
                        scoreDelta += sl > 0 ? (1 << sl) : 0;
                    }
                    break;
                case Direction.Down:
                    for (int i = 0; i < board.Size; i++)
                    {
                        var r = board.ExtractColumn(i);
                        board.SetColumn(__ReverseSwipeLookup[r], i);
                        var sl = __ReverseScoreLookup[r];
                        scoreDelta += sl > 0 ? (1 << sl) : 0;
                    }
                    break;
                case Direction.Right:
                    for (int i = 0; i < board.Size; i++)
                    {
                        var r = board.ExtractRow(i);
                        board.SetRow(__ReverseSwipeLookup[r], i);
                        var sl = __ReverseScoreLookup[r];
                        scoreDelta += sl > 0 ? (1 << sl) : 0;
                    }
                    break;
                case Direction.Left:
                    for (int i = 0; i < board.Size; i++)
                    {
                        var r = board.ExtractRow(i);
                        board.SetRow(__SwipeLookup[r], i);
                        var sl = __ScoreLookup[r];
                        scoreDelta += sl > 0 ? (1 << sl) : 0;
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
                int s;
                bool changed;
                MakePlayerMove(board, dir, out s, out changed);
                if (changed)
                    return false;
            }

            return true;
        }

        public Board2048 MakePlayerMoveOld(Direction dir, Board2048 board, out int score, out bool isMovePossible)
        {
            score = 0;
            //http://www.cyberforum.ru/csharp-net/thread1172757.html
            bool movePossible = false;
            switch (dir)
            {
                case Direction.Up:

                    #region Up

                    for (int col = 0; col < board.Size; col++)
                    {
                        // Проверяемая (опорная) и текущая ячейки
                        int pivot = 0, row = pivot + 1;

                        while (row < board.Size)
                        {
                            // Текущая ячейка пуста, переходим на следующую
                            if (board[row, col] == 0)
                                row++;
                            // Опорная ячейка пуста, переносим в нее значение текущей
                            else if (board[pivot, col] == 0)
                            {
                                board[pivot, col] = board[row, col];
                                board[row++, col] = 0;
                                movePossible = true;
                            }
                            // Значения опорной и текущей ячеек совпадают — складываем их и переходим на следующую строчку
                            else if (board[pivot, col] == board[row, col])
                            {
                                board[pivot++, col] += 1;
                                board[row++, col] = 0;
                                score += (1 << (int)board[pivot - 1, col]);
                                movePossible = true;
                            }
                            // Нечего двигать — едем дальше
                            else if (++pivot == row)
                                row++;
                        }
                    }

                    #endregion

                    break;
                case Direction.Down:

                    #region Down

                    for (int col = 0; col < board.Size; col++)
                    {
                        int pivot = board.Size - 1, row = pivot - 1;

                        while (row >= 0)
                        {
                            if (board[row, col] == 0)
                                row--;
                            else if (board[pivot, col] == 0)
                            {
                                board[pivot, col] = board[row, col];
                                board[row--, col] = 0;
                                movePossible = true;
                            }
                            else if (board[pivot, col] == board[row, col])
                            {
                                board[pivot--, col] += 1;
                                board[row--, col] = 0;
                                score += (1 << (int)board[pivot + 1, col]);
                                movePossible = true;
                            }
                            else if (--pivot == row)
                                row--;
                        }
                    }

                    #endregion

                    break;
                case Direction.Left:

                    #region Left

                    for (int row = 0; row < board.Size; row++)
                    {
                        int pivot = 0, col = pivot + 1;
                        while (col < board.Size)
                        {
                            if (board[row, col] == 0)
                                col++;
                            else if (board[row, pivot] == 0)
                            {
                                board[row, pivot] = board[row, col];
                                board[row, col++] = 0;
                                movePossible = true;
                            }
                            else if (board[row, pivot] == board[row, col])
                            {
                                board[row, pivot++] += 1;
                                board[row, col++] = 0;
                                score += (1 << (int)board[row, pivot - 1]);
                                movePossible = true;
                            }
                            else if (++pivot == col)
                                col++;
                        }
                    }

                    #endregion

                    break;
                case Direction.Right:

                    #region Right

                    for (int row = 0; row < board.Size; row++)
                    {
                        int pivot = board.Size - 1, col = pivot - 1;
                        while (col >= 0)
                        {
                            if (board[row, col] == 0)
                                col--;
                            else if (board[row, pivot] == 0)
                            {
                                board[row, pivot] = board[row, col];
                                board[row, col--] = 0;
                                movePossible = true;
                            }
                            else if (board[row, pivot] == board[row, col])
                            {
                                board[row, pivot--] += 1;
                                board[row, col--] = 0;
                                score += (1 << (int)board[row, pivot + 1]);
                                movePossible = true;
                            }
                            else if (--pivot == col)
                                col--;
                        }
                    }

                    #endregion

                    break;
                default:
                    throw new ArgumentOutOfRangeException("dir");
            }
            isMovePossible = movePossible;
            return board;
        }

        public Board2048 MakeSpecificGameMove(Board2048 board, int row, int col, long tile)
        {
            board[row, col] = tile;
            return board;
        }

        public Board2048 MakeGameMove(Board2048 board)
        {
            int randRow;
            int randCol;
            do
            {
                randRow = __Rand.Next(board.Size);
                randCol = __Rand.Next(board.Size);
            }
            while (board[randRow, randCol] > 0);

            var tile = __Rand.NextDouble() > Board2048.CONTS2_PROB ? Board2048.CONST4 : Board2048.CONST2;

            board[randRow, randCol] = tile;
            return board;
        }
    }
}
