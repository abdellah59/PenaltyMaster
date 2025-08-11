using Godot;
using System;

public partial class Ball : RigidBody2D
{
    [Signal] public delegate void GoalScoredEventHandler();
    [Signal] public delegate void ShotMissedEventHandler();
    [Signal] public delegate void BallStoppedEventHandler();

    private Vector2 startPosition;
    private bool isActive = false;
    private Timer stopCheckTimer;

    public override void _Ready()
    {
        GD.Print("Ball ready");

        // Sauvegarder la position de départ
        startPosition = GlobalPosition;

        // Timer pour vérifier si la balle est arrêtée
        stopCheckTimer = new Timer
        {
            WaitTime = 0.1f,
            OneShot = false
        };
        stopCheckTimer.Timeout += CheckBallStopped;
        AddChild(stopCheckTimer);

        SetActive(false);
    }

    public void ShootBall(Vector2 direction, float power)
    {
        GD.Print($"Ball shooting! Direction: {direction}, Power: {power}");

        // S'assurer que la physique est active
        Freeze = false;

        // Normaliser la direction pour éviter un vecteur nul
        if (direction.Length() < 0.01f)
        {
            GD.PrintErr("Direction de tir trop faible, tir annulé.");
            return;
        }
        direction = direction.Normalized();

        SetActive(true);

        // Appliquer l'impulsion après un frame
        CallDeferred(nameof(ApplyForceDeferred), direction, power);
    }

    private void ApplyForceDeferred(Vector2 direction, float power)
    {
        Freeze = false;

        // Réinitialiser les vitesses
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0.0f;

        // Calcul de la force
        Vector2 force = direction * power * 100; // 100 = multiplicateur ajustable

        // Appliquer l'impulsion
        ApplyCentralImpulse(force);

        // Faire tourner la balle pendant le vol (effet visuel simple)
        AngularVelocity = direction.X * -5f; // rotation selon le côté du tir

        // Lancer le timer pour vérifier l'arrêt
        stopCheckTimer.Start();

        GD.Print($"Applied force: {force}");
    }

    private void CheckBallStopped()
    {
        if (!isActive) return;

        float velocity = LinearVelocity.Length();

        if (velocity < 5.0f) // seuil de vitesse pour considérer la balle arrêtée
        {
            GD.Print("Ball stopped");
            stopCheckTimer.Stop();
            EmitSignal(SignalName.BallStopped);

            // Réinitialiser après un délai plus court
            GetTree().CreateTimer(0.5f).Timeout += ResetBall;
        }
    }

    public void ResetBall()
    {
        GD.Print($"Ball reset - Moving from {GlobalPosition} to {startPosition}");
        
        // Arrêter toute physique
        Freeze = true;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0.0f;
        
        // Repositionner à la position de départ
        GlobalPosition = startPosition;
        
        // Désactiver la balle
        SetActive(false);
        stopCheckTimer.Stop();
         
        GD.Print($"Ball position after reset: {GlobalPosition}");
    }

    // Nouvelle méthode pour repositionner manuellement la balle
    public void SetPosition(Vector2 newPosition)
    {
        startPosition = newPosition;
        GlobalPosition = newPosition;
        GD.Print($"Ball position manually set to: {GlobalPosition}");
    }

    public void SetActive(bool active)
    {
        isActive = active;
        Visible = active;

        if (active)
        {
            Freeze = false;
            GD.Print("Ball activated and unfreezed");
        }
        else
        {
            Freeze = true;
            GD.Print("Ball deactivated and freezed");
        }

        GD.Print($"Ball active: {active}, Freeze: {Freeze}");
    }

    public void OnGoalAreaEntered()
    {
        if (isActive)
        {
            GD.Print("Goal scored from ball!");
            EmitSignal(SignalName.GoalScored);
            stopCheckTimer.Stop();
            
            // Réinitialiser immédiatement après un but
            GetTree().CreateTimer(1.0f).Timeout += ResetBall;
        }
    }
}