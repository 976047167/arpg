using UnityEngine;
namespace GameAction
{
	[GameActionType(ACTION_TYPE.StopMove)]
	[StartType(START_TYPE.Automatic)]
	[AnimatorIndex(7)]
	public class StopMove : GameActionBase
	{
		private bool Moving = false;
		private bool Grounded = false;
		private enum StopIndex {
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

		public int MaxInputCount { get; private set; } = 10;

		protected Vector2[] Inputs;
		protected int InputCount;
		protected int InputIndex = -1;
		protected int LastCanStartFrame = -1;
		/// <summary>
		/// 是否记录输入
		/// </summary>
		private bool AccumulateInputs = true;
		/// <summary>
		/// 在canactive成功足够的次数后才启动
		/// </summary>
		private int StartCount = 7;
		private Vector2 AverageInput;
		private bool AnimationFinished;

		public override void Initialize(CharacterLocomotion owner,int priority)
		{
			base. Initialize(owner,priority);
			Notification.CreateBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.OnMoving);
			Notification.CreateBinding<CharacterLocomotion, bool>(GameEvent.OnCharacterGroundedHash, this.OnGrounded);
			Notification.CreateBinding<CharacterLocomotion, string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
			this.Inputs = new Vector2[this.MaxInputCount];
		}

		public override void UpdateInDeactive()
		{

			if (!this.AccumulateInputs)
			{
				this.AccumulateInputs = true;
				return;
			}

			if (!this.Grounded ) return;

			if (!this.Moving)
			{
				if (this.InputCount > 0)
				{
					this.InputCount--;
				}
				return;
			}

			this.InputIndex = (this.InputIndex + 1) % this.MaxInputCount;
			this.Inputs[this.InputIndex] = this.OwnerLocomotion.InputVector;
			if (this.InputIndex == this.InputCount)
			{
				this.InputCount++;
			}
		}

		public override bool CanActivate()
		{
			if (!base.CanActivate())
			{
				return false;
			}

			if (!this.Grounded || (this.InputCount == 0))
			{
				return false;
			}
            var input = this.OwnerLocomotion.RawInputVector;
            if (Mathf.Abs(input.x) > Mathf.Abs(this.Inputs[this.InputIndex].x) ||
                Mathf.Abs(input.y) > Mathf.Abs(this.Inputs[this.InputIndex].y)) {
                return false;
            }

            if (this.OwnerLocomotion.RawInputVector.sqrMagnitude > 0.01f) {
                return false;
            }

            this.AverageInput = Vector2.zero;
            for (int i = 0; i < this.InputCount; ++i) {
                this.AverageInput += this.Inputs[i];
            }
            this.AverageInput /= this.InputCount;
            if (this.AverageInput.sqrMagnitude < 0.01f) {
                return false;
            }

			return true;
		}
        public override bool CanDeactivate(bool force =false)
        {
			if(force) return true;
			return this.AnimationFinished || this.OwnerLocomotion.RawInputVector.sqrMagnitude > 0.001f;
        }
		public override void Activavte()
		{
			this.AnimatorInt = (int)StopIndex.None;
			if(this.StartCount == 0){
				this.DeterminStopIndex();
			}
			this.AnimationFinished = false;
			base.Activavte();
			this.AccumulateInputs = false;
			ResetStoredInputs();
		}

		public override void Deactivate(bool force = false)
		{
			base.Deactivate(force);
			if(this.StartCount ==0){
				this.ResetStoredInputs();
			}
			this.StartCount = this.MaxInputCount;
		}



        private void DeterminStopIndex()
        {
			float speed = Constants.SpeedAcceleration;
            if (this.AverageInput.x > speed && this.AverageInput.y > speed) {
                this.AnimatorInt = (int)StopIndex.RunForwardTurnRight;
            } else if (this.AverageInput.x > 0 && this.AverageInput.y > 0) {
                this.AnimatorInt= (int)StopIndex.WalkForwardTurnRight;
            } else if (this.AverageInput.x < -speed && this.AverageInput.y > speed) {
                this.AnimatorInt = (int)StopIndex.RunForwardTurnLeft;
            } else if (this.AverageInput.x < 0 && this.AverageInput.y > 0) {
                this.AnimatorInt = (int)StopIndex.WalkForwardTurnLeft;
            } else if (this.AverageInput.x < -speed && this.AverageInput.y < -speed) {
                this.AnimatorInt = (int)StopIndex.RunBackwardTurnLeft;
            } else if (this.AverageInput.x < 0 && this.AverageInput.y < 0) {
                this.AnimatorInt = (int)StopIndex.WalkBackwardTurnLeft;
            } else if (this.AverageInput.x > speed && this.AverageInput.y < -speed) {
                this.AnimatorInt = (int)StopIndex.RunBackwardTurnRight;
            } else if (this.AverageInput.x > 0 && this.AverageInput.y < 0) {
                this.AnimatorInt = (int)StopIndex.WalkBackwardTurnRight;
            } else if (this.AverageInput.y > speed) {
                this.AnimatorInt = (int)StopIndex.RunForward;
            } else if (this.AverageInput.y > 0) {
                this.AnimatorInt = (int)StopIndex.WalkForward;
            } else if (this.AverageInput.y < -speed) {
                this.AnimatorInt = (int)StopIndex.RunBackward;
            } else if (this.AverageInput.y < 0) {
                this.AnimatorInt = (int)StopIndex.WalkBackward;
            } else if (this.AverageInput.x > speed) {
                this.AnimatorInt = (int)StopIndex.RunStrafeRight;
            } else if (this.AverageInput.x > 0) {
                this.AnimatorInt = (int)StopIndex.WalkStrafeRight;
            } else if (this.AverageInput.x < -speed) {
                this.AnimatorInt = (int)StopIndex.RunStrafeLeft;
            } else if (this.AverageInput.x < 0) {
                this.AnimatorInt = (int)StopIndex.WalkStrafeLeft;
            }
        }

		protected void ResetStoredInputs()
		{
			this.InputIndex = -1;
			this.InputCount = 0;
		}

		public override void Release()
		{
			Notification.RemoveBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.OnMoving);
			Notification.RemoveBinding<CharacterLocomotion, bool>(GameEvent.OnCharacterGroundedHash, this.OnGrounded);
			Notification.RemoveBinding<CharacterLocomotion, string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
			base.Release();
		}
		private void OnMoving(CharacterLocomotion locomotion,bool state)
		{
			if(this.OwnerLocomotion != locomotion) return;
			this.Moving = state;
		}
		private void OnGrounded(CharacterLocomotion locomotion,bool state)
		{
			if(this.OwnerLocomotion != locomotion) return;
			this.Grounded = state;
			if (!this.Grounded)
			{
				ResetStoredInputs();
			}
		}
		public override void Update()
		{
			base.Update();
            if (this.StartCount > 0) {
                if (!this.CanActivate() && this.CanDeactivate()) {
                    this.Deactivate();
                }
                this.StartCount--;
                if (this.StartCount == 0) {
                    DeterminStopIndex();
                    this.OwnerLocomotion.UpdateActionAnimator();
                }
            }
		}
		private void OnAnimationEvent(CharacterLocomotion locomotion,string evnet)
		{
			if(this.OwnerLocomotion != locomotion) return;
			if(evnet != "OnAnimatorStopMovementComplete")return;
			this.AnimationFinished = true;
		}
	}
}