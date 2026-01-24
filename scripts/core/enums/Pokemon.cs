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

public enum PokemonNature
{
    Hardy,
    Lonely,
    Brave,
    Adamant,
    Naughty,
    Bold,
    Docile,
    Relaxed,
    Impish,
    Lax,
    Timid,
    Hasty,
    Serious,
    Jolly,
    Naive,
    Modest,
    Mild,
    Quiet,
    Bashful,
    Rash,
    Calm,
    Gentle,
    Sassy,
    Careful,
    Quirky
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

    public static readonly Dictionary<string, PokemonNature> NatureMap = new()
    {
        { "hardy", PokemonNature.Hardy },
        { "lonely", PokemonNature.Lonely },
        { "brave", PokemonNature.Brave },
        { "adamant", PokemonNature.Adamant },
        { "naughty", PokemonNature.Naughty },
        { "bold", PokemonNature.Bold },
        { "docile", PokemonNature.Docile },
        { "relaxed", PokemonNature.Relaxed },
        { "impish", PokemonNature.Impish },
        { "lax", PokemonNature.Lax },
        { "timid", PokemonNature.Timid },
        { "hasty", PokemonNature.Hasty },
        { "serious", PokemonNature.Serious },
        { "jolly", PokemonNature.Jolly },
        { "naive", PokemonNature.Naive },
        { "modest", PokemonNature.Modest },
        { "mild", PokemonNature.Mild },
        { "quiet", PokemonNature.Quiet },
        { "bashful", PokemonNature.Bashful },
        { "rash", PokemonNature.Rash },
        { "calm", PokemonNature.Calm },
        { "gentle", PokemonNature.Gentle },
        { "sassy", PokemonNature.Sassy },
        { "careful", PokemonNature.Careful },
        { "quirky", PokemonNature.Quirky }
    };
}