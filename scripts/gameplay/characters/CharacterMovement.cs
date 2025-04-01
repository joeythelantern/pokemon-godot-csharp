using Game.Core;
using Godot;
using System;

namespace Game.Gameplay
{
	public partial class CharacterMovement : Node
	{
		[Signal] public delegate void AnimationEventHandler(string animationType);

		[ExportCategory("Nodes")]
		[Export] public Node2D Character;
		[Export] public CharacterInput CharacterInput;
		[Export] public CharacterCollisionRayCast CharacterCollisionRayCast;

		[ExportCategory("Movement")]
		[Export] public Vector2 TargetPosition = Vector2.Down;
		[Export] public bool IsWalking = false;
		[Export] public bool CollisionDetected = false;

		public override void _Ready()
		{
			CharacterInput.Walk += StartWalking;
			CharacterInput.Turn += Turn;

			CharacterCollisionRayCast.Collision += (value) => CollisionDetected = value;

			Logger.Info("Loading character movement component ...");
		}

		public override void _Process(double delta)
		{
			Walk(delta);
		}

		public bool IsMoving()
		{
			return IsWalking;
		}

		public bool IsColliding()
		{
			return CollisionDetected;
		}

		public void StartWalking()
		{
			if (!IsMoving() && !IsColliding())
			{
				EmitSignal(SignalName.Animation, "walk");
				TargetPosition = Character.Position + CharacterInput.Direction * Globals.Instance.GRID_SIZE;
				Logger.Info($"Moving from {Character.Position} to {TargetPosition}");
				IsWalking = true;
			}
		}

		public void Walk(double delta)
		{
			if (IsWalking) 
			{
				Character.Position = Character.Position.MoveToward(TargetPosition, (float)delta * Globals.Instance.GRID_SIZE * 4);

				if (Character.Position.DistanceTo(TargetPosition) < 1f)
				{
					StopWalking();
				}
			}
			else
			{
				EmitSignal(SignalName.Animation, "idle");
			}
		}

		public void StopWalking()
		{
			IsWalking = false;
			SnapPositionToGrid();
		}

		public void Turn()
		{
			EmitSignal(SignalName.Animation, "turn");
		}

		public void SnapPositionToGrid()
		{
			Character.Position = new Vector2(
				Mathf.Round(Character.Position.X / Globals.Instance.GRID_SIZE) * Globals.Instance.GRID_SIZE,
				Mathf.Round(Character.Position.Y / Globals.Instance.GRID_SIZE) * Globals.Instance.GRID_SIZE
			);
		}
	}
}

