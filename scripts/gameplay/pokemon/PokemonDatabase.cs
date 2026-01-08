using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;

public partial class PokemonDatabase : Node
{
    private readonly Dictionary<string, PokemonResource> _pokemonDatabase = [];

    public static Dictionary<string, PokemonResource> All
    {
        get
        {
            return new Dictionary<string, PokemonResource>(Instance._pokemonDatabase);
        }
    }

    public static PokemonDatabase Instance { get; private set; }

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
        var dir = DirAccess.Open("res://resources/pokemon/");
        if (dir == null)
        {
            Logger.Error("PokemonDatabase folder not found!");
            return;
        }

        dir.ListDirBegin();
        string file;

        while ((file = dir.GetNext()) != "")
        {
            if (!file.EndsWith(".tres") && !file.EndsWith(".res"))
                continue;

            var data = ResourceLoader.Load<PokemonResource>($"res://resources/pokemon/{file}");
            if (data != null)
                _pokemonDatabase[data.Name] = data;
        }

        dir.ListDirEnd();
        Logger.Info($"PokemonDatabase loaded {_pokemonDatabase.Count} Pokémon");
    }

    public static PokemonResource Get(string name)
    {
        if (Instance._pokemonDatabase.TryGetValue(name, out PokemonResource value))
            return value;

        Logger.Error($"Pokemon not found: {name}");
        return null;
    }
}
