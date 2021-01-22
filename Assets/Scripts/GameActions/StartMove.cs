using UnityEngine;
/// <summary>
/// 开始移动的行为，用于启动不同的动画
/// </summary>
public class StartMove : GameActionBase
{
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

	public override bool canActivate(PlayerInput input)
	{
		if (!base.canActivate(input))
		{
			return false;
		}

		// The ability can't start if the character is stopped.
		if (input.getDirection().magnitude == 0)
		{
			return false;
		}
		return true;
	}

	public override bool canDeactivate(PlayerInput input)
	{
		if (!base.canDeactivate(input))
		{
			return false;
		}

		// The ability can't start if the character is stopped.
		if (input.getDirection().magnitude > 0)
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
			int AnimationIndex = (int)StartIndex.None;
            if (inputValue.x > speed && inputValue.y > speed) {
                AnimationIndex = (int)StartIndex.RunForwardTurnRight;
            } else if (inputValue.x > 0 && inputValue.y > 0) {
                AnimationIndex = (int)StartIndex.WalkForwardTurnRight;
            } else if (inputValue.x < -speed && inputValue.y > speed) {
                AnimationIndex = (int)StartIndex.RunForwardTurnLeft;
            } else if (inputValue.x < 0 && inputValue.y > 0) {
                AnimationIndex = (int)StartIndex.WalkForwardTurnLeft;
            } else if (inputValue.x < -speed && inputValue.y < -speed) {
                AnimationIndex = (int)StartIndex.RunBackwardTurnLeft;
            } else if (inputValue.x < 0 && inputValue.y < 0) {
                AnimationIndex = (int)StartIndex.WalkBackwardTurnLeft;
            } else if (inputValue.x > speed && inputValue.y < -speed) {
                AnimationIndex = (int)StartIndex.RunBackwardTurnRight;
            } else if (inputValue.x > 0 && inputValue.y < 0) {
                AnimationIndex = (int)StartIndex.WalkBackwardTurnRight;
            } else if (inputValue.y > speed) {
                AnimationIndex = (int)StartIndex.RunForward;
            } else if (inputValue.y > 0) {
                AnimationIndex = (int)StartIndex.WalkForward;
            } else if (inputValue.y < -speed) {
                AnimationIndex = (int)StartIndex.RunBackward;
            } else if (inputValue.y < 0) {
                AnimationIndex = (int)StartIndex.WalkBackward;
            } else if (inputValue.x > speed) {
                AnimationIndex = (int)StartIndex.RunStrafeRight;
            } else if (inputValue.x > 0) {
                AnimationIndex = (int)StartIndex.WalkStrafeRight;
            } else if (inputValue.x < -speed) {
                AnimationIndex = (int)StartIndex.RunStrafeLeft;
            } else if (inputValue.x < 0) {
                AnimationIndex = (int)StartIndex.WalkStrafeLeft;
            }
		this.AnimatorInt = AnimationIndex;

		base.Activavte();
	}

}