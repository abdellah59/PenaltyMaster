using Godot;

public partial class SettingsManager : Node
{
    public static SettingsManager Instance { get; private set; }
    
    private const string SETTINGS_FILE = "user://settings.cfg";
    
    // Paramètres par défaut
    public float MasterVolume { get; private set; } = 1.0f;
    public float SFXVolume { get; private set; } = 0.8f;
    public float MusicVolume { get; private set; } = 0.6f;
    public bool Fullscreen { get; private set; } = false;
    public string Language { get; private set; } = "fr";
    public float AIDifficulty { get; private set; } = 0.5f;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadSettings();
            ApplySettings();
        }
        else
        {
            QueueFree();
        }
    }

    private void LoadSettings()
    {
        var config = new ConfigFile();
        var error = config.Load(SETTINGS_FILE);
        
        if (error == Error.Ok)
        {
            MasterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            SFXVolume = (float)config.GetValue("audio", "sfx_volume", 0.8f);
            MusicVolume = (float)config.GetValue("audio", "music_volume", 0.6f);
            Fullscreen = (bool)config.GetValue("video", "fullscreen", false);
            Language = (string)config.GetValue("game", "language", "fr");
            AIDifficulty = (float)config.GetValue("game", "ai_difficulty", 0.5f);
        }
    }

    public void SaveSettings()
    {
        var config = new ConfigFile();
        
        config.SetValue("audio", "master_volume", MasterVolume);
        config.SetValue("audio", "sfx_volume", SFXVolume);
        config.SetValue("audio", "music_volume", MusicVolume);
        config.SetValue("video", "fullscreen", Fullscreen);
        config.SetValue("game", "language", Language);
        config.SetValue("game", "ai_difficulty", AIDifficulty);
        
        var error = config.Save(SETTINGS_FILE);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Erreur lors de la sauvegarde des paramètres: {error}");
        }
    }

    private void ApplySettings()
    {
        // Appliquer les paramètres audio
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(MasterVolume);
            SoundManager.Instance.SetSFXVolume(SFXVolume);
            SoundManager.Instance.SetMusicVolume(MusicVolume);
        }
        
        // Appliquer les paramètres vidéo
        if (Fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        SoundManager.Instance?.SetMasterVolume(MasterVolume);
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        SoundManager.Instance?.SetSFXVolume(SFXVolume);
        SaveSettings();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        SoundManager.Instance?.SetMusicVolume(MusicVolume);
        SaveSettings();
    }

    public void SetFullscreen(bool fullscreen)
    {
        Fullscreen = fullscreen;
        
        if (fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
        
        SaveSettings();
    }

    public void SetAIDifficulty(float difficulty)
    {
        AIDifficulty = Mathf.Clamp(difficulty, 0.0f, 1.0f);
        SaveSettings();
    }
}