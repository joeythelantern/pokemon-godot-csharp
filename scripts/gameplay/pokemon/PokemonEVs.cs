using Game.Core;
using Godot;

[GlobalClass]
public partial class PokemonEVs : Resource
{
    public const int MaxPerStat = 255;
    public const int MaxTotal = 510;

    [Export] public int HP { get; private set; }
    [Export] public int Attack { get; private set; }
    [Export] public int Defense { get; private set; }
    [Export] public int SpecialAttack { get; private set; }
    [Export] public int SpecialDefense { get; private set; }
    [Export] public int Speed { get; private set; }

    public int Total =>
        HP + Attack + Defense + SpecialAttack + SpecialDefense + Speed;

    public int AddEV(PokemonStat stat, int amount)
    {
        if (amount <= 0 || Total >= MaxTotal)
            return 0;

        int allowed = Mathf.Min(amount, MaxPerStat - GetStat(stat));
        allowed = Mathf.Min(allowed, MaxTotal - Total);

        if (allowed <= 0)
            return 0;

        SetStat(stat, GetStat(stat) + allowed);
        return allowed;
    }


    private int GetStat(PokemonStat stat) => stat switch
    {
        PokemonStat.Hp => HP,
        PokemonStat.Attack => Attack,
        PokemonStat.Defense => Defense,
        PokemonStat.SpecialAttack => SpecialAttack,
        PokemonStat.SpecialDefense => SpecialDefense,
        PokemonStat.Speed => Speed,
        _ => 0
    };

    private void SetStat(PokemonStat stat, int value)
    {
        switch (stat)
        {
            case PokemonStat.Hp: HP = value; break;
            case PokemonStat.Attack: Attack = value; break;
            case PokemonStat.Defense: Defense = value; break;
            case PokemonStat.SpecialAttack: SpecialAttack = value; break;
            case PokemonStat.SpecialDefense: SpecialDefense = value; break;
            case PokemonStat.Speed: Speed = value; break;
        }
    }
}
