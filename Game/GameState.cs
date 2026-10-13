using System.Text.Json.Serialization;

namespace SpiderBlazor.Game;

public sealed class GameState
{
    public List<List<Card>> Columns { get; set; } = [];
    public List<Card> Stock { get; set; } = [];
    public List<Suit> Completed { get; set; } = [];
    public int Moves { get; set; }
    public int Score { get; set; } = 500;
    public int SuitCount { get; set; } = 1;

    // Cards are immutable records, so copying the lists is enough
    public GameState Clone() => new()
    {
        Columns = Columns.Select(c => c.ToList()).ToList(),
        Stock = Stock.ToList(),
        Completed = Completed.ToList(),
        Moves = Moves,
        Score = Score,
        SuitCount = SuitCount
    };

    public bool IsValid() =>
        Columns.Count == 10
        && SuitCount is 1 or 2 or 4
        && Columns.Sum(c => c.Count) + Stock.Count + Completed.Count * 13 == 104;
}

// Source-generated JSON, safe with WASM trimming
[JsonSerializable(typeof(GameState))]
[JsonSerializable(typeof(int[]))]
internal partial class SpiderJson : JsonSerializerContext
{
}
