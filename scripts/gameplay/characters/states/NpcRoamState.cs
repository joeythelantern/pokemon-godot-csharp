using Game.Gameplay;
using Game.Utilities;
using Godot;

public partial class NpcRoamState : State
{
    [ExportCategory("State Vars")]
    [Export]
    public NpcInput NpcInput;

    [Export]
    public CharacterMovement CharacterMovement;

    public override void _Ready()
    {

    }

    public override void _Process(double delta)
    {

    }
}
