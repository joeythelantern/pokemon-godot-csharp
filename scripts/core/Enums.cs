namespace Game.Core;

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
