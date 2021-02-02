using UnityEngine;
namespace GameAction
{

	/// <summary>
	/// 开始移动的行为，用于启动不同的动画
	/// </summary>
	[GameActionType(ACTION_TYPE.StartMove)]
	[StartType(START_TYPE.Automatic)]
	[AnimatorIndex(6)]
	public class StartMove : GameActionBase
	{
		private bool moving = false;
		private bool animationFinished = false;
		private enum StartIndex
		{
			None,
			WalkForward,
			WalkForwardTurnLeft,
			WalkForwardTurnRight,
			WalkStrafeLeft,
			WalkStrafeRight,
			WalkBackward,
			WalkBackwardTurnLeft,
			WalkBackwardTurnRight,
			RunForward,
			RunForwardTurnLeft,
			RunForwardTurnRight,
			RunStrafeLeft,
			RunStrafeRight,
			RunBackward,
			RunBackwardTurnLeft,
			RunBackwardTurnRight
		}
		public override void Initialize(CharacterLocomotion owner)
		{
			base.Initialize(owner);
			Notification.CreateBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.onMoving);
			Notification.CreateBinding<CharacterLocomotion,string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
		}
		public override bool canActivate()
		{
			if (!base.canActivate())
			{
				return false;
			}

			if (this.moving ) return false;
			// The ability can't start if the character is stopped.
			if (this.ownerLocomotion.GetInputVector().magnitude == 0)
			{
				return false;
			}

			return true;
		}

		public override bool canDeactivate()
		{
			if (!base.canDeactivate())
			{
				return false;
			}
			if(animationFinished)return true;
			// The ability can't start if the character is stopped.
			if (this.ownerLocomotion.GetInputVector().magnitude > 0)
			{
				return false;
			}
			return true;
		}

		public override void Activavte()
		{
			// The start index is based on the input value.
			Vector2 inputValue = this.ownerLocomotion.GetInputVector();
			float speed = Constants.SpeedAcceleration;
			int moveArg = (int)StartIndex.None;
			if (inputValue.x > speed && inputValue.y > speed)
			{
				moveArg = (int)StartIndex.RunForwardTurnRight;
			}
			else if (inputValue.x > 0 && inputValue.y > 0)
			{
				moveArg = (int)StartIndex.WalkForwardTurnRight;
			}
			else if (inputValue.x < -speed && inputValue.y > speed)
			{
				moveArg = (int)StartIndex.RunForwardTurnLeft;
			}
			else if (inputValue.x < 0 && inputValue.y > 0)
			{
				moveArg = (int)StartIndex.WalkForwardTurnLeft;
			}
			else if (inputValue.x < -speed && inputValue.y < -speed)
			{
				moveArg = (int)StartIndex.RunBackwardTurnLeft;
			}
			else if (inputValue.x < 0 && inputValue.y < 0)
			{
				moveArg = (int)StartIndex.WalkBackwardTurnLeft;
			}
			else if (inputValue.x > speed && inputValue.y < -speed)
			{
				moveArg = (int)StartIndex.RunBackwardTurnRight;
			}
			else if (inputValue.x > 0 && inputValue.y < 0)
			{
				moveArg = (int)StartIndex.WalkBackwardTurnRight;
			}
			else if (inputValue.y > speed)
			{
				moveArg = (int)StartIndex.RunForward;
			}
			else if (inputValue.y > 0)
			{
				moveArg = (int)StartIndex.WalkForward;
			}
			else if (inputValue.y < -speed)
			{
				moveArg = (int)StartIndex.RunBackward;
			}
			else if (inputValue.y < 0)
			{
				moveArg = (int)StartIndex.WalkBackward;
			}
			else if (inputValue.x > speed)
			{
				moveArg = (int)StartIndex.RunStrafeRight;
			}
			else if (inputValue.x > 0)
			{
				moveArg = (int)StartIndex.WalkStrafeRight;
			}
			else if (inputValue.x < -speed)
			{
				moveArg = (int)StartIndex.RunStrafeLeft;
			}
			else if (inputValue.x < 0)
			{
				moveArg = (int)StartIndex.WalkStrafeLeft;
			}
			this.AnimatorInt = moveArg;
			this.animationFinished = false;

			base.Activavte();
		}
		private void onMoving(CharacterLocomotion locomotion,bool state)
		{
			if(this.ownerLocomotion != locomotion) return;
			this.moving = state;
		}
		private void OnAnimationEvent(CharacterLocomotion locomotion,string evnet)
		{
			if(this.ownerLocomotion != locomotion) return;
			if(evnet != "OnAnimatorStartMovementComplete")return;
			this.animationFinished = true;
		}
		public override void Release()
		{
			Notification.RemoveBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.onMoving);
			Notification.RemoveBinding<CharacterLocomotion,string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
			base.Release();
		}
	}

}