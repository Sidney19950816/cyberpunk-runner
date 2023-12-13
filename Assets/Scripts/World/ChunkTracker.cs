public static class ChunkTracker
{
    private static int _passedChunksCount;

    public static void IncrementPassedChunks()
    {
        _passedChunksCount++;
    }

    public static void ResetChunksCount()
    {
        _passedChunksCount = 0;
    }

    public static int GetPassedChunksCount()
    {
        return _passedChunksCount;
    }
}
