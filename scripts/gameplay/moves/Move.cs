using Game.Core;
using Godot;

namespace Game.Gameplay
{
    [GlobalClass]
    [Tool]
    public partial class Move : Resource
    {
        [Export]
        public MoveName Name;

        [Export]
        public PokemonType Type;

        [Export]
        public MoveCategory Category;

        [Export]
        public int Power;

        [Export]
        public int PP;

        [Export]
        public int Accuracy;
    }
}