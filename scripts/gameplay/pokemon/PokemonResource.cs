using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay
{
	[GlobalClass]
	[Tool]
	public partial class PokemonResource : Resource
	{
		[ExportCategory("Basic Info")]
		[Export]
		public PokemonName Name { get; set; }

		[Export]
		public int Number { get; set; }

		[Export]
		public PokemonType PrimaryType { get; set; }

		[Export]
		public PokemonType SecondaryType { get; set; }

		[Export]
		public ExperienceGroup ExperienceGroup { get; set; }

		[Export]
		public Array<Gender> Genders { get; set; }

		[Export]
		public string Description { get; set; }

		[ExportCategory("Stats")]
		[Export]
		public int Weight { get; set; }

		[Export]
		public int Height { get; set; }

		[Export]
		public int BaseHP { get; set; }

		[Export]
		public int BaseAttack { get; set; }

		[Export]
		public int BaseDefence { get; set; }

		[Export]
		public int BaseSpecialAttack { get; set; }

		[Export]
		public int BaseSpecialDefence { get; set; }

		[Export]
		public int BaseSpeed { get; set; }

		[ExportCategory("Moves")]
		[Export]
		public Dictionary<int, MoveName> LevelUpMoves { get; set; }

		[Export]
		public Array<int> TechnicalMachines { get; set; }

		[Export]
		public Array<int> HiddenMachines { get; set; }

		[ExportCategory("Sprites")]
		[Export]
		public Texture2D FrontSprite { get; set; }

		[Export]
		public Texture2D BackSprite { get; set; }
	}
}
