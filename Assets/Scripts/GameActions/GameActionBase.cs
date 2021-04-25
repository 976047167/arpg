using UnityEngine;
namespace GameAction
{

	//动作启动方式
	public enum START_TYPE
	{
		// 自动启动,每个update都会调用启动
		Automatic,

		// 手动调用
		Manual,
	}
	//动作停止的方式
	public enum STOP_TYPE
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
		public START_TYPE StartType;
		public STOP_TYPE StopType;
		public float ActiveTime { get; private set; }
		public float ActiveCount { get; private set; }
		protected CharacterLocomotion OwnerLocomotion;
		protected CharacterLocomotion playerAnimator;
		public virtual int AnimatorInt { get; internal set; }
		public virtual int AnimatorIndex { get; internal set; }
		public virtual float AnimatorFloat { get; internal set; }
		protected GameObject gameObject;
		protected Transform transform;
		/// <summary>
		/// 优先级
		/// </summary>
		public int PriorityIndex{ get; private set; }
		/// <summary>
		/// 动作分为独占动作和并行动作。
		/// 独占动作启动时，根据优先级判断，如果高于在运行的所有动作，那么独占动作启动，关闭其他动作，否则启动失败。
		/// 并行动作启动时，如果有独占动作，根据优先级是否关闭独占动作。之后可以与其他并行动作一起运行
		/// 即使有多个并行动作，但是animator只有一个，仍然以优先级判断动画
		/// </summary>
		public virtual bool IsConcurrent {get { return false; } }

		public virtual ACTION_TYPE type {get; internal set;}
		public virtual void Initialize(CharacterLocomotion owner,int priority)
		{
			this.OwnerLocomotion = owner;
			this.PriorityIndex = priority;
			this.ActiveCount = 0;
			this.gameObject = owner.gameObject;
			this.transform = owner.transform;
			this.playerAnimator = gameObject.GetComponent<CharacterLocomotion>();
		}
		public virtual bool CanActivate()
		{
			if(this.StartType != START_TYPE.Automatic) return false;
			return true;
		}
		public virtual bool CanActivate(PlayerInput input)
		{
			if(this.StartType == START_TYPE.Automatic) return false;
			return true;
		}
		
		/// <summary>
		/// 检测是否可以停止行为
		/// </summary>
		/// <param name="force">通常只有行为冲突的时候，会根据优先级强制停止一个</param>
		public virtual bool CanDeactivate(bool force = false)
		{
			if(force)return true;
			if(this.StopType != STOP_TYPE.Automatic) return false;
			return true;
		}
		public virtual bool CanDeactivate(PlayerInput input)
		{
			if(this.StopType == STOP_TYPE.Automatic) return false;
			return true;
		}
		public virtual void Activavte()
		{
			this.ActiveTime = Time.time;
			this.ActiveCount++;
			this.Active = true;
		}
		/// <summary>
		/// 停止行为
		/// </summary>
		/// <param name="force">通常只有行为冲突的时候，会根据优先级强制停止一个</param>
		public virtual void Deactivate(bool force = false)
		{
			this.Active = false;
		}

		public virtual void Release() { }
		public virtual void Update(){ }
		public virtual void UpdateInDeactive(){ }
	}
}