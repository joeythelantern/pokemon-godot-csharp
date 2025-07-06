#if TOOLS
using Game.Core;
using Game.Resources;
using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

[Tool]
public partial class MoveImporter : EditorPlugin
{
	public override void _EnterTree()
	{
		AddToolMenuItem("Import Gen 1 Moves", Callable.From(() => { ImportMoves(); }));
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}

	public async void ImportMoves()
	{
		Logger.Info($"Fetching Generation I moves from ID 1 to 165");

		var client = new System.Net.Http.HttpClient();
		var folderPath = "res://resources/moves/";

		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));

		for (int moveId = 1; moveId <= 165; moveId++)
		{
			JObject? data = await FetchMoveData(client, moveId);
			if (data == null)
				continue;

			var gen = data["generation"]?["name"]?.ToString();
			if (gen != "generation-i")
				continue;

			var moveName = data["name"]?.ToString();
			if (string.IsNullOrEmpty(moveName))
				continue;

			Logger.Debug($"Importing: {moveName}");

			CreateMoveResource(moveName, data, folderPath);

			await Task.Delay(50);
		}

	}

	private async Task<JObject?> FetchMoveData(System.Net.Http.HttpClient client, int id)
	{
		string url = $"https://pokeapi.co/api/v2/move/{id}/";

		for (int attempt = 0; attempt < 3; attempt++)
		{
			try
			{
				var response = await client.GetAsync(url);
				if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					Logger.Info($"404 for move ID {id}");
					return null;
				}

				response.EnsureSuccessStatusCode();
				var json = await response.Content.ReadAsStringAsync();
				return JObject.Parse(json);
			}
			catch (Exception e)
			{
				Logger.Error($"Error fetching move {id}: {e.Message}");
				await Task.Delay(1000);
			}
		}

		Logger.Error($"Failed to fetch move {id} after retries.");
		return null;
	}

	private void CreateMoveResource(string moveName, JObject data, string folderPath)
	{
		var meta = data["meta"];
		var statChanges = data["stat_changes"] as JArray;

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
			Logger.Warning($"Missing 'ailment' field in meta for move: {moveName}");
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
			Logger.Error($"Failed to save move {moveName}: {err}");
	}

	private MoveCategory ParseMoveCategory(string? name)
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
			return [];

		var results = new Array<StatChangeResource>();

		foreach (var entry in statChanges)
		{
			string? statName = entry["stat"]?["name"]?.ToString();
			if (string.IsNullOrEmpty(statName))
				continue;

			statName = ToPascalCase(statName.Replace("-", "_")); // e.g. "special-attack" -> "SpecialAttack"

			if (!Enum.TryParse<PokemonStat>(statName, out var parsedStat))
			{
				Logger.Warning($"Unknown stat name '{statName}' in API.");
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

	private string ToPascalCase(string? snake)
	{
		if (string.IsNullOrEmpty(snake)) return "";
		var parts = snake.Split('_');
		return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
	}

}
#endif
