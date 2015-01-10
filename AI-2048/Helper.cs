using System;
using System.Collections.Generic;
namespace AI_2048
{
    public static class Helper
    {
        private static int GetChunk(int entry, int i)
        {
            return (entry >> (i * 4)) & (0xF); 
        }

        private static int SetChunk(int entry, int chunk, int i)
        {
            return (~(0xF << (i * 4)) & entry) | (chunk << (i * 4));
        }

        private static int[] GetSingleBlock(int entry)
        {
            var block = new int[4];

            for (var i = 0; i < 4; i++)
            {
                block[i] = GetChunk(entry, i);
            }

            return block;
        }

        public static void GenerateHeurScoresTable(out double[] heur)
        {
            heur = new double[65536];

            for (var i = 0; i < 65536; i++)
            {
                var block = GetSingleBlock(i);

                var monIncrease = 0.0;
                var monDecrease = 0.0;
                var empty = 0;
                var merges = 0;
                var sumAll = 0.0;

                int prev = 0;
                int counter = 0;
                for (int j = 0; j < 4; ++j)
                {
                    int rank = block[j];
                    sumAll += Math.Pow(rank, 3.5);
                    if (rank == 0)
                        empty++;
                    else
                    {
                        if (prev == rank)
                        {
                            counter++;
                        }
                        else if (counter > 0)
                        {
                            merges += 1 + counter;
                            counter = 0;
                        }
                        prev = rank;
                    }
                }
                if (counter > 0)
                {
                    merges += 1 + counter;
                }

                for (int j = 1; j < block.Length; j++)
                {
                    if (block[j] > block[j - 1])
                    {
                        monIncrease += Math.Pow(block[j], 4) - Math.Pow(block[j - 1], 4);
                    }
                    else if (block[j] < block[j - 1])
                    {
                        monDecrease += Math.Pow(block[j - 1], 4) - Math.Pow(block[j], 4);
                    }
                }

                const double SCORE_EMPTY_WEIGHT = 270.0;
                const double SCORE_MERGES_WEIGHT = 700.0;
                const double SCORE_SUM_WEIGHT = 11.0;
                const double SCORE_MONOTONICITY_WEIGHT = 47;
                heur[i] =   SCORE_EMPTY_WEIGHT * empty 
                          + SCORE_MERGES_WEIGHT * merges
                          - SCORE_SUM_WEIGHT * sumAll
                          - SCORE_MONOTONICITY_WEIGHT * Math.Min(monIncrease, monDecrease);

            }
        }

        public static void GenerateLookupScoresTable(out ushort[] lookup, out uint[] scores)
        {
            //Направление роста индекса чанков противоположно направлению свайпа

            //[CHUNK1] [CHUNK2] [CHUNK3] [CHUNK4]
            //SWIPE <====================== SWIPE

            //Комбинация техник из
            ////http://stackoverflow.com/a/22498940
            ////http://www.cyberforum.ru/csharp-net/thread1172757.html
            var tableLookup = new ushort[65536];
            var tableScores = new uint[65536];

            for (int i = 0; i < tableLookup.Length; i++)
            {
                int entry = i;

                int pivot = 0, elem = pivot + 1;
                while (elem < 4)
                {
                    if (GetChunk(entry, elem) == 0)
                        elem++;
                    else if (GetChunk(entry, pivot) == 0)
                    {
                        entry = SetChunk(entry, GetChunk(entry, elem), pivot);
                        entry = SetChunk(entry, 0, elem++);
                    }
                    else if (GetChunk(entry, pivot) == GetChunk(entry, elem))
                    {
                        var chunk = GetChunk(entry, pivot);

                        entry = SetChunk(entry, chunk + 1, pivot);
                        tableScores[i] += (uint)(1 << (chunk + 1));
                        pivot++;

                        entry = SetChunk(entry, 0, elem);
                        elem++;
                    }
                    else if (++pivot == elem)
                        elem++;
                }

                tableLookup[i] = (ushort)entry;
            }

            lookup = tableLookup;
            scores = tableScores;
        }

        public static void GenerateLookupScoresReverseTable(out ushort[] lookup, out uint[] scores)
        {
            //Направление роста индекса чанков по направлению свайпа

            //[CHUNK1] [CHUNK2] [CHUNK3] [CHUNK4]
            //SWIPE ======================> SWIPE

            var tableLookup = new ushort[65536];
            var tableScores = new uint[65536];

            for (int i = 0; i < tableLookup.Length; i++)
            {
                int entry = i;

                int pivot = 4 - 1, row = pivot - 1;
                while (row >= 0)
                {
                    if (GetChunk(entry, row) == 0)
                        row--;
                    else if (GetChunk(entry, pivot) == 0)
                    {
                        entry = SetChunk(entry, GetChunk(entry, row), pivot);
                        entry = SetChunk(entry, 0, row--);
                    }
                    else if (GetChunk(entry, pivot) == GetChunk(entry, row))
                    {
                        var chunk = GetChunk(entry, pivot);

                        entry = SetChunk(entry, chunk + 1, pivot);
                        tableScores[i] += (uint)(1 << (chunk + 1));
                        pivot--;

                        entry = SetChunk(entry, 0, row);
                        row--;
                    }
                    else if (--pivot == row)
                        row--;
                }

                tableLookup[i] = (ushort)entry;
            }

            lookup = tableLookup;
            scores = tableScores;
        }
    }
}
