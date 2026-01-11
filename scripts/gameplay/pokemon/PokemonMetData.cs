using Godot;

namespace Game.Gameplay;

[GlobalClass]
public partial class PokemonMetData : Resource
{
    [Export]
    public string OriginalTrainer { get; private set; }

    [Export]
    public string Location { get; private set; }

    [Export]
    public int LevelMet { get; private set; }

    [Export]
    public float UnixTimeMet { get; private set; }
}
