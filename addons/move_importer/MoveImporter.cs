#if TOOLS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Core;
using Game.Resources;
using Godot;
using Newtonsoft.Json;

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

	[JsonProperty("target")]
	public ApiResource Target { get; set; }
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
	private const string importMenuItemText = "Import All Gen 1 Moves";
	private const string folderPath = "res://resources/moves/";
	private const string apiPath = "https://pokeapi.co/api/v2/move/";

	public override void _EnterTree()
	{
		AddToolMenuItem(importMenuItemText, Callable.From(ImportMoves));
	}

	public override void _ExitTree()
	{
		RemoveToolMenuItem(importMenuItemText);
	}

	public async void ImportMoves()
	{
		Logger.Info("Starting to fetch Generation I moves from ID 1 to 165...");

		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));

		const int gcInterval = 10;

		for (int moveId = 1; moveId <= 165; moveId++)
		{
			Logger.Info($"Processing Move ID: {moveId}");

			MoveApiResponse data = await Modules.FetchData<MoveApiResponse>($"{apiPath}{moveId}");

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

			CreateMoveResource(moveName, data);

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

	private void CreateMoveResource(string moveName, MoveApiResponse apiData)
	{
		var move = new MoveResource
		{
			Name = moveName,
			Type = PokemonEnum.TypeMap.TryGetValue(apiData.Type?.Name ?? "", out var type) ? type : PokemonType.None,
			Category = MovesEnum.CategoryMap.TryGetValue(apiData.DamageClass?.Name ?? "", out var cat) ? cat : MoveCategory.Physical,
			Target = MovesEnum.MoveTargetMap.TryGetValue(apiData.Target?.Name ?? "", out var target) ? target : MoveTarget.SelectedPokemon,

			Accuracy = apiData.Accuracy ?? 0,
			PP = apiData.Pp ?? 0,
			Power = apiData.Power ?? 0,
			CritRate = apiData.Meta?.CritRate ?? 0,
			Drain = apiData.Meta?.Drain ?? 0,
			FlinchChance = apiData.Meta?.FlinchChance ?? 0,
			Healing = apiData.Meta?.Healing ?? 0,
			MaxHits = apiData.Meta?.MaxHits ?? 0,
			MaxTurns = apiData.Meta?.MaxTurns ?? 0,
			MinHits = apiData.Meta?.MinHits ?? 0,
			MinTurns = apiData.Meta?.MinTurns ?? 0,

			AilmentChance = apiData.Meta?.AilmentChance ?? 0,
			Ailment = PokemonEnum.AilmentMap.TryGetValue(apiData.Meta?.Ailment?.Name ?? "", out var ailment) ? ailment : PokemonAilment.None,
			StatChanges = []
		};

		if (apiData.StatChanges != null)
		{
			foreach (var change in apiData.StatChanges)
			{
				if (PokemonEnum.StatMap.TryGetValue(change.Stat?.Name ?? "", out var stat))
				{
					move.StatChanges[stat] = change.Change;
				}
			}
		}

		var savePath = $"{folderPath}{moveName.ToLower()}.tres";
		var err = ResourceSaver.Save(move, savePath);

		if (err != Error.Ok)
			Logger.Error($"Failed to save move {moveName} to {savePath}: {err}");
	}
}
#endif
