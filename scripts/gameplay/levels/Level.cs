using System.Collections.Generic;
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

	private readonly HashSet<Vector2> reserverdTiles = [];

	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");
	}

	public bool ReserveTile(Vector2 position)
	{
		if (reserverdTiles.Contains(position))
			return false;

		reserverdTiles.Add(position);
		return true;
	}

	public bool IsTileFree(Vector2 position)
	{
		return !reserverdTiles.Contains(position);
	}

	public void ReleaseTile(Vector2 position)
	{
		reserverdTiles.Remove(position);
	}
}