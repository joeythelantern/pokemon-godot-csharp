using Game.Core;
using Godot;

namespace Game.Utilities
{
    public abstract partial class State : Node
    {
        [Export] public Node StateOwner;
        [Export] public StateMachine StateMachine;

        public virtual void EnterState()
        {
            Logger.Info($"Entering {GetType().Name} state ...");
        }

        public virtual void ExitState()
        {
            Logger.Info($"Exiting {GetType().Name} state ...");
        }
    }
}