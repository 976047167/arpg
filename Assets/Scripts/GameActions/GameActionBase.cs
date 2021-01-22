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
	protected CharacterLocomotion ownerLocomotion;
	protected CharacterLocomotion playerAnimator;
	public virtual int AnimatorInt { get; protected set; }
	protected GameObject gameObject;
	protected Transform transform;
	public int index;
	public virtual void Initialize(CharacterLocomotion owner, int index)
	{
		this.ownerLocomotion = owner;
		this.ActiveCount = 0;
		this.gameObject = owner.gameObject;
		this.transform = owner.transform;
		this.playerAnimator = gameObject.GetComponent<CharacterLocomotion>();
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
	public virtual void Activavte()
	{
		this.ActiveTime = Time.time;
		this.ActiveCount++;
		this.Active = true;
	}
	public virtual void Deactivate()
	{
		this.Active = false;
	}


}