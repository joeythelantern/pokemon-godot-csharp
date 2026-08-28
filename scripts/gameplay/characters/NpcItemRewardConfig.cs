using Game.Core;
using Game.Gameplay.Items;
using Godot;

namespace Game.Gameplay;

[GlobalClass]
public partial class NpcItemRewardConfig : Resource
{
    [Export]
    public NpcItemHandoverMode Mode = NpcItemHandoverMode.None;

    [Export]
    public ItemDefinition Item;

    [Export(PropertyHint.MultilineText)]
    public string ConvincingGoal = string.Empty;

    [Export(PropertyHint.Range, "1,10,1")]
    public int RequiredConvincingTurns = 1;

    [Export]
    public int Quantity = 1;

    [Export(PropertyHint.MultilineText)]
    public string HandoverMessage = "Here, I want you to have this.";
}
