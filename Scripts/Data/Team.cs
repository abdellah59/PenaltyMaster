using Godot;

[System.Serializable]
public partial class Team : Resource
{
    [Export] public string Name { get; set; }
    [Export] public string Country { get; set; }
    [Export] public Texture2D Flag { get; set; }
    [Export] public Color PrimaryColor { get; set; }
    [Export] public Color SecondaryColor { get; set; }
    [Export] public int Ranking { get; set; }

    public Team()
    {
        Name = "";
        Country = "";
        PrimaryColor = Colors.Blue;
        SecondaryColor = Colors.White;
        Ranking = 1;
    }

    public Team(string name, string country, Color primary, Color secondary, int ranking = 1)
    {
        Name = name;
        Country = country;
        PrimaryColor = primary;
        SecondaryColor = secondary;
        Ranking = ranking;
    }
}