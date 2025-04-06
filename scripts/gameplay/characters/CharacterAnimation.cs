using Game.Core;
using Godot;
using System;

namespace Game.Gameplay
{
	public partial class CharacterAnimation : AnimatedSprite2D
	{
		[ExportCategory("Nodes")]
		[Export] public CharacterInput CharacterInput;
		[Export] public CharacterMovement CharacterMovement;

		[ExportCategory("Animations Vars")]
		[Export] public ECharacterAnimation ECharacterAnimation = ECharacterAnimation.idle_down;

		public override void _Ready()
		{
			CharacterMovement.Animation += PlayAnimation;

			Logger.Info("Loading player animation component ...");
		}

		public void PlayAnimation(string animationType)
		{
			ECharacterAnimation previousAnimation = ECharacterAnimation;

			if (CharacterMovement.IsMoving()) return;

			switch (animationType)
			{
				case "walk":
					if (CharacterInput.Direction == Vector2.Up)
					{
						ECharacterAnimation = ECharacterAnimation.walk_up;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						ECharacterAnimation = ECharacterAnimation.walk_down;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						ECharacterAnimation = ECharacterAnimation.walk_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						ECharacterAnimation = ECharacterAnimation.walk_right;
					}
					break;
				case "turn":
					if (CharacterInput.Direction == Vector2.Up)
					{
						ECharacterAnimation = ECharacterAnimation.turn_up;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						ECharacterAnimation = ECharacterAnimation.turn_down;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						ECharacterAnimation = ECharacterAnimation.turn_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						ECharacterAnimation = ECharacterAnimation.turn_right;
					}
					break;
				case "idle":
					if (CharacterInput.Direction == Vector2.Up)
					{
						ECharacterAnimation = ECharacterAnimation.idle_up;
					}
					else if (CharacterInput.Direction == Vector2.Down)
					{
						ECharacterAnimation = ECharacterAnimation.idle_down;
					}
					else if (CharacterInput.Direction == Vector2.Left)
					{
						ECharacterAnimation = ECharacterAnimation.idle_left;
					}
					else if (CharacterInput.Direction == Vector2.Right)
					{
						ECharacterAnimation = ECharacterAnimation.idle_right;
					}
					break;
			}

			if (previousAnimation != ECharacterAnimation)
			{
				Play(ECharacterAnimation.ToString());
			}
		}
	}
}
