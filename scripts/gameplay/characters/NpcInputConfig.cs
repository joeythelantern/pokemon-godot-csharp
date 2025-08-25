using Game.Core;
using Godot;
using Godot.Collections;

[GlobalClass]
[Tool]
public partial class NpcInputConfig : Resource
{
    [Export]
    public NpcMovementType NpcMovementType = NpcMovementType.Static;

    [ExportCategory("Wander")]
    [Export]
    public float WanderRadius;

    [Export]
    public float WanderMoveInterval = 2f;

    [ExportCategory("Patrol")]
    [Export]
    public Array<Vector2> PatrolPoints;

    [Export]
    public float PatrolMoveInterval = 2f;

    [Export]
    public int PatrolIndex = 0;

    [ExportCategory("Look Around")]
    [Export]
    public float LookAroundInterval = 1.5f;
}
