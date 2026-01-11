using Game.Core;
using Godot;
using Godot.Collections;
using System.Linq;

namespace Game.Gameplay;

public partial class PokemonInstance : Node
{
    [ExportCategory("Basic Info")]
    [Export]
    public string Uuid;

    [Export]
    public string Nickname;

    [Export]
    public bool Shiny;

    [ExportCategory("Pokemon Info")]
    [Export]
    public PokemonNature Nature;

    [Export]
    public PokemonResource Base;

    [Export]
    public PokemonMetData Met;

    [ExportCategory("Stats")]
    [Export]
    public PokemonAilment Ailment = PokemonAilment.None;

    [Export]
    public PokemonStats Stats;

    public void Initialize(
        PokemonResource pokemonResource,
        int level,
        bool shiny,
        PokemonMetData pokemonMetData,
        PokemonNature pokemonNature,
        string nickname = "")
    {
        Uuid = System.Guid.NewGuid().ToString();
        Base = pokemonResource;
        Shiny = shiny;
        Nickname = nickname;
        Nature = pokemonNature;
        Met = pokemonMetData;

        Stats = PokemonStats.Generate(Base, Nature, PokemonIVs.Generate(), PokemonEVs.Generate(), level);

        GenerateMovesFromLearnset(level);
    }

    private void GenerateMovesFromLearnset(int level)
    {
        Array<MoveInstance> moves = [];

        var moveNames = Base.LevelUpMoves.Keys;
        var sortedMoveNames = moveNames.OrderBy(name => Base.LevelUpMoves[name]).ToList();

        Logger.Info($"Learnable move list for {Base.Name}: {sortedMoveNames}");

        int added = 0;

        for (int i = sortedMoveNames.Count - 1; i >= 0; i--)
        {
            if (added >= 4)
                break;

            string moveName = sortedMoveNames[i];
            int learnedLevel = Base.LevelUpMoves[moveName];

            if (learnedLevel > level)
                continue;

            var moveResource = MoveDatabase.Get(moveName);
            if (moveResource == null)
            {
                Logger.Warning($"Unable to get move {moveName} from move database.");
                continue;
            }

            var moveInstance = new MoveInstance();
            AddChild(moveInstance);
            moveInstance.Initialize(moveResource);

            added++;
        }
    }

    public Array<MoveInstance> GetMoves()
    {
        Array<MoveInstance> moves = [];
        var children = GetChildren();

        foreach (var child in children)
        {
            moves.Add(child as MoveInstance);
        }

        return moves;
    }
}
