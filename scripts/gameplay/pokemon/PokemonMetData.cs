using Godot;

namespace Game.Gameplay;

[GlobalClass]
public partial class PokemonMetData : Resource
{
    [Export]
    public string OriginalTrainer;

    [Export]
    public string Location;

    [Export]
    public int LevelMet;

    [Export]
    public double UnixTimeMet;
}
