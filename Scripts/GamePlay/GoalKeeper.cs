using Godot;

// Définit la classe pour le gardien de but, qui hérite de CharacterBody2D
public partial class GoalKeeper : CharacterBody2D
{
    // Exporte des variables pour les rendre modifiables depuis l'éditeur Godot
    [Export] private float moveSpeed = 300.0f;          // Vitesse de déplacement du gardien
    [Export] private float reactionTime = 0.5f;           // Temps de réaction du gardien
    [Export] private float saveRange = 120.0f;            // Distance maximale pour un arrêt réussi

    // Variables privées pour gérer l'état du gardien
    private Vector2 startPosition;                       // Position de départ du gardien
    private bool canMove = true;                         // Indique si le gardien peut bouger
    private bool isAnimating = false;                    // Indique si une animation est en cours
    private AnimationPlayer animationPlayer;             // Référence au nœud AnimationPlayer

    // Méthode appelée une seule fois lorsque le nœud est prêt
    public override void _Ready()
    {
        // Sauvegarde la position initiale du gardien
        startPosition = Position;
        // Récupère le nœud AnimationPlayer qui est un enfant de ce nœud
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
        // Initialise le générateur de nombres aléatoires pour s'assurer que les résultats sont différents à chaque partie
        GD.Randomize();

        // Vérifie si une animation "idle" (attente) existe et la joue si c'est le cas
        if (animationPlayer.HasAnimation("idle"))
        {
            animationPlayer.Play("idle");
        }
        
        // Affiche un message dans la console pour confirmer que le gardien est prêt
        GD.Print("Le gardien est prêt à la position : " + startPosition);
    }

    // Méthode asynchrone pour tenter un arrêt. "async" permet d'utiliser "await" pour faire des pauses.
    public async void AttemptSave(Vector2 ballDirection, float ballSpeed)
    {
        // Affiche des informations sur la tentative d'arrêt
        GD.Print($"Tentative d'arrêt du gardien - Direction du ballon : {ballDirection}, Vitesse : {ballSpeed}");
        
        // Empêche le gardien de bouger et indique qu'une animation commence
        canMove = false;
        isAnimating = true;
        
        // --- LOGIQUE DE DÉCISION ALÉATOIRE ---
        string animationName;
        // Choisit un nombre entier aléatoire entre 0, 1, et 2
        int randomChoice = GD.RandRange(0, 2); 

        // Décide quelle animation jouer en fonction du choix aléatoire
        switch (randomChoice)
        {
            case 0: // Plongeon à gauche
                animationName = "dive_left";
                break;
            case 1: // Plongeon à droite
                animationName = "dive_right";
                break;
            default: // Reste au centre (cas 2)
                // Si une animation "dive_center" existe, vous pouvez l'utiliser ici.
                // Sinon, on utilise l'animation "idle" pour qu'il reste sur place.
                animationName = "idle";
                // Vérifie si l'animation "idle" existe vraiment
                if (!animationPlayer.HasAnimation(animationName))
                {
                    animationName = null; // Ne rien jouer si l'animation "idle" n'existe pas
                }
                break;
        }
        
        // --- SUITE DE LA GESTION DE L'ANIMATION ---
        
        // Joue l'animation de plongeon si une a été choisie et si elle existe
        if (animationName != null && animationPlayer.HasAnimation(animationName))
        {
            GD.Print($"Joue l'animation aléatoire : {animationName}");
            animationPlayer.Play(animationName);
            // Met en pause l'exécution du code jusqu'à la fin de l'animation
            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }
        else
        {
            // Affiche une erreur si l'animation est introuvable
            GD.PrintErr($"L'animation '{animationName}' est introuvable ou n'a pas été choisie !");
            // Même sans animation, on attend un court instant pour simuler une action
            await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
        }
        
        // Calcule si l'arrêt est réussi.
        // NOTE : Ce calcul utilise toujours la direction réelle du ballon.
        // Vous pourriez simplifier ce calcul si le succès de l'arrêt devait aussi être aléatoire.
        bool successful = CalculateSaveSuccess(ballDirection, ballSpeed);
        
        // Attend un court instant avant de réinitialiser le gardien
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        
        // Réinitialise la position du gardien
        ResetPosition();
        
        // Émet le résultat de l'arrêt (réussi ou non) au groupe de nœuds "game_managers"
        GD.Print($"Résultat de l'arrêt : {successful}");
        GetTree().CallGroup("game_managers", "OnGoalkeeperResult", successful);
    }

    // Méthode appelée à chaque image (frame)
    public override void _Process(double delta)
    {
        // Si le gardien ne peut pas bouger ou qu'une animation est en cours, on ne fait rien
        if (!canMove || isAnimating) return;

        Vector2 direction = Vector2.Zero; // Initialise la direction à zéro

        // Gère les déplacements gauche/droite avec les touches du clavier
        if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
            direction.X -= 1;
        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
            direction.X += 1;

        // Si une direction a été entrée
        if (direction != Vector2.Zero)
        {
            // Applique la vitesse à la direction pour déplacer le personnage
            Velocity = direction * moveSpeed;
            
            // Joue l'animation de marche ("walk") si elle existe et n'est pas déjà en cours
            if (animationPlayer.HasAnimation("walk") && !animationPlayer.IsPlaying())
            {
                animationPlayer.Play("walk");
            }
        }
        else // Si aucune touche n'est pressée
        {
            // Arrête le mouvement
            Velocity = Vector2.Zero;
            
            // Retourne à l'animation d'attente ("idle")
            if (animationPlayer.HasAnimation("idle") &&
                (animationPlayer.CurrentAnimation == "walk" || !animationPlayer.IsPlaying()))
            {
                animationPlayer.Play("idle");
            }
        }
        // Applique le mouvement et gère les collisions
        MoveAndSlide();
    }
    
    // Calcule la probabilité de succès d'un arrêt
    private bool CalculateSaveSuccess(Vector2 ballDirection, float ballSpeed)
    {
        // Calcule la position approximative où le ballon va arriver
        Vector2 ballTargetPosition = GlobalPosition + (ballDirection.Normalized() * 200);
        float distanceToTarget = GlobalPosition.DistanceTo(ballTargetPosition);
        
        // Plus la balle est proche, plus l'arrêt est facile
        float distanceFactor = 1.0f - Mathf.Clamp(distanceToTarget / saveRange, 0.0f, 1.0f);
        
        // Plus la balle est rapide, plus l'arrêt est difficile
        float speedFactor = 1.0f - Mathf.Clamp(ballSpeed / 100.0f, 0.0f, 0.8f);
        
        // Facteur basé sur le temps de réaction du gardien
        float reactionFactor = 1.0f - (reactionTime / 2.0f);
        
        // Calcule la chance de succès finale en combinant les facteurs
        float successChance = (distanceFactor * 0.4f) + (speedFactor * 0.4f) + (reactionFactor * 0.2f);
        
        // Ajoute un peu d'aléatoire pour rendre le résultat moins prévisible
        float randomFactor = (float)GD.RandRange(0.0, 0.3);
        successChance += randomFactor;
        
        // Affiche les détails du calcul
        GD.Print($"Calcul de l'arrêt - Distance: {distanceFactor:F2}, Vitesse: {speedFactor:F2}, Réaction: {reactionFactor:F2}, Final: {successChance:F2}");
        
        // L'arrêt est réussi si la chance est supérieure à 0.5 (50%)
        return successChance > 0.5f;
    }

    // Réinitialise le gardien à son état initial
    private void ResetPosition()
    {
        GD.Print("Réinitialisation de la position du gardien");
        
        Position = startPosition; // Revient à la position de départ
        canMove = true;           // Autorise à nouveau le mouvement
        isAnimating = false;      // Indique qu'aucune animation n'est en cours
        
        // Arrête toute animation en cours
        if (animationPlayer.IsPlaying())
        {
            animationPlayer.Stop();
        }
        
        // Rejoue l'animation d'attente ("idle")
        if (animationPlayer.HasAnimation("idle"))
        {
            animationPlayer.Play("idle");
        }
    }

    // Permet d'activer ou de désactiver le mouvement du gardien depuis d'autres scripts
    public void SetCanMove(bool value)
    {
        canMove = value;
        GD.Print($"Le gardien peut bouger : {value}");
        
        // Si le mouvement est désactivé, passe à l'animation "idle"
        if (!value && animationPlayer.HasAnimation("idle"))
        {
            animationPlayer.Play("idle");
        }
    }

    // Méthode pour tester une animation spécifique (utile pour le débogage)
    public void TestAnimation(string animationName)
    {
        if (animationPlayer.HasAnimation(animationName))
        {
            isAnimating = true;
            animationPlayer.Play(animationName);
            GD.Print($"Test de l'animation : {animationName}");
        }
        else
        {
            GD.PrintErr($"L'animation '{animationName}' est introuvable pour le test !");
        }
    }

    // Méthode pour lister toutes les animations disponibles pour ce gardien
    public void ListAvailableAnimations()
    {
        var animations = animationPlayer.GetAnimationList();
        GD.Print("Animations disponibles :");
        foreach (string anim in animations)
        {
            GD.Print($"- {anim}");
        }
    }
}