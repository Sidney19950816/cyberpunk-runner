using System;
using UnityEngine;

public class PlayerScore : IScoreable
{
    private int _score;

    public int Score => _score;

    public Action<int> OnUpdate;

    public PlayerScore(int initialScore = 0)
    {
        _score = initialScore;
    }

    public void Add(int score)
    {
        _score += score;
        OnUpdate?.Invoke(_score);
    }

    public void Reset()
    {
        _score = 0;
        OnUpdate?.Invoke(_score);
    }
}
