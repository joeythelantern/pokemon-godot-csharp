using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Resources;

public enum MoveTarget
{
    // Special for moves like Counter or Curse
    SpecificMove,
    // Everyone on the user's side of the field
    UsersField,
    // Moves like rest, swords-dance, growth, etc
    User,
    // One opposing Pokemon, selected at random (double duels)
    RandomOpponent,
    // All other Pokemon
    AllOtherPokemon,
    // A single opponent
    SelectedPokemon,
    // All opponent Pokemon
    AllOpponents,
    // Status moves like Haze, Rain, Sunny Day, etc.
    EntireField
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

[GlobalClass]
[Tool]
public partial class MoveResource : Resource
{
    [ExportCategory("Basic Info")]
    [Export]
    public string Name = "";

    [Export]
    public PokemonType Type = PokemonType.None;

    [Export]
    public MoveCategory Category = MoveCategory.Physical;

    [ExportCategory("Metadata")]
    [Export]
    public int Accuracy;

    [Export]
    public int CritRate;

    [Export]
    public int Drain;

    [Export]
    public int FlinchChance;

    [Export]
    public int Healing;

    [Export]
    public int MaxHits;

    [Export]
    public int MaxTurns;

    [Export]
    public int MinHits;

    [Export]
    public int MinTurns;

    [Export]
    public int Power;

    [Export]
    public int PP;

    [ExportCategory("Move Effects")]
    [Export]
    public int AilmentChance;

    [Export]
    public PokemonAilment Ailment = PokemonAilment.None;

    [Export]
    public Dictionary<PokemonStat, int> StatChanges = [];
}
