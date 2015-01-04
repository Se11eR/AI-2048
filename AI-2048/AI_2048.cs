using System;
using System.Collections.Generic;

namespace AI_2048
{
    internal class Ai2048 : IAi2048
    {
        private readonly IMoveMaker2048 __MoveMaker;
        private const int DEPTH = 6;

        public Ai2048(IMoveMaker2048 moveMaker)
        {
            __MoveMaker = moveMaker;
        }

        public Direction CalculateNextMove(Board2048 board2048)
        {
            //TODO:
            //Heuristics: Smoothness, Monotonicity, Empty cells
            //http://stackoverflow.com/questions/22342854/what-is-the-optimal-algorithm-for-the-game-2048/22389702#22389702
            //http://blog.datumbox.com/using-artificial-intelligence-to-solve-the-2048-game-java-code/
            //Smoothness, Monotonicity, Empty cells
            //Transposition tables
            //Unlikely nodes: dont go deep in nodes that are not likely to happen (e.g. 4 "4"s in a row)

            var max = 0.0;
            var bestDir = Direction.Down;
            foreach (var dir in new[] {Direction.Up, Direction.Down, Direction.Left, Direction.Right})
            {
                int score;
                bool changed;
                var moveValue =
                    Expectimax(
                               __MoveMaker.MakePlayerMove(board2048, dir, out score, out changed),
                               0,
                               DEPTH,
                               Move.Player);

                if (!changed)
                    continue;

                if (moveValue > max)
                {
                    max = moveValue;
                    bestDir = dir;
                }
            }

            return bestDir;
        }

        private double Expectimax(Board2048 board, int currentScore, int depth, Move curMove)
        {
            if (depth <= 0)
                return StaticEvaluationFunction(board, currentScore);

            var oppositeMove = GetOppositeMove(curMove);
            switch (oppositeMove)
            {
                case Move.Player:
                    var max = double.NegativeInfinity;

                    foreach (var dir in new[] {Direction.Up, Direction.Down, Direction.Left, Direction.Right})
                    {
                        int scoreDelta;
                        bool changed;
                        var move = __MoveMaker.MakePlayerMove(board,
                                                              dir,
                                                              out scoreDelta,
                                                              out changed);

                        if (!changed)
                            continue;

                        var moveValue = Expectimax(move, currentScore + scoreDelta, depth - 1, oppositeMove);
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
                        sum += Expectimax(move, currentScore, depth - 1, oppositeMove);
                    }
                    var weightenedSum = ((sum / movesCount) * Board2048.CONTS2_PROB);

                    sum = 0.0;
                    movesCount = 0;
                    foreach (var move in GenerateAllGameMoves(board, Board2048.CONST4))
                    {
                        movesCount++;
                        sum += Expectimax(move, currentScore, depth - 1, oppositeMove);
                    }
                    weightenedSum += ((sum / movesCount) * Board2048.CONTS4_PROB);

                    return weightenedSum;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double StaticEvaluationFunction(Board2048 board, int score)
        {
            //"Насыщенность" поля: если весь счет сконцентрирован в одной клеточке - это хорошо
            //Если счет рассредоточен по разным клеточкам - плохо.
            return (double)score / (board.Size * board.Size - board.GetFreeCellsCount());
        }

        private static Move GetOppositeMove(Move move)
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

        private IEnumerable<Board2048> GenerateAllGameMoves(Board2048 original, long tile)
        {
            for (var row = 0; row < original.Size; row++)
            {
                for (var col = 0; col < original.Size; col++)
                {
                    if (original[row, col] <= 0)
                        yield return __MoveMaker.MakeSpecificGameMove(original, row, col, tile);
                }
            }
        }
    }
}