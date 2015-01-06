using System;

namespace AI_2048
{
    internal class Game2048
    {
        private readonly IMoveMaker2048 __MoveMaker;
        private readonly IAi2048 __Ai;

        private Board2048 __Board;
        private long __Score;

        public Game2048(IMoveMaker2048 moveMaker, IAi2048 ai = null)
        {
            __MoveMaker = moveMaker;
            __Ai = ai;
            __Score = 0;
            __Board = new Board2048();
            __Board = __MoveMaker.MakeGameMove(__Board, out __Score);
        }

        public void Run()
        {
            bool boardChanged = true;
            bool skip = false;
            do
            {
                if (__MoveMaker.IsGameOver(__Board))
                    break;

                if (boardChanged && !skip)
                {
                    long score;
                    __Board = __MoveMaker.MakeGameMove(__Board, out score);
                    __Score += score;
                }

                Display();

                Console.WriteLine("Use arrow keys to move the tiles. Press Ctrl-C to exit.");
                if (__Ai == null)
                {
                    ConsoleKeyInfo input = Console.ReadKey(true); // BLOCKING TO WAIT FOR INPUT
                    Console.WriteLine(input.Key.ToString());
                    switch (input.Key)
                    {
                        case ConsoleKey.UpArrow:
                            boardChanged = Update(Direction.Up);
                            skip = false;
                            break;

                        case ConsoleKey.DownArrow:
                            boardChanged = Update(Direction.Down);
                            skip = false;
                            break;

                        case ConsoleKey.LeftArrow:
                            boardChanged = Update(Direction.Left);
                            skip = false;
                            break;

                        case ConsoleKey.RightArrow:
                            boardChanged = Update(Direction.Right);
                            skip = false;
                            break;

                        default:
                            skip = true;
                            break;
                    }
                }
                else
                {
                    var dir = __Ai.CalculateNextMove(__Board, __Score);
                    if (dir == null)
                        break;
                    boardChanged = Update(dir.Value);
                }
            }
            while (true); // use CTRL-C to break out of loop

            using (new ColorOutput(ConsoleColor.Red))
            {
                Console.WriteLine("YOU ARE DEAD!!!");
            }

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }

        private static ConsoleColor GetNumberColor(ulong num)
        {
            switch (num)
            {
                case 0:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.Cyan;
                case 4:
                    return ConsoleColor.Magenta;
                case 8:
                    return ConsoleColor.Red;
                case 16:
                    return ConsoleColor.Green;
                case 32:
                    return ConsoleColor.Yellow;
                case 64:
                    return ConsoleColor.Yellow;
                case 128:
                    return ConsoleColor.DarkCyan;
                case 256:
                    return ConsoleColor.Cyan;
                case 512:
                    return ConsoleColor.DarkMagenta;
                case 1024:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.Red;
            }
        }

        private bool Update(Direction dir)
        {
            long score;
            bool boardChanged;
            __Board = __MoveMaker.MakePlayerMove(__Board, dir, out score, out boardChanged);
            __Score += score;
            return boardChanged;
        }

        private void Display()
        {
            Console.Clear();
            Console.WriteLine();
            for (var row = 0; row < Board2048.SIZE; row++)
            {
                for (var col = 0; col < Board2048.SIZE; col++)
                {
                    var val = __Board[row, col];
                    var value = (ulong)(val > 0 ? (1 << (int)val) : 0);
                    using (new ColorOutput(GetNumberColor(value)))
                    {
                        Console.Write("{0,6}", value);
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
            }

            Console.WriteLine("Score: {0}", __Score);
            Console.WriteLine();
        }

        #region Utility Classes
        class ColorOutput : IDisposable
        {
            public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
            }

            public void Dispose()
            {
                Console.ResetColor();
            }
        }
        #endregion Utility Classes
    }
}
