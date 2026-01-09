using Godot;

[GlobalClass]
public partial class PokemonIVs : Resource
{
    [Export] public int HP { get; private set; }
    [Export] public int Attack { get; private set; }
    [Export] public int Defense { get; private set; }
    [Export] public int SpecialAttack { get; private set; }
    [Export] public int SpecialDefense { get; private set; }
    [Export] public int Speed { get; private set; }

    public static PokemonIVs Generate(RandomNumberGenerator rng = null)
    {
        rng ??= new RandomNumberGenerator();
        rng.Randomize();

        return new PokemonIVs
        {
            HP = rng.RandiRange(0, 31),
            Attack = rng.RandiRange(0, 31),
            Defense = rng.RandiRange(0, 31),
            SpecialAttack = rng.RandiRange(0, 31),
            SpecialDefense = rng.RandiRange(0, 31),
            Speed = rng.RandiRange(0, 31),
        };
    }
}