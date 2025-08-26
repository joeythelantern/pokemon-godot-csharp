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

	public AStarGrid2D Grid;

	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");

		SetupGrid();
	}

	public void SetupGrid()
	{
		Logger.Info("Setting up AStar Grid");

		Grid = new()
		{
			Region = new Rect2I(0, 0, Right, Bottom),
			CellSize = new Vector2(Globals.Instance.GRID_SIZE, Globals.Instance.GRID_SIZE),
			DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never
		};

		Grid.Update();

		var mapHeight = Bottom / Globals.Instance.GRID_SIZE;
		var mapWidth = Right / Globals.Instance.GRID_SIZE;

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				Vector2I cell = new(x, y);
				Vector2 worldPos = new Vector2(x * Globals.Instance.GRID_SIZE, y * Globals.Instance.GRID_SIZE) + new Vector2(8, 8);

				var (_, collisions) = CharacterMovement.GetTargetColliders(this, worldPos);
				var blocked = collisions.Count > 0;

				Grid.SetPointSolid(cell, blocked);
			}
		}
	}
}