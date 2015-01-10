using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AI_2048
{
    internal class Ai2048 : IAi2048
    {
        private const double PROB_THRESHOLD = 0.0001;
        private const int MOVE_TIME_MS = 400;

        private readonly IMoveMaker2048 __MoveMaker;
        private readonly ConcurrentDictionary<long, double> __TransposTable = new ConcurrentDictionary<long, double>();
        private readonly double[] __HeurScoreLookup;

        public Ai2048(IMoveMaker2048 moveMaker)
        {
            __MoveMaker = moveMaker;
            Helper.GenerateHeurScoresTable(out __HeurScoreLookup);
        }

        public Direction? CalculateNextMove(Board2048 board2048, long currentScore, out int depth)
        {
            var sw = new Stopwatch();
            var timeSpent = 0L;
            Direction? bestMove = null;
            int iterativeDepth = 2;
            while (timeSpent < MOVE_TIME_MS)
            {
                var ctsts = new CancellationTokenSource();
                ctsts.CancelAfter((int)(MOVE_TIME_MS - timeSpent));
                sw.Restart();
                var move = CalculateNextMoveInternal(board2048, iterativeDepth, currentScore, ctsts.Token);
                sw.Stop();
                if (ctsts.IsCancellationRequested)
                    break;

                bestMove = move;
                iterativeDepth += 2;
                timeSpent += sw.ElapsedMilliseconds;
                ctsts.Dispose();
            }

            depth = iterativeDepth - 2;
            return bestMove;
        }

        private Direction? CalculateNextMoveInternal(Board2048 board2048,
                                                     int depth,
                                                     long currentScore,
                                                     CancellationToken token)
        {
            __TransposTable.Clear();

            var max = double.NegativeInfinity;
            Direction? bestDir = null;
            var state = new SearchState(currentScore, depth, Move.Player, 1, token);

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
            if (state.Token.IsCancellationRequested)
                return 0;

            if (state.CurProb < PROB_THRESHOLD)
                return double.MinValue;

            double weight;
            if (state.Depth <= 0)
            {
                var eval = StaticEvaluationFunction(board, state.CurrentScore);

                if (state.Token.IsCancellationRequested)
                    return 0;

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

            if (state.Token.IsCancellationRequested)
                return 0;

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
                        if (state.Token.IsCancellationRequested)
                            return 0;

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

                        var moveValue = Expectimax(move,
                                                   new SearchState(state.CurrentScore + scoreDelta,
                                                                   state.Depth - 1,
                                                                   oppositeMove,
                                                                   state.CurProb,
                                                                   state.Token));
                        if (state.Token.IsCancellationRequested)
                            return 0;

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
                    var lost = true;
                    for (var row = 0; row < Board2048.SIZE; row++)
                    {
                        for (var col = 0; col < Board2048.SIZE; col++)
                        {
                            if (state.Token.IsCancellationRequested)
                                return 0;

                            if (board[row, col] > 0) 
                                continue;

                            lost = false;
                            var move = __MoveMaker.MakeSpecificGameMove(board, row, col, Board2048.CONST2);
                            movesCount++;
                            sum += Expectimax(move,
                                new SearchState(state.CurrentScore, state.Depth - 1, oppositeMove,
                                    state.CurProb * Board2048.CONTS2_PROB, state.Token));
                        }
                    }

                    if (lost)
                        return double.MinValue;

                    var weightenedSum = ((sum / movesCount) * Board2048.CONTS2_PROB);

                    sum = 0.0;
                    movesCount = 0;
                    for (var row = 0; row < Board2048.SIZE; row++)
                    {
                        for (var col = 0; col < Board2048.SIZE; col++)
                        {
                            if (state.Token.IsCancellationRequested)
                                return 0;

                            if (board[row, col] > 0)
                                continue;

                            var move = __MoveMaker.MakeSpecificGameMove(board, row, col, Board2048.CONST4);
                            movesCount++;
                            sum += Expectimax(move,
                                new SearchState(state.CurrentScore + 4, state.Depth - 1, oppositeMove,
                                    state.CurProb * Board2048.CONTS2_PROB, state.Token));
                        }
                    }

                    weightenedSum += ((sum / movesCount) * Board2048.CONTS4_PROB);

                    return weightenedSum;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double StaticEvaluationFunction(Board2048 board, long score)
        {
            //"Насыщенность" поля: если весь счет сконцентрирован в одной клеточке - это хорошо
            //Если счет рассредоточен по разным клеточкам - плохо.
            //return (double) score / (Board2048.SIZE * Board2048.SIZE - board.GetFreeCellsCount());

            var value = 0.0;
            value += __HeurScoreLookup[board.ExtractChunkBlock(0)];
            value += __HeurScoreLookup[board.ExtractChunkBlock(1)];
            value += __HeurScoreLookup[board.ExtractChunkBlock(2)];
            value += __HeurScoreLookup[board.ExtractChunkBlock(3)];
            board.Transpose();
            value += __HeurScoreLookup[board.ExtractChunkBlock(0)];
            value += __HeurScoreLookup[board.ExtractChunkBlock(1)];
            value += __HeurScoreLookup[board.ExtractChunkBlock(2)];
            value += __HeurScoreLookup[board.ExtractChunkBlock(3)];

            return value;
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