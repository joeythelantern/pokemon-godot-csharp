using Game.Core;
using Game.Gameplay;
using Godot;
using System;

public partial class PickupPokemon : StaticBody2D
{
    [Export]
    public string PokemonName;

    [Export]
    public int Level = 5;

    private PokemonResource pokemonResource;

    public override void _Ready()
    {
        if (PokemonName == "")
            return;

        pokemonResource = PokemonDatabase.Get(PokemonName);
    }

    public void Pickup(Player player)
    {
        Logger.Info($"Attempting to pickup ${PokemonName}");

        if (PokemonName == "")
            return;

        if (pokemonResource == null)
            return;

        Level level = SceneManager.GetCurrentLevel();

        PokemonMetData pokemonMetData = new()
        {
            OriginalTrainer = "Player",
            Location = level.LevelName.ToString(),
            LevelMet = Level,
            UnixTimeMet = Time.GetUnixTimeFromSystem()
        };

        var natures = Enum.GetValues<PokemonNature>();
        int index = Globals.GetRandomNumberGenerator().RandiRange(0, natures.Length - 1);
        bool shiny = Globals.GetRandomNumberGenerator().RandiRange(1, 8192) == 1;

        player.Backpack.AddPokemonToParty(pokemonResource, Level, shiny, pokemonMetData, natures[index]);
    }
}
