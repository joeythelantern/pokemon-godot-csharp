using Game.Core;
using Godot;

namespace Game.Gameplay;

public partial class Level : Node2D
{
	[ExportCategory("Level Basics")]
	[Export]
	public LevelName LevelName;

	[ExportCategory("Camera Limits")]
	[Export]
	public int Top;

	[Export]
	public int Bottom;

	[Export]
	public int Left;

	[Export]
	public int Right;

	[ExportCategory("Pokemon")]
	[Export]
	public int EncounterRate = 20;

	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");
	}
}