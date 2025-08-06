using Godot;

public partial class PenaltyShooter : Node2D
{
    [Signal] public delegate void ShotTakenEventHandler(Vector2 direction, float power);
    
    [Export] private float maxPower = 100.0f;
    [Export] private float powerIncreaseSpeed = 50.0f;
    
    private Vector2 aimDirection = Vector2.Zero;
    private float currentPower = 0.0f;
    private bool chargingPower = false;
    private bool canShoot = true;
    
    private Line2D aimLine;
    private ProgressBar powerBar;

    public override void _Ready()
    {
        // Initialiser les UI elements
        aimLine = GetNode<Line2D>("AimLine");
        powerBar = GetNode<ProgressBar>("PowerBar");
        
        powerBar.MaxValue = maxPower;
        SetCanShoot(false);
    }

    public override void _Input(InputEvent @event)
    {
        if (!canShoot) return;

        if (@event is InputEventMouseMotion mouseMotion)
        {
            UpdateAim(mouseMotion.Position);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    StartChargingPower();
                }
                else
                {
                    Shoot();
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (chargingPower)
        {
            currentPower += powerIncreaseSpeed * (float)delta;
            currentPower = Mathf.Clamp(currentPower, 0, maxPower);
            powerBar.Value = currentPower;
        }
    }

    private void UpdateAim(Vector2 mousePosition)
    {
        Vector2 worldMousePos = GetGlobalMousePosition();
        aimDirection = (worldMousePos - GlobalPosition).Normalized();
        
        // Mettre à jour la ligne de visée
        aimLine.ClearPoints();
        aimLine.AddPoint(Vector2.Zero);
        aimLine.AddPoint(aimDirection * 100);
    }

    private void StartChargingPower()
    {
        chargingPower = true;
        currentPower = 0.0f;
    }

    private void Shoot()
    {
        if (!chargingPower) return;
        
        chargingPower = false;
        SetCanShoot(false);
        
        EmitSignal(SignalName.ShotTaken, aimDirection, currentPower);
        
        // Reset
        currentPower = 0.0f;
        powerBar.Value = 0.0f;
        aimLine.ClearPoints();
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
        Visible = value;
    }
}