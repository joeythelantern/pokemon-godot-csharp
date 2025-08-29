using Game.Core;
using Game.Gameplay;
using Game.Utilities;
using Godot;
using Godot.Collections;

public partial class NpcRoamState : State
{
    [ExportCategory("State Vars")]
    [Export]
    public NpcInput NpcInput;

    [Export]
    public CharacterMovement CharacterMovement;

    private double timer = 5f;
    private Array<Vector2> currentPatrolPoints = new();

    public override void _Process(double delta)
    {
        if (CharacterMovement.IsMoving())
            return;

        switch (NpcInput.Config.NpcMovementType)
        {
            case NpcMovementType.Wander:
                HandleWander(delta, NpcInput.Config.WanderMoveInterval);
                break;

            case NpcMovementType.Patrol:
                HandlePatrol(delta, NpcInput.Config.PatrolMoveInterval);
                break;

            case NpcMovementType.LookAround:
                HandleLookAround(delta, NpcInput.Config.LookAroundInterval);
                break;
        }
    }

    private void HandleWander(double delta, double interval)
    {
        timer -= delta;

        if (timer > 0)
            return;

        var (chosenDirection, targetDirection) = GetNewDirections();

        NpcInput.Direction = chosenDirection;
        NpcInput.TargetPosition = targetDirection;

        NpcInput.EmitSignal(CharacterInput.SignalName.Walk);
        timer = interval;
    }

    private void HandlePatrol(double delta, double interval)
    {
        if (NpcInput.Config.PatrolPoints.Count == 0)
            return;

        timer -= delta;

        if (timer > 0)
            return;

        Vector2 currentPosition = ((Npc)StateOwner).Position;

        if (currentPatrolPoints.Count == 0)
        {
            var level = SceneManager.GetCurrentLevel();
            var TargetPosition = NpcInput.Config.PatrolPoints[NpcInput.Config.PatrolIndex];
            NpcInput.Config.PatrolIndex = (NpcInput.Config.PatrolIndex + 1) % NpcInput.Config.PatrolPoints.Count;

            var pathing = level.Grid.GetIdPath(Modules.ConvertToVector2I(currentPosition), Modules.ConvertToVector2I(TargetPosition));

            for (int path = 1; path < pathing.Count; path++)
            {
                var point = pathing[path];
                currentPatrolPoints.Add(Modules.ConvertToVector2(point));
            }

            if (currentPatrolPoints.Count == 0)
                return;

            SceneManager.GetCurrentLevel().currentPatrolPoints = currentPatrolPoints;
        }

        if (((Npc)StateOwner).Position.DistanceTo(currentPatrolPoints[0]) < 1f)
        {
            currentPatrolPoints.RemoveAt(0);
            return;
        }

        NpcInput.TargetPosition = currentPatrolPoints[0];
        currentPatrolPoints.RemoveAt(0);

        SceneManager.GetCurrentLevel().TargetPosition = NpcInput.TargetPosition;
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

    private void HandleLookAround(double delta, double interval)
    {
        timer -= delta;

        if (timer > 0)
            return;

        var (chosenDirection, targetDirection) = GetNewDirections();

        if (chosenDirection == NpcInput.Direction)
        {
            timer = interval;
            return;
        }

        NpcInput.Direction = chosenDirection;
        NpcInput.TargetPosition = targetDirection;
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
            Vector2 nextPosition = CharacterMovement.Character.Position + chosenDirection * Globals.Instance.GRID_SIZE;

            float distanceFromOrigin = nextPosition.DistanceTo(NpcInput.Config.WanderOrigin);
            if (distanceFromOrigin <= NpcInput.Config.WanderRadius)
                break;

            tries++;
        } while (tries < 10);

        return (chosenDirection, chosenDirection * Globals.Instance.GRID_SIZE);
    }
}
