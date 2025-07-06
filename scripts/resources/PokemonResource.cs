using Godot;
using Godot.Collections;

namespace Game.Resources;

[GlobalClass]
[Tool]
public partial class PokemonResource : Resource
{
    [ExportCategory("Basic Info")]
    [Export]
    public string Name = "";

    [Export]
    public string UUID = "";

    [Export]
    public string Description = "";

    [ExportCategory("Stats")]
    [Export]
    public int BaseHP;

    [Export]
    public int BaseAttack;

    [Export]
    public int BaseDefence;

    [Export]
    public int BaseSpecialAttack;

    [Export]
    public int BaseSpecialDefence;

    [Export]
    public int BaseSpeed;

    [ExportCategory("Sprites")]
    [Export]
    public Texture2D FrontSprite;

    [Export]
    public Texture2D BackSprite;

    [Export]
    public Texture2D ShinyFrontSprite;

    [Export]
    public Texture2D ShinyBackSprite;

    [ExportCategory("Moves")]
    [Export]
    public Array<LevelUpMoveResource> LevelUpMoves;

    [Export]
    public Array<TMResource> TechnicalMachines;

    [Export]
    public Array<HMResource> HiddenMachines;
}