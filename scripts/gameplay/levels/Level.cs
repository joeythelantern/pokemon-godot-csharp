using Game.Core;
using Godot;

namespace Game.Gameplay;

public partial class Level : Node2D
{
	[ExportCategory("Level Basics")]
	[Export]
	public LevelName LevelName;

	[Export(PropertyHint.Range, "0,100")]
	public int EncounterRate;

	[ExportCategory("Camera Limits")]
	[Export]
	public int Top;

	[Export]
	public int Bottom;

	[Export]
	public int Left;

	[Export]
	public int Right;

	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");
	}
}