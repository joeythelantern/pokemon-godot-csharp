using Game.Core;
using Godot;

namespace Game.Gameplay.Items;

[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export] public string Id = string.Empty;
    [Export] public string DisplayName = string.Empty;
    [Export(PropertyHint.MultilineText)] public string Description = string.Empty;
    [Export] public Texture2D Icon;
    [Export] public ItemCategory Category = ItemCategory.General;

    // Item behavior is intentionally a no-op until Pokémon and field actions exist.
    public virtual void Use(Node context) { }
}
