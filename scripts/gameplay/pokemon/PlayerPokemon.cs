using Godot;
using Game.Core;
using Godot.Collections;

namespace Game.Gameplay
{
	public partial class PlayerPokemon : Node
	{
		[ExportCategory("Basic Info")]
		[Export]
		public string UUID;

		[Export]
		public string NickName;

		[Export]
		public bool Shiney;

		[Export]
		public PokemonResource BasePokemon;

		[ExportCategory("Characteristics")]
		[Export]
		public Ability Ability;

		[Export]
		public Nature Nature;

		[Export]
		public Gender Gender;

		[Export]
		public Status Status;

		[Export]
		public string Characteristic;

		[ExportCategory("Moves")]
		[Export]
		public Array<Move> Moves;

		[ExportCategory("Components")]
		[Export]
		public PlayerPokemonMeet Meet;

		[Export]
		public PlayerPokemonExperience Experience;

		[Export]
		public PlayerPokemonStats Stats;
	}
}
