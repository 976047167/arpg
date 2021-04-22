using UnityEngine;
namespace GameAction
{
	/// <summary>
	/// 快速转身，不是转向
	/// </summary>
	[GameActionType(ACTION_TYPE.TurnMove)]
	[StartType(START_TYPE.Automatic)]
	[AnimatorIndex(8)]
	public class TurnMoce : GameActionBase
	{
		private bool Moving = false;
		private bool Grounded = false;
		public int MaxInputCount { get; private set; } = 10;

		protected Vector2[] Inputs;
		protected int InputCount;
		protected int InputIndex = -1;
		protected int LastCanStartFrame = -1;
		/// <summary>
		/// 是否记录输入
		/// </summary>
		private bool AccumulateInputs = true;
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
			// 这个地方和stopmove不一样，用原始的inputVector
			this.Inputs[this.InputIndex] = this.OwnerLocomotion.RawInputVector;
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
            if (input.sqrMagnitude < 0.01f) {
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
            // 确保x和y至少有一个相反方向
            if ((this.OwnerLocomotion.RawInputVector.x == 0 || this.AverageInput.x == 0 || Mathf.Sign(this.AverageInput.x) == Mathf.Sign(this.OwnerLocomotion.RawInputVector.x)) && (this.OwnerLocomotion.RawInputVector.y == 0 || this.AverageInput.y == 0 || Mathf.Sign(this.AverageInput.y) == Mathf.Sign(this.OwnerLocomotion.RawInputVector.y))) {
                return false;
            }

            // 大于270度才算转身
            if (Vector2.Dot(this.OwnerLocomotion.RawInputVector, this.AverageInput) > -0.5f) {
                return false;
            }

			return true;
		}
        public override bool CanDeactivate(bool force =false)
        {
			if(force) return true;
			return this.AnimationFinished || !this.Moving;
        }
		public override void Activavte()
		{
            if (Mathf.Abs(this.AverageInput.x) > Constants.SpeedAcceleration || Mathf.Abs(this.AverageInput.y) > Constants.SpeedAcceleration ) {
                this.AnimatorInt = 1;
            } else {
                this.AnimatorInt = 0;
            }
            this.OwnerLocomotion.UseAnimatorRotation = true;
			this.AnimationFinished = false;
			base.Activavte();
			this.AccumulateInputs = false;
			ResetStoredInputs();
		}

		public override void Deactivate(bool force = false)
		{
			base.Deactivate(force);
			this.ResetStoredInputs();
            this.OwnerLocomotion.UseAnimatorRotation = false;
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

		private void OnAnimationEvent(CharacterLocomotion locomotion,string evnet)
		{
			if(this.OwnerLocomotion != locomotion) return;
			if(evnet != "OnAnimatorQuickTurnComplete")return;
			this.AnimationFinished = true;
		}
	}
}