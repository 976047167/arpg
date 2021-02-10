using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家所控制角色的管理类
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterLocomotion))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private CharacterLocomotion locomotion;
	private Vector3 moveInput = Vector3.zero;
	private void Awake()
    {
        this.playerInput = this.GetComponent<PlayerInput>();
        this.locomotion = this.GetComponent<CharacterLocomotion>();
	}
    private void Start() {
	}
    private void Update() {
		this.locomotion.PlayerUpdate(this.playerInput);
	}
    private void FixedUpdate() {
    }
}
