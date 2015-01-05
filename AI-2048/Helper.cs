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

                int pivot = 0, elem = pivot + 1;
                while (elem < entryLengthInChunks)
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
                        tableScores[i] += (sbyte)(chunk + 1);
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

        public static void GenerateLookupScoresReverseTable(int entryLengthInChunks, out ushort[] lookup, out sbyte[] scores)
        {
            //Направление роста индекса чанков по направлению свайпа

            //[CHUNK1] [CHUNK2] [CHUNK3] [CHUNK4]
            //SWIPE ======================> SWIPE

            var tableLookup = new ushort[65536];
            var tableScores = new sbyte[65536];

            for (int i = 0; i < tableLookup.Length; i++)
            {
                int entry = i;

                int pivot = entryLengthInChunks - 1, row = pivot - 1;
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
                        tableScores[i] += (sbyte)(chunk + 1);
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
