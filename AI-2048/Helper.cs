using System;
using System.Collections.Generic;
using System.Linq;

namespace AI_2048
{
    public static class Helper
    {
        public static long ReverseChunksInFirst2Bytes(long entry)
        {
            return ((entry & 0xF) << 12) | ((entry & 0xF000) >> 12) | ((entry & 0xF0) << 4)
                   | ((entry & 0xF00) >> 4);
        }

        private static int GetChunk(int entry, int i)
        {
            return (entry >> (i * 4)) & (0xF); 
        }

        public static long GetChunk(long entry, int i)
        {
            return (entry >> (i * 4)) & (0xFL);
        }

        private static int SetChunk(int entry, int chunk, int i)
        {
            return (~(0xF << (i * 4)) & entry) | (chunk << (i * 4));
        }

        public static long SetChunk(long entry, long chunk, int i)
        {
            return (~(0xFL << (i * 4)) & entry) | (chunk << (i * 4));
        }

        public static void GenerateLookupScoresTable(int entryLengthInChunks, out ushort[] lookup, out sbyte[] scores)
        {
            //Направление роста индекса чанков противоположно направлению свайпа

            //[CHUNK1] [CHUNK2] [CHUNK3] [CHUNK4]
            //SWIPE <====================== SWIPE

            //Комбинация техник из
            ////http://stackoverflow.com/a/22498940
            ////http://www.cyberforum.ru/csharp-net/thread1172757.html
            var tableLookup = new ushort[65536];
            var tableScores = new sbyte[65536];

            for (int i = 0; i < tableLookup.Length; i++)
            {
                int entry = i;

                int pivot = 0, col = pivot + 1;
                while (col < entryLengthInChunks)
                {
                    if (GetChunk(entry, col) == 0)
                        col++;
                    else if (GetChunk(entry, pivot) == 0)
                    {
                        entry = SetChunk(entry, GetChunk(entry, col), pivot);
                        entry = SetChunk(entry, 0, col++);
                    }
                    else if (GetChunk(entry, pivot) == GetChunk(entry, col))
                    {
                        var chunk = GetChunk(entry, pivot);

                        entry = SetChunk(entry, chunk + 1, pivot);
                        tableScores[i] += (sbyte)(chunk + 1);
                        pivot++;

                        entry = SetChunk(entry, 0, col);
                        col++;
                    }
                    else if (++pivot == col)
                        col++;
                }

                tableLookup[i] = (ushort)entry;
            }

            lookup = tableLookup;
            scores = tableScores;
        }
    }
}
