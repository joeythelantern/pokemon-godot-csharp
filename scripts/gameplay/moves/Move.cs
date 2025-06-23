using Game.Core;
using Godot;
using System;

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
        public string Category;

        [Export]
        public int Power;

        [Export]
        public int PP;

        [Export]
        public int MaxPP;

        [Export]
        public int Accuracy;
    }
}