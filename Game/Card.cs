namespace SpiderBlazor.Game;

public enum Suit { Spades, Hearts, Clubs, Diamonds }

// Rank: 1 = Ace, 13 = King
public sealed record Card(int Id, Suit Suit, int Rank, bool FaceUp);

public readonly record struct HintMove(int From, int Index, int To);

public static class CardText
{
    public static string Label(int rank) => rank switch
    {
        1 => "A",
        11 => "J",
        12 => "Q",
        13 => "K",
        _ => rank.ToString()
    };

    public static string Symbol(Suit suit) => suit switch
    {
        Suit.Spades => "♠",
        Suit.Hearts => "♥",
        Suit.Clubs => "♣",
        _ => "♦"
    };

    public static bool IsRed(Suit suit) => suit is Suit.Hearts or Suit.Diamonds;
}
