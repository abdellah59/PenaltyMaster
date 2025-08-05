using Godot;
using System.Collections.Generic;

public partial class GameData : Node
{
    public static GameData Instance { get; private set; }
    
    public Team PlayerTeam { get; set; }
    public Team OpponentTeam { get; set; }
    public int PlayerScore { get; set; }
    public int OpponentScore { get; set; }
    public int CurrentRound { get; set; } = 1;
    public bool PlayerTurn { get; set; } = true;
    public int MaxRounds { get; set; } = 5;

    private List<Team> availableTeams;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTeams();
        }
        else
        {
            QueueFree();
        }
    }

    private void InitializeTeams()
    {
        availableTeams = new List<Team>
        {
            new Team("France", "France", new Color(0.0f, 0.2f, 0.6f), Colors.White, 1),
            new Team("Brésil", "Brazil", new Color(1.0f, 0.8f, 0.0f), new Color(0.0f, 0.4f, 0.0f), 2),
            new Team("Allemagne", "Germany", Colors.Black, Colors.White, 3),
            new Team("Espagne", "Spain", new Color(0.8f, 0.0f, 0.0f), new Color(1.0f, 0.8f, 0.0f), 4),
            new Team("Italie", "Italy", new Color(0.0f, 0.3f, 0.6f), Colors.White, 5),
            new Team("Argentine", "Argentina", new Color(0.4f, 0.7f, 1.0f), Colors.White, 6),
            new Team("Angleterre", "England", Colors.White, new Color(0.8f, 0.0f, 0.0f), 7),
            new Team("Portugal", "Portugal", new Color(0.0f, 0.4f, 0.0f), new Color(0.8f, 0.0f, 0.0f), 8),
            new Team("Maroc", "Morocco", new Color(0.8f, 0.0f, 0.0f), new Color(0.0f, 0.5f, 0.0f), 9),
            new Team("Algérie", "Algeria", new Color(0.0f, 0.6f, 0.3f), Colors.White, 10),
            new Team("Tunisie", "Tunisia", new Color(0.9f, 0.1f, 0.1f), Colors.White, 11)
        };
    }

    public List<Team> GetAvailableTeams()
    {
        return new List<Team>(availableTeams);
    }

    public void ResetGame()
    {
        PlayerScore = 0;
        OpponentScore = 0;
        CurrentRound = 1;
        PlayerTurn = true;
    }
}
