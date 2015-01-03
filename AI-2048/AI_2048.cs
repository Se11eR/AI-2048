using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AI_2048
{
    internal class Ai2048 : IAi2048, I2048MoveMaker
    {
        private readonly Random __Rand = new Random(Int32.MaxValue / 2);
        private const int DEPTH = 8;

        public Direction CalculateNextMove(Board2048 board2048)
        {
            var max = 0.0;
            var bestDir = Direction.Down;
            foreach (var dir in Helper.GetValues<Direction>())
            {
                var moveValue = Expectimax(MakePlayerMove(dir, board2048), DEPTH, Move.Game);
                if (moveValue > max)
                {
                    max = moveValue;
                    bestDir = dir;
                }
            }

            return bestDir;
        }

        public double Expectimax(Board2048 board, int depth, Move curMove)
        {
            if (depth <= 0)
                return StaticEvaluationFunction(board);

            var oppositeMove = GetOppositeMove(curMove);
            switch (oppositeMove)
            {
                case Move.Player:
                    var max = double.NegativeInfinity;

                    foreach (var move in GenerateAllPlayerMoves(board))
                    {
                        var moveValue = Expectimax(move, depth - 1, oppositeMove);
                        if (moveValue > max)
                        {
                            max = moveValue;
                        }
                    }

                    return max;
                case Move.Game:
                    var movesCount = 0;

                    var sum = 0.0;
                    foreach (var move in GenerateAllGameMoves(board, Board2048.CONST2))
                    {
                        movesCount++;
                        sum += Expectimax(move, depth - 1, oppositeMove);
                    }
                    var weightenedSum = ((sum / movesCount) * Board2048.CONTS2_PROB);

                    sum = 0.0;
                    movesCount = 0;
                    foreach (var move in GenerateAllGameMoves(board, Board2048.CONST4))
                    {
                        movesCount++;
                        sum += Expectimax(move, depth - 1, oppositeMove);
                    }
                    weightenedSum += ((sum / movesCount) * Board2048.CONTS4_PROB);

                    return weightenedSum;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Board2048 MakePlayerMove(Direction dir, Board2048 board)
        {
            //http://www.cyberforum.ru/csharp-net/thread1172757.html
            int score = 0;
            switch (dir)
            {
                case Direction.Up:

                    #region Up

                    for (int col = 0; col < board.Cols; col++)
                    {
                        // Проверяемая (опорная) и текущая ячейки
                        int pivot = 0, row = pivot + 1;

                        while (row < board.Rows)
                        {
                            // Текущая ячейка пуста, переходим на следующую
                            if (board[row, col] == 0)
                                row++;
                                // Опорная ячейка пуста, переносим в нее значение текущей
                            else if (board[pivot, col] == 0)
                            {
                                board[pivot, col] = board[row, col];
                                board[row++, col] = 0;
                            }
                                // Значения опорной и текущей ячеек совпадают — складываем их и переходим на следующую строчку
                            else if (board[pivot, col] == board[row, col])
                            {
                                board[pivot++, col] += 1;
                                board[row++, col] = 0;
                                score += (1 << (int)board[pivot - 1, col]);
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

                    for (int col = 0; col < board.Cols; col++)
                    {
                        int pivot = board.Rows - 1, row = pivot - 1;

                        while (row >= 0)
                        {
                            if (board[row, col] == 0)
                                row--;
                            else if (board[pivot, col] == 0)
                            {
                                board[pivot, col] = board[row, col];
                                board[row--, col] = 0;
                            }
                            else if (board[pivot, col] == board[row, col])
                            {
                                board[pivot--, col] += 1;
                                board[row--, col] = 0;
                                score += (1 << (int)board[pivot + 1, col]);
                            }
                            else if (--pivot == row)
                                row--;
                        }
                    }

                    #endregion

                    break;
                case Direction.Left:

                    #region Left

                    for (int row = 0; row < board.Rows; row++)
                    {
                        int pivot = 0, col = pivot + 1;
                        while (col < board.Cols)
                        {
                            if (board[row, col] == 0)
                                col++;
                            else if (board[row, pivot] == 0)
                            {
                                board[row, pivot] = board[row, col];
                                board[row, col++] = 0;
                            }
                            else if (board[row, pivot] == board[row, col])
                            {
                                board[row, pivot++] += 1;
                                board[row, col++] = 0;
                                score += (1 << (int)board[row, pivot - 1]);
                            }
                            else if (++pivot == col)
                                col++;
                        }
                    }

                    #endregion

                    break;
                case Direction.Right:

                    #region Right

                    for (int row = 0; row < board.Rows; row++)
                    {
                        int pivot = board.Cols - 1, col = pivot - 1;
                        while (col >= 0)
                        {
                            if (board[row, col] == 0)
                                col--;
                            else if (board[row, pivot] == 0)
                            {
                                board[row, pivot] = board[row, col];
                                board[row, col--] = 0;
                            }
                            else if (board[row, pivot] == board[row, col])
                            {
                                board[row, pivot--] += 1;
                                board[row, col--] = 0;
                                score += (1 << (int)board[row, pivot + 1]);
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

            board.Score = score;
            return board;
        }

        public Board2048 MakeGameMove(Board2048 board)
        {
            int randRow;
            int randCol;
            do
            {
                randRow = __Rand.Next(board.Rows);
                randCol = __Rand.Next(board.Cols);
            }
            while (board[randRow, randCol] > 0);

            var tile = __Rand.NextDouble() > Board2048.CONTS2_PROB ? Board2048.CONST4 : Board2048.CONST2;

            board[randRow, randCol] = tile;
            return board;
        }

        private static double StaticEvaluationFunction(Board2048 board)
        {
            throw new NotImplementedException();
        }

        private static Board2048 MakeGameMove(Board2048 b, int row, int col, long tile)
        {
            b[row, col] = tile;
            return b;
        }

        private Move GetOppositeMove(Move move)
        {
            switch (move)
            {
                case Move.Player:
                    return Move.Game;
                case Move.Game:
                    return Move.Player;
                default:
                    throw new ArgumentOutOfRangeException("move");
            }
        }

        private IEnumerable<Board2048> GenerateAllPlayerMoves(Board2048 original)
        {
            return Helper.GetValues<Direction>().Select(dir => MakePlayerMove(dir, original));
        }

        private IEnumerable<Board2048> GenerateAllGameMoves(Board2048 original, long tile)
        {
            for (var row = 0; row < original.Rows; row++)
            {
                for (var col = 0; col < original.Cols; col++)
                {
                    if (original[row, col] <= 0)
                        yield return MakeGameMove(original, row, col, tile);
                }
            }
        }
    }
}