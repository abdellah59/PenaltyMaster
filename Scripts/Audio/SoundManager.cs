using Godot;
using System.Collections.Generic;

public partial class SoundManager : Node
{
    public static SoundManager Instance { get; private set; }
    
    private Dictionary<string, AudioStream> sounds = new Dictionary<string, AudioStream>();
    private AudioStreamPlayer2D sfxPlayer;
    private AudioStreamPlayer musicPlayer;
    
    [Export] private float masterVolume = 1.0f;
    [Export] private float sfxVolume = 0.8f;
    [Export] private float musicVolume = 0.6f;

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAudio();
            LoadSounds();
        }
        else
        {
            QueueFree();
        }
    }

    private void InitializeAudio()
    {
        sfxPlayer = new AudioStreamPlayer2D();
        musicPlayer = new AudioStreamPlayer();
        
        AddChild(sfxPlayer);
        AddChild(musicPlayer);
        
        musicPlayer.VolumeDb = Mathf.LinearToDb(musicVolume * masterVolume);
        sfxPlayer.VolumeDb = Mathf.LinearToDb(sfxVolume * masterVolume);
    }

    private void LoadSounds()
    {
        // Charger les sons (à adapter selon vos fichiers audio)
        sounds["kick"] = GD.Load<AudioStream>("res://audio/kick.ogg");
        sounds["goal"] = GD.Load<AudioStream>("res://audio/goal.ogg");
        sounds["save"] = GD.Load<AudioStream>("res://audio/save.ogg");
        sounds["whistle"] = GD.Load<AudioStream>("res://audio/whistle.ogg");
        sounds["crowd_cheer"] = GD.Load<AudioStream>("res://audio/crowd_cheer.ogg");
        sounds["crowd_disappointed"] = GD.Load<AudioStream>("res://audio/crowd_disappointed.ogg");
        
        // Musique de fond
        sounds["menu_music"] = GD.Load<AudioStream>("res://audio/menu_music.ogg");
        sounds["game_music"] = GD.Load<AudioStream>("res://audio/game_music.ogg");
    }

    public void PlaySFX(string soundName)
    {
        if (sounds.ContainsKey(soundName) && sounds[soundName] != null)
        {
            sfxPlayer.Stream = sounds[soundName];
            sfxPlayer.Play();
        }
        else
        {
            GD.PrintErr($"Son non trouvé: {soundName}");
        }
    }

    public void PlayMusic(string musicName, bool loop = true)
    {
        if (sounds.ContainsKey(musicName) && sounds[musicName] != null)
        {
            musicPlayer.Stream = sounds[musicName];
            
            if (sounds[musicName] is AudioStreamOggVorbis oggStream)
            {
                oggStream.Loop = loop;
            }
            
            musicPlayer.Play();
        }
        else
        {
            GD.PrintErr($"Musique non trouvée: {musicName}");
        }
    }

    public void StopMusic()
    {
        musicPlayer.Stop();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp(volume, 0.0f, 1.0f);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        musicPlayer.VolumeDb = Mathf.LinearToDb(musicVolume * masterVolume);
        sfxPlayer.VolumeDb = Mathf.LinearToDb(sfxVolume * masterVolume);
    }
}
