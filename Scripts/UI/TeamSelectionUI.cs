using Godot;
using System.Collections.Generic;

public partial class TeamSelectionUI : Control
{
    [Signal] public delegate void TeamsSelectedEventHandler(Team playerTeam, Team opponentTeam);

    // Variables pour les références UI
    private GridContainer playerTeamGrid;
    private GridContainer opponentTeamGrid;
    private Label playerTeamLabel;
    private Label opponentTeamLabel;
    private Button startGameButton;
    
    private Team selectedPlayerTeam;
    private Team selectedOpponentTeam;
    private List<Button> playerTeamButtons = new List<Button>();
    private List<Button> opponentTeamButtons = new List<Button>();

    public override void _Ready()
    {
        GD.Print("TeamSelectionUI - Démarrage");
        InitializeUI();
        PopulateTeamSelections();
        GD.Print("TeamSelectionUI - Initialisé");
    }

    private void InitializeUI()
    {
        try 
        {
            // Récupérer les références avec les vrais chemins
            playerTeamGrid = GetNode<GridContainer>("VBoxContainer/HBoxContainer/PlayerTeamPanel/ScrollContainer/GridContainer");
            opponentTeamGrid = GetNode<GridContainer>("VBoxContainer/HBoxContainer/OpponentTeamPanel/ScrollContainer/GridContainer");
            playerTeamLabel = GetNode<Label>("VBoxContainer/HBoxContainer/PlayerTeamPanel/SelectedTeamLabel");
            opponentTeamLabel = GetNode<Label>("VBoxContainer/HBoxContainer/OpponentTeamPanel/SelectedTeamLabel");
            startGameButton = GetNode<Button>("VBoxContainer/StartGameButton");
            
            GD.Print("Toutes les références UI trouvées");
            
            startGameButton.Pressed += OnStartGamePressed;
            startGameButton.Disabled = true;
            
            playerTeamLabel.Text = "Sélectionnez votre équipe";
            opponentTeamLabel.Text = "Sélectionnez l'équipe adverse";
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Erreur InitializeUI: {e.Message}");
        }
    }

    private void PopulateTeamSelections()
    {
        try
        {
            // Créer des équipes de test si GameData n'existe pas
            var teams = GetTestTeams();
            
            GD.Print($"Création de {teams.Count} équipes");
            
            foreach (var team in teams)
            {
                // Bouton pour l'équipe du joueur
                var playerButton = CreateTeamButton(team, true);
                playerTeamGrid.AddChild(playerButton);
                playerTeamButtons.Add(playerButton);
                
                // Bouton pour l'équipe adverse
                var opponentButton = CreateTeamButton(team, false);
                opponentTeamGrid.AddChild(opponentButton);
                opponentTeamButtons.Add(opponentButton);
            }
            
            GD.Print("Boutons d'équipes créés");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Erreur PopulateTeamSelections: {e.Message}");
        }
    }

    private List<Team> GetTestTeams()
    {
        // Créer des équipes de test si GameData n'est pas disponible
        return new List<Team>
        {
            new Team("France", "France", new Color(0.0f, 0.2f, 0.6f), Colors.White, 1),
            new Team("Brésil", "Brazil", new Color(1.0f, 0.8f, 0.0f), new Color(0.0f, 0.4f, 0.0f), 2),
            new Team("Allemagne", "Germany", Colors.Black, Colors.White, 3),
            new Team("Espagne", "Spain", new Color(0.8f, 0.0f, 0.0f), new Color(1.0f, 0.8f, 0.0f), 4),
            new Team("Maroc", "Morocco", new Color(0.8f, 0.0f, 0.0f), new Color(0.0f, 0.5f, 0.0f), 9),
            new Team("Algérie", "Algeria", new Color(0.0f, 0.6f, 0.3f), Colors.White, 10),
            new Team("Tunisie", "Tunisia", new Color(0.9f, 0.1f, 0.1f), Colors.White, 11)
        };
    }

    private Button CreateTeamButton(Team team, bool isPlayerTeam)
    {
        var button = new Button();
        button.Text = team.Name;
        button.CustomMinimumSize = new Vector2(120, 80);
        
        // Style du bouton avec les couleurs de l'équipe
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = team.PrimaryColor;
        styleBox.BorderColor = team.SecondaryColor;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.CornerRadiusTopLeft = 5;
        styleBox.CornerRadiusTopRight = 5;
        styleBox.CornerRadiusBottomLeft = 5;
        styleBox.CornerRadiusBottomRight = 5;
        
        button.AddThemeStyleboxOverride("normal", styleBox);
        button.AddThemeColorOverride("font_color", team.SecondaryColor);
        
        if (isPlayerTeam)
        {
            button.Pressed += () => OnPlayerTeamSelected(team, button);
        }
        else
        {
            button.Pressed += () => OnOpponentTeamSelected(team, button);
        }
        
        return button;
    }

    private void OnPlayerTeamSelected(Team team, Button button)
    {
        GD.Print($"Équipe joueur sélectionnée: {team.Name}");
        selectedPlayerTeam = team;
        playerTeamLabel.Text = $"Équipe sélectionnée: {team.Name}";
        
        // Désélectionner les autres boutons
        foreach (var btn in playerTeamButtons)
        {
            btn.ButtonPressed = (btn == button);
        }
        
        CheckStartGameAvailability();
    }

    private void OnOpponentTeamSelected(Team team, Button button)
    {
        GD.Print($"Équipe adverse sélectionnée: {team.Name}");
        selectedOpponentTeam = team;
        opponentTeamLabel.Text = $"Adversaire: {team.Name}";
        
        // Désélectionner les autres boutons
        foreach (var btn in opponentTeamButtons)
        {
            btn.ButtonPressed = (btn == button);
        }
        
        CheckStartGameAvailability();
    }

    private void CheckStartGameAvailability()
    {
        bool canStart = selectedPlayerTeam != null && selectedOpponentTeam != null;
        startGameButton.Disabled = !canStart;
        
        if (canStart)
        {
            startGameButton.Text = $"Commencer: {selectedPlayerTeam.Name} vs {selectedOpponentTeam.Name}";
        }
        else
        {
            startGameButton.Text = "Sélectionnez les deux équipes";
        }
    }

    private void OnStartGamePressed()
    {
        GD.Print("Bouton Commencer pressé");
        
        if (selectedPlayerTeam != null && selectedOpponentTeam != null)
        {
            GD.Print($"Démarrage du jeu: {selectedPlayerTeam.Name} vs {selectedOpponentTeam.Name}");
            
            // Sauvegarder les équipes sélectionnées (si GameData existe)
            if (GameData.Instance != null)
            {
                GameData.Instance.PlayerTeam = selectedPlayerTeam;
                GameData.Instance.OpponentTeam = selectedOpponentTeam;
            }
            
            // Changer de scène
            GetTree().ChangeSceneToFile("res://scenes/PenaltyGame.tscn");
        }
        else
        {
            GD.PrintErr("Équipes non sélectionnées");
        }
    }
}