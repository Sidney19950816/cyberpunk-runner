public interface IScoreable
{
    int Score { get; }
    void Add(int score);
    void Reset();
}