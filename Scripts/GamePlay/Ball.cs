using Godot;

public partial class Ball : RigidBody2D
{
    [Signal] public delegate void GoalScoredEventHandler();
    [Signal] public delegate void ShotMissedEventHandler();
    [Signal] public delegate void BallStoppedEventHandler();

    private Vector2 startPosition;
    private bool isActive = false;

    public override void _Ready()
    {
        startPosition = Position;
        SetActive(false);
    }

    public void ShootBall(Vector2 direction, float power)
    {
        SetActive(true);
        
        // Appliquer la force
        Vector2 force = direction * power * 10;
        ApplyImpulse(force);
        
        // Démarrer le timer pour vérifier l'arrêt
        GetTree().CreateTimer(3.0f).Timeout += CheckBallStopped;
    }

    private void CheckBallStopped()
    {
        if (LinearVelocity.Length() < 10.0f)
        {
            EmitSignal(SignalName.BallStopped);
            ResetBall();
        }
    }

    public void ResetBall()
    {
        Position = startPosition;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0.0f;
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;
        Visible = active;
        SetDeferred("freeze", !active);
    }

    private void _on_goal_area_entered(Area2D area)
    {
        if (area.IsInGroup("goal") && isActive)
        {
            EmitSignal(SignalName.GoalScored);
            ResetBall();
        }
    }
}