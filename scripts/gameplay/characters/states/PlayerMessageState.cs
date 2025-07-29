using Game.UI;
using Game.Utilities;
using Godot;
using System;

namespace Game.Gameplay;

public partial class PlayerMessageState : State
{
    public override void _Process(double delta)
    {
        if (!MessageManager.Scrolling() && Input.IsActionJustReleased("use"))
        {
            MessageManager.ScrollText();

            if (MessageManager.GetMessages().Count == 0)
            {
                StateMachine.ChangeState("Roam");
            }
        }
    }
}
