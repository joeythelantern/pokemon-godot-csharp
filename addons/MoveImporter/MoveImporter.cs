#if TOOLS
using System;
using System.Threading.Tasks;
using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;

[Tool]
public partial class MoveImporter : EditorPlugin
{
	private const string importMenuItemText = "Import Moves";
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
		Logger.Info("Attempting to import moves ...");

		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(folderPath));

		const int gcInterval = 10;

		for (int i = 1; i <= Globals.MOVE_NUMBERS; i++)
		{
			Logger.Info($"Processing move with ID: {i}");

			Variant response = await Modules.FetchDataFromPokeApi($"{apiPath}{i}");
			Dictionary<string, Variant> data = response.AsGodotDictionary<string, Variant>();

			var generation = data["generation"].AsGodotDictionary<string, string>()["name"];
			if (generation != "generation-i")
			{
				Logger.Warning($"Move {i} is not from Gen 1 ...");
				continue;
			}

			var moveName = data["name"].AsString();
			if (string.IsNullOrEmpty(moveName))
			{
				Logger.Warning($"Move {i} has no name ...");
				continue;
			}

			Logger.Info($"Creating resource for {moveName} ...");

			var meta = data["meta"].AsGodotDictionary<string, Variant>();
			var statChanges = data["stat_changes"].AsGodotArray<Dictionary<string, Variant>>();

			CreateMoveResource(moveName, data, meta, statChanges);

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

	private void CreateMoveResource(string moveName, Dictionary<string, Variant> data, Dictionary<string, Variant> meta, Array<Dictionary<string, Variant>> statChanges)
	{
		var pokemonType = data["type"].AsGodotDictionary<string, string>()["name"];
		var damageCategory = data["damage_class"].AsGodotDictionary<string, string>()["name"];
		var attackTarget = data["target"].AsGodotDictionary<string, string>()["name"];
		var ailmentName = meta["ailment"].AsGodotDictionary<string, string>()["name"];

		var move = new MoveResource
		{
			Name = moveName,
			PokemonType = PokemonEnum.TypeMap.TryGetValue(pokemonType, out var type) ? type : PokemonType.None,
			Category = MovesEnum.CategoryMap.TryGetValue(damageCategory, out var category) ? category : MoveCategory.Physical,
			Target = MovesEnum.MoveTargetMap.TryGetValue(attackTarget, out var target) ? target : MoveTarget.SelectedPokemon,

			Accuracy = data["accuracy"].AsInt32(),
			PP = data["pp"].AsInt32(),
			Power = data["power"].AsInt32(),
			CritRate = meta["crit_rate"].AsInt32(),
			Drain = meta["crit_rate"].AsInt32(),
			FlinchChance = meta["drain"].AsInt32(),
			Healing = meta["healing"].AsInt32(),
			MaxHits = meta["max_hits"].AsGodotObject() != null ? meta["max_hits"].AsInt32() : -1,
			MaxTurns = meta["max_turns"].AsGodotObject() != null ? meta["max_turns"].AsInt32() : -1,
			MinHits = meta["min_hits"].AsGodotObject() != null ? meta["min_hits"].AsInt32() : -1,
			MinTurns = meta["min_turns"].AsGodotObject() != null ? meta["min_turns"].AsInt32() : -1,

			AilmentChance = meta["ailment_chance"].AsInt32(),
			Ailment = PokemonEnum.AilmentMap.TryGetValue(ailmentName, out var ailment) ? ailment : PokemonAilment.None,
			StatChanges = []
		};

		foreach (var statChange in statChanges)
		{
			var changeAmmount = statChange["change"].AsInt32();
			var changeName = statChange["stat"].AsGodotDictionary<string, string>()["name"];

			if (PokemonEnum.StatMap.TryGetValue(changeName, out var stat))
			{
				move.StatChanges[stat] = changeAmmount;
			}
		}

		var savePath = $"{folderPath}{moveName.ToLower()}.tres";
		var error = ResourceSaver.Save(move, savePath);

		if (error != Error.Ok)
			Logger.Error($"There was a problem saving the move {moveName} to {savePath}: {error}");
	}

}
#endif
