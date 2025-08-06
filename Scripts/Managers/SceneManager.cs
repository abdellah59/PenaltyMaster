using Godot;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    
    [Signal] public delegate void SceneChangedEventHandler(string sceneName);
    
    private string currentSceneName;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            currentSceneName = GetTree().CurrentScene.Name;
        }
        else
        {
            QueueFree();
        }
    }

    public void ChangeScene(string scenePath)
    {
        var error = GetTree().ChangeSceneToFile(scenePath);
        if (error == Error.Ok)
        {
            string sceneName = scenePath.GetFile().GetBaseName();
            currentSceneName = sceneName;
            EmitSignal(SignalName.SceneChanged, sceneName);
            
            // Jouer la musique appropriée
            PlaySceneMusic(sceneName);
        }
        else
        {
            GD.PrintErr($"Erreur lors du changement de scène: {error}");
        }
    }

    private void PlaySceneMusic(string sceneName)
    {
        if (SoundManager.Instance == null) return;
        
        switch (sceneName.ToLower())
        {
            case "main":
            case "teamselection":
                SoundManager.Instance.PlayMusic("menu_music");
                break;
            case "penaltygame":
                SoundManager.Instance.PlayMusic("game_music");
                break;
            default:
                SoundManager.Instance.StopMusic();
                break;
        }
    }

    public string GetCurrentSceneName()
    {
        return currentSceneName;
    }
}
