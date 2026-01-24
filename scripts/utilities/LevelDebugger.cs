using Game.Core;
using Game.Gameplay;
using Godot;

public partial class LevelDebugger : Node2D
{
    [Export]
    public bool DebugOn = false;

    private Level level;

    public override void _Ready()
    {
        level = GetParent<Level>();
    }

    public override void _Process(double delta)
    {
        if (DebugOn && !Visible)
            Visible = true;
        else if (!DebugOn && Visible)
            Visible = false;

        if (level != null && DebugOn)
        {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (!DebugOn)
        {
            return;
        }

        if (level == null)
        {
            return;
        }

        var Grid = level.Grid;

        if (Grid == null)
            return;

        var mapHeight = level.Bottom / Globals.GRID_SIZE;
        var mapWidth = level.Right / Globals.GRID_SIZE;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2I cell = new(x, y);
                Vector2 worldPosition = new(x * Globals.GRID_SIZE, y * Globals.GRID_SIZE);

                var color = Grid.IsPointSolid(cell) ? new Color(1, 0, 0, 0.7f) : new Color(0, 1, 0, 0.7f);
                DrawRect(new Rect2(worldPosition, Grid.CellSize), color, filled: true);
            }
        }

        foreach (var point in level.CurrentPatrolPoints)
        {
            DrawRect(new Rect2(point, Grid.CellSize), new Color(0, 0, 1, 0.3f), filled: true);
        }

        if (level.TargetPosition != Vector2.Zero)
            DrawRect(new Rect2(level.TargetPosition, Grid.CellSize), new Color(0, 1, 1, 0.3f), filled: true);
    }
}
