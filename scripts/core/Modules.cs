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
}