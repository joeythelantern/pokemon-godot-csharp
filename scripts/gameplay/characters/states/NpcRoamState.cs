using Game.Core;
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

    private double timer = 5f;

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

        Logger.Info($"Patrol points: {NpcInput.Config.PatrolPoints.Count}");

        var level = SceneManager.GetCurrentLevel();
        var currentPosition = ((Npc)StateOwner).Position;
        var x = (int)currentPosition.X / Globals.Instance.GRID_SIZE;
        var y = (int)currentPosition.Y / Globals.Instance.GRID_SIZE;

        Logger.Info($"Start: {x}, {y}");

        var TargetPosition = NpcInput.Config.PatrolPoints[NpcInput.Config.PatrolIndex];
        var tx = (int)TargetPosition.X / Globals.Instance.GRID_SIZE;
        var ty = (int)TargetPosition.Y / Globals.Instance.GRID_SIZE;

        Logger.Info($"End: {tx}, {ty}");

        Logger.Info(level.Grid.GetIdPath(new Vector2I(x, y), new Vector2I(tx, ty)));
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
