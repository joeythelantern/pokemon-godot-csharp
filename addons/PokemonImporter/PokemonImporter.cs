#if TOOLS
using System;
using System.Threading.Tasks;
using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;

[Tool]
public partial class PokemonImporter : EditorPlugin
{
	private const string importMenuItemText = "Import Pokemon";
	private const string resourcePath = "res://resources/pokemon/";
	private const string spritePath = "res://assets/pokemon/";
	private const string apiPath = "https://pokeapi.co/api/v2/pokemon/";

	public override void _EnterTree()
	{
		AddToolMenuItem(importMenuItemText, Callable.From(ImportPokemon));
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(importMenuItemText);
	}

	public async void ImportPokemon()
	{
		Logger.Info("Attempting to import Pokemon ...");

		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(resourcePath));
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(spritePath));

		const int gcInterval = 10;

		for (int i = 1; i <= Globals.POKEMON_NUMBERS; i++)
		{
			Logger.Info($"Processing Pokemon with ID: {i}");

			Variant response = await Modules.FetchDataFromPokeApi($"{apiPath}{i}");
			Dictionary<string, Variant> data = response.AsGodotDictionary<string, Variant>();

			var pokemonName = data["name"].AsString();
			if (string.IsNullOrEmpty(pokemonName))
			{
				Logger.Warning($"Pokemon {i} has no name ...");
				continue;
			}

			Logger.Info($"Creating resource for {pokemonName} ...");

			if (i % gcInterval == 0)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Logger.Info("Garbage collected!");
			}



			await Task.Delay(100);
		}

		EditorInterface.Singleton.GetResourceFilesystem().Scan();
	}
}
#endif
