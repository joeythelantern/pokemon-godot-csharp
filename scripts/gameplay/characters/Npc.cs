using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

[Tool]
public partial class Npc : CharacterBody2D
{
    private NpcAppearance npcAppearance = NpcAppearance.Worker;
    private AnimatedSprite2D animatedSprite2D;
    private NpcInput npcInput;

    private readonly Dictionary<NpcAppearance, SpriteFrames> appearanceFrames = new()
    {
        { NpcAppearance.BugCatcher, GD.Load<SpriteFrames>("res://resources/spriteframes/bug_catcher.tres") },
        { NpcAppearance.Gardener, GD.Load<SpriteFrames>("res://resources/spriteframes/gardener.tres") },
        { NpcAppearance.Worker, GD.Load<SpriteFrames>("res://resources/spriteframes/worker.tres") }
    };

    [ExportCategory("Traits")]
    [Export]
    public NpcAppearance NpcAppearance
    {
        get => npcAppearance;
        set
        {
            if (npcAppearance != value)
            {
                npcAppearance = value;
                UpdateAppearance();
            }
        }
    }

    [Export]
    public NpcInputConfig NpcInputConfig;

    public override void _Ready()
    {
        npcInput ??= GetNode<NpcInput>("Input");
        npcInput.NpcInputConfig = NpcInputConfig;

        animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        if (animatedSprite2D == null)
        {
            animatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

            if (animatedSprite2D == null)
            {
                Logger.Error("AnimatedSprite2D node not found");
                return;
            }
        }

        if (appearanceFrames.TryGetValue(npcAppearance, out var spriteFrames))
        {
            animatedSprite2D.SpriteFrames = spriteFrames;
        }
        else
        {
            animatedSprite2D.SpriteFrames = null;
        }
    }
}
