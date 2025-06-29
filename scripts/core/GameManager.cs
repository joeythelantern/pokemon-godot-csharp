using Godot;
using Game.Gameplay;
using Game.UI;

namespace Game.Core;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[ExportCategory("Nodes")]
	[Export]
	public SubViewport GameViewPort;

	[ExportCategory("Vars")]
	[Export]
	public Player Player;

	public override void _Ready()
	{
		Instance = this;

		Logger.Info("Loading game manager ...");

		SceneManager.ChangeLevel(spawn: true);

		MessageManager.PlayText("Hey!", "Welcome to PokeGodot!");
	}

	public static SubViewport GetGameViewPort()
	{
		return Instance.GameViewPort;
	}

	public static Player AddPlayer(Player player)
	{
		Instance.GameViewPort.AddChild(player);
		Instance.Player = player;
		return Instance.Player;
	}

	public static Player GetPlayer()
	{
		return Instance.Player;
	}
}