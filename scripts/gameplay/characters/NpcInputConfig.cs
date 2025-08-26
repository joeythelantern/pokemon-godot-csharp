using Game.Core;
using Godot;
using Godot.Collections;

[GlobalClass]
[Tool]
public partial class NpcInputConfig : Resource
{
    [ExportGroup("Movement")]
    [ExportSubgroup("Common")]
    [Export]
    public NpcMovementType NpcMovementType = NpcMovementType.Static;

    [ExportSubgroup("Wander")]
    [Export]
    public Vector2 WanderOrigin = Vector2.Zero;

    [Export]
    public double WanderRadius = 64;

    [Export]
    public double WanderMoveInterval = 2;

    [ExportSubgroup("Patrol")]
    [Export]
    public Array<Vector2> PatrolPoints;

    [Export]
    public double PatrolMoveInterval = 2;

    [Export]
    public int PatrolIndex = 0;

    [ExportSubgroup("Look Around")]
    [Export]
    public double LookAroundInterval = 3;
}
