using Godot;
using System;

namespace Game.Resources;

public enum Stat
{
    HP,
    Attack,
    Defence,
    SpecialAttack,
    SpecialDefence,
    Speed

}

[GlobalClass]
[Tool]
public partial class StatChangeResource : Resource
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
    public int Difference = 1;

    [Export]
    public bool TriggerAfterHit = true;
}