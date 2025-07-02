using Godot;

namespace Game.Resources;

public enum AilmentTarget
{
    Self,
    Opponent,
    All
}

public enum StatusCondition
{
    None,
    Burn,
    Freeze,
    Paralyze,
    Poison,
    Toxic,
    Sleep,
    Confusion
}

[GlobalClass]
[Tool]
public partial class AilmentResource : Resource
{
    [Export]
    public StatusCondition Status;

    [Export]
    public string Description;

    [Export]
    public AilmentTarget Target = AilmentTarget.Opponent;

    [Export(PropertyHint.Range, "0,100,1")]
    public int Chance = 100;

    [Export]
    public bool TriggerAfterHit = true;
}