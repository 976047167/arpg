using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAction;
/// <summary>
/// 控制角色行动
/// 可以放在任何角色上
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class CharacterLocomotion : MonoBehaviour
{
	/// <summary>
	/// 时间缩放，控制单位动画速度
	/// </summary>
	public float TimeScale =1;
	private PlayerController controller;
	private CharacterAnimator animator;
	private Vector2 InputVector = Vector2.zero;
	private Vector3 InputRotation = Vector3.zero;
	private bool Moving = false;
	private Transform cameraTrans;
	
	private Vector3 AnimatorDeltaPosition = Vector3.zero;

	public GameActionBase[] actions;
	/// <summary>
	/// 是否动画需要更新
	/// </summary>
	private bool isAnimatorDirty;
	private bool isPlayer;
	/// <summary>
	/// 速度
	/// </summary>
	/// <value></value>
	public Vector3 Velocity{get;private set;} 
	/// <summary>
	/// 前一帧的位置
	/// </summary>
	public Vector3 PrevPosition;
	private void Awake()
	{
		this.controller = this.GetComponent<PlayerController>();
		this.cameraTrans = Camera.main.transform;
		this.animator = this.GetComponent<CharacterAnimator>();

		//刚体组件不参与任何移动的判定，它唯一的用途是告诉unity这不是一个静态的物体
		var rigidbody = this.GetComponent<Rigidbody>();
		rigidbody.mass = 100;
		rigidbody.isKinematic = true;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		this.actions = new GameActionBase[0];
		this.AddAction(ACTION_TYPE.StartMove);
	}

	/// <summary>
	/// 更新玩家的输入,
	/// 只有玩家控制的角色才会被调用这个接口
	/// </summary>
	/// <param name="input">输入脚本</param>
	public void PlayerUpdate(PlayerInput input)
	{
		this.SetInputVector(input.getDirection());
		this.SetInputVector(input.getDirection());
		this.UpdateInputActions(input);
	}

	/// <summary>
	/// 由AI或者玩家转传来的角色移动方向的向量；
	/// </summary>
	/// <param name="direction">移动的方向</param>
	public void SetInputVector(Vector2 direction)
	{
		this.InputVector.Set(direction.x, direction.y);
	}
	public Vector2 GetInputVector()
	{
		return this.InputVector;
	}

	/// <summary>
	/// 由AI或者玩家转传来的角色转动方向的向量；
	/// </summary>
	/// <param name="direction">转动的方向</param>
	public void SetInputRotation(float yaw)
	{
		this.InputRotation.Set(0, yaw, 0);
	}
	public Vector3 GetInputRotation()
	{
		return this.InputRotation;
	}



	/// <summary>
	/// 更新角色状态
	/// </summary>
	private void Update()
	{
		this.UpdateAutoActions();
		this.UpdateAnimator();
		this.UpdateMoveState();
		if (!this.isPlayer)
		{
			//玩家控制的角色在onAnimatorMove里调用
			this.UpdatePosAndRota();
		}
	}
	/// <summary>
	/// 角色尝试启动行为
	/// </summary>
	/// <param name="action">被启动的行为</param>
	public void tryActiveAction(GameActionBase action)
	{
		if (!action.Enabled) return;
		if (!action.canActivate()) return;
		if (action.Active) return;
		action.Activavte();
		this.UpdateActionAnimator();
	}
	/// <summary>
	/// 角色尝试停止行为
	/// </summary>
	/// <param name="action">被停止的行为</param>
	public void tryDeactivateAction(GameActionBase action)
	{
		if (!action.Enabled) return;
		if (!action.canDeactivate()) return;
		if (!action.Active) return;
		action.Deactivate();
		this.UpdateActionAnimator();
	}

	/// <summary>
	/// 将玩家输入传入给行为，让其判断是否启动
	/// </summary>
	/// <param name="input">玩家输入</param>
	private void UpdateInputActions(PlayerInput input)
	{
		if (this.actions == null) return;
		for (int i = 0; i < this.actions.Length; i++)
		{

			GameActionBase action = actions[i];
			if (!action.Enabled) continue;
			if (action.Active)
			{
				if (action.canDeactivate(input))
				{
					this.tryDeactivateAction(action);
				}
			}
			else
			{
				if (action.canActivate(input))
				{
					this.tryActiveAction(action);
				}
			}
		}

	}

	/// <summary>
	/// 让自动启动的行为根据情况判断是否启用或停止
	/// </summary>
	private void UpdateAutoActions()
	{
		if (this.actions == null) return;
		for (int i = 0; i < this.actions.Length; i++)
		{
			GameActionBase action = actions[i];
			if (!action.Enabled) continue;
			if (action.Active && action.StopType == STOP_TYPE.Automatic)
			{
				this.tryDeactivateAction(action);
			}
			else if (!action.Active && action.StartType == START_TYPE.Automatic)
			{
				this.tryActiveAction(action);
			}
		}

	}
	/// <summary>
	/// 更新Animator的参数
	/// </summary>
	private void UpdateAnimator(){

		if (this.animator == null) return;
		
		this.animator.SetHorizontalMovementParameter(this.InputVector.x, this.TimeScale);
		this.animator.SetForwardMovementParameter(this.InputVector.y, this.TimeScale);
		// this.animator.SetYawParameter(m_YawAngle * m_YawMultiplier, this.TimeScale);
		this.animator.SetMovingParameter(this.Moving);

		this.UpdateActionAnimator(true);
	}
	public void UpdateActionAnimator(bool immediateUpdate = false)
	{
		//是否立即更新,否则将在下一次fixupdate更新
		if (!immediateUpdate)
		{
			this.isAnimatorDirty = true;
			return;
		}
		//如果没有行为变化，不用更新动画
		if (!this.isAnimatorDirty) return;
		this.isAnimatorDirty = false;
		int idx = 0;
		bool idxChange = false;
		int argInt = 0;
		bool intChange = false;
		float argFloat = 0f;
		bool floatChange = false;
		for (int i = 0; i < this.actions.Length; i++)
		{
			var action = this.actions[i];
			if (!action.Active) continue;
			if (!idxChange && action.AnimatorIndex != -1)
			{
				idx = action.AnimatorIndex;
				idxChange = true;
			}
			//idx可能为0，后续数值为0状态下的动画参数
			if (!intChange && action.AnimatorInt != -1)
			{
				argInt = action.AnimatorInt;
				intChange = true;

			}
			if (!floatChange && action.AnimatorFloat != -1)
			{
				argFloat = action.AnimatorFloat;
				floatChange = true;
			}
		}
		this.animator.SetAnimatorIdx(idx);
		this.animator.SetAnimatorInt(argInt);
		this.animator.SetAnimatorFloat(argFloat);
	}
	public void AddAction(ACTION_TYPE type)
	{
		var action = GameActionFactory.GetAction(type);
		action.Enabled = true;
		int length = this.actions.Length;
		Array.Resize(ref this.actions, length + 1);
		this.actions[length] = action;
		action.Initialize(this);
	}
	private void UpdateMoveState()
	{

		bool temp = this.Moving;
		this.Moving = this.InputVector.magnitude > 0.01f;
		if (this.Moving != temp)
		{
			Notification.Emit<CharacterLocomotion, bool>(GameEvent.OnMoving, this, this.Moving);
		}
	}

	protected void OnAnimatorMove()
	{
		this.AnimatorDeltaPosition += this.animator.GetDeltaPos();
		if (Time.deltaTime == 0) return;
		if (this.AnimatorDeltaPosition.magnitude == 0) return;
		this.UpdatePosAndRota();

	}
	private void UpdatePosAndRota()
	{
		UpdateRotation();
		UpdatePosition();
	}
	/// <summary>
	/// 计算需要旋转的角度
	/// </summary>
	private void UpdateRotation()
	{
		//当前角度
		Quaternion curRoation = transform.rotation;
		//要转到的角度
		Quaternion targetRotation = Quaternion.Slerp(curRoation, curRoation * Quaternion.Euler(this.InputRotation), Constants.RoundSpeed * TimeUtility.DeltaTimeScaled * this.TimeScale);
		//两个角度的差值 力矩
		// Quaternion ret = Quaternion.Inverse(curRoation) * targetRotation;

		Quaternion ret = targetRotation;
		//结果四舍五入一下
		this.transform.rotation = MathUtils.Round(ret, 1000000);
	}
	private void UpdatePosition()
	{
		//用t的平方可能是之后要与力相乘计算距离
		float deltaTime = this.TimeScale * this.TimeScale * Time.timeScale * TimeUtility.FramerateDeltaTime;
		//计算动画位移，别问我为什么成了力
		Vector3 localMotionForce = this.transform.InverseTransformDirection(this.AnimatorDeltaPosition);
		this.AnimatorDeltaPosition = Vector3.zero;
		//实际方向
        Vector3 motorThrottle = MathUtils.TransformDirection(localMotionForce, this.transform.rotation);

		// MoveDirection += m_ExternalForce * deltaTime + (m_MotorThrottle * (UsingRootMotionPosition ? 1 : deltaTime)) - m_GravityDirection * m_GravityAmount * deltaTime;
		Vector3 MoveDirection  = motorThrottle;

		this.transform.position = this.transform.position + MoveDirection;

		this.Velocity = (this.transform.position - this.PrevPosition) / (this.TimeScale * Time.deltaTime);
		this.PrevPosition = this.transform.position;
	}
}
