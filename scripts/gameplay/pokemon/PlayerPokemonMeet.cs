using Godot;

namespace Game.Gameplay
{
	public partial class PlayerPokemonMeet : Node
	{
		[Export]
		public string OriginalTrainer;

		[Export]
		public string Date;

		[Export]
		public string Location;

		[Export]
		public int Level;
	}
}
