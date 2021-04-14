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
		private bool AnimationFinished = false;
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
		public override void Initialize(CharacterLocomotion owner,int priority)
		{
			base.Initialize(owner,priority);
			Notification.CreateBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.onMoving);
			Notification.CreateBinding<CharacterLocomotion,string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
		}
		public override bool CanActivate()
		{
			if (!base.CanActivate())
			{
				return false;
			}

			if (this.moving ) return false;
			//无移动，不启动
			if (this.OwnerLocomotion.InputVector.magnitude == 0)
			{
				return false;
			}

			return true;
		}

		public override bool CanDeactivate(bool force = false)
		{
			if (!base.CanDeactivate(force))
			{
				return false;
			}
			if(this.AnimationFinished)return true;
			if (this.OwnerLocomotion.InputVector.magnitude > 0)
			{
				return false;
			}
			return true;
		}

		public override void Activavte()
		{
			Vector2 inputValue = this.OwnerLocomotion.InputVector;
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
			this.AnimationFinished = false;

			base.Activavte();
		}
		private void onMoving(CharacterLocomotion locomotion,bool state)
		{
			if(this.OwnerLocomotion != locomotion) return;
			this.moving = state;
		}
		private void OnAnimationEvent(CharacterLocomotion locomotion,string evnet)
		{
			if(this.OwnerLocomotion != locomotion) return;
			if(evnet != "OnAnimatorStartMovementComplete")return;
			this.AnimationFinished = true;
		}
		public override void Release()
		{
			Notification.RemoveBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.onMoving);
			Notification.RemoveBinding<CharacterLocomotion,string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
			base.Release();
		}
	}

}