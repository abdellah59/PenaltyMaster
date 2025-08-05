using Godot;

public partial class GameAutoload : Node
{
    public override void _Ready()
    {
        // Initialiser les managers globaux
        var gameData = new GameData();
        gameData.Name = "GameData";
        AddChild(gameData);
        
        var gameManager = new GameManager();
        gameManager.Name = "GameManager";
        AddChild(gameManager);
        
        var soundManager = new SoundManager();
        soundManager.Name = "SoundManager";
        AddChild(soundManager);
        
        var sceneManager = new SceneManager();
        sceneManager.Name = "SceneManager";
        AddChild(sceneManager);
        
        var settingsManager = new SettingsManager();
        settingsManager.Name = "SettingsManager";
        AddChild(settingsManager);
        
        GD.Print("Autoload initialisé avec succès");
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            // Sauvegarder avant de quitter
            SettingsManager.Instance?.SaveSettings();
            GetTree().Quit();
        }
    }
}