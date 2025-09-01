using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

[GlobalClass]
[Tool]
public partial class NpcInputConfig : Resource
{
    [ExportGroup("Movement")]
    [ExportSubgroup("Common")]
    [Export]
    public NpcMovementType NpcMovementType = NpcMovementType.Static;

    [Export]
    public Array<string> Messages;

    [ExportSubgroup("Wander")]
    [Export]
    public Vector2 WanderOrigin = Vector2.Zero;

    [Export]
    public double WanderRadius = 64f;

    [Export]
    public double WanderMoveInterval = 2f;

    [ExportSubgroup("Patrol")]
    [Export]
    public Array<Vector2> PatrolPoints;

    [Export]
    public double PatrolMoveInterval = 2f;

    [Export]
    public int PatrolIndex = 0;

    [ExportSubgroup("LookAround")]
    [Export]
    public double LookAroundInterval = 2f;
}