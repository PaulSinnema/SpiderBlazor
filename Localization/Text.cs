namespace SpiderBlazor.Localization;

public enum Lang { En, Nl, De, Fr }

public static class Text
{
    // Order per entry: En, Nl, De, Fr
    private static readonly Dictionary<string, string[]> Map = new()
    {
        ["New"] = ["New", "Nieuw", "Neu", "Nouveau"],
        ["Hint"] = ["Hint", "Hint", "Tipp", "Indice"],
        ["Undo"] = ["Undo", "Ongedaan", "Rückgängig", "Annuler"],
        ["Settings"] = ["Settings", "Instellingen", "Einstellungen", "Paramètres"],
        ["Score"] = ["Score", "Score", "Punkte", "Score"],
        ["Moves"] = ["Moves", "Zetten", "Züge", "Coups"],
        ["FillEmpty"] = ["Fill all empty columns first", "Vul eerst alle lege kolommen", "Fülle zuerst alle leeren Spalten", "Remplissez d'abord les colonnes vides"],
        ["NoMovesDeal"] = ["No moves — deal new cards", "Geen zetten — deel nieuwe kaarten", "Keine Züge — neue Karten geben", "Aucun coup — distribuez de nouvelles cartes"],
        ["NoMoves"] = ["No moves left", "Geen zetten meer", "Keine Züge mehr", "Plus aucun coup possible"],
        ["Difficulty"] = ["Difficulty", "Moeilijkheid", "Schwierigkeit", "Difficulté"],
        ["Easy"] = ["Easy", "Makkelijk", "Leicht", "Facile"],
        ["Medium"] = ["Medium", "Gemiddeld", "Mittel", "Moyen"],
        ["Hard"] = ["Hard", "Moeilijk", "Schwer", "Difficile"],
        ["Suit1"] = ["1 suit", "1 kleur", "1 Farbe", "1 couleur"],
        ["Suit2"] = ["2 suits", "2 kleuren", "2 Farben", "2 couleurs"],
        ["Suit4"] = ["4 suits", "4 kleuren", "4 Farben", "4 couleurs"],
        ["Won"] = ["won", "gewonnen", "gewonnen", "gagnées"],
        ["NextGame"] = ["Applies from the next game", "Geldt vanaf het volgende spel", "Gilt ab dem nächsten Spiel", "S'applique à la prochaine partie"],
        ["StartNow"] = ["Start new game now", "Nu nieuw spel starten", "Jetzt neues Spiel starten", "Nouvelle partie maintenant"],
        ["Language"] = ["Language", "Taal", "Sprache", "Langue"],
        ["Restart"] = ["Restart", "Opnieuw", "Neu starten", "Recommencer"],
        ["RestartQ"] = ["Replay this deal from the start?", "Dit spel opnieuw vanaf het begin spelen?", "Dieses Spiel von vorne beginnen?", "Rejouer cette donne depuis le début ?"],
        ["Update"] = ["Update", "Bijwerken", "Aktualisieren", "Mettre à jour"],
        ["UpdateHint"] = ["Get the latest version", "Nieuwste versie ophalen", "Neueste Version laden", "Dernière version"],
        ["Updating"] = ["Updating…", "Bijwerken…", "Aktualisiere…", "Mise à jour…"],
        ["Offline"] = ["No internet connection", "Geen internetverbinding", "Keine Internetverbindung", "Pas de connexion Internet"],
        ["Display"] = ["Display", "Weergave", "Anzeige", "Affichage"],
        ["HighlightMovable"] = ["Highlight movable cards", "Verplaatsbare kaarten markeren", "Bewegliche Karten hervorheben", "Cartes déplaçables en évidence"],
        ["On"] = ["On", "Aan", "Ein", "Activé"],
        ["Off"] = ["Off", "Uit", "Aus", "Désactivé"],
        ["Close"] = ["Close", "Sluiten", "Schließen", "Fermer"],
        ["NewGame"] = ["New game", "Nieuw spel", "Neues Spiel", "Nouvelle partie"],
        ["StartNewQ"] = ["Start a new game?", "Nieuw spel starten?", "Neues Spiel starten?", "Commencer une nouvelle partie ?"],
        ["Start"] = ["Start", "Starten", "Starten", "Commencer"],
        ["Cancel"] = ["Cancel", "Annuleren", "Abbrechen", "Annuler"],
        ["YouWon"] = ["You won! 🎉", "Gewonnen! 🎉", "Gewonnen! 🎉", "Gagné ! 🎉"]
    };

    // Ace, Jack, Queen, King per language
    private static readonly string[][] Ranks =
    [
        ["A", "J", "Q", "K"],
        ["A", "B", "V", "H"],
        ["A", "B", "D", "K"],
        ["A", "V", "D", "R"]
    ];

    public static readonly Lang[] All = [Lang.En, Lang.Nl, Lang.De, Lang.Fr];

    public static string Get(Lang lang, string key) =>
        Map.TryGetValue(key, out var v) ? v[(int)lang] : key;

    public static string RankLabel(Lang lang, int rank) => rank switch
    {
        1 => Ranks[(int)lang][0],
        11 => Ranks[(int)lang][1],
        12 => Ranks[(int)lang][2],
        13 => Ranks[(int)lang][3],
        _ => rank.ToString()
    };

    public static string Name(Lang lang) => lang switch
    {
        Lang.En => "English",
        Lang.Nl => "Nederlands",
        Lang.De => "Deutsch",
        _ => "Français"
    };

    public static string Code(Lang lang) => lang.ToString().ToLowerInvariant();

    public static Lang FromCode(string? code) => code?.ToLowerInvariant() switch
    {
        var c when c is not null && c.StartsWith("nl") => Lang.Nl,
        var c when c is not null && c.StartsWith("de") => Lang.De,
        var c when c is not null && c.StartsWith("fr") => Lang.Fr,
        _ => Lang.En
    };
}
