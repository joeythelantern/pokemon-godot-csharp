using Game.Core;
using Godot;

namespace Game.Gameplay;

public partial class TallGrass : Area2D
{
    [Export]
    public AnimatedSprite2D AnimatedSprite2D;

    public override void _Ready()
    {
        AnimatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void OnBodyEntered(Node2D node2D)
    {
        var className = node2D.GetType().Name;

        switch (className)
        {
            case "Player":
                CalculateEncounterChance();
                break;
        }

        AnimatedSprite2D.Play("down");
    }

    public void OnBodyExited(Node2D node2D)
    {
        AnimatedSprite2D.Play("up");
    }

    public void CalculateEncounterChance()
    {
        int encounterRate = SceneManager.GetCurrentLevel().EncounterRate;
        int encounter = Globals.GetRandomNumberGenerator().RandiRange(0, 100);

        if (encounter <= encounterRate)
        {
            Logger.Info($"POKEMON ENCOUNTER: {encounter} <= {encounterRate}");
        }
    }
}
