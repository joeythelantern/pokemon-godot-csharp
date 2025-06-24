using Godot;

namespace Game.Gameplay
{
	public partial class PlayerPokemonStats : Node
	{
		[ExportCategory("Base Stats")]
		[Export]
		public int HP;

		[Export]
		public int CurrentHP;

		[Export]
		public int Attack;

		[Export]
		public int Defence;

		[Export]
		public int SpecialAttack;

		[Export]
		public int SpecialDefence;

		[Export]
		public int Speed;

		[ExportCategory("IV Stats")]
		[Export]
		public int IVHP;

		[Export]
		public int IVAttack;

		[Export]
		public int IVDefence;

		[Export]
		public int IVSpecialAttack;

		[Export]
		public int IVSpecialDefence;

		[Export]
		public int IVSpeed;

		[ExportCategory("EV Stats")]
		[Export]
		public int EVHP;

		[Export]
		public int EVAttack;

		[Export]
		public int EVDefence;

		[Export]
		public int EVSpecialAttack;

		[Export]
		public int EVSpecialDefence;

		[Export]
		public int EVSpeed;
	}
}
