using Godot;
using System;
using System.Threading.Tasks;

public partial class OpponentAI : Node
{
    [Export] private float shootingAccuracy = 0.7f; // Précision de 70%
    [Export] private float goalkeeperReflexes = 0.6f; // Réflexes de 60%
    [Export] private float decisionDelay = 1.5f; // Délai de décision en secondes

    public async Task<bool> TakeShot()
    {
        // Simuler le temps de préparation
        await ToSignal(GetTree().CreateTimer(decisionDelay), SceneTreeTimer.SignalName.Timeout);
        
        // Calculer si le tir est réussi
        float random = GD.Randf();
        bool scored = random < shootingAccuracy;
        
        // Animation/effet visuel du tir de l'adversaire
        ShowOpponentShot(scored);
        
        return scored;
    }

    public async Task<bool> AttemptSave(Vector2 ballDirection, float ballSpeed)
    {
        // Simuler le temps de réaction
        await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
        
        // Calculer si l'arrêt est réussi
        float saveChance = goalkeeperReflexes;
        
        // Ajuster la chance selon la direction et la puissance
        float directionFactor = Mathf.Abs(ballDirection.X); // Plus difficile sur les côtés
        float speedFactor = Mathf.Clamp(ballSpeed / 100.0f, 0.5f, 1.5f); // Plus difficile si rapide
        
        saveChance = saveChance * (1.0f - directionFactor * 0.3f) / speedFactor;
        
        float random = GD.Randf();
        bool saved = random < saveChance;
        
        ShowOpponentSave(saved, ballDirection);
        
        return saved;
    }

    private void ShowOpponentShot(bool scored)
    {
        // Créer un effet visuel pour le tir de l'adversaire
        var label = new Label();
        label.Text = scored ? "⚽ BUT!" : "❌ RATÉ!";
        label.AddThemeColorOverride("font_color", scored ? Colors.Green : Colors.Red);
        label.AddThemeStyleboxOverride("normal", new StyleBoxFlat());
        
        // Positionner au centre de l'écran
        var viewport = GetViewport();
        label.Position = viewport.GetVisibleRect().Size / 2;
        label.AnchorLeft = 0.5f;
        label.AnchorTop = 0.5f;
        
        GetTree().CurrentScene.AddChild(label);
        
        // Animation et suppression
        var tween = CreateTween();
        tween.TweenProperty(label, "modulate:a", 0.0f, 2.0f);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    private void ShowOpponentSave(bool saved, Vector2 ballDirection)
    {
        var label = new Label();
        label.Text = saved ? "🥅 ARRÊT!" : "⚽ BUT!";
        label.AddThemeColorOverride("font_color", saved ? Colors.Blue : Colors.Red);
        
        var viewport = GetViewport();
        label.Position = viewport.GetVisibleRect().Size / 2;
        label.AnchorLeft = 0.5f;
        label.AnchorTop = 0.5f;
        
        GetTree().CurrentScene.AddChild(label);
        
        var tween = CreateTween();
        tween.TweenProperty(label, "modulate:a", 0.0f, 2.0f);
        tween.TweenCallback(Callable.From(label.QueueFree));
    }

    public void SetDifficulty(float difficulty)
    {
        // Ajuster la difficulté (0.0 = facile, 1.0 = difficile)
        shootingAccuracy = 0.4f + (difficulty * 0.5f); // 40% à 90%
        goalkeeperReflexes = 0.3f + (difficulty * 0.6f); // 30% à 90%
        decisionDelay = 2.0f - (difficulty * 1.0f); // 2s à 1s
    }
}
