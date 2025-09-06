using Godot.Collections;

namespace Game.Core;

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

public enum MoveTarget
{
    SpecificMove,
    UsersField,
    User,
    RandomOpponent,
    AllOtherPokemon,
    SelectedPokemon,
    AllOpponents,
    EntireField
}

public static class MovesEnum
{
    public static readonly Dictionary<string, MoveCategory> CategoryMap = new()
    {
        { "physical", MoveCategory.Physical },
        { "special", MoveCategory.Special },
        { "status", MoveCategory.Status }
    };

    public static readonly Dictionary<string, MoveTarget> MoveTargetMap = new()
    {
        { "specific-move", MoveTarget.SpecificMove },
        { "users-field", MoveTarget.UsersField },
        { "user", MoveTarget.User },
        { "random-opponent", MoveTarget.RandomOpponent },
        { "all-other-pokemon", MoveTarget.AllOtherPokemon },
        { "selected-pokemon", MoveTarget.SelectedPokemon },
        { "all-opponents", MoveTarget.AllOpponents },
        { "entire-field", MoveTarget.EntireField }
    };
}