using Game.Core;
using Godot;

namespace Game.Gameplay
{
    [GlobalClass]
    [Tool]
    public partial class TechnicalMachine : Resource
    {
        [Export]
        public int Number;

        [Export]
        public Move Move;
    }
}
