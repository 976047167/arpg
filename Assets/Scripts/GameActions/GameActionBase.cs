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
	public bool Active { get; private set; }
	public StartType StartType;
	public StopType StopType;
	public float ActiveTime { get; private set; }
	public float ActiveCount { get; private set; }
	protected PlayerLocomotion ownerLocomotion;
	protected PlayerAnimator playerAnimator;
	protected GameObject gameObject;
	protected Transform transform;
	public int index;
	public virtual void Initialize(PlayerLocomotion owner, int index)
	{
		this.ownerLocomotion = owner;
		this.gameObject = owner.gameObject;
		this.transform = owner.transform;
		this.playerAnimator = gameObject.GetComponent<PlayerAnimator>();
		this.index = index;
	}
	public virtual bool canActivate()
	{
		return true;
	}
	public virtual bool canActivate(PlayerInput input)
	{
		return true;
	}
	public virtual bool canDeactivate()
	{
		return true;
	}
	public virtual bool canDeactivate(PlayerInput input)
	{
		return true;
	}
	public virtual bool Activavte()
	{
		this.Active = true;
		return true;
	}
	public virtual bool Deactivate()
	{
		this.Active = false;
		return true;
	}


}