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
        powerBar.Value = 0;
        
        GD.Print("PenaltyShooter prêt");
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
                if (mouseButton.Pressed && !chargingPower)
                {
                    StartChargingPower();
                }
                else if (!mouseButton.Pressed && chargingPower)
                {
                    Shoot();
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        if (chargingPower && canShoot)
        {
            currentPower += powerIncreaseSpeed * (float)delta;
            currentPower = Mathf.Clamp(currentPower, 0, maxPower);
            powerBar.Value = currentPower;
            
            GD.Print($"Power charging: {currentPower}");
        }
    }

    private void UpdateAim(Vector2 mousePosition)
    {
        if (!canShoot) return;
        
        Vector2 worldMousePos = GetGlobalMousePosition();
        aimDirection = (worldMousePos - GlobalPosition).Normalized();
        
        // Mettre à jour la ligne de visée
        aimLine.ClearPoints();
        aimLine.AddPoint(Vector2.Zero);
        aimLine.AddPoint(aimDirection * 100);
        
        // Changer la couleur de la ligne
        aimLine.DefaultColor = Colors.Red;
    }

    private void StartChargingPower()
    {
        GD.Print("Start charging power");
        chargingPower = true;
        currentPower = 0.0f;
        powerBar.Value = 0.0f;
    }

    private void Shoot()
    {
        if (!chargingPower || !canShoot) return;
        
        GD.Print($"SHOOTING! Direction: {aimDirection}, Power: {currentPower}");
        
        // Arrêter le chargement
        chargingPower = false;
        
        // Émettre le signal avec les bonnes valeurs
        EmitSignal(SignalName.ShotTaken, aimDirection, currentPower);
        
        // Désactiver temporairement
        SetCanShoot(false);
        
        // Reset l'interface
        ResetUI();
    }

    private void ResetUI()
    {
        currentPower = 0.0f;
        powerBar.Value = 0.0f;
        aimLine.ClearPoints();
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
        Visible = value;
        
        if (!value)
        {
            chargingPower = false;
            ResetUI();
        }
        
        GD.Print($"Can shoot set to: {value}");
    }
}