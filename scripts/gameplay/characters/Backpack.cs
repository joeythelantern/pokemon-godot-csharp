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

    public void AddPokemonToParty(PokemonResource pokemonResource, int level, bool shiny, PokemonMetData pokemonMetData, PokemonNature pokemonNature, string nickName = "")
    {
        Logger.Info($"Adding pokemon to {BackpackOwner.Name}'s party: {pokemonResource.Name}");
        PokemonInstance pokemon = new();
        pokemon.Name = nickName == "" ? pokemonResource.Name : nickName;
        Party.AddChild(pokemon);
        pokemon.Initialize(pokemonResource, level, shiny, pokemonMetData, pokemonNature);
    }

}
