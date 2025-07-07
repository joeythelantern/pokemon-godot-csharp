#if TOOLS
using Game.Core;
using Game.Resources;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

[Tool]
public partial class MoveImporter : EditorPlugin
{
	private const string ImportMenuItemText = "Import Gen 1 Moves";
	private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

	public override void _EnterTree()
	{
		// Add a menu item under the 'Project' menu
		AddToolMenuItem(ImportMenuItemText, Callable.From(() => { ImportMoves(); }));
		Logger.Info("Pokemon Move Importer plugin initialized.");
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin: remove the menu item
		RemoveToolMenuItem(ImportMenuItemText);
		Logger.Info("Pokemon Move Importer plugin exited.");
		_httpClient.Dispose(); // Dispose HttpClient when plugin exits
	}

	public async void ImportMoves()
	{
		Logger.Info($"Starting to fetch Generation I moves from ID 1 to 165...");

		var folderPath = "res://resources/moves/";

		// Ensure the directory exists. ProjectSettings.GlobalizePath converts res:// to an OS path.
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));

		for (int moveId = 1; moveId <= 165; moveId++)
		{
			Logger.Debug($"Processing Move ID: {moveId}");
			JObject? data = await FetchMoveData(moveId);
			if (data == null)
				continue; // Move not found or failed after retries, continue to next ID

			var gen = data["generation"]?["name"]?.ToString();
			if (gen != "generation-i")
			{
				Logger.Debug($"Move ID {moveId} ({data["name"]?.ToString() ?? "Unknown"}) is not Generation I. Skipping.");
				continue; // Not a Generation I move, skip
			}

			var moveName = data["name"]?.ToString();
			if (string.IsNullOrEmpty(moveName))
			{
				Logger.Warning($"Move ID {moveId} has no name. Skipping.");
				continue;
			}

			Logger.Debug($"Importing: {moveName}");

			CreateMoveResource(moveName, data, folderPath);

			// Add a small delay to be polite to the API and avoid hitting rate limits
			await Task.Delay(50); // 50 milliseconds
		}

		Logger.Info("Finished importing Generation I moves. Refreshing filesystem...");
		// Refresh Godot's filesystem to show newly created .tres files
		EditorInterface.Singleton.GetResourceFilesystem().Scan();
		Logger.Info("Move import process completed.");
	}

	/// <summary>
	/// Fetches move data from PokeAPI for a given ID with retry logic.
	/// </summary>
	/// <param name="id">The ID of the move to fetch.</param>
	/// <returns>A JObject containing the move data, or null if not found or failed after retries.</returns>
	private async Task<JObject?> FetchMoveData(int id)
	{
		string url = $"https://pokeapi.co/api/v2/move/{id}/";
		const int maxAttempts = 3;
		const int retryDelayMs = 1000; // 1 second

		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			try
			{
				var response = await _httpClient.GetAsync(url);
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					Logger.Debug($"404 Not Found for move ID {id}.");
					return null; // Explicitly return null for 404, no retries needed for this status
				}

				response.EnsureSuccessStatusCode(); // Throws HttpRequestException for other 4xx/5xx errors
				var json = await response.Content.ReadAsStringAsync();
				return JObject.Parse(json);
			}
			catch (Exception e)
			{
				Logger.Error($"Error fetching move {id} (attempt {attempt + 1}/{maxAttempts}): {e.Message}");
				if (attempt < maxAttempts - 1)
				{
					await Task.Delay(retryDelayMs); // Wait before retrying
				}
			}
		}

		Logger.Error($"Failed to fetch move {id} after {maxAttempts} retries.");
		return null;
	}

	/// <summary>
	/// Creates and saves a Godot MoveResource from the fetched JSON data.
	/// </summary>
	/// <param name="moveName">The name of the move.</param>
	/// <param name="data">The JObject containing the move's full API data.</param>
	/// <param name="folderPath">The base folder path to save the resource.</param>
	private void CreateMoveResource(string moveName, JObject data, string folderPath)
	{
		var meta = data["meta"];
		var statChanges = data["stat_changes"] as JArray;

		// Parse PokemonType
		var typeString = data["type"]?["name"]?.ToString();
		var type = PokemonType.None; // Default to None
		if (!string.IsNullOrEmpty(typeString))
		{
			if (Enum.TryParse(typeString, true, out PokemonType parsedType))
			{
				type = parsedType;
			}
			else
			{
				Logger.Warning($"Unknown PokemonType: '{typeString}' (move: {moveName})");
			}
		}
		else
		{
			Logger.Warning($"Missing 'type' field for move: {moveName}");
		}

		// Parse PokemonAilment from meta
		var ailmentString = meta?["ailment"]?["name"]?.ToString();
		var ailment = PokemonAilment.None; // Default to None
		if (!string.IsNullOrEmpty(ailmentString))
		{
			// Convert snake-case to PascalCase for enum parsing
			ailmentString = ToPascalCase(ailmentString.Replace("-", "_"));

			if (Enum.TryParse(ailmentString, true, out PokemonAilment parsedAilment))
			{
				ailment = parsedAilment;
			}
			else
			{
				Logger.Warning($"Unknown PokemonAilment: '{ailmentString}' (move: {moveName})");
			}
		}
		else
		{
			Logger.Debug($"Missing 'ailment' field in meta for move: {moveName} (this is common for many moves).");
		}

		MoveResource move = new()
		{
			// Basic move properties
			Name = moveName,
			Power = data["power"]?.ToObject<int?>() ?? 0,
			PP = data["pp"]?.ToObject<int?>() ?? 0,
			Accuracy = data["accuracy"]?.ToObject<int?>() ?? 0,
			Type = type,
			Category = ParseMoveCategory(data["damage_class"]?["name"]?.ToString()),

			// Meta properties (all included as requested)
			CritRate = meta?["crit_rate"]?.ToObject<int?>() ?? 0,
			Drain = meta?["drain"]?.ToObject<int?>() ?? 0,
			FlinchChance = meta?["flinch_chance"]?.ToObject<int?>() ?? 0,
			Healing = meta?["healing"]?.ToObject<int?>() ?? 0,
			MaxHits = meta?["max_hits"]?.ToObject<int?>() ?? -1, // -1 for no max hits
			MaxTurns = meta?["max_turns"]?.ToObject<int?>() ?? -1, // -1 for no max turns
			MinHits = meta?["min_hits"]?.ToObject<int?>() ?? -1, // -1 for no min hits
			MinTurns = meta?["min_turns"]?.ToObject<int?>() ?? -1, // -1 for no min turns

			AilmentChance = meta?["ailment_chance"]?.ToObject<int?>() ?? 0,
			Ailment = ailment,

			// Stat changes
			StatChanges = CreateStatChanges(statChanges)
		};

		// Format the filename for the .tres resource (e.g., "double-kick" -> "double_kick.tres")
		string fileName = moveName.Replace('-', '_');
		var savePath = $"{folderPath}{fileName}.tres";

		var err = ResourceSaver.Save(move, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save move {moveName} to {savePath}: {err}");
		else
			Logger.Debug($"Successfully saved {moveName} to {savePath}");
	}

	/// <summary>
	/// Parses the move category string to the MoveCategory enum.
	/// </summary>
	/// <param name="name">The damage class name string (e.g., "physical", "special", "status").</param>
	/// <returns>The corresponding MoveCategory enum value.</returns>
	private MoveCategory ParseMoveCategory(string? name)
	{
		return name?.ToLower() switch
		{
			"physical" => MoveCategory.Physical,
			"special" => MoveCategory.Special,
			"status" => MoveCategory.Status,
			_ => MoveCategory.Physical // Default if unknown or null
		};
	}

	/// <summary>
	/// Converts a JArray of stat changes from the API into a Godot Array of StatChangeResource.
	/// </summary>
	/// <param name="statChanges">The JArray containing stat change entries.</param>
	/// <returns>A Godot Array of StatChangeResource objects.</returns>
	private Array<StatChangeResource> CreateStatChanges(JArray? statChanges)
	{
		if (statChanges == null || statChanges.Count == 0)
			return new Array<StatChangeResource>(); // Return empty Godot Array

		var results = new Array<StatChangeResource>();

		foreach (var entry in statChanges)
		{
			string? statName = entry["stat"]?["name"]?.ToString();
			if (string.IsNullOrEmpty(statName))
			{
				Logger.Warning($"Stat change entry found with no stat name. Skipping.");
				continue;
			}

			// Convert snake-case (e.g., "special-attack") to PascalCase (e.g., "SpecialAttack") for enum parsing
			statName = ToPascalCase(statName.Replace("-", "_"));

			if (!Enum.TryParse<PokemonStat>(statName, out var parsedStat))
			{
				Logger.Warning($"Unknown PokemonStat name '{statName}' in API. Skipping stat change.");
				continue;
			}

			int change = entry["change"]?.ToObject<int?>() ?? 0;

			results.Add(new StatChangeResource
			{
				Stat = parsedStat,
				Change = change
			});
		}

		return results;
	}

	/// <summary>
	/// Converts a snake_case string (or kebab-case) to PascalCase.
	/// </summary>
	private string ToPascalCase(string? input)
	{
		if (string.IsNullOrEmpty(input)) return "";
		// Replace hyphens with underscores, then split by underscore
		var parts = input.Replace("-", "_").Split('_');
		return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
	}
}
#endif
