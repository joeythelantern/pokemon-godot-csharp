using Game.Utilities;
using Godot;

namespace Game.Gameplay;

public partial class Player : CharacterBody2D
{
    [Export]
    public StateMachine StateMachine;

    public override void _Ready()
    {
        StateMachine.Customer = this;
        StateMachine.ChangeState(StateMachine.GetNode<State>("Roam"));
    }
}
