using Game.Core;
using Godot;

namespace Game.Gameplay
{
	public partial class PlayerInput : CharacterInput
	{
		[ExportCategory("Player Input")]
		[Export] public double HoldThreshold = 0.1f;
		[Export] public double HoldTime = 0.0f;

		public override void _Ready()
		{
			Logger.Info("Loading player input component ...");
		}
	}
}
