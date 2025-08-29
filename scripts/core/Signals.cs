using Godot;

namespace Game.Core;

public partial class Signals : Node
{
    public static Signals Instance { get; private set; }

    [Signal] public delegate void MessageBoxOpenEventHandler(bool value);

    public override void _Ready()
    {
        Instance = this;

        Logger.Info("Loading Global Signals ...");
    }

    public static void EmitGlobalSignal(StringName signal, params Variant[] args)
    {
        Instance.EmitSignal(signal, args);
    }
}