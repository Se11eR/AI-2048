namespace AI_2048
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var moveMaker = new MoveMaker2048();
            var game = new Game2048(moveMaker);//, new Ai2048(moveMaker));
            game.Run();
        }
    }
}
