using Game.Core;
using Godot;

namespace Game.Gameplay.Items;

[GlobalClass]
public partial class TechnicalMachineItemDefinition : ItemDefinition
{
    [Export] public string MoveId = string.Empty;
    [Export] public string MoveName = string.Empty;

    public TechnicalMachineItemDefinition()
    {
        Category = ItemCategory.TechnicalMachine;
    }

    public override void Use(Node context) { }
}
