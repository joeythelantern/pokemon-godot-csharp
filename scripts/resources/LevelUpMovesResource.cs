using Godot;

namespace Game.Resources;

[GlobalClass]
[Tool]
public partial class LevelUpMoveResource : Resource
{
    [Export]
    public int Level;

    [Export]
    public MoveResource Move;
}