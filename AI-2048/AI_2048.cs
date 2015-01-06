﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AI_2048
{
    internal class Ai2048 : IAi2048
    {
        private const int DEPTH = 6;
        private const double PROB_THRESHOLD = 0.0001;

        private readonly IMoveMaker2048 __MoveMaker;
        private readonly ConcurrentDictionary<long, double> __TransposTable = new ConcurrentDictionary<long, double>();

        public Ai2048(IMoveMaker2048 moveMaker)
        {
            __MoveMaker = moveMaker;
        }

        public Direction? CalculateNextMove(Board2048 board2048, long currentScore)
        {
            //TODO:
            //Heuristics: Smoothness, Monotonicity, Empty cells
            //http://stackoverflow.com/questions/22342854/what-is-the-optimal-algorithm-for-the-game-2048/22389702#22389702
            //http://blog.datumbox.com/using-artificial-intelligence-to-solve-the-2048-game-java-code/
            //Iterative deeping, fixed time moves

            __TransposTable.Clear();

            var max = double.NegativeInfinity;
            Direction? bestDir = null;
            var state = new SearchState(currentScore, DEPTH, Move.Player, 1);

            Parallel.ForEach(new[] {Direction.Up, Direction.Down, Direction.Left, Direction.Right},
                             dir =>
                             {
                                 long score;
                                 bool changed;
                                 var moveValue =
                                     Expectimax(
                                         __MoveMaker.MakePlayerMove(board2048,
                                             dir,
                                             out score,
                                             out changed),
                                         state);

                                 if (!changed)
                                     return;

                                 if (moveValue > max)
                                 {
                                     max = moveValue;
                                     bestDir = dir;
                                 }
                             });

            return bestDir;
        }

        private double Expectimax(Board2048 board, SearchState state)
        {
            if (state.CurProb < PROB_THRESHOLD)
                return double.MinValue;

            double weight;
            if (state.Depth <= 0)
            {
                var eval = StaticEvaluationFunction(board, state.CurrentScore);

                if (__TransposTable.TryGetValue(board.Repr, out weight))
                {
                    if (eval > weight)
                        __TransposTable[board.Repr] = eval;
                    else
                        eval = weight;
                }
                else
                {
                    __TransposTable[board.Repr] = eval;
                }

                return eval;
            }

            if (__TransposTable.TryGetValue(board.Repr, out weight))
            {
                return weight;
            }

            var oppositeMove = GetOppositeMove(state.CurMove);
            switch (oppositeMove)
            {
                case Move.Player:
                    var max = double.MinValue;

                    foreach (var dir in new[] {Direction.Up, Direction.Down, Direction.Left, Direction.Right})
                    {
                        long scoreDelta;
                        bool boardChanged;
                        var move = __MoveMaker.MakePlayerMove(board,
                            dir,
                            out scoreDelta,
                            out boardChanged);

                        if (!boardChanged)
                        {
                            continue;
                        }

                        var moveValue = Expectimax(move, new SearchState(state.CurrentScore + scoreDelta, state.Depth - 1, oppositeMove, state.CurProb));
                        if (moveValue > max)
                        {
                            max = moveValue;
                        }
                    }

                    //Если значение осталось double.MinValue, это означает что или 
                    //1. Эта позиция проигрышная (некуда ходить, сработали все continue в цикле)
                    //2. Из этой позиции все ведут ведут в такую же позицию, как эта 
                    //(из этой позиции ВСЕ ходы ведут к неминуемому проигрышку в пределах горизонта видимости)
                    return max;
                case Move.Game:
                    var movesCount = 0;

                    var sum = 0.0;
                    for (var row = 0; row < Board2048.SIZE; row++)
                    {
                        for (var col = 0; col < Board2048.SIZE; col++)
                        {
                            if (board[row, col] > 0) 
                                continue;

                            var move = __MoveMaker.MakeSpecificGameMove(board, row, col, Board2048.CONST2);
                            movesCount++;
                            sum += Expectimax(move,
                                new SearchState(state.CurrentScore, state.Depth - 1, oppositeMove,
                                    state.CurProb * Board2048.CONTS2_PROB));
                        }
                    }

                    var weightenedSum = ((sum / movesCount) * Board2048.CONTS2_PROB);

                    sum = 0.0;
                    movesCount = 0;
                    for (var row = 0; row < Board2048.SIZE; row++)
                    {
                        for (var col = 0; col < Board2048.SIZE; col++)
                        {
                            if (board[row, col] > 0)
                                continue;

                            var move = __MoveMaker.MakeSpecificGameMove(board, row, col, Board2048.CONST4);
                            movesCount++;
                            sum += Expectimax(move,
                                new SearchState(state.CurrentScore + 4, state.Depth - 1, oppositeMove,
                                    state.CurProb * Board2048.CONTS2_PROB));
                        }
                    }

                    weightenedSum += ((sum / movesCount) * Board2048.CONTS4_PROB);

                    return weightenedSum;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static double StaticEvaluationFunction(Board2048 board, long score)
        {
            //"Насыщенность" поля: если весь счет сконцентрирован в одной клеточке - это хорошо
            //Если счет рассредоточен по разным клеточкам - плохо.
            return (double) score / (Board2048.SIZE * Board2048.SIZE - board.GetFreeCellsCount());
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
    }
}