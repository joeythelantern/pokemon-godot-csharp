using Godot;

namespace Game.Core;

public static class Modules
{
    public static bool IsActionJustPressed()
    {
        return Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down") || Input.IsActionJustPressed("ui_left") || Input.IsActionJustPressed("ui_right");
    }

    public static bool IsActionPressed()
    {
        return Input.IsActionPressed("ui_up") || Input.IsActionPressed("ui_down") || Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right");
    }

    public static bool IsActionJustReleased()
    {
        return Input.IsActionJustReleased("ui_up") || Input.IsActionJustReleased("ui_down") || Input.IsActionJustReleased("ui_left") || Input.IsActionJustReleased("ui_right");
    }

    public static Vector2 ConvertToVector2(Vector2I vector)
    {
        return new Vector2(vector.X * Globals.GRID_SIZE, vector.Y * Globals.GRID_SIZE);
    }

    public static Vector2I ConvertToVector2I(Vector2 vector)
    {
        return new Vector2I((int)vector.X / Globals.GRID_SIZE, (int)vector.Y / Globals.GRID_SIZE);
    }
}