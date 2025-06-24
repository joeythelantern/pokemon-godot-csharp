namespace Game.Core
{
    #region Debugging
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
    }
    #endregion

    #region Characters
    public enum ECharacterAnimation
    {
        idle_down,
        idle_up,
        idle_left,
        idle_right,
        turn_down,
        turn_up,
        turn_left,
        turn_right,
        walk_down,
        walk_up,
        walk_left,
        walk_right,
    }

    public enum ECharacterMovement
    {
        WALKING,
        JUMPING
    }
    #endregion

    #region levels
    public enum LevelName
    {
        small_town,
        small_town_greens_house,
        small_town_purples_house,
        small_town_pokemon_center,
        small_town_cave,
    }

    public enum LevelGroup
    {
        SPAWNPOINTS,
        SCENETRIGGERS,
    }
    #endregion

    #region pokemon
    public enum PokemonName
    {
        Bulbasaur,
        Charmander
    }

    public enum PokemonType
    {
        Normal,
        Fire,
        Water,
        Grass,
        Electric,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dark,
        Dragon,
        Steel,
        Fairy,
        NONE
    }

    public enum Nature
    {
        Hardy,
        Lonely,
        Brave,
        Adamant,
        Naughty,
        Bold,
        Docile,
        Relaxed,
        Impish,
        Lax,
        Timid,
        Hasty,
        Serious,
        Jolly,
        Naive,
        Modest,
        Mild,
        Quiet,
        Bashful,
        Rash,
        Calm,
        Gentle,
        Sassy,
        Careful,
        Quirky
    }

    public enum Status
    {
        SLEEP,
        POISON,
        PARALYSIS,
        BURN,
        FREEZE,
        FAINT,
        NONE
    }

    public enum Gender
    {
        MALE,
        FEMALE,
        GENDERLESS
    }

    public enum ExperienceGroup
    {
        MediumFast,
        Eratic,
        Fluctuating,
        MediumSlow,
        Fast,
        Slow,
    }

    public enum Ability
    {
        Overgrow,
        Chlorophyll,
        Blaze,
        SolarPower
    }
    #endregion

    #region moves
    public enum MoveName
    {
        Flamethrower,
        GigaDrain,
        Scratch,
        Tackle,
    }

    public enum MoveCategory
    {
        Physical,
        Special,
        Effect
    }

    #endregion
}