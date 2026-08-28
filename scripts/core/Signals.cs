using Godot;

namespace Game.Core;

public partial class Signals : Node
{
    public static Signals Instance { get; private set; }

    [Signal] public delegate void MessageBoxOpenEventHandler(bool value);
    [Signal] public delegate void ItemReceivedEventHandler(string itemId, string itemName, int quantity);
    [Signal] public delegate void InventoryChangedEventHandler();

    public override void _Ready()
    {
        Instance = this;

        Logger.Info("Loading Global Signals ...");
    }

    public static void EmitGlobalSignal(StringName signal, params Variant[] args)
    {
        Logger.Info("Global signal emitted: ", signal, args);
        Instance.EmitSignal(signal, args);
    }
}
