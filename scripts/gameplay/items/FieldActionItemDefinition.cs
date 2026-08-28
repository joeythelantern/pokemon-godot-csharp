using Game.Core;
using Godot;

namespace Game.Gameplay.Items;

[GlobalClass]
public partial class FieldActionItemDefinition : ItemDefinition
{
    [Export] public FieldItemAction Action = FieldItemAction.None;

    public FieldActionItemDefinition()
    {
        Category = ItemCategory.FieldAction;
    }

    public override void Use(Node context) { }
}
