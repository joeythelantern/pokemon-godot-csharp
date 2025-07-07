#if TOOLS
using Game.Core;
using Game.Resources;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;

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

		const int gcInterval = 10; // Perform GC every 10 Pokemon
		for (int pokemonId = 1; pokemonId <= 151; pokemonId++)
		{
			Logger.Info($"Processing Pokemon ID: {pokemonId}");

			JObject? pokemonData = await FetchData($"pokemon/{pokemonId}");
			if (pokemonData == null)
			{
				Logger.Warning($"Could not fetch data for Pokemon ID {pokemonId}. Skipping.");
				continue;
			}

			JObject? speciesData = await FetchData($"pokemon-species/{pokemonId}");
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

			Logger.Debug($"Importing: {pokemonName}");

			await CreatePokemonResource(pokemonName, pokemonData, speciesData, pokemonFolderPath, spriteFolderPath);

			if (pokemonId % gcInterval == 0)
			{
				Logger.Debug($"Triggering garbage collection after {pokemonId} Pokemon...");
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Logger.Debug("Garbage collection completed.");
			}

			await Task.Delay(100);
		}

		Logger.Info("Finished importing Generation I Pokemon. Refreshing filesystem...");
		EditorInterface.Singleton.GetResourceFilesystem().Scan();
		Logger.Info("Pokemon import process completed.");
	}

	private async Task<JObject?> FetchData(string endpoint)
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
					return null; // Explicitly return null for 404, no retries needed
				}

				response.EnsureSuccessStatusCode(); // Throws HttpRequestException for other 4xx/5xx errors
				var json = await response.Content.ReadAsStringAsync();
				return JObject.Parse(json);
			}
			catch (Exception e)
			{
				Logger.Error($"Error fetching {endpoint} (attempt {attempt + 1}/{maxAttempts}): {e.Message}");
				if (attempt < maxAttempts - 1)
				{
					await Task.Delay(retryDelayMs); // Wait before retrying
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

	private async Task CreatePokemonResource(string pokemonName, JObject pokemonData, JObject? speciesData, string pokemonFolderPath, string spriteFolderPath)
	{
		var pokemonResource = new PokemonResource();
		pokemonResource.Name = pokemonName;
		pokemonResource.UUID = pokemonData["id"]?.ToString() ?? Guid.NewGuid().ToString(); // Use ID as UUID, fallback to new GUID

		pokemonResource.Height = pokemonData["height"]?.ToObject<int>() ?? 0;
		pokemonResource.Weight = pokemonData["weight"]?.ToObject<int>() ?? 0;
		pokemonResource.BaseExperience = pokemonData["base_experience"]?.ToObject<int>() ?? 0;

		var englishFlavorText = speciesData?["flavor_text_entries"]?
			.Children<JObject>()
			.FirstOrDefault(entry => entry["language"]?["name"]?.ToString() == "en" && entry["version"]?["name"]?.ToString() == "red")? // Prioritize 'red' version flavor text
			["flavor_text"]?.ToString();
		pokemonResource.Description = englishFlavorText?.Replace("\n", " ") ?? "No description available.";

		// Base Stats
		var stats = pokemonData["stats"] as JArray;
		if (stats != null)
		{
			foreach (var statEntry in stats)
			{
				string? statName = statEntry["stat"]?["name"]?.ToString();
				int baseStatValue = statEntry["base_stat"]?.ToObject<int>() ?? 0;

				if (!string.IsNullOrEmpty(statName))
				{
					// Convert snake-case to PascalCase for enum parsing
					string pascalStatName = ToPascalCase(statName.Replace("-", "_"));
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

		// Sprites
		var sprites = pokemonData["sprites"];
		string formattedPokemonName = pokemonName.Replace('-', '_'); // For consistent file naming

		string? frontDefaultUrl = sprites?["front_default"]?.ToString();
		string? backDefaultUrl = sprites?["back_default"]?.ToString();
		string? frontShinyUrl = sprites?["front_shiny"]?.ToString();
		string? backShinyUrl = sprites?["back_shiny"]?.ToString();

		pokemonResource.FrontSprite = await LoadTextureFromUrl(frontDefaultUrl, spriteFolderPath, $"{formattedPokemonName}_front.png");
		pokemonResource.BackSprite = await LoadTextureFromUrl(backDefaultUrl, spriteFolderPath, $"{formattedPokemonName}_back.png");
		pokemonResource.ShinyFrontSprite = await LoadTextureFromUrl(frontShinyUrl, spriteFolderPath, $"{formattedPokemonName}_shiny_front.png");
		pokemonResource.ShinyBackSprite = await LoadTextureFromUrl(backShinyUrl, spriteFolderPath, $"{formattedPokemonName}_shiny_back.png");

		pokemonResource.LevelUpMoves = new Array<LevelUpMoveResource>();
		pokemonResource.TechnicalMachines = new Array<TMResource>();
		pokemonResource.HiddenMachines = new Array<HMResource>();

		string fileName = pokemonName.Replace('-', '_');
		var savePath = $"{pokemonFolderPath}{fileName}.tres";

		var err = ResourceSaver.Save(pokemonResource, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save Pokemon {pokemonName} to {savePath}: {err}");
		else
			Logger.Debug($"Successfully saved Pokemon {pokemonName} to {savePath}");
	}

	private async Task<Texture2D?> LoadTextureFromUrl(string? imageUrl, string saveFolderPath, string fileName)
	{
		if (string.IsNullOrEmpty(imageUrl)) return null;

		string fullSavePath = ProjectSettings.GlobalizePath($"{saveFolderPath}{fileName}");
		string resourcePath = $"{saveFolderPath}{fileName}"; // This is the path where the file is saved

		try
		{
			// First, download and save the sprite to disk
			string? downloadedPath = await DownloadSprite(imageUrl, saveFolderPath, fileName);

			if (downloadedPath == null)
			{
				return null; // Download failed
			}

			// Now, read the bytes directly from the saved file
			// This bypasses the need for Godot's editor to "import" the file immediately
			byte[] imageBytes = File.ReadAllBytes(fullSavePath);

			// Create a Godot.Image from the bytes
			var image = new Image();
			var err = image.LoadPngFromBuffer(imageBytes); // Or LoadJpgFromBuffer etc.
			if (err != Error.Ok)
			{
				Logger.Error($"Failed to load image from bytes for {resourcePath}: {err}");
				return null;
			}

			// Create an ImageTexture from the Godot.Image
			var texture = ImageTexture.CreateFromImage(image);

			// It's good practice to free the Image object if it's no longer needed
			// However, Godot's C# bindings often handle this through reference counting.
			// For safety, if Image was a C# object, you might free it, but here it's managed.

			return texture;
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to create Texture2D from downloaded sprite {resourcePath}: {e.Message}");
			return null;
		}
	}

	private string ToPascalCase(string? input)
	{
		if (string.IsNullOrEmpty(input)) return "";
		var parts = input.Replace("-", "_").Split('_');
		return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
	}
}
#endif
