using Game.Core;
using Game.Core.AI;
using Game.UI;
using Game.Utilities;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

[Tool]
public partial class Npc : CharacterBody2D
{
    private NpcAppearance npcAppearance = NpcAppearance.Worker;
    
    // AI Conversation
    private OllamaAI.ConversationContext conversationContext;
    private int convincingTurns;
    private bool rewardGiven;

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
            if (animatedSprite2D.SpriteFrames != spriteFrames)
            {
                Logger.Info($"Updating appearance for {Name} to {spriteFrames.ResourcePath}");
                animatedSprite2D.SpriteFrames = spriteFrames;
            }
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

        Logger.Info(new object[] { Name, ": PlayMessage called with direction:", Direction });

        if (characterMovement.IsMoving())
        {
            Logger.Info(new object[] { Name, ": Character is moving, ignoring PlayMessage" });
            return;
        }

        if (npcInput.Direction != Direction * -1)
        {
            npcInput.Direction = Direction * -1;
            npcInput.EmitSignal(CharacterInput.SignalName.Turn);
        }

        Logger.Info(new object[] { Name, ": Changing to Message state" });
        stateMachine.ChangeState("Message");
        
        // Initialize AI conversation if not already done
        if (conversationContext == null)
        {
            Logger.Info(new object[] { Name, ": Initializing AI conversation context" });
            conversationContext = CreateConversationContext();
        }
        else
        {
            Logger.Info(new object[] { Name, ": Using existing conversation context" });
        }
        
        // Start AI conversation
        Logger.Info(new object[] { Name, ": Starting AI talk" });
        StartAITalk();
    }
    
    /// <summary>
    /// Start conversation with AI
    /// </summary>
    public async void StartAITalk()
    {
        Logger.Info(new object[] { Name, ": StartAITalk - checking Ollama availability" });
        
        // Check if Ollama is available
        if (!OllamaAI.IsAvailable())
        {
            // Fallback to static messages
            Logger.Warning(new object[] { Name, ": Ollama not available, using fallback messages" });
            MessageManager.StartAIConversation(this, [.. NpcInputConfig.Messages]);
            TryHandOverItem(talkCompleted: true, convinced: false);
            return;
        }
        
        Logger.Info(new object[] { Name, ": StartAITalk - Ollama available, sending initial message" });
        
        // Get AI response
        var response = await OllamaAI.SendMessageAsync(
            conversationContext,
            "Hello there!"  // Initial greeting
        );
        
        Logger.Info(new object[] { Name, ": StartAITalk - AI response received, success:", response.Success, "message:", response.Message });
        
        if (response.Success && !string.IsNullOrEmpty(response.Message))
        {
            // Display AI response
            Logger.Info(new object[] { Name, ": StartAITalk - Starting AI conversation with response" });
            MessageManager.StartAIConversation(this, new[] { response.Message });
            TryHandOverItem(talkCompleted: true, response.Convinced);
        }
        else
        {
            // Fallback to static messages
            Logger.Warning(new object[] { Name, ": StartAITalk - AI response failed, using fallback. Error:", response.Error });
            MessageManager.StartAIConversation(this, [.. NpcInputConfig.Messages]);
        }
    }
    
    /// <summary>
    /// Send user message to AI and get response
    /// </summary>
    public async void SendUserMessage(string userMessage)
    {
        Logger.Info(new object[] { Name, ": SendUserMessage - user said:", userMessage });
        
        if (conversationContext == null)
        {
            Logger.Info(new object[] { Name, ": SendUserMessage - Creating new conversation context" });
            conversationContext = CreateConversationContext();
        }
        
        if (!OllamaAI.IsAvailable())
        {
            Logger.Warning(new object[] { Name, ": SendUserMessage - Ollama not available, using contextual fallback" });
            MessageManager.AddAIResponse(this, "Sorry, I lost my train of thought. Could you say that again in a moment?");
            return;
        }
        
        Logger.Info(new object[] { Name, ": SendUserMessage - Sending to AI" });
        
        // Send to AI
        var response = await OllamaAI.SendMessageAsync(conversationContext, userMessage);
        
        Logger.Info(new object[] { Name, ": SendUserMessage - AI response:", response.Message });
        
        if (response.Success && !string.IsNullOrEmpty(response.Message))
        {
            // Display AI response - this will add it to the messages and display it
            Logger.Info(new object[] { Name, ": SendUserMessage - Adding AI response to message manager" });
            MessageManager.AddAIResponse(this, response.Message);
            TryHandOverItem(talkCompleted: true, response.Convinced);
        }
        else
        {
            Logger.Warning(new object[] { Name, ": SendUserMessage - AI error:", response.Error });
            // End conversation on error
            MessageManager.EndAIConversation(this);
            stateMachine.ChangeState("Roam");
        }
    }

    private OllamaAI.ConversationContext CreateConversationContext()
    {
        string convincingGoal = NpcInputConfig?.ItemReward?.Mode == NpcItemHandoverMode.AfterConvincing
            ? NpcInputConfig.ItemReward.ConvincingGoal
            : string.Empty;

        return OllamaAI.InitializeConversation(
            Name,
            NpcAppearance.ToString(),
            convincingGoal: convincingGoal);
    }

    private void TryHandOverItem(bool talkCompleted, bool convinced)
    {
        NpcItemRewardConfig reward = NpcInputConfig?.ItemReward;
        if (reward == null || rewardGiven || reward.Mode == NpcItemHandoverMode.None)
            return;

        bool shouldGiveItem = reward.Mode switch
        {
            NpcItemHandoverMode.AfterTalking => talkCompleted,
            NpcItemHandoverMode.AfterConvincing => RegisterConvincingTurn(convinced, reward.RequiredConvincingTurns),
            _ => false
        };

        if (!shouldGiveItem || reward.Item == null || string.IsNullOrWhiteSpace(reward.Item.Id))
            return;

        rewardGiven = true;
        int quantity = Mathf.Max(1, reward.Quantity);
        string itemName = string.IsNullOrWhiteSpace(reward.Item.DisplayName) ? reward.Item.Id : reward.Item.DisplayName;
        Inventory.AddItem(reward.Item, quantity);

        if (!string.IsNullOrWhiteSpace(reward.HandoverMessage))
            MessageManager.AddAIResponse(this, reward.HandoverMessage);
        MessageManager.AddSystemMessage($"You received {quantity} x {itemName}!");
    }

    private bool RegisterConvincingTurn(bool convinced, int requiredTurns)
    {
        if (convinced)
            convincingTurns++;

        return convincingTurns >= Mathf.Max(1, requiredTurns);
    }
}
