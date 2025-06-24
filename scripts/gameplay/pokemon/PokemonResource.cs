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
		public PokemonName Name;

		[Export]
		public int Number;

		[Export]
		public string Description;

		[ExportCategory("Attributes")]
		[Export]
		public PokemonType PrimaryType;

		[Export]
		public PokemonType SecondaryType;

		[Export]
		public ExperienceGroup ExperienceGroup;

		[Export]
		public Array<Gender> Genders;

		[Export]
		public Array<Ability> Abilities;

		[ExportCategory("Stats")]
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

		[Export]
		public Texture2D ShinyFrontSprite;

		[Export]
		public Texture2D ShinyBackSprite;
	}
}
