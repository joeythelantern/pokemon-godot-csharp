#if TOOLS
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game.Core;
using Game.Gameplay;
using Game.Resources;
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

			Variant pokemonResponse = await Modules.FetchDataFromPokeApi($"{apiPath}{i}");
			Dictionary<string, Variant> pokemonData = pokemonResponse.AsGodotDictionary<string, Variant>();

			var pokemonName = pokemonData["name"].AsString();
			if (string.IsNullOrEmpty(pokemonName))
			{
				Logger.Warning($"Pokemon {i} has no name ...");
				continue;
			}

			var species = pokemonData["species"].AsGodotDictionary<string, string>();
			Variant speciesResponse = await Modules.FetchDataFromPokeApi($"{species["url"]}");
			Dictionary<string, Variant> speciesData = speciesResponse.AsGodotDictionary<string, Variant>();

			Logger.Info($"Creating resource for {pokemonName} ...");

			await CreatePokemonResource(pokemonName, pokemonData, speciesData);

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

	private async Task CreatePokemonResource(string pokemonName, Dictionary<string, Variant> pokemonData, Dictionary<string, Variant> speciesData)
	{
		var flavorTextEntries = speciesData["flavor_text_entries"].AsGodotArray<Dictionary<string, Variant>>();
		var description = flavorTextEntries.FirstOrDefault(entry => entry["language"].AsGodotDictionary<string, string>()["name"] == "en");

		var pokemon = new PokemonResource()
		{
			Name = pokemonName,
			Id = pokemonData["id"].AsInt32(),
			Description = description != null ? description["flavor_text"].AsString() : "Description not available.",

			Height = pokemonData["height"].AsInt32(),
			Weight = pokemonData["weight"].AsInt32(),
			BaseExperience = pokemonData["base_experience"].AsInt32()
		};

		var stats = pokemonData["stats"].AsGodotArray<Dictionary<string, Variant>>();

		for (int i = 0; i < stats.Count; i++)
		{
			var stat = stats[i]["stat"].AsGodotDictionary<string, string>()["name"];
			var value = stats[i]["base_stat"].AsInt32();
			var parsed = PokemonEnum.StatMap.TryGetValue(stat, out var parsedStat) ? parsedStat : PokemonStat.None;

			switch (parsed)
			{
				case PokemonStat.Hp: pokemon.BaseHp = value; break;
				case PokemonStat.Attack: pokemon.BaseAttack = value; break;
				case PokemonStat.Defense: pokemon.BaseDefense = value; break;
				case PokemonStat.SpecialAttack: pokemon.BaseSpecialAttack = value; break;
				case PokemonStat.SpecialDefense: pokemon.BaseSpecialDefense = value; break;
				case PokemonStat.Speed: pokemon.BaseSpeed = value; break;
			}
		}

		var moves = pokemonData["moves"].AsGodotArray<Dictionary<string, Variant>>();

		pokemon.LearnableMoves = GetGenerationOneLearnableMoves(moves);
		pokemon.LevelUpMoves = GetLevelUpMoves(moves);

		var sprites = pokemonData["sprites"].AsGodotDictionary<string, string>();

		pokemon.FrontSprite = await LoadTextureFromUrl(sprites["front_default"], spritePath, $"{pokemonName}_front.png");
		pokemon.ShinyFrontSprite = await LoadTextureFromUrl(sprites["front_shiny"], spritePath, $"{pokemonName}_shiny_front.png");
		pokemon.BackSprite = await LoadTextureFromUrl(sprites["back_default"], spritePath, $"{pokemonName}_back.png");
		pokemon.ShinyBackSprite = await LoadTextureFromUrl(sprites["back_shiny"], spritePath, $"{pokemonName}_shiny_back.png");

		var savePath = $"{resourcePath}{pokemonName.ToLower()}.tres";
		var error = ResourceSaver.Save(pokemon, savePath);

		if (error != Error.Ok)
			Logger.Error($"There was a problem saving the Pokemon {pokemonName} to {savePath}: {error}");
	}

	private Array<string> GetGenerationOneLearnableMoves(Array<Dictionary<string, Variant>> moves)
	{
		Array<string> learnableMoves = new();

		if (moves == null)
			return learnableMoves;

		foreach (var move in moves)
		{
			var versionGroupDetails = move["version_group_details"].AsGodotArray<Dictionary<string, Variant>>();
			bool isGenOneMove = versionGroupDetails.Any(versionGroup =>
			{
				var version = versionGroup["version_group"].AsGodotDictionary<string, string>()["name"];

				if (version == "red-blue" || version == "yellow")
				{
					return true;
				}
				else
				{
					return false;
				}
			});

			if (isGenOneMove)
			{
				var moveName = move["move"].AsGodotDictionary<string, string>()["name"];
				learnableMoves.Add(moveName);
			}
		}

		return learnableMoves;
	}

	private Dictionary<string, int> GetLevelUpMoves(Array<Dictionary<string, Variant>> moves)
	{
		Dictionary<string, int> levelUpMoves = new();

		if (moves == null)
			return levelUpMoves;

		foreach (var move in moves)
		{
			var versionGroupDetails = move["version_group_details"].AsGodotArray<Dictionary<string, Variant>>();
			var generationOneLevelUpMove = versionGroupDetails.FirstOrDefault(versionGroup =>
			{
				var method = versionGroup["move_learn_method"].AsGodotDictionary<string, string>()["name"];
				var version = versionGroup["version_group"].AsGodotDictionary<string, string>()["name"];

				if (method == "level-up" && (version == "red-blue" || version == "yellow"))
				{
					return true;
				}
				else
				{
					return false;
				}
			});

			if (generationOneLevelUpMove != null)
			{
				var moveName = move["move"].AsGodotDictionary<string, string>()["name"];

				if (!levelUpMoves.ContainsKey(moveName))
				{
					levelUpMoves[moveName] = generationOneLevelUpMove["level_learned_at"].AsInt32();
				}
			}
		}

		return levelUpMoves;
	}

	private async Task<Texture2D> LoadTextureFromUrl(string imageUrl, string folder, string fileName)
	{
		string resourcePath = $"{folder}{fileName}";
		string fullSavePath = ProjectSettings.GlobalizePath(resourcePath);

		try
		{
			if (!File.Exists(fullSavePath))
			{
				string downloadedTexture = await Modules.DownloadSprite(imageUrl, folder, fileName);

				if (downloadedTexture == null)
				{
					return null;
				}
			}

			byte[] imageBytes = File.ReadAllBytes(fullSavePath);

			var image = new Image();
			var error = image.LoadPngFromBuffer(imageBytes);

			if (error != Error.Ok)
			{
				Logger.Error($"Failed to load image from bytes for {resourcePath}: {error}");
				return null;
			}

			return ImageTexture.CreateFromImage(image);
		}
		catch (Exception)
		{
			return null;
		}
	}
}
#endif
