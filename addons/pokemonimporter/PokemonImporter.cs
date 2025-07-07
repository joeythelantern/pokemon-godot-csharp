#if TOOLS
using Game.Core;
using Game.Utilities;
using Game.Resources;
using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using Godot.Collections;

[Tool]
public partial class PokemonImporter : EditorPlugin
{
	private const string ImportMenuItemText = "Import Gen 1 Pokemon";
	private static readonly HttpClient _httpClient = new HttpClient();

	public override void _EnterTree()
	{
		AddToolMenuItem(ImportMenuItemText, Callable.From(() => { ImportPokemon(); }));
		Logger.Info("Pokemon Importer plugin initialized.");
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(ImportMenuItemText);
		Logger.Info("Pokemon Importer plugin exited.");
		_httpClient.Dispose();
	}

	public async void ImportPokemon()
	{
		Logger.Info($"Starting to fetch Generation I Pokemon from ID 1 to 151...");

		var pokemonFolderPath = "res://resources/pokemon/";
		var spriteFolderPath = "res://assets/pokemon/ui/";

		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(pokemonFolderPath));
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(spriteFolderPath));

		const int gcInterval = 10;
		for (int pokemonId = 1; pokemonId <= 1; pokemonId++)
		{
			Logger.Info($"Processing Pokemon ID: {pokemonId}");

			JObject? pokemonData = await HttpModule.FetchDataFromPokeApi(_httpClient, $"pokemon/{pokemonId}");
			if (pokemonData == null)
			{
				Logger.Warning($"Could not fetch data for Pokemon ID {pokemonId}. Skipping.");
				continue;
			}

			JObject? speciesData = await HttpModule.FetchDataFromPokeApi(_httpClient, $"pokemon-species/{pokemonId}");
			if (speciesData == null)
			{
				Logger.Warning($"Could not fetch species data for Pokemon ID {pokemonId}. Description might be missing.");
			}

			var pokemonName = pokemonData["name"]?.ToString();
			if (string.IsNullOrEmpty(pokemonName))
			{
				Logger.Warning($"Pokemon ID {pokemonId} has no name. Skipping.");
				continue;
			}

			Logger.Info($"Importing: {pokemonName}");

			await CreatePokemonResource(pokemonName, pokemonData, speciesData, pokemonFolderPath, spriteFolderPath);

			if (pokemonId % gcInterval == 0)
			{
				Logger.Info($"Triggering garbage collection after {pokemonId} Pokemon...");
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Logger.Info("Garbage collection completed.");
			}

			await Task.Delay(100);
		}

		Logger.Info("Finished importing Generation I Pokemon. Refreshing filesystem...");
		EditorInterface.Singleton.GetResourceFilesystem().Scan();
		Logger.Info("Pokemon import process completed.");
	}

	private async Task<string?> DownloadSprite(string imageUrl, string saveFolderPath, string fileName)
	{
		if (string.IsNullOrEmpty(imageUrl)) return null;

		string fullSavePath = ProjectSettings.GlobalizePath($"{saveFolderPath}{fileName}");
		string resourcePath = $"{saveFolderPath}{fileName}";

		try
		{
			byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
			File.WriteAllBytes(fullSavePath, imageBytes);
			return resourcePath;
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to download sprite from {imageUrl} to {resourcePath}: {e.Message}");
			return null;
		}
	}

	private async Task CreatePokemonResource(string pokemonName, JObject pokemonData, JObject? speciesData, string pokemonFolderPath, string spriteFolderPath)
	{
		PokemonResource pokemonResource = new()
		{
			Name = pokemonName,
			Id = (int)pokemonData["id"],
			Height = (int)pokemonData["height"],
			Weight = (int)pokemonData["weight"],
			BaseExperience = (int)pokemonData["base_experience"],
		};

		var englishFlavorText = speciesData?["flavor_text_entries"]?
			.Children<JObject>()
			.FirstOrDefault(entry => entry["language"]?["name"]?.ToString() == "en" && entry["version"]?["name"]?.ToString() == "red")?
			["flavor_text"]?.ToString();
		pokemonResource.Description = englishFlavorText?.Replace("\n", " ") ?? "No description available.";

		var stats = pokemonData["stats"] as JArray;

		if (stats != null)
		{
			foreach (var statEntry in stats)
			{
				string? statName = (string)statEntry["stat"]["name"];
				int baseStatValue = (int)statEntry["base_stat"];

				if (!string.IsNullOrEmpty(statName))
				{
					string pascalStatName = StringModule.ToPascalCase(statName.Replace("-", "_"));
					if (Enum.TryParse<PokemonStat>(pascalStatName, out var parsedStat))
					{
						switch (parsedStat)
						{
							case PokemonStat.Hp: pokemonResource.BaseHp = baseStatValue; break;
							case PokemonStat.Attack: pokemonResource.BaseAttack = baseStatValue; break;
							case PokemonStat.Defense: pokemonResource.BaseDefense = baseStatValue; break;
							case PokemonStat.SpecialAttack: pokemonResource.BaseSpecialAttack = baseStatValue; break;
							case PokemonStat.SpecialDefense: pokemonResource.BaseSpecialDefense = baseStatValue; break;
							case PokemonStat.Speed: pokemonResource.BaseSpeed = baseStatValue; break;
						}
					}
					else
					{
						Logger.Warning($"Unknown PokemonStat: '{statName}' for {pokemonName}");
					}
				}
			}
		}

		var sprites = pokemonData["sprites"];

		string frontDefaultUrl = (string)sprites["front_default"];
		string backDefaultUrl = (string)sprites["back_default"];
		string frontShinyUrl = (string)sprites["front_shiny"];
		string backShinyUrl = (string)sprites["back_shiny"];

		pokemonResource.FrontSprite = await LoadTextureFromUrl(frontDefaultUrl, spriteFolderPath, $"{pokemonName}_front.png");
		pokemonResource.BackSprite = await LoadTextureFromUrl(backDefaultUrl, spriteFolderPath, $"{pokemonName}_back.png");
		pokemonResource.ShinyFrontSprite = await LoadTextureFromUrl(frontShinyUrl, spriteFolderPath, $"{pokemonName}_shiny_front.png");
		pokemonResource.ShinyBackSprite = await LoadTextureFromUrl(backShinyUrl, spriteFolderPath, $"{pokemonName}_shiny_back.png");

		pokemonResource.LearnableMoves = GetAllLearnableMovesForGenOne((JArray)pokemonData["moves"]);
		pokemonResource.LevelUpMoves = GetLevelUpResourcesForGenOne((JArray)pokemonData["moves"]);
		pokemonResource.Machines = GetAllMachinesForGenOne((JArray)pokemonData["moves"]);

		var savePath = $"{pokemonFolderPath}{pokemonName}.tres";

		var err = ResourceSaver.Save(pokemonResource, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save Pokemon {pokemonName} to {savePath}: {err}");
	}

	private async Task<Texture2D?> LoadTextureFromUrl(string? imageUrl, string saveFolderPath, string fileName)
	{
		if (string.IsNullOrEmpty(imageUrl)) return null;

		string fullSavePath = ProjectSettings.GlobalizePath($"{saveFolderPath}{fileName}");
		string resourcePath = $"{saveFolderPath}{fileName}";

		try
		{
			string? downloadedPath = await DownloadSprite(imageUrl, saveFolderPath, fileName);

			if (downloadedPath == null)
			{
				return null;
			}

			byte[] imageBytes = File.ReadAllBytes(fullSavePath);

			var image = new Image();
			var err = image.LoadPngFromBuffer(imageBytes);

			if (err != Error.Ok)
			{
				Logger.Error($"Failed to load image from bytes for {resourcePath}: {err}");
				return null;
			}

			var texture = ImageTexture.CreateFromImage(image);

			return texture;
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to create Texture2D from downloaded sprite {resourcePath}: {e.Message}");
			return null;
		}
	}

	public Array<string> GetAllLearnableMovesForGenOne(JArray moves)
	{
		Array<string> learnableMoves = [];

		foreach (var move in moves)
		{
			string name = (string)move["move"]["name"];

			foreach (var versionGroup in move["version_group_details"])
			{
				string version = (string)versionGroup["version_group"]["name"];

				if (version == "red-blue" || version == "yellow")
				{
					learnableMoves.Add(name);
					break;
				}
			}
		}

		return learnableMoves;
	}

	public Dictionary<string, int> GetLevelUpResourcesForGenOne(JArray moves)
	{
		Dictionary<string, int> levelUpMoves = [];

		foreach (var move in moves)
		{
			string name = (string)move["move"]["name"];

			foreach (var versionGroup in move["version_group_details"])
			{
				string version = (string)versionGroup["version_group"]["name"];
				int level = (int)versionGroup["level_learned_at"];

				if ((version == "red-blue" || version == "yellow") && level != 0)
				{
					levelUpMoves.Add(name, level);

					break;
				}
			}
		}

		return levelUpMoves;
	}

	public Array<string> GetAllMachinesForGenOne(JArray moves)
	{
		Array<string> machineMoves = [];

		foreach (var move in moves)
		{
			string name = (string)move["move"]["name"];

			foreach (var versionGroup in move["version_group_details"])
			{
				string version = (string)versionGroup["version_group"]["name"];
				string method = (string)versionGroup["move_learn_method"]["name"];

				if ((version == "red-blue" || version == "yellow") && method == "machine")
				{
					machineMoves.Add(name);
					break;
				}
			}
		}

		return machineMoves;
	}
}
#endif
