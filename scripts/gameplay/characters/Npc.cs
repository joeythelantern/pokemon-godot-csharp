using Game.Core;
using Game.UI;
using Game.Utilities;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

public partial class Npc : CharacterBody2D
{
    private NpcAppearance npcAppearance = NpcAppearance.Worker;
    private AnimatedSprite2D animatedSprite2D;
    private NpcInput npcInput;
    private StateMachine stateMachine;
    private CharacterMovement CharacterMovement;

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
        npcInput.Config = NpcInputConfig;

        stateMachine ??= GetNode<StateMachine>("StateMachine");
        stateMachine.ChangeState("Roam");

        animatedSprite2D ??= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        CharacterMovement ??= GetNode<CharacterMovement>("Movement");

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        Logger.Info("Updating Appearince in Editor");

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

    public void PlayMessage(Vector2 Direction)
    {
        if (CharacterMovement.IsMoving())
            return;

        Logger.Info("PLAYING MESSAGE");

        if (npcInput.Direction != Direction * -1)
        {
            npcInput.Direction = Direction * -1;
            npcInput.EmitSignal(CharacterInput.SignalName.Turn);
        }

        stateMachine.ChangeState("Message");
        MessageManager.PlayText([.. NpcInputConfig.Messages]);
    }
}

#if TOOLS
[Tool]
public partial class Npc : CharacterBody2D
{
    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint())
        {
            var player = GameManager.GetPlayer();

            if (player != null)
            {
                if (player.Position.Y <= Position.Y)
                {
                    if (ZIndex != 6)
                        ZIndex = 6;
                }
                else
                {
                    if (ZIndex != 4)
                        ZIndex = 4;
                }
            }

            return;
        }


        if (animatedSprite2D.SpriteFrames != appearanceFrames[npcAppearance])
        {
            Logger.Info("Updating Appearince in Editor");
            UpdateAppearance();
        }
    }
}
#endif