using Godot;

public partial class Goalkeeper : CharacterBody2D
{
    [Export] private float moveSpeed = 300.0f;
    [Export] private float reactionTime = 0.5f;
    
    private Vector2 startPosition;
    private bool canMove = true;
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        startPosition = Position;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(double delta)
    {
        if (!canMove) return;

        Vector2 direction = Vector2.Zero;
        
        if (Input.IsActionPressed("move_left"))
            direction.X -= 1;
        if (Input.IsActionPressed("move_right"))
            direction.X += 1;

        if (direction != Vector2.Zero)
        {
            Velocity = direction * moveSpeed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }
        
        MoveAndSlide();
    }

    public async void AttemptSave(Vector2 ballDirection, float ballSpeed)
    {
        canMove = false;
        
        // Animation de plongeon vers la direction du ballon
        string animationName = ballDirection.X > 0 ? "dive_right" : "dive_left";
        
        if (animationPlayer.HasAnimation(animationName))
        {
            animationPlayer.Play(animationName);
        }
        
        // Calculer si l'arrêt est réussi
        float distance = Position.DistanceTo(ballDirection * 200);
        bool successful = distance < 100; // Zone d'arrêt
        
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
        
        ResetPosition();
        
        // Émettre le résultat
        GetTree().CallGroup("game_managers", "OnGoalkeeperResult", successful);
    }

    private void ResetPosition()
    {
        Position = startPosition;
        canMove = true;
        
        if (animationPlayer.IsPlaying())
        {
            animationPlayer.Stop();
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}