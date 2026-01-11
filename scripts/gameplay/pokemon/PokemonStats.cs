using Game.Core;
using Godot;

namespace Game.Gameplay;

[GlobalClass]
public partial class PokemonStats : Resource
{
    [Export] public int Level { get; set; } = 1;
    [Export] public int CurrentHP { get; set; }
    [Export] public PokemonIVs IVs { get; private set; }
    [Export] public PokemonEVs EVs { get; private set; }

    public PokemonResource Base { get; set; }
    public PokemonNature Nature { get; set; }

    public int MaxHP => CalculateHP(Base.BaseHp, IVs.HP, EVs.HP, Level);

    public int Attack =>
        CalculateStat(Base.BaseAttack, IVs.Attack, EVs.Attack, Level, Nature, PokemonStat.Attack);

    public int Defense =>
        CalculateStat(Base.BaseDefense, IVs.Defense, EVs.Defense, Level, Nature, PokemonStat.Defense);

    public int SpecialAttack =>
        CalculateStat(Base.BaseSpecialAttack, IVs.SpecialAttack, EVs.SpecialAttack, Level, Nature, PokemonStat.SpecialAttack);

    public int SpecialDefense =>
        CalculateStat(Base.BaseSpecialDefense, IVs.SpecialDefense, EVs.SpecialDefense, Level, Nature, PokemonStat.SpecialDefense);

    public int Speed =>
        CalculateStat(Base.BaseSpeed, IVs.Speed, EVs.Speed, Level, Nature, PokemonStat.Speed);

    public static PokemonStats Generate(
        PokemonResource basePokemon,
        PokemonNature nature,
        PokemonIVs ivs,
        PokemonEVs evs,
        int level)
    {
        var stats = new PokemonStats()
        {
            Base = basePokemon,
            Nature = nature,
            IVs = ivs,
            EVs = evs,
            Level = level,
        };

        stats.CurrentHP = stats.MaxHP;

        return stats;
    }

    private static int CalculateHP(int baseStat, int iv, int ev, int level)
    {
        return Mathf.FloorToInt(
            (2 * baseStat + iv + (ev / 4f)) * level / 100f
        ) + level + 10;
    }

    private static int CalculateStat(
        int baseStat,
        int iv,
        int ev,
        int level,
        PokemonNature nature,
        PokemonStat stat)
    {
        int value = Mathf.FloorToInt(
            (2 * baseStat + iv + (ev / 4f)) * level / 100f
        ) + 5;

        return Mathf.FloorToInt(value * GetNatureMultiplier(nature, stat));
    }

    private static float GetNatureMultiplier(PokemonNature nature, PokemonStat stat)
    {
        return nature switch
        {
            PokemonNature.Lonely when stat == PokemonStat.Attack => 1.1f,
            PokemonNature.Lonely when stat == PokemonStat.Defense => 0.9f,

            PokemonNature.Brave when stat == PokemonStat.Attack => 1.1f,
            PokemonNature.Brave when stat == PokemonStat.Speed => 0.9f,

            PokemonNature.Adamant when stat == PokemonStat.Attack => 1.1f,
            PokemonNature.Adamant when stat == PokemonStat.SpecialAttack => 0.9f,

            PokemonNature.Naughty when stat == PokemonStat.Attack => 1.1f,
            PokemonNature.Naughty when stat == PokemonStat.SpecialDefense => 0.9f,

            PokemonNature.Bold when stat == PokemonStat.Defense => 1.1f,
            PokemonNature.Bold when stat == PokemonStat.Attack => 0.9f,

            PokemonNature.Relaxed when stat == PokemonStat.Defense => 1.1f,
            PokemonNature.Relaxed when stat == PokemonStat.Speed => 0.9f,

            PokemonNature.Impish when stat == PokemonStat.Defense => 1.1f,
            PokemonNature.Impish when stat == PokemonStat.SpecialAttack => 0.9f,

            PokemonNature.Lax when stat == PokemonStat.Defense => 1.1f,
            PokemonNature.Lax when stat == PokemonStat.SpecialDefense => 0.9f,

            PokemonNature.Timid when stat == PokemonStat.Speed => 1.1f,
            PokemonNature.Timid when stat == PokemonStat.Attack => 0.9f,

            PokemonNature.Hasty when stat == PokemonStat.Speed => 1.1f,
            PokemonNature.Hasty when stat == PokemonStat.Defense => 0.9f,

            PokemonNature.Jolly when stat == PokemonStat.Speed => 1.1f,
            PokemonNature.Jolly when stat == PokemonStat.SpecialAttack => 0.9f,

            PokemonNature.Naive when stat == PokemonStat.Speed => 1.1f,
            PokemonNature.Naive when stat == PokemonStat.SpecialDefense => 0.9f,

            PokemonNature.Modest when stat == PokemonStat.SpecialAttack => 1.1f,
            PokemonNature.Modest when stat == PokemonStat.Attack => 0.9f,

            PokemonNature.Mild when stat == PokemonStat.SpecialAttack => 1.1f,
            PokemonNature.Mild when stat == PokemonStat.Defense => 0.9f,

            PokemonNature.Quiet when stat == PokemonStat.SpecialAttack => 1.1f,
            PokemonNature.Quiet when stat == PokemonStat.Speed => 0.9f,

            PokemonNature.Rash when stat == PokemonStat.SpecialAttack => 1.1f,
            PokemonNature.Rash when stat == PokemonStat.SpecialDefense => 0.9f,

            PokemonNature.Calm when stat == PokemonStat.SpecialDefense => 1.1f,
            PokemonNature.Calm when stat == PokemonStat.Attack => 0.9f,

            PokemonNature.Gentle when stat == PokemonStat.SpecialDefense => 1.1f,
            PokemonNature.Gentle when stat == PokemonStat.Defense => 0.9f,

            PokemonNature.Sassy when stat == PokemonStat.SpecialDefense => 1.1f,
            PokemonNature.Sassy when stat == PokemonStat.Speed => 0.9f,

            PokemonNature.Careful when stat == PokemonStat.SpecialDefense => 1.1f,
            PokemonNature.Careful when stat == PokemonStat.SpecialAttack => 0.9f,

            _ => 1.0f
        };
    }
}
