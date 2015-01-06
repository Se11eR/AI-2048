using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_2048
{
    internal struct SearchState
    {
        public long CurrentScore;
        public int Depth;
        public Move CurMove;
        public double CurProb;

        public SearchState(long score, int depth, Move move, double prob)
        {
            CurrentScore = score;
            Depth = depth;
            CurMove = move;
            CurProb = prob;
        }
    }
}
