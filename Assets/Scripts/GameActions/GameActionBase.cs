using UnityEngine;
//动作启动方式
public enum StartType
{
	// 自动启动,每个update都会调用启动
	Automatic,

	// 手动调用
	Manual,
}
//动作停止的方式
public enum StopType
{
	// 自动启动,每个update都会调用启动
	Automatic,
	// 手动调用
	Manual,
}
public class GameActionBase
{
	public bool NeedMove;
	/// <summary>
	/// 动作是可以使用
	/// </summary>
	public bool Enabled;

	/// <summary>
	/// 动作正在执行中
	/// </summary>
	public bool Actived;
	public StartType StartType;
	public StopType StopType;
	protected PlayerLocomotion playerLocomotion;
	protected PlayerAnimator playerAnimator;
	protected GameObject gameObject;
	protected Transform transform;
	public int index;
	public virtual void Initialize(PlayerLocomotion playerLocomotion, int index)
	{
		this.playerLocomotion = playerLocomotion;
		this.gameObject = playerLocomotion.gameObject;
		this.transform = playerLocomotion.transform;
		this.playerAnimator = gameObject.GetComponent<PlayerAnimator>();
		this.index = index;
	}
	public virtual bool canStartAction()
	{
		return true;
	}
	public virtual bool canStartAction(PlayerInput input)
	{
		return true;
	}
	public virtual bool canStopAction()
	{
		return true;
	}
	public virtual bool canStopAction(PlayerInput input)
	{
		return true;
	}


}