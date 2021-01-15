using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerLocomotion))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput Input;
    private PlayerLocomotion locomotion;
    private PlayerAnimator animator;
	private Vector3 moveInput = Vector3.zero;
	private void Awake()
    {
        this.Input = this.GetComponent<PlayerInput>();
        this.locomotion = this.GetComponent<PlayerLocomotion>();
        this.animator = this.GetComponent<PlayerAnimator>();
	}
    private void Start() {
        
    }
    private void Update() {
        this.locomotion.Tick(this.Input);
	}
    private void FixedUpdate() {
    }
}
