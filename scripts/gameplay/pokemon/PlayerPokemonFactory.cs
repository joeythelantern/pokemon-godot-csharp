using System;
using Game.Core;
using Game.Utilities;
using Godot;

namespace Game.Gameplay
{
    public static class PlayerPokemonFactory
    {
        public static void CreateNewPokemon(PokemonName pokemonName, int level = 5, string Location = "", string nickName = "", Status status = Status.NONE)
        {
            PokemonResource pokemonResource = ResourceLoader.Load<PokemonResource>($@"res://resources/pokemon/{pokemonName}.tres");

            PlayerPokemon playerPokemon = new()
            {
                UUID = Guid.NewGuid().ToString(),
                Shiney = new Random().Next(1024) != 0,
                NickName = nickName,
                BasePokemon = pokemonResource,
                Ability = pokemonResource.Abilities.PickRandom(),
                Gender = pokemonResource.Genders.PickRandom(),
                Nature = (Nature)Enum.GetValues(typeof(Nature)).GetValue(new Random().Next(25)),
                Status = status,
                Meet = new()
                {
                    Location = Location,
                    Level = level,
                    OriginalTrainer = "JTL",
                },
                Experience = new()
                {
                    Experience = Stats.GetExperienceRequiredForLevel(pokemonResource.ExperienceGroup, level),
                    Level = level
                },
                Stats = Stats.GetAllStats(pokemonResource, level)
            };
        }
    }
}