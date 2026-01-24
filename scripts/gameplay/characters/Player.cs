using Game.Utilities;
using Godot;

namespace Game.Gameplay;

public partial class Player : CharacterBody2D
{
    [Export]
    public StateMachine StateMachine;

    [Export]
    public Backpack Backpack;

    public override void _Ready()
    {
        Backpack ??= GetNode<Backpack>("Backpack");
        StateMachine ??= GetNode<StateMachine>("StateMachine");

        StateMachine.Customer = this;
        StateMachine.ChangeState(StateMachine.GetNode<State>("Roam"));
    }
}
