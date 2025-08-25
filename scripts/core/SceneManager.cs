using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Gameplay;
using Godot;
using Godot.Collections;

namespace Game.Core;

public partial class SceneManager : Node
{
	public static SceneManager Instance { get; private set; }
	public static bool IsChanging { get; private set; }

	[ExportCategory("Scene Manager Variables")]
	[Export]
	public ColorRect FadeRect;

	[Export]
	public Level CurrentLevel;

	[Export]
	public Array<Level> AllLevels;

	public override void _Ready()
	{
		Instance = this;
		IsChanging = false;

		Logger.Info("Loading scene manager ...");
	}

	public static async void ChangeLevel(LevelName levelName = LevelName.small_town, int trigger = 0, bool spawn = false)
	{
		while (IsChanging)
		{
			await Instance.ToSignal(Instance.GetTree(), "process_frame");
		}

		IsChanging = true;

		await Instance.GetLevel(levelName);

		if (spawn)
		{
			Instance.Spawn();
		}
		else
		{
			Instance.Switch(trigger);
		}

		await Instance.FadeIn();

		IsChanging = false;
	}

	public async Task GetLevel(LevelName levelName)
	{
		if (CurrentLevel != null)
		{
			await Instance.FadeOut();
			GameManager.GetGameViewPort().RemoveChild(CurrentLevel);
		}

		CurrentLevel = AllLevels.FirstOrDefault(level => level.LevelName == levelName);

		if (CurrentLevel != null)
		{
			GameManager.GetGameViewPort().AddChild(CurrentLevel);
		}
		else
		{
			CurrentLevel = GD.Load<PackedScene>("res://scenes/levels/" + levelName + ".tscn").Instantiate<Level>();
			AllLevels.Add(CurrentLevel);
			GameManager.GetGameViewPort().AddChild(CurrentLevel);
		}
	}

	public void Spawn()
	{
		var spawnPoints = CurrentLevel.GetTree().GetNodesInGroup(LevelGroup.SPAWNPOINTS.ToString());

		if (spawnPoints.Count <= 0)
			throw new Exception("Missing spawn point(s)!");

		var spawnPoint = (SpawnPoint)spawnPoints[0];
		var player = GD.Load<PackedScene>("res://scenes/characters/player.tscn").Instantiate<Player>();

		GameManager.AddPlayer(player);
		GameManager.GetPlayer().Position = spawnPoint.Position;
	}

	public void Switch(int trigger)
	{
		var sceneTriggers = CurrentLevel.GetTree().GetNodesInGroup(LevelGroup.SCENETRIGGERS.ToString());

		if (sceneTriggers.Count <= 0)
			throw new Exception("Missing scene trigger(s)!");

		if (sceneTriggers.FirstOrDefault(st => ((SceneTrigger)st).CurrentLevelTrigger == trigger) is not SceneTrigger sceneTrigger)
			throw new Exception($"Missing scene trigger {trigger}");

		GameManager.GetPlayer().Position = sceneTrigger.Position + sceneTrigger.EntryDirection * Globals.Instance.GRID_SIZE;
	}

	public async Task FadeOut()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(FadeRect, "color:a", 1.0, 0.25);
		await ToSignal(tween, "finished");
	}

	public async Task FadeIn()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(FadeRect, "color:a", 0.0, 0.25);
		await ToSignal(tween, "finished");
	}

	public static Level GetCurrentLevel()
	{
		return Instance.CurrentLevel;
	}
}