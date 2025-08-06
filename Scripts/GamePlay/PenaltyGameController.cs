using Godot;
using System.Threading.Tasks;

public partial class PenaltyGameController : Node2D
{
    [Export] private PackedScene ballScene;
    [Export] private PackedScene effectScene;
    
    // Références des nodes
    private PenaltyShooter playerShooter;
    private Goalkeeper playerGoalkeeper;
    private Ball gameBall;
    private GameUI gameUI;
    private OpponentAI opponentAI;
    
    // Zones du terrain
    private Area2D goalArea;
    private Area2D penaltySpot;
    private Area2D goalkeeperArea;
    
    // État du jeu
    private bool waitingForInput = false;
    private bool gameInProgress = false;

    public override void _Ready()
    {
        InitializeReferences();
        ConnectSignals();
        SetupGame();
    }

    private void InitializeReferences()
    {
        // Récupérer les références des nodes
        playerShooter = GetNode<PenaltyShooter>("PlayerShooter");
        playerGoalkeeper = GetNode<Goalkeeper>("PlayerGoalkeeper");
        gameBall = GetNode<Ball>("Ball");
        gameUI = GetNode<GameUI>("UI/GameUI");
        opponentAI = GetNode<OpponentAI>("OpponentAI");
        
        goalArea = GetNode<Area2D>("Goal/GoalArea");
        penaltySpot = GetNode<Area2D>("PenaltySpot");
        goalkeeperArea = GetNode<Area2D>("GoalkeeperArea");
        
        // Initialiser les composants
        playerShooter.SetCanShoot(false);
        playerGoalkeeper.SetCanMove(false);
        gameBall.SetActive(false);
    }

    private void ConnectSignals()
    {
        // Signaux du game manager
        GameManager.Instance.Connect(GameManager.SignalName.GameStateChanged, Callable.From<int>(OnGameStateChanged));
        GameManager.Instance.Connect(GameManager.SignalName.GameFinished, Callable.From<Team>(OnGameFinished));

        
        // Signaux du tireur
        playerShooter.ShotTaken += OnPlayerShotTaken;
        
        // Signaux du ballon
        gameBall.GoalScored += OnGoalScored;
        gameBall.ShotMissed += OnShotMissed;
        gameBall.BallStopped += OnBallStopped;
        
        // Signaux de la zone de but
        goalArea.BodyEntered += OnGoalAreaEntered;
    }

    private void SetupGame()
    {
        // Démarrer le jeu
        GameManager.Instance.StartPenaltySession();
        
        // Configurer la difficulté de l'IA
        if (SettingsManager.Instance != null)
        {
            opponentAI.SetDifficulty(SettingsManager.Instance.AIDifficulty);
        }
        
        gameInProgress = true;
    }

    private async void OnGameStateChanged(int stateInt)
    {
        var state = (GameManager.GameState)stateInt;
        
        switch (state)
        {
            case GameManager.GameState.PlayerShooting:
                await HandlePlayerShooting();
                break;
                
            case GameManager.GameState.OpponentShooting:
                await HandleOpponentShooting();
                break;
                
            case GameManager.GameState.GoalkeepingPlayerTurn:
                await HandlePlayerGoalkeeping();
                break;
                
            case GameManager.GameState.GoalkeepingOpponentTurn:
                await HandleOpponentGoalkeeping();
                break;
                
            case GameManager.GameState.RoundEnd:
                await HandleRoundEnd();
                break;
                
            case GameManager.GameState.GameEnd:
                HandleGameEnd();
                break;
        }
    }

    private async Task HandlePlayerShooting()
    {
        GD.Print("Tour du joueur - Tir");
        
        // Positionner le ballon et le tireur
        ResetBallPosition();
        playerShooter.Position = penaltySpot.Position;
        playerShooter.SetCanShoot(true);
        
        // Jouer le son de sifflet
        SoundManager.Instance?.PlaySFX("whistle");
        
        // Attendre le tir du joueur
        waitingForInput = true;
    }

    private async Task HandleOpponentShooting()
    {
        GD.Print("Tour de l'adversaire - Tir");
        
        // Positionner le gardien du joueur
        playerGoalkeeper.Position = goalkeeperArea.Position;
        playerGoalkeeper.SetCanMove(true);
        
        // L'IA tire
        bool scored = await opponentAI.TakeShot();
        
        // Désactiver le gardien
        playerGoalkeeper.SetCanMove(false);
        
        // Jouer les effets sonores
        if (scored)
        {
            SoundManager.Instance?.PlaySFX("goal");
            SoundManager.Instance?.PlaySFX("crowd_cheer");
        }
        else
        {
            SoundManager.Instance?.PlaySFX("save");
            SoundManager.Instance?.PlaySFX("crowd_disappointed");
        }
        
        // Compléter le tir
        GameManager.Instance.CompleteShot(scored);
    }

    private async Task HandlePlayerGoalkeeping()
    {
        GD.Print("Tour du joueur - Gardien");
        
        // Ce mode n'est pas utilisé dans cette version simplifiée
        // Mais peut être ajouté pour une version plus complexe
        await Task.Delay(100);
    }

    private async Task HandleOpponentGoalkeeping()
    {
        GD.Print("Tour de l'adversaire - Gardien");
        
        // L'IA défend automatiquement quand le joueur tire
        await Task.Delay(100);
    }

    private async Task HandleRoundEnd()
    {
        GD.Print("Fin de manche");
        
        // Afficher les résultats de la manche
        ShowRoundResult();
        
        // Attendre avant de continuer
        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout);
        
        // Continuer vers le prochain état
        if (GameData.Instance.CurrentRound <= GameData.Instance.MaxRounds)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.PlayerShooting);
        }
    }

    private void HandleGameEnd()
    {
        GD.Print("Fin du jeu");
        gameInProgress = false;
        
        // Désactiver tous les contrôles
        playerShooter.SetCanShoot(false);
        playerGoalkeeper.SetCanMove(false);
        gameBall.SetActive(false);
        
        // Jouer la musique de fin
        var gameData = GameData.Instance;
        if (gameData.PlayerScore > gameData.OpponentScore)
        {
            SoundManager.Instance?.PlaySFX("crowd_cheer");
        }
        else
        {
            SoundManager.Instance?.PlaySFX("crowd_disappointed");
        }
    }

    private async void OnPlayerShotTaken(Vector2 direction, float power)
    {
        if (!waitingForInput) return;
        
        waitingForInput = false;
        playerShooter.SetCanShoot(false);
        
        // Jouer le son de frappe
        SoundManager.Instance?.PlaySFX("kick");
        
        // Tirer le ballon
        gameBall.ShootBall(direction, power);
        
        // L'IA du gardien adverse tente l'arrêt
        bool saved = await opponentAI.AttemptSave(direction, power);
        
        // Si le ballon n'a pas été sauvé et qu'il va vers le but
        if (!saved && IsDirectionTowardsGoal(direction))
        {
            // Le but sera compté par OnGoalAreaEntered
        }
        else if (saved)
        {
            // Arrêt réussi
            OnShotSaved();
        }
        else
        {
            // Tir à côté
            OnShotMissed();
        }
    }

    private bool IsDirectionTowardsGoal(Vector2 direction)
    {
        // Vérifier si la direction pointe vers le but
        Vector2 goalDirection = (goalArea.GlobalPosition - penaltySpot.GlobalPosition).Normalized();
        float dot = direction.Dot(goalDirection);
        return dot > 0.5f; // Au moins 50% dans la direction du but
    }

    private void OnGoalAreaEntered(Node2D body)
    {
        if (body == gameBall && gameBall.Visible)
        {
            OnGoalScored();
        }
    }

    private void OnGoalScored()
    {
        GD.Print("BUT!");
        
        // Jouer les effets
        SoundManager.Instance?.PlaySFX("goal");
        SoundManager.Instance?.PlaySFX("crowd_cheer");
        
        // Créer un effet visuel
        ShowGoalEffect();
        
        // Compléter le tir
        GameManager.Instance.CompleteShot(true);
    }

    private void OnShotSaved()
    {
        GD.Print("Arrêt!");
        
        SoundManager.Instance?.PlaySFX("save");
        GameManager.Instance.CompleteShot(false);
    }

    private void OnShotMissed()
    {
        GD.Print("Tir manqué!");
        
        SoundManager.Instance?.PlaySFX("crowd_disappointed");
        GameManager.Instance.CompleteShot(false);
    }

    private void OnBallStopped()
    {
        // Le ballon s'est arrêté sans but
        if (gameInProgress && waitingForInput)
        {
            OnShotMissed();
        }
    }

    private void ResetBallPosition()
    {
        gameBall.Position = penaltySpot.Position;
        gameBall.ResetBall();
        gameBall.SetActive(true);
    }

    private void ShowGoalEffect()
    {
        // Créer un effet de particules ou une animation
        var label = new Label();
        label.Text = "⚽ BUT! ⚽";
        label.AddThemeColorOverride("font_color", Colors.Gold);
        label.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        
        // Positionner au-dessus du but
        label.Position = goalArea.Position + Vector2.Up * 100;
        AddChild(label);
        
        // Animation
        var tween = CreateTween();
        tween.Parallel().TweenProperty(label, "position", label.Position + Vector2.Up * 50, 1.0f);
        tween.Parallel().TweenProperty(label, "modulate:a", 0.0f, 1.0f);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    private void ShowRoundResult()
    {
        var gameData = GameData.Instance;
        
        var label = new Label();
        label.Text = $"Manche {gameData.CurrentRound - 1} terminée\n" +
                    $"{gameData.PlayerTeam.Name}: {gameData.PlayerScore}\n" +
                    $"{gameData.OpponentTeam.Name}: {gameData.OpponentScore}";
        
        label.AddThemeColorOverride("font_color", Colors.White);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        
        // Centrer à l'écran
        var viewport = GetViewport();
        label.Position = viewport.GetVisibleRect().Size / 2 - label.Size / 2;
        
        GetTree().CurrentScene.AddChild(label);
        
        // Animation de disparition
        var tween = CreateTween();
        tween.TweenInterval(1.5f);
        tween.TweenProperty(label, "modulate:a", 0.0f, 0.5f);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    private void OnGameFinished(Team winner)
    {
        GD.Print($"Jeu terminé! Gagnant: {winner.Name}");
    }

    // Méthodes utilitaires pour debugging
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Raccourcis de debug (à supprimer en production)
            switch (keyEvent.Keycode)
            {
                case Key.R:
                    // Restart le jeu
                    GetTree().ReloadCurrentScene();
                    break;
                    
                case Key.Escape:
                    // Retour au menu
                    GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
                    break;
            }
        }
    }
}