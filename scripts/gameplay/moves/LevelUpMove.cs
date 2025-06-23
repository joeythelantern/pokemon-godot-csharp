using Game.Core;
using Godot;

namespace Game.Gameplay
{
    [GlobalClass]
    [Tool]
    public partial class LevelUpMove : Resource
    {
        [Export]
        public int Level;

        [Export]
        public MoveName Move;
    }
}
