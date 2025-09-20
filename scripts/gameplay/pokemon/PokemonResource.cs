using System.Linq;
using Godot;
using Godot.Collections;

namespace Game.Resources;

[GlobalClass]
[Tool]
public partial class PokemonResource : Resource
{
    [ExportCategory("Basic Info")]
    [Export]
    public string Name;

    [Export]
    public int Id;

    [Export]
    public string Description;

    [ExportCategory("Stats")]
    [Export]
    public int Height;

    [Export]
    public int Weight;

    [Export]
    public int BaseExperience;

    [Export]
    public int BaseHp;

    [Export]
    public int BaseAttack;

    [Export]
    public int BaseDefense;

    [Export]
    public int BaseSpecialAttack;

    [Export]
    public int BaseSpecialDefense;

    [Export]
    public int BaseSpeed;

    [ExportCategory("Moves")]
    [Export]
    public Array<string> LearnableMoves;

    [Export]
    public Dictionary<string, int> LevelUpMoves;

    [ExportCategory("Sprites")]
    [Export]
    public Texture2D FrontSprite;

    [Export]
    public Texture2D BackSprite;

    [Export]
    public Texture2D ShinyFrontSprite;

    [Export]
    public Texture2D ShinyBackSprite;

    [Export]
    public Texture2D MenuIconSprite;

    public override string ToString()
    {
        return $"Name: {Name}\n" +
               $"Id: {Id}\n" +
               $"Description: {Description}\n" +
               $"Height: {Height}\n" +
               $"Weight: {Weight}\n" +
               $"Base Experience: {BaseExperience}\n" +
               $"Base HP: {BaseHp}\n" +
               $"Base Attack: {BaseAttack}\n" +
               $"Base Defense: {BaseDefense}\n" +
               $"Base Special Attack: {BaseSpecialAttack}\n" +
               $"Base Special Defense: {BaseSpecialDefense}\n" +
               $"Base Speed: {BaseSpeed}\n" +
               $"Learnable Moves: {string.Join(", ", LearnableMoves)}\n" +
               $"Level-Up Moves: {string.Join(", ", LevelUpMoves.Select(kv => $"{kv.Key} (Level {kv.Value})"))}";
    }
}