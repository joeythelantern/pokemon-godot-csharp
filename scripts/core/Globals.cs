using Godot;

namespace Game.Core;

public partial class Globals : Node
{
    public static Globals Instance { get; private set; }

    [ExportCategory("Gameplay")]
    [Export]
    public int GRID_SIZE = 16;

    [Export]
    public ulong Seed = 1337;

    private RandomNumberGenerator RandomNumberGenerator;

    public override void _Ready()
    {
        Instance = this;

        RandomNumberGenerator = new()
        {
            Seed = Seed
        };

        Logger.Info("Loading Globals ...");
    }

    public static RandomNumberGenerator GetRandomNumberGenerator()
    {
        return Instance.RandomNumberGenerator;
    }
}