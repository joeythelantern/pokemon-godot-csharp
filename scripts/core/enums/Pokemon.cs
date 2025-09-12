using Godot.Collections;

namespace Game.Core;

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Grass,
    Electric,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum PokemonAilment
{
    None,
    Burn,
    Freeze,
    Paralysis,
    Poison,
    Toxic,
    Sleep,
    Confusion,
    Trap,
    LeechSeed,
    Disable,
    Unknown
}

public enum PokemonStat
{
    None,
    Hp,
    Attack,
    Defense,
    SpecialAttack,
    SpecialDefense,
    Speed,
    Accuracy,
    Evasion
}

public static class PokemonEnum
{
    public static readonly Dictionary<string, PokemonType> TypeMap = new()
    {
        { "normal", PokemonType.Normal },
        { "fire", PokemonType.Fire },
        { "water", PokemonType.Water },
        { "grass", PokemonType.Grass },
        { "electric", PokemonType.Electric },
        { "ice", PokemonType.Ice },
        { "fighting", PokemonType.Fighting },
        { "poison", PokemonType.Poison },
        { "ground", PokemonType.Ground },
        { "flying", PokemonType.Flying },
        { "psychic", PokemonType.Psychic },
        { "bug", PokemonType.Bug },
        { "rock", PokemonType.Rock },
        { "ghost", PokemonType.Ghost },
        { "dragon", PokemonType.Dragon },
        { "dark", PokemonType.Dark },
        { "steel", PokemonType.Steel },
        { "fairy", PokemonType.Fairy }
    };

    public static readonly Dictionary<string, PokemonAilment> AilmentMap = new()
    {
        { "none", PokemonAilment.None },
        { "burn", PokemonAilment.Burn },
        { "freeze", PokemonAilment.Freeze },
        { "paralysis", PokemonAilment.Paralysis },
        { "poison", PokemonAilment.Poison },
        { "toxic", PokemonAilment.Toxic },
        { "sleep", PokemonAilment.Sleep },
        { "confusion", PokemonAilment.Confusion },
        { "trap", PokemonAilment.Trap },
        { "leech-seed", PokemonAilment.LeechSeed },
        { "disable", PokemonAilment.Disable },
        { "unknown", PokemonAilment.Unknown }
    };

    public static readonly Dictionary<string, PokemonStat> StatMap = new()
    {
        { "hp", PokemonStat.Hp },
        { "attack", PokemonStat.Attack },
        { "defense", PokemonStat.Defense },
        { "special-attack", PokemonStat.SpecialAttack },
        { "special-defense", PokemonStat.SpecialDefense },
        { "speed", PokemonStat.Speed },
        { "accuracy", PokemonStat.Accuracy },
        { "evasion", PokemonStat.Evasion }
    };
}