using Game.Core;
using Game.Utilities;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

public partial class NpcRoamState : State
{
    [ExportCategory("State Vars")]
    [Export]
    public NpcInput NpcInput;

    [Export]
    public CharacterMovement CharacterMovement;

    private double timer = 2f;
    private Array<Vector2> currentPatrolPoints = [];

    public override void _Process(double delta)
    {
        if (CharacterMovement.IsMoving())
            return;

        switch (NpcInput.Config.NpcMovementType)
        {
            case NpcMovementType.Wander:
                HandleWander(delta, NpcInput.Config.WanderMoveInterval);
                break;
            case NpcMovementType.LookAround:
                HandleLookAround(delta, NpcInput.Config.LookAroundInterval);
                break;
            case NpcMovementType.Patrol:
                HandlePatrol(delta, NpcInput.Config.PatrolMoveInterval);
                break;
        }
    }

    private void HandlePatrol(double delta, double interval)
    {
        if (NpcInput.Config.PatrolPoints.Count == 0)
            return;

        timer -= delta;

        if (timer > 0)
            return;

        Vector2 currentPosition = ((Npc)StateOwner).Position;
        var level = SceneManager.GetCurrentLevel();

        if (currentPatrolPoints.Count == 0)
        {
            var patrolPoint = NpcInput.Config.PatrolPoints[NpcInput.Config.PatrolIndex];
            NpcInput.Config.PatrolIndex = (NpcInput.Config.PatrolIndex + 1) % NpcInput.Config.PatrolPoints.Count;

            var pathing = level.Grid.GetIdPath(Modules.ConvertVector2ToVector2I(currentPosition), Modules.ConvertVector2ToVector2I(patrolPoint));

            for (int i = 1; i < pathing.Count; i++)
            {
                var point = pathing[i];
                currentPatrolPoints.Add(Modules.ConvertVector2IToVector2(point));
            }

            level.CurrentPatrolPoints = currentPatrolPoints;

            if (currentPatrolPoints.Count == 0)
                return;
        }

        if (((Npc)StateOwner).Position.DistanceTo(currentPatrolPoints[0]) < 1f)
        {
            currentPatrolPoints.RemoveAt(0);
            return;
        }

        NpcInput.TargetPosition = currentPatrolPoints[0];
        level.TargetPosition = NpcInput.TargetPosition;

        Vector2 difference = NpcInput.TargetPosition - currentPosition;

        if (Mathf.Abs(difference.X) > Mathf.Abs(difference.Y))
        {
            NpcInput.Direction = difference.X > 0 ? Vector2.Right : Vector2.Left;
        }
        else
        {
            NpcInput.Direction = difference.Y > 0 ? Vector2.Down : Vector2.Up;
        }

        NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
        timer = interval;
    }

    private void HandleWander(double delta, double interval)
    {
        timer -= delta;

        if (timer > 0)
            return;

        var (direction, targetPosition) = GetNewDirections();

        NpcInput.Direction = direction;
        NpcInput.TargetPosition = targetPosition;

        NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
        timer = interval;
    }

    private void HandleLookAround(double delta, double interval)
    {
        timer -= delta;

        if (timer > 0)
            return;

        var (direction, targetPosition) = GetNewDirections();

        if (direction == NpcInput.Direction)
        {
            timer = interval;
            return;
        }

        NpcInput.Direction = direction;
        NpcInput.TargetPosition = targetPosition;

        NpcInput.EmitSignal(CharacterInput.SignalName.Turn);
        timer = interval;
    }

    private (Vector2, Vector2) GetNewDirections()
    {
        Vector2[] directions = [Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right];
        Vector2 chosenDirection;

        int tries = 0;

        do
        {
            chosenDirection = directions[Globals.GetRandomNumberGenerator().RandiRange(0, directions.Length - 1)];
            Vector2 nextPosition = CharacterMovement.Character.Position + chosenDirection * Globals.GRID_SIZE;

            if (NpcInput.Config.NpcMovementType == NpcMovementType.Wander)
            {
                float distanceFromOrigin = nextPosition.DistanceTo(NpcInput.Config.WanderOrigin);
                if (distanceFromOrigin <= NpcInput.Config.WanderRadius)
                    break;
            }
            else
            {
                break;
            }

            tries++;
        } while (tries < 10);

        return (chosenDirection, chosenDirection * Globals.GRID_SIZE);
    }

}
