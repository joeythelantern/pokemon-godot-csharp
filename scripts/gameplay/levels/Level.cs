using System.Collections.Generic;
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

	[ExportCategory("Debugging")]
	[Export]
	public bool DebugLayerOn = false;

	private readonly HashSet<Vector2> reserverdTiles = [];

	public AStarGrid2D Grid;
	public Vector2 TargetPosition = Vector2.Zero;
	public Array<Vector2> CurrentPatrolPoints = [];

	public override void _Ready()
	{
		Logger.Info($"Loading level {LevelName} ...");

		GetNode<LevelDebugger>("DebugLayer").DebugOn = DebugLayerOn;
	}

	public override void _Process(double delta)
	{
		if (Grid == null && GameManager.GetPlayer() != null)
		{
			SetupGrid();
		}
	}

	public void SetupGrid()
	{
		Logger.Info("Setting up A* Grid ...");

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
				Vector2 worldPosition = new(x * Globals.GRID_SIZE, y * Globals.GRID_SIZE);

				var (_, collisions) = GameManager.GetPlayer().GetNode<CharacterMovement>("Movement").GetTargetColliders(worldPosition);

				foreach (var collision in collisions)
				{
					var collider = (Node)(GodotObject)collision["collider"];
					var colliderType = collider.GetType().Name;

					if (colliderType == "TallGrass" || colliderType == "Player")
					{
						continue;
					}

					if (colliderType == "Npc")
					{
						switch (((Npc)collider).NpcInputConfig.NpcMovementType)
						{
							case NpcMovementType.Patrol:
								continue;
							case NpcMovementType.Wander:
								continue;
						}
					}

					Grid.SetPointSolid(cell, true);
				}
			}
		}
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

	public Node2D GetEntities()
	{
		var entities = GetNodeOrNull<Node2D>("Entities");

		if (entities == null)
		{
			var node = new Node2D()
			{
				Name = "Entities"
			};

			AddChild(node);
		}

		return GetNode<Node2D>("Entities");
	}
}