using Game.Core;
using Game.Utilities;
using Godot;

namespace Game.Gameplay;

public partial class NpcRoamState : State
{
    [ExportCategory("State Vars")]
    [Export]
    public NpcInput NpcInput;

    [Export]
    public CharacterMovement CharacterMovement;

    private double timer = 2f;

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
        }
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
