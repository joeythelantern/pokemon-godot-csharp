using Game.Core;
using Game.UI;
using Game.Utilities;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

[Tool]
public partial class Npc : CharacterBody2D
{
    private NpcAppearance npcAppearance = NpcAppearance.Worker;

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

    private AnimatedSprite2D animatedSprite2D;
    private NpcInput npcInput;
    private StateMachine stateMachine;
    private CharacterMovement characterMovement;

    private readonly Dictionary<NpcAppearance, SpriteFrames> appearanceFrames = new()
    {
        { NpcAppearance.BugCatcher, GD.Load<SpriteFrames>("res://resources/spriteframes/bug_catcher.tres") },
        { NpcAppearance.Gardener, GD.Load<SpriteFrames>("res://resources/spriteframes/gardener.tres") },
        { NpcAppearance.Worker, GD.Load<SpriteFrames>("res://resources/spriteframes/worker.tres") }
    };

    [Export]
    public NpcInputConfig NpcInputConfig;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            UpdateAppearance();
            return;
        }

        npcInput ??= GetNode<NpcInput>("Input");
        npcInput.Config = NpcInputConfig;

        stateMachine ??= GetNode<StateMachine>("StateMachine");
        stateMachine.ChangeState("Roam");

        animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        characterMovement ??= GetNode<CharacterMovement>("Movement");
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        var player = GameManager.GetPlayer();

        if (player != null)
        {
            ZIndex = (player.Position.Y <= Position.Y) ? 6 : 4;
        }
    }

    private void UpdateAppearance()
    {
        Logger.Info("Changing appearance in editor!");

        if (animatedSprite2D == null)
        {
            animatedSprite2D = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

            if (animatedSprite2D == null)
            {
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

    public void PlayMessage(Vector2 Direction)
    {
        if (Engine.IsEditorHint())
            return;

        if (characterMovement.IsMoving())
            return;

        if (npcInput.Direction != Direction * -1)
        {
            npcInput.Direction = Direction * -1;
            npcInput.EmitSignal(CharacterInput.SignalName.Turn);
        }

        stateMachine.ChangeState("Message");
        MessageManager.PlayText([.. NpcInputConfig.Messages]);
    }
}
