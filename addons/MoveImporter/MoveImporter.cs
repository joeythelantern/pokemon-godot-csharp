#if TOOLS
using Game.Core;
using Game.Utilities;
using Game.Resources;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using System.Collections.Generic;

public class ApiResource
{
	[JsonProperty("name")]
	public string Name { get; set; }
	[JsonProperty("url")]
	public string Url { get; set; }
}

public class EffectEntry
{
	[JsonProperty("effect")]
	public string Effect { get; set; }
	[JsonProperty("short_effect")]
	public string ShortEffect { get; set; }
	[JsonProperty("language")]
	public ApiResource Language { get; set; }
	[JsonProperty("version_group")]
	public ApiResource VersionGroup { get; set; }
}

public class MoveMeta
{
	[JsonProperty("ailment")]
	public ApiResource Ailment { get; set; }
	[JsonProperty("category")]
	public ApiResource Category { get; set; }
	[JsonProperty("crit_rate")]
	public int? CritRate { get; set; }
	[JsonProperty("drain")]
	public int? Drain { get; set; }
	[JsonProperty("flinch_chance")]
	public int? FlinchChance { get; set; }
	[JsonProperty("healing")]
	public int? Healing { get; set; }
	[JsonProperty("max_hits")]
	public int? MaxHits { get; set; }
	[JsonProperty("max_turns")]
	public int? MaxTurns { get; set; }
	[JsonProperty("min_hits")]
	public int? MinHits { get; set; }
	[JsonProperty("min_turns")]
	public int? MinTurns { get; set; }
	[JsonProperty("stat_chance")]
	public int? StatChance { get; set; }
	[JsonProperty("ailment_chance")]
	public int? AilmentChance { get; set; }
}

public class StatChangeEntry
{
	[JsonProperty("change")]
	public int Change { get; set; }
	[JsonProperty("stat")]
	public ApiResource Stat { get; set; }
}

public class MoveApiResponse
{
	[JsonProperty("id")]
	public int Id { get; set; }
	[JsonProperty("name")]
	public string Name { get; set; }
	[JsonProperty("accuracy")]
	public int? Accuracy { get; set; }
	[JsonProperty("effect_chance")]
	public int? EffectChance { get; set; }
	[JsonProperty("pp")]
	public int? Pp { get; set; }
	[JsonProperty("priority")]
	public int Priority { get; set; }
	[JsonProperty("power")]
	public int? Power { get; set; }

	[JsonProperty("damage_class")]
	public ApiResource DamageClass { get; set; }
	[JsonProperty("generation")]
	public ApiResource Generation { get; set; }
	[JsonProperty("meta")]
	public MoveMeta Meta { get; set; }
	[JsonProperty("stat_changes")]
	public List<StatChangeEntry> StatChanges { get; set; }
	[JsonProperty("type")]
	public ApiResource Type { get; set; }
	[JsonProperty("effect_entries")]
	public List<EffectEntry> EffectEntries { get; set; }
}

[Tool]
public partial class MoveImporter : EditorPlugin
{
	private const string ImportMenuItemText = "Import All Gen 1 Moves";
	private static readonly HttpClient _httpClient = new HttpClient();

	public override void _EnterTree()
	{
		AddToolMenuItem(ImportMenuItemText, Callable.From(ImportMoves));
		Logger.Info("Pokemon Move Importer plugin initialized.");
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(ImportMenuItemText);
		Logger.Info("Pokemon Move Importer plugin exited.");
		_httpClient.Dispose();
	}

	public async void ImportMoves()
	{
		Logger.Info($"Starting to fetch Generation I moves from ID 1 to 165...");

		var folderPath = "res://resources/moves/";

		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));

		const int gcInterval = 10;

		for (int moveId = 1; moveId <= 165; moveId++)
		{
			Logger.Info($"Processing Move ID: {moveId}");

			MoveApiResponse? data = await FetchData<MoveApiResponse>($"move/{moveId}");
			if (data == null)
			{
				Logger.Warning($"Could not fetch data for Move ID {moveId}. Skipping.");
				continue;
			}

			var gen = data.Generation?.Name;
			if (gen != "generation-i")
			{
				Logger.Info($"Move ID {moveId} ({data.Name ?? "Unknown"}) is not Generation I. Skipping.");
				continue;
			}

			var moveName = data.Name;
			if (string.IsNullOrEmpty(moveName))
			{
				Logger.Warning($"Move ID {moveId} has no name. Skipping.");
				continue;
			}

			CreateMoveResource(moveName, data, folderPath);

			if (moveId % gcInterval == 0)
			{
				Logger.Debug($"Triggering garbage collection after {moveId} moves...");
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Logger.Debug("Garbage collection completed.");
			}

			await Task.Delay(100);
		}

		Logger.Info("Finished importing Generation I moves. Refreshing filesystem...");
		EditorInterface.Singleton.GetResourceFilesystem().Scan();
		Logger.Info("Move import process completed.");
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

	private void CreateMoveResource(string moveName, MoveApiResponse data, string folderPath)
	{
		var typeString = data.Type?.Name;
		var type = PokemonType.None;

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

		var ailmentString = data.Meta?.Ailment?.Name;
		var ailment = PokemonAilment.None;

		if (!string.IsNullOrEmpty(ailmentString))
		{
			ailmentString = StringModule.ToPascalCase(ailmentString.Replace("-", "_"));

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
			Name = moveName,
			Power = data.Power ?? 0,
			PP = data.Pp ?? 0,
			Accuracy = data.Accuracy ?? 0,
			Type = type,
			Category = ParseMoveCategory(data.DamageClass?.Name),

			CritRate = data.Meta?.CritRate ?? 0,
			Drain = data.Meta?.Drain ?? 0,
			FlinchChance = data.Meta?.FlinchChance ?? 0,
			Healing = data.Meta?.Healing ?? 0,
			MaxHits = data.Meta?.MaxHits ?? -1,
			MaxTurns = data.Meta?.MaxTurns ?? -1,
			MinHits = data.Meta?.MinHits ?? -1,
			MinTurns = data.Meta?.MinTurns ?? -1,

			AilmentChance = data.Meta?.AilmentChance ?? 0,
			Ailment = ailment,

			StatChanges = CreateStatChanges(data.StatChanges)
		};

		var savePath = $"{folderPath}{moveName}.tres";

		var err = ResourceSaver.Save(move, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save move {moveName} to {savePath}: {err}");
	}

	private static MoveCategory ParseMoveCategory(string? name)
	{
		return name?.ToLower() switch
		{
			"physical" => MoveCategory.Physical,
			"special" => MoveCategory.Special,
			"status" => MoveCategory.Status,
			_ => MoveCategory.Physical
		};
	}

	private Godot.Collections.Dictionary<PokemonStat, int> CreateStatChanges(List<StatChangeEntry>? statChanges)
	{
		if (statChanges == null || statChanges.Count == 0)
			return [];

		Godot.Collections.Dictionary<PokemonStat, int> results = [];

		foreach (var entry in statChanges)
		{
			string? statName = entry.Stat?.Name;
			if (string.IsNullOrEmpty(statName))
			{
				Logger.Warning($"Stat change entry found with no stat name. Skipping.");
				continue;
			}

			statName = StringModule.ToPascalCase(statName.Replace("-", "_"));

			if (!Enum.TryParse<PokemonStat>(statName, out var parsedStat))
			{
				Logger.Warning($"Unknown PokemonStat name '{statName}' in API. Skipping stat change.");
				continue;
			}

			int change = entry.Change;

			results.Add(parsedStat, change);
		}

		return results;
	}
}
#endif
