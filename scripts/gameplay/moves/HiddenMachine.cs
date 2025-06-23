using Game.Core;
using Godot;

namespace Game.Gameplay
{
    [GlobalClass]
    [Tool]
    public partial class HiddenMachine : Resource
    {
        [Export]
        public int Number;

        [Export]
        public MoveName Move;
    }
}
