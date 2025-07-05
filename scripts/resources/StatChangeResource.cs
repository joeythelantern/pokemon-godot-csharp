using Game.Core;
using Godot;

namespace Game.Resources;

[GlobalClass]
[Tool]
public partial class StatChangeResource : Resource
{
    [Export]
    public PokemonStat Stat;

    [Export]
    public int Change = 1;
}