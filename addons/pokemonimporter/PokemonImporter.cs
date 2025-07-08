#if TOOLS
using Game.Core; // Assuming Logger is here
using Game.Utilities; // Assuming StringModule.ToPascalCase is here
using Game.Resources; // Assuming PokemonResource, LevelUpMoveResource, MoveResource, etc. are here
using Godot;
using Godot.Collections;
using Newtonsoft.Json; // Required for JsonConvert and JsonProperty attributes
using System;
using System.Collections.Generic; // Required for List<T> and Dictionary<TKey, TValue>
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using System.IO;

public class PokemonStatEntry
{
	[JsonProperty("base_stat")]
	public int BaseStat { get; set; }
	[JsonProperty("effort")]
	public int Effort { get; set; }
	[JsonProperty("stat")]
	public ApiResource Stat { get; set; }
}

public class PokemonSpriteUrls
{
	[JsonProperty("front_default")]
	public string FrontDefault { get; set; }
	[JsonProperty("back_default")]
	public string BackDefault { get; set; }
	[JsonProperty("front_shiny")]
	public string FrontShiny { get; set; }
	[JsonProperty("back_shiny")]
	public string BackShiny { get; set; }
}

public class VersionGroupDetail
{
	[JsonProperty("level_learned_at")]
	public int LevelLearnedAt { get; set; }
	[JsonProperty("move_learn_method")]
	public ApiResource MoveLearnMethod { get; set; }
	[JsonProperty("version_group")]
	public ApiResource VersionGroup { get; set; }
	[JsonProperty("order")]
	public int? Order { get; set; }
}

public class PokemonMoveEntry
{
	[JsonProperty("move")]
	public ApiResource Move { get; set; }
	[JsonProperty("version_group_details")]
	public List<VersionGroupDetail> VersionGroupDetails { get; set; }
}

public class PokemonTypeEntry
{
	[JsonProperty("slot")]
	public int Slot { get; set; }
	[JsonProperty("type")]
	public ApiResource Type { get; set; }
}

public class PokemonApiResponse
{
	[JsonProperty("id")]
	public int Id { get; set; }
	[JsonProperty("name")]
	public string Name { get; set; }
	[JsonProperty("height")]
	public int Height { get; set; }
	[JsonProperty("weight")]
	public int Weight { get; set; }
	[JsonProperty("base_experience")]
	public int BaseExperience { get; set; }

	[JsonProperty("stats")]
	public List<PokemonStatEntry> Stats { get; set; }

	[JsonProperty("sprites")]
	public PokemonSpriteUrls Sprites { get; set; }

	[JsonProperty("moves")]
	public List<PokemonMoveEntry> Moves { get; set; }

	[JsonProperty("types")]
	public List<PokemonTypeEntry> Types { get; set; }
}

public class FlavorTextEntry
{
	[JsonProperty("flavor_text")]
	public string FlavorText { get; set; }
	[JsonProperty("language")]
	public ApiResource Language { get; set; }
	[JsonProperty("version")]
	public ApiResource Version { get; set; }
}

public class PokemonSpeciesApiResponse
{
	[JsonProperty("id")]
	public int Id { get; set; }
	[JsonProperty("name")]
	public string Name { get; set; }
	[JsonProperty("flavor_text_entries")]
	public List<FlavorTextEntry> FlavorTextEntries { get; set; }
	[JsonProperty("generation")]
	public ApiResource Generation { get; set; }
}

[Tool]
public partial class PokemonImporter : EditorPlugin
{
	private const string ImportMenuItemText = "Import All Gen 1 Pokemon";
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

		const int gcInterval = 10; // Perform GC every 10 Pokemon
		for (int pokemonId = 1; pokemonId <= 151; pokemonId++)
		{
			Logger.Info($"Processing Pokemon ID: {pokemonId}");

			PokemonApiResponse? pokemonData = await FetchData<PokemonApiResponse>($"pokemon/{pokemonId}");
			if (pokemonData == null)
			{
				Logger.Warning($"Could not fetch data for Pokemon ID {pokemonId}. Skipping.");
				continue;
			}

			PokemonSpeciesApiResponse? speciesData = await FetchData<PokemonSpeciesApiResponse>($"pokemon-species/{pokemonId}");
			if (speciesData == null)
			{
				Logger.Warning($"Could not fetch species data for Pokemon ID {pokemonId}. Description might be missing.");
			}

			var pokemonName = pokemonData.Name;
			if (string.IsNullOrEmpty(pokemonName))
			{
				Logger.Warning($"Pokemon ID {pokemonId} has no name. Skipping.");
				continue;
			}

			Logger.Info($"Importing: {pokemonName}");

			await CreatePokemonResource(pokemonName, pokemonData, speciesData, pokemonFolderPath, spriteFolderPath);

			pokemonData = null; // Aid GC
			speciesData = null; // Aid GC

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

	private async Task<T?> FetchData<T>(string endpoint) where T : class
	{
		string url = $"https://pokeapi.co/api/v2/{endpoint}/";
		const int maxAttempts = 3;
		const int retryDelayMs = 1000; // 1 second

		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			try
			{
				var response = await _httpClient.GetAsync(url);
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					Logger.Debug($"404 Not Found for {endpoint}.");
					return null;
				}

				response.EnsureSuccessStatusCode();
				var json = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception e)
			{
				Logger.Error($"Error fetching {endpoint} (attempt {attempt + 1}/{maxAttempts}): {e.Message}");
				if (attempt < maxAttempts - 1)
				{
					await Task.Delay(retryDelayMs);
				}
			}
		}

		Logger.Error($"Failed to fetch {endpoint} after {maxAttempts} retries.");
		return null;
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

	/// <summary>
	/// Helper to load a Texture2D from a URL by downloading and then creating ImageTexture from bytes.
	/// </summary>
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

	private async Task CreatePokemonResource(string pokemonName, PokemonApiResponse pokemonData, PokemonSpeciesApiResponse? speciesData, string pokemonFolderPath, string spriteFolderPath)
	{
		PokemonResource pokemonResource = new()
		{
			Name = pokemonName,
			Id = pokemonData.Id, // Changed from UUID to Id (int)
			Height = pokemonData.Height,
			Weight = pokemonData.Weight,
			BaseExperience = pokemonData.BaseExperience,
		};

		// Description from species data
		var englishFlavorText = speciesData?.FlavorTextEntries?
			.FirstOrDefault(entry => entry.Language?.Name == "en" && entry.Version?.Name == "red")?
			.FlavorText;
		pokemonResource.Description = englishFlavorText?.Replace("\n", " ") ?? "No description available.";

		// Base Stats
		if (pokemonData.Stats != null)
		{
			foreach (var statEntry in pokemonData.Stats)
			{
				string? statName = statEntry.Stat?.Name;
				int baseStatValue = statEntry.BaseStat;

				if (!string.IsNullOrEmpty(statName))
				{
					string pascalStatName = StringModule.ToPascalCase(statName.Replace("-", "_"));
					if (Enum.TryParse<PokemonStat>(pascalStatName, out var parsedStat))
					{
						switch (parsedStat)
						{
							// --- MODIFIED: Casing for BaseHp, BaseDefense, BaseSpecialDefense ---
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

		// Sprites
		string formattedPokemonName = pokemonName.Replace('-', '_');

		pokemonResource.FrontSprite = await LoadTextureFromUrl(pokemonData.Sprites?.FrontDefault, spriteFolderPath, $"{formattedPokemonName}_front.png");
		pokemonResource.BackSprite = await LoadTextureFromUrl(pokemonData.Sprites?.BackDefault, spriteFolderPath, $"{formattedPokemonName}_back.png");
		pokemonResource.ShinyFrontSprite = await LoadTextureFromUrl(pokemonData.Sprites?.FrontShiny, spriteFolderPath, $"{formattedPokemonName}_shiny_front.png");
		pokemonResource.ShinyBackSprite = await LoadTextureFromUrl(pokemonData.Sprites?.BackShiny, spriteFolderPath, $"{formattedPokemonName}_shiny_back.png");

		// --- MODIFIED: Move parsing methods to match new PokemonResource properties ---
		pokemonResource.LearnableMoves = GetAllLearnableMovesForGenOne(pokemonData.Moves);
		pokemonResource.LevelUpMoves = GetLevelUpMovesForGenOne(pokemonData.Moves); // Returns Dictionary<string, int>
		pokemonResource.Machines = GetAllMachineMovesForGenOne(pokemonData.Moves); // Returns Array<string>

		var savePath = $"{pokemonFolderPath}{formattedPokemonName}.tres";

		var err = ResourceSaver.Save(pokemonResource, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save Pokemon {pokemonName} to {savePath}: {err}");
		else
			Logger.Info($"Successfully saved Pokemon {pokemonName} to {savePath}");
	}

	/// <summary>
	/// Gathers all moves learnable by a Pokemon in Generation 1 (any method) by name.
	/// </summary>
	/// <param name="moves">List of PokemonMoveEntry objects.</param>
	/// <returns>Godot Array of string (move names).</returns>
	public Array<string> GetAllLearnableMovesForGenOne(List<PokemonMoveEntry>? moves)
	{
		Array<string> learnableMoves = new();
		if (moves == null) return learnableMoves;

		foreach (var moveEntry in moves)
		{
			// Check if this move is learnable in any Gen 1 version group by any method
			bool isGen1Move = moveEntry.VersionGroupDetails?
				.Any(detail => detail.VersionGroup?.Name == "red-blue" || detail.VersionGroup?.Name == "yellow") ?? false;

			if (isGen1Move)
			{
				string? moveName = moveEntry.Move?.Name;
				if (!string.IsNullOrEmpty(moveName))
				{
					learnableMoves.Add(moveName);
				}
			}
		}
		return learnableMoves;
	}


	/// <summary>
	/// Parses level-up moves for Generation 1 (red-blue or yellow version groups) from strongly-typed data.
	/// Returns a Dictionary of move name to level learned.
	/// </summary>
	/// <param name="moves">List of PokemonMoveEntry objects.</param>
	/// <returns>Godot Dictionary<string, int> of level-up moves.</returns>
	public Godot.Collections.Dictionary<string, int> GetLevelUpMovesForGenOne(List<PokemonMoveEntry>? moves) // Renamed from GetLevelUpResourcesForGenOne
	{
		Godot.Collections.Dictionary<string, int> levelUpMoves = new(); // Changed to Dictionary<string, int>

		if (moves == null) return levelUpMoves;

		foreach (var moveEntry in moves)
		{
			// Find a level-up method detail that belongs to a Generation 1 version group
			var gen1LevelUpDetail = moveEntry.VersionGroupDetails?
				.FirstOrDefault(detail =>
					detail.MoveLearnMethod?.Name == "level-up" &&
					(detail.VersionGroup?.Name == "red-blue" ||
					 detail.VersionGroup?.Name == "yellow"));

			if (gen1LevelUpDetail != null)
			{
				int levelLearned = gen1LevelUpDetail.LevelLearnedAt;
				string? moveNameFromEntry = moveEntry.Move?.Name;

				if (!string.IsNullOrEmpty(moveNameFromEntry))
				{
					// Add to dictionary (handle potential duplicates if a move is learned at different levels in red/blue/yellow)
					// Prioritize the lowest level if multiple entries for the same move exist.
					if (!levelUpMoves.ContainsKey(moveNameFromEntry) || levelLearned < levelUpMoves[moveNameFromEntry])
					{
						levelUpMoves[moveNameFromEntry] = levelLearned;
					}
				}
			}
		}
		return levelUpMoves;
	}

	public Array<string> GetAllMachineMovesForGenOne(List<PokemonMoveEntry>? moves) // New method, combines TM/HM logic
	{
		Array<string> machineMoves = new();
		if (moves == null) return machineMoves;

		foreach (var moveEntry in moves)
		{
			var gen1MachineDetail = moveEntry.VersionGroupDetails?
				.FirstOrDefault(detail =>
					detail.MoveLearnMethod?.Name == "machine" &&
					(detail.VersionGroup?.Name == "red-blue" ||
					 detail.VersionGroup?.Name == "yellow"));

			if (gen1MachineDetail != null)
			{
				string? moveNameFromEntry = moveEntry.Move?.Name;
				if (!string.IsNullOrEmpty(moveNameFromEntry))
				{
					if (!machineMoves.Contains(moveNameFromEntry))
					{
						machineMoves.Add(moveNameFromEntry);
					}
				}
			}
		}
		return machineMoves;
	}
}
#endif
