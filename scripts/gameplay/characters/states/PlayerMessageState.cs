using Game.Core;
using Game.UI;
using Game.Utilities;
using Godot;

namespace Game.Gameplay
{
    public partial class PlayerMessageState : State
    {
        public override void _Process(double delta)
        {
            if (!MessageManager.Scrolling() && Input.IsActionJustReleased("use"))
            {
                if (MessageManager.GetMessages().Count != 0)
                {
                    MessageManager.ScrollText();
                }
                else
                {
                    StateMachine.ChangeState(StateMachine.GetNode<State>("Free"));
                }
            }
        }
    }
}
