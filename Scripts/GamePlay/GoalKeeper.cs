using Godot;

public partial class GoalKeeper : CharacterBody2D
{
    [Export] private float moveSpeed = 300.0f;
    [Export] private float reactionTime = 0.5f;
    [Export] private float saveRange = 120.0f; // Distance maximale pour un arrêt réussi
    
    private Vector2 startPosition;
    private bool canMove = true;
    private bool isAnimating = false;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        startPosition = Position;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
        // Démarrer l'animation idle si elle existe
        if (animationPlayer.HasAnimation("idle"))
        {
            animationPlayer.Play("idle");
        }
        
        GD.Print("Goalkeeper ready at position: " + startPosition);
    }

    public override void _Process(double delta)
    {
        if (!canMove || isAnimating) return;

        Vector2 direction = Vector2.Zero;
        
        // Contrôles clavier (vous pouvez ajouter des actions dans Input Map)
        if (Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A))
            direction.X -= 1;
        if (Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D))
            direction.X += 1;

        if (direction != Vector2.Zero)
        {
            Velocity = direction * moveSpeed;
            
            // Animation de marche (si vous l'avez créée)
            if (animationPlayer.HasAnimation("walk") && !animationPlayer.IsPlaying())
            {
                animationPlayer.Play("walk");
            }
        }
        else
        {
            Velocity = Vector2.Zero;
            
            // Retour à l'animation idle
            if (animationPlayer.HasAnimation("idle") && 
                (animationPlayer.CurrentAnimation == "walk" || !animationPlayer.IsPlaying()))
            {
                animationPlayer.Play("idle");
            }
        }
        
        MoveAndSlide();
    }

    public async void AttemptSave(Vector2 ballDirection, float ballSpeed)
    {
        GD.Print($"Goalkeeper attempting save - Ball direction: {ballDirection}, Speed: {ballSpeed}");
        
        canMove = false;
        isAnimating = true;
        
        // Déterminer la direction du plongeon
        string animationName;
        if (ballDirection.X > 0.1f) // Vers la droite
        {
            animationName = "dive_right";
        }
        else if (ballDirection.X < -0.1f) // Vers la gauche
        {
            animationName = "dive_left";
        }
        else // Tir au centre
        {
            animationName = "dive_center"; // Si vous l'avez créée, sinon utiliser dive_left ou dive_right
            if (!animationPlayer.HasAnimation(animationName))
            {
                animationName = "dive_left"; // Animation de secours
            }
        }
        
        // Jouer l'animation de plongeon
        if (animationPlayer.HasAnimation(animationName))
        {
            GD.Print($"Playing animation: {animationName}");
            animationPlayer.Play(animationName);
        }
        else
        {
            GD.PrintErr($"Animation '{animationName}' not found!");
        }
        
        // Calculer si l'arrêt est réussi
        bool successful = CalculateSaveSuccess(ballDirection, ballSpeed);
        
        // Attendre la fin de l'animation
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        
        // Petit délai avant de reset
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        
        // Reset position
        ResetPosition();
        
        // Émettre le résultat
        GD.Print($"Save result: {successful}");
        GetTree().CallGroup("game_managers", "OnGoalkeeperResult", successful);
    }

    private bool CalculateSaveSuccess(Vector2 ballDirection, float ballSpeed)
    {
        // Facteurs qui influencent la réussite de l'arrêt
        Vector2 ballTargetPosition = GlobalPosition + (ballDirection.Normalized() * 200); // Position approximative où la balle va
        float distanceToTarget = GlobalPosition.DistanceTo(ballTargetPosition);
        
        // Plus la balle est proche du gardien, plus c'est facile à arrêter
        float distanceFactor = 1.0f - Mathf.Clamp(distanceToTarget / saveRange, 0.0f, 1.0f);
        
        // Plus la balle est rapide, plus c'est difficile à arrêter
        float speedFactor = 1.0f - Mathf.Clamp(ballSpeed / 100.0f, 0.0f, 0.8f);
        
        // Facteur de réaction du gardien
        float reactionFactor = 1.0f - (reactionTime / 2.0f);
        
        // Chance finale de réussite
        float successChance = (distanceFactor * 0.4f) + (speedFactor * 0.4f) + (reactionFactor * 0.2f);
        
        // Ajouter un peu de randomness
        float randomFactor = (float)GD.RandRange(0.0, 0.3);
        successChance += randomFactor;
        
        GD.Print($"Save calculation - Distance: {distanceFactor:F2}, Speed: {speedFactor:F2}, Reaction: {reactionFactor:F2}, Final: {successChance:F2}");
        
        return successChance > 0.5f;
    }

    private void ResetPosition()
    {
        GD.Print("Resetting goalkeeper position");
        
        Position = startPosition;
        canMove = true;
        isAnimating = false;
        
        // Arrêter l'animation en cours et revenir à idle
        if (animationPlayer.IsPlaying())
        {
            animationPlayer.Stop();
        }
        
        if (animationPlayer.HasAnimation("idle"))
        {
            animationPlayer.Play("idle");
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        GD.Print($"Goalkeeper can move: {value}");
        
        if (!value && animationPlayer.HasAnimation("idle"))
        {
            animationPlayer.Play("idle");
        }
    }

    // Méthode pour tester les animations
    public void TestAnimation(string animationName)
    {
        if (animationPlayer.HasAnimation(animationName))
        {
            isAnimating = true;
            animationPlayer.Play(animationName);
            GD.Print($"Testing animation: {animationName}");
        }
        else
        {
            GD.PrintErr($"Animation '{animationName}' not found for testing!");
        }
    }

    // Méthode utilitaire pour lister les animations disponibles
    public void ListAvailableAnimations()
    {
        var animations = animationPlayer.GetAnimationList();
        GD.Print("Available animations:");
        foreach (string anim in animations)
        {
            GD.Print($"- {anim}");
        }
    }
}