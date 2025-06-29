using Godot;

namespace Game.Utilities;

public partial class StateMachine : Node
{
    [ExportCategory("State Machine Vars")]
    [Export]
    public Node Customer;

    [Export]
    public State CurrentState;

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                state.StateOwner = Customer;
                state.SetProcess(false);
            }
        }
    }

    public string GetCurrentState()
    {
        return CurrentState.Name.ToString();
    }

    public void ChangeState(State newState)
    {
        CurrentState?.ExitState();
        CurrentState = newState;
        CurrentState?.EnterState();

        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                state.SetProcess(child == CurrentState);
            }
        }
    }
}
