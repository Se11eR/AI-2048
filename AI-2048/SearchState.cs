using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AI_2048
{
    internal struct SearchState
    {
        public long CurrentScore;
        public int Depth;
        public Move CurMove;
        public double CurProb;
        public CancellationToken Token;

        public SearchState(long score, int depth, Move move, double prob, CancellationToken token)
        {
            CurrentScore = score;
            Depth = depth;
            CurMove = move;
            CurProb = prob;
            Token = token;
        }
    }
}
