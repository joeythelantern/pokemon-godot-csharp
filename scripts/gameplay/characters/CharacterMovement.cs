using System;
using System.Collections.Generic;
using Game.Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay;

public partial class CharacterMovement : Node
{
    [Signal]
    public delegate void AnimationEventHandler(string animationType);

    [ExportCategory("Nodes")]
    [Export]
    public Node2D Character;

    [Export]
    public CharacterInput CharacterInput;

    [ExportCategory("Movement")]
    [Export]
    public Vector2 TargetPosition = Vector2.Down;

    [Export]
    public bool IsWalking = false;

    [Export]
    public ECharacterMovement ECharacterMovement = ECharacterMovement.WALKING;

    [ExportCategory("Jumping")]
    [Export]
    public Vector2 StartPosition;

    [Export]
    public bool IsJumping = false;

    [Export]
    public float JumpHeight = 10f;

    [Export]
    public float LerpSpeed = 2f;

    [Export]
    public float Progress = 0f;

    private bool isPlayer = true;

    public override void _Ready()
    {
        CharacterInput.Walk += StartMoving;
        CharacterInput.Turn += Turn;

        if (GetParent().Name != "Player")
            isPlayer = false;

        Logger.Info("Loading character movement component ...");
    }

    public override void _Process(double delta)
    {
        Walk(delta);
        Jump(delta);

        if (!IsMoving())
        {
            if (isPlayer)
            {
                if (Modules.IsActionPressed())
                    return;
            }

            EmitSignal(SignalName.Animation, "idle");
        }
    }

    public bool IsMoving()
    {
        return IsWalking || IsJumping;
    }

    public (Vector2, Array<Dictionary>) GetTargetColliders(Vector2 targetPosition)
    {
        var spaceState = GetViewport().GetWorld2D().DirectSpaceState;

        Vector2 adjustedTargetPosition = targetPosition;
        adjustedTargetPosition.X += 8;
        adjustedTargetPosition.Y += 8;

        var query = new PhysicsPointQueryParameters2D
        {
            Position = adjustedTargetPosition,
            CollisionMask = 1,
            CollideWithAreas = true,
        };

        return (adjustedTargetPosition, spaceState.IntersectPoint(query));
    }

    public bool IsTargetOccupied(Vector2 targetPosition)
    {
        var (adjustedTargetPosition, result) = GetTargetColliders(targetPosition);

        if (result.Count == 0)
        {
            return false;
        }
        else if (result.Count == 1)
        {
            var collider = (Node)(GodotObject)result[0]["collider"];
            var colliderType = collider.GetType().Name;

            return colliderType switch
            {
                "Sign" => true,
                "TallGrass" => false,
                "TileMapLayer" => isPlayer ? GetTileMapLayerCollision((TileMapLayer)collider, adjustedTargetPosition) : true,
                "SceneTrigger" => !isPlayer,
                _ => true,
            };
        }
        else
        {
            return true;
        }
    }

    public bool GetTileMapLayerCollision(TileMapLayer tileMapLayer, Vector2 adjustedTargetPosition)
    {
        Vector2I tileCoordinates = tileMapLayer.LocalToMap(adjustedTargetPosition);
        TileData tileData = tileMapLayer.GetCellTileData(tileCoordinates);

        if (tileData == null)
            return true;

        var ledgeDirection = (string)tileData.GetCustomData("LEDGE");

        if (ledgeDirection == null)
            return true;

        Logger.Info(ledgeDirection);

        switch (ledgeDirection)
        {
            case "DOWN":
                if (CharacterInput.Direction == Vector2.Down)
                {
                    ECharacterMovement = ECharacterMovement.JUMPING;
                    return false;
                }
                break;
            case "LEFT":
                if (CharacterInput.Direction == Vector2.Left)
                {
                    ECharacterMovement = ECharacterMovement.JUMPING;
                    return false;
                }
                break;
            case "RIGHT":
                if (CharacterInput.Direction == Vector2.Right)
                {
                    ECharacterMovement = ECharacterMovement.JUMPING;
                    return false;
                }
                break;
        }

        return true;
    }

    public void StartMoving()
    {
        if (SceneManager.IsChanging)
            return;

        TargetPosition = Character.Position + CharacterInput.Direction * Globals.GRID_SIZE;

        if (!IsMoving() && !IsTargetOccupied(TargetPosition) && SceneManager.GetCurrentLevel().ReserveTile(TargetPosition))
        {
            EmitSignal(SignalName.Animation, "walk");
            Logger.Info($"{GetParent().Name} moving from {Character.Position} to {TargetPosition}");

            if (ECharacterMovement == ECharacterMovement.JUMPING)
            {
                Progress = 0f;
                StartPosition = Character.Position;
                TargetPosition = Character.Position + CharacterInput.Direction * (Globals.GRID_SIZE * 2);
                IsJumping = true;
            }
            else
            {
                IsWalking = true;
            }
        }
    }

    public void Walk(double delta)
    {
        if (IsWalking)
        {
            Character.Position = Character.Position.MoveToward(TargetPosition, (float)delta * Globals.GRID_SIZE * 4);

            if (Character.Position.DistanceTo(TargetPosition) < 1f)
            {
                StopMoving();
            }
        }
    }

    public void Jump(double delta)
    {
        if (IsJumping)
        {
            Progress += LerpSpeed * (float)delta;

            Vector2 position = StartPosition.Lerp(TargetPosition, Progress);

            float parabolicOffset = JumpHeight * (1 - 4 * (Progress - 0.5f) * (Progress - 0.5f));

            position.Y -= parabolicOffset;

            Character.Position = position;

            if (Progress >= 1f)
            {
                StopMoving();
            }
        }
    }

    public void StopMoving()
    {
        SceneManager.GetCurrentLevel().ReleaseTile(TargetPosition);
        IsWalking = false;
        IsJumping = false;
        ECharacterMovement = ECharacterMovement.WALKING;
        SnapPositionToGrid();
    }

    public void Turn()
    {
        EmitSignal(SignalName.Animation, "turn");
    }

    public void SnapPositionToGrid()
    {
        Character.Position = new Vector2(
            Mathf.Round(Character.Position.X / Globals.GRID_SIZE) * Globals.GRID_SIZE,
            Mathf.Round(Character.Position.Y / Globals.GRID_SIZE) * Globals.GRID_SIZE
        );
    }
}
