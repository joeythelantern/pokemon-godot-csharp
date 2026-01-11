using Game.Core;
using Godot;

namespace Game.Gameplay;

public partial class Backpack : Node
{
    [ExportCategory("Components")]
    [Export]
    public Node BackpackOwner;

    [Export]
    public Node Party;

    [Export]
    public Node Items;

    public override void _Ready()
    {
        BackpackOwner ??= GetParent();
        Party ??= GetNode("Party");
        Items ??= GetNode("Items");
    }

    public void AddPokemonToParty(PokemonResource pokemonResource, int level, bool shiny, PokemonMetData pokemonMetData, PokemonNature pokemonNature)
    {
        Logger.Info($"Adding pokemon to {BackpackOwner}'s party: {pokemonResource.Name}");
        PokemonInstance pokemon = new();
        Party.AddChild(pokemon);
        pokemon.Initialize(pokemonResource, level, shiny, pokemonMetData, pokemonNature);
    }

}
