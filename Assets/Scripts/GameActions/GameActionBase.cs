using UnityEngine;
namespace GameAction
{

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
		/// 动作是否可以使用
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
		public virtual int AnimatorInt { get; internal set; }
		public virtual int AnimatorIndex { get; internal set; }
		public virtual float AnimatorFloat { get; internal set; }
		protected GameObject gameObject;
		protected Transform transform;

		public virtual ACTION_TYPE type {get; internal set;}
		public virtual void Initialize(CharacterLocomotion owner)
		{
			this.ownerLocomotion = owner;
			this.ActiveCount = 0;
			this.gameObject = owner.gameObject;
			this.transform = owner.transform;
			this.playerAnimator = gameObject.GetComponent<CharacterLocomotion>();
		}
		public virtual bool canActivate()
		{
			if(this.StartType != StartType.Automatic) return false;
			return true;
		}
		public virtual bool canActivate(PlayerInput input)
		{
			if(this.StartType == StartType.Automatic) return false;
			return true;
		}
		public virtual bool canDeactivate()
		{
			if(this.StopType != StopType.Automatic) return false;
			return true;
		}
		public virtual bool canDeactivate(PlayerInput input)
		{
			if(this.StopType == StopType.Automatic) return false;
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

		public virtual void Release()
		{

		}

	}
}