using Godot;

namespace Game.Resources;

[GlobalClass]
[Tool]
public partial class TMResource : Resource
{
    [Export]
    public int Number;

    [Export]
    public string Move;
}