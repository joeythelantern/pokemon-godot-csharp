using Godot;

namespace Game.Core;

public partial class Globals : Node
{
    public static Globals Instance { get; private set; }

    public const int GRID_SIZE = 16;
    public const int MOVE_NUMBERS = 165;
    public const int POKEMON_NUMBERS = 151;

    [ExportCategory("Gameplay")]
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