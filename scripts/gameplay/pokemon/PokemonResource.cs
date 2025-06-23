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
		public string Name;

		[Export]
		public int Number;

		[Export]
		public PokemonType PrimaryType;

		[Export]
		public PokemonType SecondaryType;

		[Export]
		public ExperienceGroup ExperienceGroup;

		[Export]
		public Array<Gender> Genders;

		[Export]
		public string Description;

		[ExportCategory("Stats")]
		[Export]
		public int Weight;

		[Export]
		public int Height;

		[Export]
		public int BaseHP;

		[Export]
		public int BaseAttack;

		[Export]
		public int BaseDefence;

		[Export]
		public int BaseSpecialAttack;

		[Export]
		public int BaseSpecialDefence;

		[Export]
		public int BaseSpeed;

		[ExportCategory("Moves")]
		[Export]
		public Array<LevelUpMove> LevelUpMoves;

		[Export]
		public Array<TechnicalMachine> TechnicalMachines;

		[Export]
		public Array<HiddenMachine> HiddenMachines;

		[ExportCategory("Sprites")]
		[Export]
		public Texture2D FrontSprite;

		[Export]
		public Texture2D BackSprite;
	}
}
