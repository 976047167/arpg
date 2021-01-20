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
		WalkStrafeLeft, WalkStrafeRight,
		WalkBackward,
		WalkBackwardTurnLeft,
		WalkBackwardTurnRight,
		RunForward, RunForwardTurnLeft,
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
		if (input.getVector().magnitude == 0)
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
		if (input.getVector().magnitude > 0)
		{
			return false;
		}
		return true;
	}

	public override void Activavte()
	{
		
		base.Activavte();
	}

}