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
public class CharacterLocomotion : MonoBehaviour
{
	public float speed;
	private PlayerController controller;
	private CharacterAnimator animator;
	private Vector2 InputVector = Vector2.zero;
	private Transform cameraTrans;
	private bool isAnimatorDirty;
	public GameActionBase[] actions;
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
	}
	private void Start()
	{
	}

	/// <summary>
	/// 更新玩家的输入,
	/// 只有玩家控制的角色才会被调用这个接口
	/// </summary>
	/// <param name="input">输入脚本</param>
	public void PlayerUpdate(PlayerInput input)
	{
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
	/// 更新角色状态
	/// </summary>
	private void FixedUpdate()
	{
		this.UpdateAutoActions();
		this.UpdateAnimator();
	}
	private void OnAnimatorMove()
	{

	}
	public void tryActiveAction(GameActionBase action)
	{
		if (!action.Enabled) return;
		if (!action.canActivate()) return;
		if (action.Active) return;
		action.Activavte();
	}
	public void tryDeactivateAction(GameActionBase action)
	{
		if (!action.Enabled) return;
		if (!action.canDeactivate()) return;
		if (!action.Active) return;
		action.Deactivate();
	}

	private void UpdateInputActions(PlayerInput input)
	{
		if (this.actions == null) return;
		for (int i = 0; i < this.actions.Length; i++)
		{

			GameActionBase action = actions[i];
			if (!action.Enabled)
			{
				continue;
			}
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

	private void UpdateAutoActions()
	{
		if (this.actions == null) return;
		for (int i = 0; i < this.actions.Length; i++)
		{
			GameActionBase action = actions[i];
			if (!action.Enabled)
			{
				continue;
			}
			if (action.Active && action.StopType == StopType.Automatic)
			{
				this.tryDeactivateAction(action);
			}
			else if (!action.Active && action.StartType == StartType.Automatic)
			{
				this.tryActiveAction(action);
			}
		}

	}
	/// <summary>
	/// 根据行为数据更新动画
	/// </summary>
	/// <param name="immediateUpdate">是否立即更新,否则将在下一次fixupdate更新</param>
	private void UpdateAnimator(bool immediateUpdate =false)
	{
		if (this.animator == null) return;
		//是否立即更新,否则将在下一次fixupdate更新
		if (!immediateUpdate)
		{
			this.isAnimatorDirty = true;
			return;
		}
		//如果没有行为变化，不用更新动画
		if (!this.isAnimatorDirty) return;
		this.isAnimatorDirty = false;
		for (int i = 0; i < this.actions.Length; i++)
		{
			var action = this.actions[i];
			if (!action.Active) continue;

		}

	}
}
