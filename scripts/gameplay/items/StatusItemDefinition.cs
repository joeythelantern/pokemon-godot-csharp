using Game.Core;
using Godot;

namespace Game.Gameplay.Items;

[GlobalClass]
public partial class StatusItemDefinition : ItemDefinition
{
    [Export] public int HpChange;
    [Export] public int AttackChange;
    [Export] public int DefenseChange;
    [Export] public int SpecialAttackChange;
    [Export] public int SpecialDefenseChange;
    [Export] public int SpeedChange;

    public StatusItemDefinition()
    {
        Category = ItemCategory.Status;
    }

    public override void Use(Node context) { }
}
