using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int highScore;

    public SerializableDictionary<string, int> currencies;

    // The values defined in this constructor will be the default values if there's no data
    public GameData()
    {
        this.highScore = 0;
        currencies = new SerializableDictionary<string, int>();
    }


}
