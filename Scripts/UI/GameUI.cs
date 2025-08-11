using Godot;

public partial class GameUI : Control
{
    // Éléments UI existants
    private Label playerScoreLabel;
    private Label opponentScoreLabel;
    private Label roundLabel;
    private Label playerTeamLabel;
    private Label opponentTeamLabel;
    private Label instructionLabel;
    private Panel gameStatePanel;
    private Button menuButton;
    private ProgressBar shotPowerBar;
    private Control aimingReticle;
    
    // Nouveaux éléments pour Score Attack
    private Label attemptsLabel;
    private Label accuracyLabel;
    private ProgressBar attemptsProgressBar;
    private Panel scoreAttackPanel;
    private Label finalResultLabel;
    
    // Variables Score Attack
    private int goalsScored = 0;
    private int currentAttempt = 0;
    private int maxAttempts = 5;
    private bool scoreAttackMode = true; // Mode Score Attack activé

    public override void _Ready()
    {
        InitializeUI();
        SetupScoreAttackMode();
        UpdateUI();
    }

    private void InitializeUI()
    {
        // Récupérer les références UI existantes
        playerScoreLabel = GetNodeOrNull<Label>("TopPanel/ScoreContainer/PlayerScore");
        opponentScoreLabel = GetNodeOrNull<Label>("TopPanel/ScoreContainer/OpponentScore");
        roundLabel = GetNodeOrNull<Label>("TopPanel/RoundLabel");
        playerTeamLabel = GetNodeOrNull<Label>("TopPanel/PlayerTeamLabel");
        opponentTeamLabel = GetNodeOrNull<Label>("TopPanel/OpponentTeamLabel");
        instructionLabel = GetNodeOrNull<Label>("BottomPanel/InstructionLabel");
        gameStatePanel = GetNodeOrNull<Panel>("GameStatePanel");
        menuButton = GetNodeOrNull<Button>("TopPanel/MenuButton");
        shotPowerBar = GetNodeOrNull<ProgressBar>("BottomPanel/ShotPowerBar");
        aimingReticle = GetNodeOrNull<Control>("AimingReticle");

        if (menuButton != null)
            menuButton.Pressed += OnMenuPressed;
        
        // Initialiser les éléments
        if (shotPowerBar != null) shotPowerBar.Visible = false;
        if (aimingReticle != null) aimingReticle.Visible = false;
    }

    private void SetupScoreAttackMode()
    {
        // Créer un panneau pour les nouvelles informations Score Attack
        CreateScoreAttackPanel();
        
        // Modifier les labels existants pour le mode Score Attack
        if (playerTeamLabel != null)
        {
            playerTeamLabel.Text = "🏆 SCORE ATTACK";
            playerTeamLabel.AddThemeColorOverride("font_color", Colors.Gold);
            playerTeamLabel.AddThemeFontSizeOverride("font_size", 20);
        }
        
        if (opponentTeamLabel != null)
        {
            opponentTeamLabel.Text = "Cible: 5/5";
            opponentTeamLabel.AddThemeColorOverride("font_color", Colors.LightBlue);
        }
        
        // Masquer le score adversaire (pas utilisé en Score Attack)
        if (opponentScoreLabel != null)
        {
            opponentScoreLabel.Visible = false;
        }
        
        // Transformer le label "VS" en indicateur d'essais
        var vsLabel = GetNodeOrNull<Label>("TopPanel/ScoreContainer/VSLabel");
        if (vsLabel != null)
        {
            vsLabel.Text = "/";
            vsLabel.AddThemeColorOverride("font_color", Colors.White);
        }
    }

    private void CreateScoreAttackPanel()
    {
        // Créer un panneau pour les informations Score Attack
        scoreAttackPanel = new Panel();
        scoreAttackPanel.Name = "ScoreAttackPanel";
        
        // Position et taille
        scoreAttackPanel.Position = new Vector2(500, 460);
        scoreAttackPanel.Size = new Vector2(250, 100);
        
        // Style du panneau
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.3f, 0.8f);
        styleBox.BorderColor = Colors.Gold;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.CornerRadiusTopLeft = 8;
        styleBox.CornerRadiusTopRight = 8;
        styleBox.CornerRadiusBottomLeft = 8;
        styleBox.CornerRadiusBottomRight = 8;
        scoreAttackPanel.AddThemeStyleboxOverride("panel", styleBox);
        
        // Label des tentatives
        attemptsLabel = new Label();
        attemptsLabel.Text = $"Essai: {currentAttempt}/{maxAttempts}";
        attemptsLabel.Position = new Vector2(20, 15);
        attemptsLabel.Size = new Vector2(200, 30);
        attemptsLabel.AddThemeColorOverride("font_color", Colors.White);
        attemptsLabel.AddThemeFontSizeOverride("font_size", 13);
        scoreAttackPanel.AddChild(attemptsLabel);
        
        // Barre de progression des essais
        attemptsProgressBar = new ProgressBar();
        attemptsProgressBar.Position = new Vector2(20, 45);
        attemptsProgressBar.Size = new Vector2(200, 15);
        attemptsProgressBar.MinValue = 0;
        attemptsProgressBar.MaxValue = maxAttempts;
        attemptsProgressBar.Value = 0;
        attemptsProgressBar.AddThemeColorOverride("fill", Colors.Gold);
        scoreAttackPanel.AddChild(attemptsProgressBar);
        
        // Label de précision
        accuracyLabel = new Label();
        accuracyLabel.Text = "Précision: 0%";
        accuracyLabel.Position = new Vector2(20, 75);
        accuracyLabel.Size = new Vector2(100, 20);
        accuracyLabel.AddThemeColorOverride("font_color", Colors.LightGreen);
        accuracyLabel.AddThemeFontSizeOverride("font_size", 12);
        scoreAttackPanel.AddChild(accuracyLabel);
        
        
        
        // Ajouter le panneau à l'interface
        AddChild(scoreAttackPanel);
    }



    private void UpdateUI()
    {
        if (scoreAttackMode)
        {
            UpdateScoreAttackUI();
        }
        else
        {
            // Mode normal (conservé pour compatibilité)
            UpdateNormalUI();
        }
    }

    private void UpdateScoreAttackUI()
    {
        // Mettre à jour le score principal
        if (playerScoreLabel != null)
        {
            playerScoreLabel.Text = $"⚽ {goalsScored}";
            playerScoreLabel.AddThemeColorOverride("font_color", Colors.Gold);
            playerScoreLabel.AddThemeFontSizeOverride("font_size", 36);
        }
        
        // Mettre à jour les tentatives dans le score container
        if (opponentScoreLabel != null)
        {
            opponentScoreLabel.Text = maxAttempts.ToString();
            opponentScoreLabel.Visible = true;
            opponentScoreLabel.AddThemeColorOverride("font_color", Colors.LightBlue);
        }
        
        // Mettre à jour le panneau Score Attack
        if (attemptsLabel != null)
        {
            attemptsLabel.Text = $"Essai: {currentAttempt}/{maxAttempts}";
        }
        
        if (attemptsProgressBar != null)
        {
            attemptsProgressBar.Value = currentAttempt;
            
            // Changer la couleur selon la performance
            float successRate = currentAttempt > 0 ? (float)goalsScored / currentAttempt : 0;
            if (successRate >= 0.8f)
                attemptsProgressBar.AddThemeColorOverride("fill", Colors.Green);
            else if (successRate >= 0.5f)
                attemptsProgressBar.AddThemeColorOverride("fill", Colors.Orange);
            else
                attemptsProgressBar.AddThemeColorOverride("fill", Colors.Red);
        }
        
        if (accuracyLabel != null)
        {
            float accuracy = currentAttempt > 0 ? (float)goalsScored / currentAttempt * 100 : 0;
            accuracyLabel.Text = $"Précision: {accuracy:F1}%";
        }
        
        // Mettre à jour le label de manche pour afficher l'essai actuel
        if (roundLabel != null)
        {
            if (currentAttempt <= maxAttempts)
                roundLabel.Text = $"Essai {currentAttempt}/{maxAttempts}";
            else
                roundLabel.Text = "Terminé!";
        }
    }

    private void UpdateNormalUI()
    {
        // Mode normal conservé pour compatibilité
        var gameData = GameData.Instance;
        
        if (gameData.PlayerTeam != null && gameData.OpponentTeam != null)
        {
            if (playerTeamLabel != null) playerTeamLabel.Text = gameData.PlayerTeam.Name;
            if (opponentTeamLabel != null) opponentTeamLabel.Text = gameData.OpponentTeam.Name;
        }
        
        if (playerScoreLabel != null) playerScoreLabel.Text = gameData.PlayerScore.ToString();
        if (opponentScoreLabel != null) opponentScoreLabel.Text = gameData.OpponentScore.ToString();
        if (roundLabel != null) roundLabel.Text = $"Manche {gameData.CurrentRound}/{gameData.MaxRounds}";
    }

    // Méthodes publiques pour contrôler le Score Attack
    public void StartScoreAttack()
    {
        goalsScored = 0;
        currentAttempt = 0;
        scoreAttackMode = true;
        UpdateUI();
    }

    public void RegisterAttempt()
    {
        currentAttempt++;
        UpdateUI();
        UpdateInstruction($"Essai {currentAttempt}/{maxAttempts} - Manqué => Visez et tirez!");
    }

    public void RegisterGoal()
    {
        goalsScored++;
        UpdateUI();
        ShowTemporaryMessage("⚽ BUT!", Colors.Gold, 2.0f);
        
        if (currentAttempt >= maxAttempts)
        {
            // Fin du Score Attack
            GetTree().CreateTimer(2.0f).Timeout += ShowFinalResults;
        }
        else
        {
            UpdateInstruction($"⚽ BUT! Score: {goalsScored}/{maxAttempts}");
        }
    }

    public void RegisterMiss()
    {
        UpdateUI();
        ShowTemporaryMessage("Manqué!", Colors.Red, 1.5f);
        
        if (currentAttempt >= maxAttempts)
        {
            // Fin du Score Attack
            GetTree().CreateTimer(2.0f).Timeout += ShowFinalResults;
        }
        else
        {
            UpdateInstruction($"Manqué! Score: {goalsScored}/{maxAttempts}");
        }
    }

    public void RegisterSave()
    {
        UpdateUI();
        ShowTemporaryMessage("Arrêté!", Colors.Blue, 1.5f);
        
        if (currentAttempt >= maxAttempts)
        {
            // Fin du Score Attack
            GetTree().CreateTimer(2.0f).Timeout += ShowFinalResults;
        }
        else
        {
            UpdateInstruction($"Arrêté par le gardien! Score: {goalsScored}/{maxAttempts}");
        }
    }

    private void ShowFinalResults()
    {
        // Créer le panneau de résultats finaux
        var resultsPanel = new Panel();
        resultsPanel.Size = new Vector2(500, 300);
        resultsPanel.Position = GetViewport().GetVisibleRect().Size / 2 - resultsPanel.Size / 2;
        
        // Style
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.05f, 0.05f, 0.2f, 0.95f);
        styleBox.BorderColor = Colors.Gold;
        styleBox.BorderWidthTop = 3;
        styleBox.BorderWidthBottom = 3;
        styleBox.BorderWidthLeft = 3;
        styleBox.BorderWidthRight = 3;
        styleBox.CornerRadiusTopLeft = 15;
        styleBox.CornerRadiusTopRight = 15;
        styleBox.CornerRadiusBottomLeft = 15;
        styleBox.CornerRadiusBottomRight = 15;
        resultsPanel.AddThemeStyleboxOverride("panel", styleBox);
        
        // Titre
        var titleLabel = new Label();
        titleLabel.Text = "🏆 RÉSULTATS FINAUX";
        titleLabel.Position = new Vector2(0, 20);
        titleLabel.Size = new Vector2(500, 40);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeColorOverride("font_color", Colors.Gold);
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        resultsPanel.AddChild(titleLabel);
        
        // Score
        var scoreLabel = new Label();
        scoreLabel.Text = $"Score Final: {goalsScored}/{maxAttempts}";
        scoreLabel.Position = new Vector2(0, 80);
        scoreLabel.Size = new Vector2(500, 40);
        scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
        scoreLabel.AddThemeColorOverride("font_color", Colors.White);
        scoreLabel.AddThemeFontSizeOverride("font_size", 24);
        resultsPanel.AddChild(scoreLabel);
        
        // Précision
        float finalAccuracy = (float)goalsScored / maxAttempts * 100;
        var accuracyFinalLabel = new Label();
        accuracyFinalLabel.Text = $"Précision: {finalAccuracy:F1}%";
        accuracyFinalLabel.Position = new Vector2(0, 120);
        accuracyFinalLabel.Size = new Vector2(500, 30);
        accuracyFinalLabel.HorizontalAlignment = HorizontalAlignment.Center;
        accuracyFinalLabel.AddThemeColorOverride("font_color", Colors.LightGreen);
        accuracyFinalLabel.AddThemeFontSizeOverride("font_size", 18);
        resultsPanel.AddChild(accuracyFinalLabel);
        
        // Évaluation
        string evaluation = GetPerformanceEvaluation(goalsScored, maxAttempts);
        var evaluationLabel = new Label();
        evaluationLabel.Text = evaluation;
        evaluationLabel.Position = new Vector2(0, 160);
        evaluationLabel.Size = new Vector2(500, 30);
        evaluationLabel.HorizontalAlignment = HorizontalAlignment.Center;
        evaluationLabel.AddThemeColorOverride("font_color", GetEvaluationColor(goalsScored, maxAttempts));
        evaluationLabel.AddThemeFontSizeOverride("font_size", 20);
        resultsPanel.AddChild(evaluationLabel);
        
        // Boutons
        var restartButton = new Button();
        restartButton.Text = "Rejouer";
        restartButton.Position = new Vector2(100, 220);
        restartButton.Size = new Vector2(120, 40);
        restartButton.Pressed += () => {
            GetTree().ReloadCurrentScene();
        };
        resultsPanel.AddChild(restartButton);
        
        var menuButtonFinal = new Button();
        menuButtonFinal.Text = "🏠 Menu";
        menuButtonFinal.Position = new Vector2(280, 220);
        menuButtonFinal.Size = new Vector2(120, 40);
        menuButtonFinal.Pressed += OnMenuPressed;
        resultsPanel.AddChild(menuButtonFinal);
        
        // Ajouter à la scène
        GetTree().CurrentScene.AddChild(resultsPanel);
        
        // Animation d'apparition
        resultsPanel.Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(resultsPanel, "modulate:a", 1.0f, 0.5f);
    }

    private string GetPerformanceEvaluation(int score, int total)
    {
        float ratio = (float)score / total;
        
        if (ratio == 1.0f) return "🌟 PARFAIT! 🌟";
        else if (ratio >= 0.8f) return "🔥 EXCELLENT! 🔥";
        else if (ratio >= 0.6f) return "👍 TRÈS BIEN!";
        else if (ratio >= 0.4f) return "😐 CORRECT";
        else return "😞 À AMÉLIORER";
    }

    private Color GetEvaluationColor(int score, int total)
    {
        float ratio = (float)score / total;
        
        if (ratio == 1.0f) return Colors.Gold;
        else if (ratio >= 0.8f) return Colors.Orange;
        else if (ratio >= 0.6f) return Colors.Green;
        else if (ratio >= 0.4f) return Colors.Yellow;
        else return Colors.Red;
    }

       public void UpdateInstruction(string instruction)
    {
        if (instructionLabel != null)
        {
            instructionLabel.Text = instruction;
            
            // Animation du texte
            var tween = CreateTween();
            tween.TweenProperty(instructionLabel, "modulate:a", 0.7f, 0.1f);
            tween.TweenProperty(instructionLabel, "modulate:a", 1.0f, 0.1f);
        }
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }

    // Méthodes utilitaires
    public void UpdateShotPower(float power, float maxPower)
    {
        if (shotPowerBar != null && shotPowerBar.Visible)
        {
            shotPowerBar.Value = (power / maxPower) * 100;
        }
    }

    public void UpdateAimingReticle(Vector2 position)
    {
        if (aimingReticle != null && aimingReticle.Visible)
        {
            aimingReticle.Position = position;
        }
    }

    private void ShowTemporaryMessage(string message, Color color, float duration)
    {
        var tempLabel = new Label();
        tempLabel.Text = message;
        tempLabel.AddThemeColorOverride("font_color", color);
        tempLabel.AddThemeFontSizeOverride("font_size", 32);
        tempLabel.HorizontalAlignment = HorizontalAlignment.Center;
        tempLabel.Position = GetViewport().GetVisibleRect().Size / 2 + Vector2.Up * 100;
        tempLabel.Size = new Vector2(300, 50);
        
        GetTree().CurrentScene.AddChild(tempLabel);
        
        var tween = CreateTween();
        tween.TweenProperty(tempLabel, "position", tempLabel.Position + Vector2.Up * 80, duration);
        tween.Parallel().TweenProperty(tempLabel, "modulate:a", 0.0f, duration);
        tween.TweenCallback(Callable.From(tempLabel.QueueFree));
    }

    // Propriétés publiques pour accès externe
    public int GetGoalsScored() => goalsScored;
    public int GetCurrentAttempt() => currentAttempt;
    public int GetMaxAttempts() => maxAttempts;
    public bool IsScoreAttackMode() => scoreAttackMode;
}