using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    
    [Signal] public delegate void GameStateChangedEventHandler(GameState newState);
    [Signal] public delegate void RoundCompletedEventHandler();
    [Signal] public delegate void GameFinishedEventHandler(Team winner);

    public enum GameState
    {
        TeamSelection,
        PlayerShooting,
        OpponentShooting,
        GoalkeepingPlayerTurn,
        GoalkeepingOpponentTurn,
        RoundEnd,
        GameEnd
    }

    private GameState currentState = GameState.TeamSelection;
    public GameState CurrentState => currentState;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        EmitSignal(SignalName.GameStateChanged, (int)newState);
        
        GD.Print($"État du jeu changé: {newState}");
    }

    public void StartPenaltySession()
    {
        GameData.Instance.ResetGame();
        ChangeState(GameState.PlayerShooting);
    }

    public void CompleteShot(bool scored)
    {
        var gameData = GameData.Instance;
        
        if (currentState == GameState.PlayerShooting)
        {
            if (scored) gameData.PlayerScore++;
            ChangeState(GameState.OpponentShooting);
        }
        else if (currentState == GameState.OpponentShooting)
        {
            if (scored) gameData.OpponentScore++;
            
            gameData.CurrentRound++;
            
            if (ShouldEndGame())
            {
                EndGame();
            }
            else if (gameData.CurrentRound > gameData.MaxRounds)
            {
                // Mort subite
                gameData.MaxRounds++;
                ChangeState(GameState.PlayerShooting);
            }
            else
            {
                ChangeState(GameState.PlayerShooting);
            }
        }
    }

    private bool ShouldEndGame()
    {
        var gameData = GameData.Instance;
        int remainingRounds = gameData.MaxRounds - gameData.CurrentRound + 1;
        int scoreDifference = Mathf.Abs(gameData.PlayerScore - gameData.OpponentScore);
        
        return scoreDifference > remainingRounds;
    }

    private void EndGame()
    {
        var gameData = GameData.Instance;
        Team winner = gameData.PlayerScore > gameData.OpponentScore ? 
                     gameData.PlayerTeam : gameData.OpponentTeam;
        
        ChangeState(GameState.GameEnd);
        EmitSignal(SignalName.GameFinished, winner);
    }
}
