#if TOOLS
using Game.Core;
using Game.Utilities;
using Game.Resources;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;

[Tool]
public partial class MoveImporter : EditorPlugin
{
	private const string ImportMenuItemText = "Import Gen 1 Moves";
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

		for (int moveId = 1; moveId <= 1; moveId++)
		{
			Logger.Info($"Processing Move ID: {moveId}");
			JObject? data = await HttpModule.FetchDataFromPokeApi(_httpClient, $"move/{moveId}");
			if (data == null)
				continue;

			var gen = data["generation"]?["name"]?.ToString();
			if (gen != "generation-i")
			{
				Logger.Info($"Move ID {moveId} ({data["name"]?.ToString() ?? "Unknown"}) is not Generation I. Skipping.");
				continue;
			}

			var moveName = data["name"]?.ToString();
			if (string.IsNullOrEmpty(moveName))
			{
				Logger.Warning($"Move ID {moveId} has no name. Skipping.");
				continue;
			}

			Logger.Info($"Importing: {moveName}");

			CreateMoveResource(moveName, data, folderPath);

			await Task.Delay(100);
		}

		Logger.Info("Finished importing Generation I moves. Refreshing filesystem...");
		EditorInterface.Singleton.GetResourceFilesystem().Scan();
		Logger.Info("Move import process completed.");
	}

	private void CreateMoveResource(string moveName, JObject data, string folderPath)
	{
		var meta = data["meta"];
		var statChanges = data["stat_changes"] as JArray;

		// Parse PokemonType
		var typeString = data["type"]?["name"]?.ToString();
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

		var ailmentString = meta?["ailment"]?["name"]?.ToString();
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
			Logger.Error($"Missing 'ailment' field in meta for move: {moveName} (this is common for many moves).");
		}

		MoveResource move = new()
		{
			Name = moveName,
			Power = data["power"]?.ToObject<int?>() ?? 0,
			PP = data["pp"]?.ToObject<int?>() ?? 0,
			Accuracy = data["accuracy"]?.ToObject<int?>() ?? 0,
			Type = type,
			Category = ParseMoveCategory(data["damage_class"]?["name"]?.ToString()),

			CritRate = meta?["crit_rate"]?.ToObject<int?>() ?? 0,
			Drain = meta?["drain"]?.ToObject<int?>() ?? 0,
			FlinchChance = meta?["flinch_chance"]?.ToObject<int?>() ?? 0,
			Healing = meta?["healing"]?.ToObject<int?>() ?? 0,
			MaxHits = meta?["max_hits"]?.ToObject<int?>() ?? -1,
			MaxTurns = meta?["max_turns"]?.ToObject<int?>() ?? -1,
			MinHits = meta?["min_hits"]?.ToObject<int?>() ?? -1,
			MinTurns = meta?["min_turns"]?.ToObject<int?>() ?? -1,

			AilmentChance = meta?["ailment_chance"]?.ToObject<int?>() ?? 0,
			Ailment = ailment,

			StatChanges = CreateStatChanges(statChanges)
		};

		var savePath = $"{folderPath}{moveName}.tres";

		var err = ResourceSaver.Save(move, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save move {moveName} to {savePath}: {err}");
		else
			Logger.Info($"Successfully saved {moveName} to {savePath}");
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

			statName = StringModule.ToPascalCase(statName.Replace("-", "_"));

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
}
#endif
