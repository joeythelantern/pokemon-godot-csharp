using Game.Core;
using Game.Utilities;
using Godot;

namespace Game.Gameplay;

public partial class PlayerRoamState : State
{
    [ExportCategory("State Vars")]
    [Export]
    public PlayerInput PlayerInput;

    [Export]
    public CharacterMovement CharacterMovement;

    public override void _Ready()
    {
        Signals.Instance.MessageBoxOpen += (value) =>
        {
            if (value)
            {
                StateMachine.ChangeState("Message");
            }
        };
    }

    public override void _Process(double delta)
    {
        GetInputDirection();
        GetInput(delta);
        GetUseInput();
    }

    public void GetInputDirection()
    {
        if (Input.IsActionJustPressed("ui_up"))
        {
            PlayerInput.Direction = Vector2.Up;
            PlayerInput.TargetPosition = new Vector2(0, -Globals.GRID_SIZE);
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            PlayerInput.Direction = Vector2.Down;
            PlayerInput.TargetPosition = new Vector2(0, Globals.GRID_SIZE);
        }
        else if (Input.IsActionJustPressed("ui_left"))
        {
            PlayerInput.Direction = Vector2.Left;
            PlayerInput.TargetPosition = new Vector2(-Globals.GRID_SIZE, 0);
        }
        else if (Input.IsActionJustPressed("ui_right"))
        {
            PlayerInput.Direction = Vector2.Right;
            PlayerInput.TargetPosition = new Vector2(Globals.GRID_SIZE, 0);
        }
    }

    public void GetInput(double delta)
    {
        if (CharacterMovement.IsMoving())
            return;

        if (Modules.IsActionJustReleased())
        {
            if (PlayerInput.HoldTime > PlayerInput.HoldThreshold)
            {
                PlayerInput.EmitSignal(CharacterInput.SignalName.Walk);
            }
            else
            {
                PlayerInput.EmitSignal(CharacterInput.SignalName.Turn);
            }

            PlayerInput.HoldTime = 0.0f;
        }

        if (Modules.IsActionPressed())
        {
            PlayerInput.HoldTime += delta;

            if (PlayerInput.HoldTime > PlayerInput.HoldThreshold)
            {
                PlayerInput.EmitSignal(CharacterInput.SignalName.Walk);
            }
        }
    }

    public void GetUseInput()
    {
        if (Input.IsActionJustReleased("use"))
        {
            var (_, result) = CharacterMovement.GetTargetColliders((PlayerInput.Direction * Globals.GRID_SIZE) + ((Player)StateOwner).Position);

            foreach (var collision in result)
            {
                var collider = (Node)(GodotObject)collision["collider"];
                var colliderType = collider.GetType().Name;

                switch (colliderType)
                {
                    case "Sign":
                        ((Sign)collider).PlayMessage();
                        break;
                    case "Npc":
                        ((Npc)collider).PlayMessage(PlayerInput.Direction);
                        break;
                }
            }
        }
    }
}
