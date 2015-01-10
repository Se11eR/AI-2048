namespace AI_2048
{
    interface IAi2048
    {
        Direction? CalculateNextMove(Board2048 board2048, long currentScore, out int depth);
    }
}
