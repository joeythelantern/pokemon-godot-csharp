using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;

public partial class MoveDatabase : Node
{
    private readonly Dictionary<string, MoveResource> _moveDatabase = [];

    public static Dictionary<string, MoveResource> All
    {
        get
        {
            return new Dictionary<string, MoveResource>(Instance._moveDatabase);
        }
    }

    public static MoveDatabase Instance { get; private set; }

    public override void _EnterTree()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        LoadPokemon();
    }

    public void LoadPokemon()
    {
        var dir = DirAccess.Open("res://resources/moves/");
        if (dir == null)
        {
            Logger.Error("MoveDatabase folder not found!");
            return;
        }

        dir.ListDirBegin();
        string file;

        while ((file = dir.GetNext()) != "")
        {
            if (!file.EndsWith(".tres") && !file.EndsWith(".res"))
                continue;

            var data = ResourceLoader.Load<MoveResource>($"res://resources/moves/{file}");
            if (data != null)
                _moveDatabase[data.Name] = data;
        }

        dir.ListDirEnd();
        Logger.Info($"MoveDatabase loaded {_moveDatabase.Count} moves.");
    }

    public static MoveResource Get(string name)
    {
        if (Instance._moveDatabase.TryGetValue(name, out MoveResource value))
            return value;

        Logger.Error($"Move not found: {name}");
        return null;
    }
}
