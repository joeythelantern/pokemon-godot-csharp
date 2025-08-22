using Game.Core;
using Godot;
using System;

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
                Logger.Info("Checking encounter chance");
                break;
        }

        AnimatedSprite2D.Play("down");
    }

    public void OnBodyExited(Node2D node2D)
    {
        AnimatedSprite2D.Play("up");
    }
}
