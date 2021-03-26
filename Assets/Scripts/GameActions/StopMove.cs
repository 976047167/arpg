using UnityEngine;
namespace GameAction
{
	[GameActionType(ACTION_TYPE.StopMove)]
	[StartType(START_TYPE.Automatic)]
	[AnimatorIndex(6)]
	public abstract class StopMove : GameActionBase
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

		protected abstract bool UseRawInput { get; }

		public override void Initialize(CharacterLocomotion owner)
		{
			base. Initialize(owner);
			Notification.CreateBinding<CharacterLocomotion, bool>(GameEvent.OnMoving, this.OnMoving);
			Notification.CreateBinding<CharacterLocomotion, bool>(GameEvent.OnCharacterGroundedHash, this.OnGrounded);
			this.Inputs = new Vector2[this.MaxInputCount];
		}

		public override void DeactiveUpdate()
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
			this.Inputs[this.InputIndex] = this.ownerLocomotion.InputVector;
			if (this.InputIndex == this.InputCount)
			{
				this.InputCount++;
			}
		}

		public override bool canActivate()
		{
			if (!base.canActivate())
			{
				return false;
			}

			if (!this.Grounded || (this.InputCount == 0))
			{
				return false;
			}
            var input = this.ownerLocomotion.RawInputVector;
            if (Mathf.Abs(input.x) > Mathf.Abs(this.Inputs[this.InputIndex].x) ||
                Mathf.Abs(input.y) > Mathf.Abs(this.Inputs[this.InputIndex].y)) {
                return false;
            }

            if (this.ownerLocomotion.RawInputVector.sqrMagnitude > 0.01f) {
                return false;
            }

            var averageInput = Vector2.zero;
            for (int i = 0; i < this.InputCount; ++i) {
                averageInput += this.Inputs[i];
            }
            averageInput /= this.InputCount;
            if (averageInput.sqrMagnitude < 0.01f) {
                return false;
            }

			return true;
		}

		public override void Activavte()
		{
			base.Activavte();

			this.AccumulateInputs = false;
			ResetStoredInputs();
		}

		public override void Deactivate()
		{
			base.Deactivate();
			this.ResetStoredInputs();
		}


		/// <summary>
		/// The character's position or rotation has been teleported.
		/// </summary>
		/// <param name="snapAnimator">Should the animator be snapped?</param>
		private void OnImmediateTransformChange(bool snapAnimator)
		{
			this.AccumulateInputs = false;
			ResetStoredInputs();
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
			base.Release();
		}
		private void OnMoving(CharacterLocomotion locomotion,bool state)
		{
			if(this.ownerLocomotion != locomotion) return;
			this.Moving = state;
		}
		private void OnGrounded(CharacterLocomotion locomotion,bool state)
		{
			if(this.ownerLocomotion != locomotion) return;
			this.Grounded = state;
			if (!this.Grounded)
			{
				ResetStoredInputs();
			}
		}
	}
}