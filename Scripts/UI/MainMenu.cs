using Godot;

public partial class MainMenu : Control
{
    private Button playButton;
    private Button quitButton;
    private Label titleLabel;

    public override void _Ready()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        playButton = GetNode<Button>("VBoxContainer/PlayButton");
        quitButton = GetNode<Button>("VBoxContainer/QuitButton");
        titleLabel = GetNode<Label>("VBoxContainer/TitleLabel");
        
        playButton.Pressed += OnPlayPressed;
        quitButton.Pressed += OnQuitPressed;
        
        titleLabel.Text = "🏆 TIRS AU BUT 🏆\nCoupe du Monde";
        playButton.Text = "JOUER";
        quitButton.Text = "QUITTER";
        
        // Style des boutons
        StyleButtons();
    }

    private void StyleButtons()
    {
        var buttonStyle = new StyleBoxFlat();
        buttonStyle.BgColor = new Color(0.2f, 0.6f, 0.2f);
        buttonStyle.BorderColor = Colors.White;
        buttonStyle.BorderWidthTop = 2;
        buttonStyle.BorderWidthBottom = 2;
        buttonStyle.BorderWidthLeft = 2;
        buttonStyle.BorderWidthRight = 2;
        buttonStyle.CornerRadiusTopLeft = 10;
        buttonStyle.CornerRadiusTopRight = 10;
        buttonStyle.CornerRadiusBottomLeft = 10;
        buttonStyle.CornerRadiusBottomRight = 10;
        
        playButton.AddThemeStyleboxOverride("normal", buttonStyle);
        playButton.AddThemeColorOverride("font_color", Colors.White);
        
        var quitStyle = new StyleBoxFlat();
        quitStyle.BgColor = new Color(0.6f, 0.2f, 0.2f);
        quitStyle.BorderColor = Colors.White;
        quitStyle.BorderWidthTop = 2;
        quitStyle.BorderWidthBottom = 2;
        quitStyle.BorderWidthLeft = 2;
        quitStyle.BorderWidthRight = 2;
        quitStyle.CornerRadiusTopLeft = 10;
        quitStyle.CornerRadiusTopRight = 10;
        quitStyle.CornerRadiusBottomLeft = 10;
        quitStyle.CornerRadiusBottomRight = 10;
        
        quitButton.AddThemeStyleboxOverride("normal", quitStyle);
        quitButton.AddThemeColorOverride("font_color", Colors.White);
    }

    private void OnPlayPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/TeamSelection.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}