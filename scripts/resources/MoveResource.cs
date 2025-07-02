using Godot;
using System;

namespace Game.Resources;

public enum MoveTarget
{
    Self,
    Opponent,
    All
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

public partial class MoveResource : Resource
{
    [Export]
    public string Name;

    [Export]
    public PokemonType Type = PokemonType.None;

    [Export]
    public MoveCategory Category = MoveCategory.Physical;

    [Export]
    public int Power = 0;

    [Export]
    public int PP = 40;

    [Export]
    public int Accuracy;

    [Export]
    public AilmentResource Ailment = null;

    [Export]
    public StatChangeResource StatChange = null;
}
