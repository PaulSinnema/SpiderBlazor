namespace SpiderBlazor.Game;

public sealed class SpiderGame
{
    private readonly List<GameState> history = [];

    public GameState State { get; private set; } = new();
    public HintMove? Hint { get; set; }

    public List<List<Card>> Columns => State.Columns;
    public bool IsWon => State.Completed.Count == 8;
    public bool CanUndo => history.Count > 0;
    public GameState? Previous => history.Count > 0 ? history[^1] : null;
    public bool CanDeal => State.Stock.Count > 0 && Columns.All(c => c.Count > 0);
    public int DealsLeft => State.Stock.Count / 10;

    public SpiderGame() => NewGame(4);

    // MARK: Setup

    public void NewGame(int suits)
    {
        var copies = 8 / suits;
        var deck = new List<Card>(104);
        var id = 0;
        for (var s = 0; s < suits; s++)
            for (var k = 0; k < copies; k++)
                for (var rank = 1; rank <= 13; rank++)
                    deck.Add(new Card(id++, (Suit)s, rank, false));

        var cards = deck.ToArray();
        Random.Shared.Shuffle(cards);

        // 4 columns of 6, 6 columns of 5; top card face up
        var cols = new List<List<Card>>(10);
        var pos = 0;
        for (var c = 0; c < 10; c++)
        {
            var n = c < 4 ? 6 : 5;
            var col = cards.Skip(pos).Take(n).ToList();
            pos += n;
            col[^1] = col[^1] with { FaceUp = true };
            cols.Add(col);
        }

        State = new GameState { Columns = cols, Stock = cards.Skip(pos).ToList(), SuitCount = suits };
        State.Start = State.Clone();
        history.Clear();
        Hint = null;
    }

    public bool CanRestart => State.Start is not null;

    // Replay the same deal from the beginning
    public bool Restart()
    {
        if (State.Start is not { } start) return false;
        State = start.Clone();
        State.Start = start;
        history.Clear();
        Hint = null;
        return true;
    }

    public void Restore(GameState state)
    {
        State = state;
        history.Clear();
        Hint = null;
    }

    // MARK: Rules

    public bool CanPickUp(int col, int index)
    {
        if (col < 0 || col > 9) return false;
        var c = Columns[col];
        if (index < 0 || index >= c.Count || !c[index].FaceUp) return false;
        for (var i = index; i < c.Count - 1; i++)
            if (c[i + 1].Suit != c[i].Suit || c[i + 1].Rank != c[i].Rank - 1) return false;
        return true;
    }

    public bool CanDrop(Card card, int col)
    {
        var c = Columns[col];
        if (c.Count == 0) return true;
        var top = c[^1];
        return top.FaceUp && top.Rank == card.Rank + 1;
    }

    // MARK: Actions

    public bool Move(int from, int index, int to)
    {
        if (from == to || to < 0 || to > 9 || !CanPickUp(from, index) || !CanDrop(Columns[from][index], to)) return false;
        SaveSnapshot();
        var run = Columns[from].GetRange(index, Columns[from].Count - index);
        Columns[from].RemoveRange(index, run.Count);
        Columns[to].AddRange(run);
        FlipTop(from);
        State.Moves++;
        State.Score--;
        CollectRun(to);
        Hint = null;
        return true;
    }

    public bool Tap(int col, int index) =>
        CanPickUp(col, index) && BestDestination(col, index) is int dest && Move(col, index, dest);

    // Returns ids of dealt cards, in column order
    public List<int> Deal()
    {
        var ids = new List<int>(10);
        if (!CanDeal) return ids;
        SaveSnapshot();
        for (var c = 0; c < 10; c++)
        {
            var card = State.Stock[^1] with { FaceUp = true };
            State.Stock.RemoveAt(State.Stock.Count - 1);
            Columns[c].Add(card);
            ids.Add(card.Id);
        }
        State.Moves++;
        for (var c = 0; c < 10; c++) CollectRun(c);
        Hint = null;
        return ids;
    }

    public void Undo()
    {
        if (history.Count == 0) return;
        State = history[^1];
        history.RemoveAt(history.Count - 1);
        Hint = null;
    }

    // Prefer same-suit target, then any match, then empty column
    public int? BestDestination(int col, int index)
    {
        var card = Columns[col][index];
        var others = Enumerable.Range(1, 9).Select(i => (col + i) % 10).ToList();

        foreach (var c in others)
            if (Columns[c].Count > 0 && Columns[c][^1] is { FaceUp: true } top && top.Suit == card.Suit && top.Rank == card.Rank + 1)
                return c;

        foreach (var c in others)
            if (Columns[c].Count > 0 && CanDrop(card, c))
                return c;

        if (index > 0)
            foreach (var c in others)
                if (Columns[c].Count == 0)
                    return c;

        return null;
    }

    public void FindHint()
    {
        Hint = null;
        HintMove? fallback = null;
        HintMove? toEmpty = null;

        for (var from = 0; from < 10; from++)
        {
            var c = Columns[from];
            for (var index = 0; index < c.Count; index++)
            {
                if (!CanPickUp(from, index)) continue;
                var card = c[index];
                var parent = index > 0 ? c[index - 1] : null;

                // Skip runs already resting on a same-suit parent
                if (parent is { FaceUp: true } && parent.Suit == card.Suit && parent.Rank == card.Rank + 1) continue;

                for (var to = 0; to < 10; to++)
                {
                    if (to == from) continue;
                    if (Columns[to].Count == 0)
                    {
                        if (index > 0) toEmpty ??= new HintMove(from, index, to);
                        continue;
                    }
                    if (!CanDrop(card, to)) continue;

                    var move = new HintMove(from, index, to);
                    if (Columns[to][^1].Suit == card.Suit) { Hint = move; return; }

                    // Off-suit move only useful if it doesn't just swap parents
                    if (fallback is null && (parent is null || !parent.FaceUp || parent.Rank != card.Rank + 1))
                        fallback = move;
                }
            }
        }
        Hint = fallback ?? toEmpty;
    }

    // MARK: Helpers

    private void SaveSnapshot() => history.Add(State.Clone());

    private void FlipTop(int col)
    {
        var c = Columns[col];
        if (c.Count > 0 && !c[^1].FaceUp) c[^1] = c[^1] with { FaceUp = true };
    }

    // Remove complete K..A same-suit run
    private void CollectRun(int col)
    {
        var c = Columns[col];
        if (c.Count < 13) return;
        var start = c.Count - 13;
        var first = c[start];
        if (first.Rank != 13) return;
        for (var i = start; i < c.Count; i++)
            if (!c[i].FaceUp || c[i].Suit != first.Suit || c[i].Rank != 13 - (i - start)) return;

        c.RemoveRange(start, 13);
        State.Completed.Add(first.Suit);
        State.Score += 100;
        FlipTop(col);
    }
}
