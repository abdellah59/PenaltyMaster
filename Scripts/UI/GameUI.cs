using Godot;

public partial class GameUI : Control
{
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

    public override void _Ready()
    {
        InitializeUI();
        ConnectSignals();
        UpdateUI();
    }

    private void InitializeUI()
    {
        // Récupérer les références UI
        playerScoreLabel = GetNode<Label>("TopPanel/ScoreContainer/PlayerScore");
        opponentScoreLabel = GetNode<Label>("TopPanel/ScoreContainer/OpponentScore");
        roundLabel = GetNode<Label>("TopPanel/RoundLabel");
        playerTeamLabel = GetNode<Label>("TopPanel/PlayerTeamLabel");
        opponentTeamLabel = GetNode<Label>("TopPanel/OpponentTeamLabel");
        instructionLabel = GetNode<Label>("BottomPanel/InstructionLabel");
        gameStatePanel = GetNode<Panel>("GameStatePanel");
        menuButton = GetNode<Button>("TopPanel/MenuButton");
        shotPowerBar = GetNode<ProgressBar>("BottomPanel/ShotPowerBar");
        aimingReticle = GetNode<Control>("AimingReticle");

        menuButton.Pressed += OnMenuPressed;
        
        // Initialiser les éléments
        shotPowerBar.Visible = false;
        aimingReticle.Visible = false;
    }

    private void ConnectSignals()
    {
        GameManager.Instance.Connect(GameManager.SignalName.GameStateChanged, Callable.From<int>(OnGameStateChanged));
        GameManager.Instance.Connect(GameManager.SignalName.GameFinished, Callable.From<Team>(OnGameFinished));


    }

    private void UpdateUI()
    {
        var gameData = GameData.Instance;
        
        if (gameData.PlayerTeam != null && gameData.OpponentTeam != null)
        {
            playerTeamLabel.Text = gameData.PlayerTeam.Name;
            opponentTeamLabel.Text = gameData.OpponentTeam.Name;
        }
        
        playerScoreLabel.Text = gameData.PlayerScore.ToString();
        opponentScoreLabel.Text = gameData.OpponentScore.ToString();
        roundLabel.Text = $"Manche {gameData.CurrentRound}/{gameData.MaxRounds}";
    }

    private void OnGameStateChanged(int stateInt)
    {
        var state = (GameManager.GameState)stateInt;
        UpdateInstructionText(state);
        UpdateUIVisibility(state);
        UpdateUI();
    }

    private void UpdateInstructionText(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.PlayerShooting:
                instructionLabel.Text = "Votre tour ! Visez avec la souris et maintenez le clic pour ajuster la puissance.";
                break;
            case GameManager.GameState.OpponentShooting:
                instructionLabel.Text = "Tour de l'adversaire. Préparez-vous à défendre !";
                break;
            case GameManager.GameState.GoalkeepingPlayerTurn:
                instructionLabel.Text = "Déplacez votre gardien avec A/D et essayez d'arrêter le tir !";
                break;
            case GameManager.GameState.GoalkeepingOpponentTurn:
                instructionLabel.Text = "L'adversaire défend son but...";
                break;
            case GameManager.GameState.RoundEnd:
                instructionLabel.Text = "Manche terminée ! Préparation du tour suivant...";
                break;
            case GameManager.GameState.GameEnd:
                instructionLabel.Text = "Match terminé !";
                break;
            default:
                instructionLabel.Text = "";
                break;
        }
    }

    private void UpdateUIVisibility(GameManager.GameState state)
    {
        // Afficher/masquer les éléments selon l'état
        bool showShotControls = state == GameManager.GameState.PlayerShooting;
        shotPowerBar.Visible = showShotControls;
        aimingReticle.Visible = showShotControls;
    }

    private void OnGameFinished(Team winner)
    {
        ShowGameResult(winner);
    }

    private void ShowGameResult(Team winner)
    {
        var gameData = GameData.Instance;
        string resultText;
        
        if (winner == gameData.PlayerTeam)
        {
            resultText = $"🎉 VICTOIRE ! 🎉\n{gameData.PlayerTeam.Name} {gameData.PlayerScore} - {gameData.OpponentScore} {gameData.OpponentTeam.Name}";
        }
        else
        {
            resultText = $"😞 DÉFAITE 😞\n{gameData.PlayerTeam.Name} {gameData.PlayerScore} - {gameData.OpponentScore} {gameData.OpponentTeam.Name}";
        }
        
        // Créer une popup de résultat
        var popup = new AcceptDialog();
        popup.DialogText = resultText;
        popup.Title = "Résultat du match";
        
        var restartButton = new Button();
        restartButton.Text = "Rejouer";
        restartButton.Pressed += () => {
            GetTree().ChangeSceneToFile("res://scenes/TeamSelection.tscn");
            popup.QueueFree();
        };
        
        popup.AddChild(restartButton);
        GetTree().CurrentScene.AddChild(popup);
        popup.PopupCentered();
    }

    private void OnMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }

    public void UpdateShotPower(float power, float maxPower)
    {
        if (shotPowerBar.Visible)
        {
            shotPowerBar.Value = (power / maxPower) * 100;
        }
    }

    public void UpdateAimingReticle(Vector2 position)
    {
        if (aimingReticle.Visible)
        {
            aimingReticle.Position = position;
        }
    }
}