using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_2048
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var game = new Game2048(new Ai2048(), new Ai2048());
            game.Run();
        }
    }
}
