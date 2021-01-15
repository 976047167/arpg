using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 控制角色移动
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerLocomotion : MonoBehaviour
{
	public float speed;
	private PlayerController controller;
	private Vector3 moveCommand = Vector3.zero;
	private Vector3 movement = Vector3.zero;
	private Transform cameraTrans;
	public GameActionBase[] actions;
	private void Awake()
	{
		this.controller = this.GetComponent<PlayerController>();
		this.cameraTrans = Camera.main.transform;

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
	public void Tick(PlayerInput input)
	{
		this.UpdateInputActions(input);
	}
	private void OnAnimatorMove()
	{

	}
	public bool tryActiveAction(GameActionBase action)
	{
		if(!action.Enabled)return false;
		if(!action.canActivate())return false;
		if(action.Active)return false;
		return true;
	}
	public bool tryDeactivateAction(GameActionBase action)
	{
		return true;
	}

	private void UpdateInputActions(PlayerInput input)
	{
		if(actions == null)return;
		for (int i = 0; i < actions.Length; i++)
		{

			GameActionBase action =actions[i];
			if (!action.Enabled) {
				continue;
			}
			if (action.Active) {
				if (action.canDeactivate(input)) {
					this.tryDeactivateAction(action);
				}
			}
			else
			{
				if (action.canStartAction(input)) {
					this.tryActiveAction(action);
				}
			}
		}

	}
}
