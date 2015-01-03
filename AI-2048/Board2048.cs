using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AI_2048
{
    internal struct Board2048 : IEquatable<Board2048>
    {
        public const long CONST2 = 1;
        public const long CONST4 = 2;
        public const double CONTS2_PROB = 0.9;
        public const double CONTS4_PROB = 0.1;

        internal class Board2048EqualityComparer : IEqualityComparer<Board2048>
        {
            public bool Equals(Board2048 x, Board2048 y)
            {
                return x.__Repr == y.__Repr;
            }

            public int GetHashCode(Board2048 obj)
            {
                unchecked
                {
                    var hash = 23;
                    hash = hash * 31 + (int)obj.__Repr;
                    hash = hash * 31 + (int)(obj.__Repr << 32);

                    return hash;
                }
            }
        }

        private long __Repr;
        private long __Score;
        private bool __IsMovePossible;

        public long this[int row, int col]
        {
            get
            {
                return (__Repr >> (row*16 + col*4)) & (0xF); 
            }
            set
            {
                __Repr = (~(0xFL << (row*16 + col*4)) & __Repr) | (value << (row*16 + col*4));
            }
        }

        public int GetFreeCellsCount()
        {
            var count = 0;
            var allCells = Rows * Cols;
            for (var i = 0; i < allCells; i++)
            {
                if (((__Repr >> (i * 4)) & 0xF) == 0)
                    count++;
            }

            return count;
        }

        public int Rows
        {
            get
            {
                return 4;
            }
        }

        public int Cols
        {
            get
            {
                return 4;
            }
        }

        public long Score
        {
            get { return __Score; }
            set { __Score = value; }
        }

        public bool IsMovePossible
        {
            get
            {
                return __IsMovePossible; 
            }
            set { __IsMovePossible = value; }
        }

        public bool Equals(Board2048 other)
        {
            return __Repr == other.__Repr;
        }

        public override string ToString()
        {
            var s = new StringWriter();

            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    var val = this[row, col];
                    var value = (ulong)(val > 0 ? (1 << (int)val) : 0);
                    s.Write("{0,6}", value);
                }

                s.WriteLine();
                s.WriteLine();
            }

            return s.ToString();
        }
    }
}
