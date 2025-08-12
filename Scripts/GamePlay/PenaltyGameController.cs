using Godot;
using System.Threading.Tasks;

public partial class PenaltyGameController : Node2D
{
    [Export] private PackedScene ballScene;
    [Export] private PackedScene effectScene;
    
    // Références des nodes
    private PenaltyShooter playerShooter;
    private GoalKeeper playerGoalkeeper;
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
    private bool gameFinished = false; //  Variable pour indiquer si le jeu est terminé

    public override void _Ready()
    {
        InitializeReferences();
        ConnectSignals();
        SetupGame();

        GetTree().CreateTimer(1.0f).Timeout += () =>
        {
            if (!gameFinished) // Vérifier si le jeu n'est pas fini
            {
                waitingForInput = true;
                playerShooter.SetCanShoot(true);
                GD.Print("Debug: Force enabled shooting mode");
            }
        };
    }

    private void InitializeReferences()
    {
        GD.Print("=== INITIALISATION DES RÉFÉRENCES ===");

        // Player shooter
        playerShooter = GetNodeOrNull<PenaltyShooter>("PlayerShooter");
        if (playerShooter == null)
            GD.PrintErr("[ERREUR] PlayerShooter introuvable !");
        else
            GD.Print($"[OK] PlayerShooter trouvé : {playerShooter.GetPath()}");

        // Player goalkeeper - CORRECTION ICI !
        playerGoalkeeper = GetNodeOrNull<GoalKeeper>("PlayerGoalkeeper");
        if (playerGoalkeeper == null)
            GD.PrintErr("[ERREUR] PlayerGoalkeeper introuvable !");
        else
            GD.Print($"[OK] PlayerGoalkeeper trouvé : {playerGoalkeeper.GetPath()}");

        // Ball
        gameBall = GetNodeOrNull<Ball>("Ball");
        if (gameBall == null)
            GD.PrintErr("[ERREUR] Ball introuvable ! Vérifie le chemin dans InitializeReferences()");
        else
            GD.Print($"[OK] Ball trouvée : {gameBall.GetPath()}");

        // UI
        gameUI = GetNodeOrNull<GameUI>("UI/GameUI");
        if (gameUI == null)
            GD.PrintErr("[ERREUR] GameUI introuvable !");
        else
            GD.Print($"[OK] GameUI trouvée : {gameUI.GetPath()}");

        // Opponent AI
        opponentAI = GetNodeOrNull<OpponentAI>("OpponentAI");
        if (opponentAI == null)
            GD.PrintErr("[ERREUR] OpponentAI introuvable !");
        else
            GD.Print($"[OK] OpponentAI trouvé : {opponentAI.GetPath()}");

        // Goal Area
        goalArea = GetNodeOrNull<Area2D>("Goal/GoalArea");
        if (goalArea == null)
            GD.PrintErr("[ERREUR] GoalArea introuvable !");
        else
            GD.Print($"[OK] GoalArea trouvée : {goalArea.GetPath()}");

        // Penalty Spot
        penaltySpot = GetNodeOrNull<Area2D>("PenaltySpot");
        if (penaltySpot == null)
            GD.PrintErr("[ERREUR] PenaltySpot introuvable !");
        else
            GD.Print($"[OK] PenaltySpot trouvé : {penaltySpot.GetPath()}");

        // Goalkeeper Area
        goalkeeperArea = GetNodeOrNull<Area2D>("GoalkeeperArea");
        if (goalkeeperArea == null)
            GD.PrintErr("[ERREUR] GoalkeeperArea introuvable !");
        else
            GD.Print($"[OK] GoalkeeperArea trouvée : {goalkeeperArea.GetPath()}");

        GD.Print("=== FIN INITIALISATION ===");

        // Sécurité : empêcher la suite si des éléments critiques manquent
        if (gameBall == null || playerGoalkeeper == null)
        {
            GD.PrintErr("Impossible de continuer : éléments critiques manquants !");
            SetProcess(false);
            return;
        }

        // Activer le tir et le mouvement
        playerShooter?.SetCanShoot(true);
        playerGoalkeeper?.SetCanMove(true);
        gameBall?.SetActive(true);
    }

    private void ConnectSignals()
    {
        GD.Print("Connecting signals...");
        
        // Signaux du game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Connect(GameManager.SignalName.GameStateChanged, Callable.From<int>(OnGameStateChanged));
            GameManager.Instance.Connect(GameManager.SignalName.GameFinished, Callable.From<Team>(OnGameFinished));
        }
        
        // Signaux du tireur - CONNECTION CRITIQUE
        if (playerShooter != null)
        {
            playerShooter.Connect("ShotTaken", Callable.From<Vector2, float>(OnPlayerShotTaken));
            GD.Print("PlayerShooter signal connected");
        }
        else
        {
            GD.Print("ERROR: playerShooter is null!");
        }
        
        // Signaux du ballon
        if (gameBall != null)
        {
            gameBall.Connect("GoalScored", Callable.From(OnGoalScored));
            gameBall.Connect("ShotMissed", Callable.From(OnShotMissed));
            gameBall.Connect("BallStopped", Callable.From(OnBallStopped));
            GD.Print("Ball signals connected");
        }
        else
        {
            GD.Print("ERROR: gameBall is null!");
        }
        
        // Signaux de la zone de but
        if (goalArea != null)
        {
            goalArea.Connect("body_entered", Callable.From<Node2D>(_on_goal_area_body_entered));
            GD.Print("Goal area signal connected");
        }
        else
        {
            GD.Print("ERROR: goalArea is null!");
        }
    }
    
    private void _on_goal_area_body_entered(Node2D body)
    {
        GD.Print($"Body entered goal: {body.Name}");
        
        if (body == gameBall && gameBall.Visible)
        {
            GD.Print("Ball entered goal!");
            gameBall.OnGoalAreaEntered();
        }
    }

    private void SetupGame()
    {
        GD.Print("Setting up game...");
        
        gameFinished = false; //  Initialiser l'état du jeu
        
        // FORCER le mode tir directement pour bypass le GameManager
        GD.Print("Force starting direct shooting mode");
        waitingForInput = true;
        playerShooter.SetCanShoot(true);
        GD.Print($"waitingForInput set to: {waitingForInput}");
        
        // Démarrer le jeu (optionnel)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartPenaltySession();
        }

        if (gameUI != null)
        {
            gameUI.StartScoreAttack(); // Active le mode Score Attack
        }
        
        // Configurer la difficulté de l'IA
        if (SettingsManager.Instance != null && opponentAI != null)
        {
            opponentAI.SetDifficulty(SettingsManager.Instance.AIDifficulty);
        }
        
        gameInProgress = true;
    }

    //MÉTHODE: Vérifier si le jeu doit se terminer
    private bool ShouldEndScoreAttack()
    {
        if (gameUI != null)
        {
            return gameUI.GetCurrentAttempt() >= gameUI.GetMaxAttempts();
        }
        return false;
    }

    // MÉTHODE: Terminer le jeu Score Attack
    private void EndScoreAttack()
    {
        GD.Print("Score Attack terminé!");
        gameFinished = true;
        waitingForInput = false;
        
        // Désactiver tous les contrôles
        playerShooter?.SetCanShoot(false);
        playerGoalkeeper?.SetCanMove(false);
        gameBall?.SetActive(false);
        
        gameInProgress = false;
    }

    private async void OnGameStateChanged(int stateInt)
    {
        if (gameFinished) return; // Ne pas traiter les changements d'état si le jeu est fini
        
        var state = (GameManager.GameState)stateInt;
        
        switch (state)
        {
            case GameManager.GameState.PlayerShooting:
                await HandlePlayerShooting();
                break;
                
          
            
           
        }
    }

    private async Task HandlePlayerShooting()
    {
        if (gameFinished) return; // Ne pas permettre de nouveau tir si le jeu est fini
        
        GD.Print("Tour du joueur - Tir");
        
        // Positionner le ballon et le tireur
        ResetBallPosition();
        
        // S'assurer que le tireur est à la bonne position
        if (penaltySpot != null)
        {
            playerShooter.Position = penaltySpot.Position;
            GD.Print($"Player shooter positioned at: {playerShooter.Position}");
        }
        
        playerShooter.SetCanShoot(true);
        
        // Jouer le son de sifflet
        SoundManager.Instance?.PlaySFX("whistle");
        
        // Attendre le tir du joueur
        waitingForInput = true;
        GD.Print("Waiting for player input...");
    }

    

   
   

    private void HandleGameEnd()
    {
        GD.Print("Fin du jeu");
        EndScoreAttack(); // Utiliser la nouvelle méthode
        
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
        //  Vérifier si le jeu est terminé avant de traiter le tir
        if (gameFinished)
        {
            GD.Print("Game finished - ignoring shot");
            return;
        }
        
        GD.Print($"OnPlayerShotTaken called! Direction: {direction}, Power: {power}");
        
        waitingForInput = false;
        playerShooter.SetCanShoot(false);
        
        // Jouer le son de frappe
        SoundManager.Instance?.PlaySFX("kick");
        
        //  Déclencher l'animation du gardien EN MÊME TEMPS que le tir
        if (playerGoalkeeper != null)
        {
            GD.Print("Triggering goalkeeper animation...");
            // Le gardien réagit à la direction et puissance du tir
            playerGoalkeeper.AttemptSave(direction, power);
        }

        if (gameUI != null)
        {
            gameUI.RegisterAttempt(); // Enregistre la tentative
            
            //  Vérifier si le jeu doit se terminer après cet essai
            if (ShouldEndScoreAttack())
            {
                GD.Print("Maximum attempts reached!");
            }
        }
                
        // TIRER LE BALLON
        GD.Print("Shooting ball...");
        gameBall.ShootBall(direction, power);
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
        GameManager.Instance?.CompleteShot(true);
        
        if (gameUI != null)
        {
            gameUI.RegisterGoal(); // Enregistre le but
            
            // NOUVEAU: Vérifier si c'était le dernier essai
            if (ShouldEndScoreAttack())
            {
                EndScoreAttack();
                return; // Sortir de la fonction pour éviter de préparer le prochain tir
            }
        }

        // NOUVEAU: Ne préparer le prochain tir que si le jeu n'est pas fini
        if (!gameFinished)
        {
            // Attendre avant de repositionner
            GetTree().CreateTimer(2.0f).Timeout += () =>
            {
                if (!gameFinished) // Double vérification
                {
                    // Repositionner la balle pour le prochain tir
                    ResetBallPosition();

                    // Réactiver le mode tir
                    waitingForInput = true;
                    playerShooter.SetCanShoot(true);
                    GD.Print("Ready for next shot after goal");
                }
            };
        }
    }

    private void OnShotSaved()
    {
        GD.Print("Arrêt!");
        
        SoundManager.Instance?.PlaySFX("save");
        GameManager.Instance?.CompleteShot(false);
    }

    private void OnShotMissed()
    {
        GD.Print("Tir manqué!");

        SoundManager.Instance?.PlaySFX("crowd_disappointed");
        GameManager.Instance?.CompleteShot(false);
        
        if (gameUI != null)
        {
            gameUI.RegisterMiss(); // Enregistre le tir manqué
            
            // : Vérifier si c'était le dernier essai
            if (ShouldEndScoreAttack())
            {
                EndScoreAttack();
                return; // Sortir pour éviter de préparer le prochain tir
            }
        }
    }

    private void OnBallStopped()
    {
        GD.Print("Ball stopped - preparing for next shot");

        if (gameUI != null)
        {
            gameUI.RegisterSave(); // Enregistre l'arrêt
            
            //  Vérifier si c'était le dernier essai
            if (ShouldEndScoreAttack())
            {
                EndScoreAttack();
                return; // Sortir pour éviter de préparer le prochain tir
            }
        }

        // Ne préparer le prochain tir que si le jeu n'est pas fini
        if (!gameFinished)
        {
            // Attendre un peu avant de repositionner
            GetTree().CreateTimer(1.0f).Timeout += () =>
            {
                if (!gameFinished) // Double vérification
                {
                    // Repositionner la balle
                    ResetBallPosition();

                    // Réactiver le mode tir
                    waitingForInput = true;
                    playerShooter.SetCanShoot(true);
                    GD.Print("Ready for next shot");
                }
            };
        }

        // Si c'est pendant le jeu normal, traiter comme tir manqué
        if (gameInProgress && !gameFinished)
        {
            OnShotMissed();
        }
    }

    private void ResetBallPosition()
    {
        if (gameBall == null || penaltySpot == null || gameFinished) return; // NOUVEAU: Vérifier gameFinished
        
        GD.Print($"Resetting ball position from {gameBall.GlobalPosition} to penalty spot {penaltySpot.GlobalPosition}");
        
        // Méthode 1: Utiliser la nouvelle méthode SetPosition
        gameBall.SetPosition(penaltySpot.GlobalPosition);
        
        // Méthode 2: Reset complet
        gameBall.ResetBall();
        
        // Méthode 3: Forcer la position après reset
        gameBall.GlobalPosition = penaltySpot.GlobalPosition;
        
        // Activer la balle
        gameBall.SetActive(true);
        
        GD.Print($"Ball position after reset: {gameBall.GlobalPosition}");
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
        EndScoreAttack(); // NOUVEAU: S'assurer que le jeu se termine
    }

    // Méthodes utilitaires pour debugging
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Space:
                    // Test direct du tir - NOUVEAU: Vérifier si le jeu n'est pas fini
                    if (!gameFinished)
                    {
                        GD.Print("SPACE pressed - Direct ball test");
                        GD.Print($"Ball position before: {gameBall.Position}");
                        GD.Print($"Ball freeze state before: {gameBall.Freeze}");
                        GD.Print($"Ball visible: {gameBall.Visible}");
                        gameBall.ShootBall(Vector2.Up, 50.0f);
                    }
                    break;

                case Key.T:
                    // Test du signal du tireur - NOUVEAU: Vérifier si le jeu n'est pas fini
                    if (!gameFinished)
                    {
                        GD.Print("T pressed - Testing shooter signal");
                        OnPlayerShotTaken(Vector2.Up, 50.0f);
                    }
                    break;

                case Key.I:
                    // Activer le mode input - NOUVEAU: Vérifier si le jeu n'est pas fini
                    if (!gameFinished)
                    {
                        GD.Print("I pressed - Enabling input mode");
                        waitingForInput = true;
                        playerShooter.SetCanShoot(true);
                    }
                    break;

                case Key.D:
                    // Debug ball state
                    GD.Print($"Ball Debug - Position: {gameBall.Position}, Velocity: {gameBall.LinearVelocity}, Freeze: {gameBall.Freeze}, Visible: {gameBall.Visible}");
                    GD.Print($"Game finished: {gameFinished}"); // NOUVEAU: Debug de l'état du jeu
                    break;

                case Key.R:
                    // Restart le jeu
                    GetTree().ReloadCurrentScene();
                    break;

                case Key.Escape:
                    // Retour au menu
                    GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
                    break;

                case Key.Key1:
                    // Tester l'animation dive_left
                    GD.Print("Testing dive_left animation");
                    playerGoalkeeper?.TestAnimation("dive_left");
                    break;

                case Key.Key2:
                    // Tester l'animation dive_right
                    GD.Print("Testing dive_right animation");
                    playerGoalkeeper?.TestAnimation("dive_right");
                    break;

                case Key.Key3:
                    // Tester l'animation idle
                    GD.Print("Testing idle animation");
                    playerGoalkeeper?.TestAnimation("idle");
                    break;

                case Key.L:
                    // Lister toutes les animations disponibles
                    GD.Print("Listing goalkeeper animations:");
                    playerGoalkeeper?.ListAvailableAnimations();
                    break;

                case Key.G:
                    // Test complet d'arrêt vers la gauche
                    if (!gameFinished)
                    {
                        GD.Print("Testing full save sequence (left)");
                        playerGoalkeeper?.AttemptSave(Vector2.Left, 50.0f);
                    }
                    break;

                case Key.H:
                    // Test complet d'arrêt vers la droite
                    if (!gameFinished)
                    {
                        GD.Print("Testing full save sequence (right)");
                        playerGoalkeeper?.AttemptSave(Vector2.Right, 50.0f);
                    }
                    break;
            }
        }
    }
}