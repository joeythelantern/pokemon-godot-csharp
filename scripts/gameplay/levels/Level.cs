using Game.Core;
using Godot;
using Godot.Collections;

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
	public Array<Vector2> currentPatrolPoints = new();
	public Vector2 TargetPosition = Vector2.Zero;

	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");
	}

	public override void _Process(double delta)
	{
		if (Grid == null && GameManager.GetPlayer() != null)
		{
			SetupGrid();
		}

		QueueRedraw();
	}

	public void SetupGrid()
	{
		Logger.Info("Setting up AStar Grid");

		Grid = new()
		{
			Region = new Rect2I(0, 0, Right, Bottom),
			CellSize = new Vector2(Globals.GRID_SIZE, Globals.GRID_SIZE),
			DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
			DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never
		};

		Grid.Update();

		var mapHeight = Bottom / Globals.GRID_SIZE;
		var mapWidth = Right / Globals.GRID_SIZE;

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				Vector2I cell = new(x, y);
				Vector2 worldPos = new Vector2(x * Globals.GRID_SIZE, y * Globals.GRID_SIZE);

				var (_, collisions) = GameManager.GetPlayer().GetNode<CharacterMovement>("Movement").GetTargetColliders(GameManager.GetPlayer(), worldPos);

				foreach (var collision in collisions)
				{
					var collider = (Node)(GodotObject)collision["collider"];
					var colliderType = collider.GetType().Name;

					if (colliderType == "Player" || colliderType == "Npc")
					{
						Logger.Info($"Ignoring {colliderType}");
						continue;
					}

					Grid.SetPointSolid(cell, true);
					break;
				}

			}
		}
	}

	public override void _Draw()
	{
		if (Grid == null)
			return;

		foreach (var point in currentPatrolPoints)
		{
			DrawRect(new Rect2(point, new Vector2(Globals.GRID_SIZE, Globals.GRID_SIZE)), Colors.Red);
		}

		if (TargetPosition != Vector2.Zero)
			DrawRect(new Rect2(TargetPosition, new Vector2(Globals.GRID_SIZE, Globals.GRID_SIZE)), Colors.Green);
	}
}