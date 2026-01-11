using Godot;
using Game.Gameplay;

namespace Game.Gameplay;

public partial class MoveInstance : Node
{
    [Export] public MoveResource Base { get; private set; }

    [Export] public int CurrentPP { get; private set; }

    public void Initialize(MoveResource move)
    {
        Base = move;
        CurrentPP = move.PP;

        Name = move.Name;
    }
}
