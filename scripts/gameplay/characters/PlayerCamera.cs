using Game.Core;
using Godot;

namespace Game.Gameplay
{
	public partial class PlayerCamera : Camera2D
	{
		[ExportCategory("Camera Vars")]
		[Export]
		public Level CurrentLevel;

		public override void _Ready()
		{
			CurrentLevel = SceneManager.Instance.CurrentLevel;
			UpdateCameraLimits();
		}

		public override void _Process(double delta)
		{
			if (CurrentLevel != SceneManager.Instance.CurrentLevel)
			{
				CurrentLevel = SceneManager.Instance.CurrentLevel;
				UpdateCameraLimits();
			}
		}

		public void UpdateCameraLimits()
		{
			LimitTop = CurrentLevel.Top;
			LimitBottom = CurrentLevel.Bottom;
			LimitLeft = CurrentLevel.Left;
			LimitRight = CurrentLevel.Right;
		}
	}
}
