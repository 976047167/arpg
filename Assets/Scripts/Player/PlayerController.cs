using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerMovement movement;
    private PlayerAnimator animator;
    private void Awake()
    {
        this.input = this.GetComponent<PlayerInput>();
        this.movement = this.GetComponent<PlayerMovement>();
        this.animator = this.GetComponent<PlayerAnimator>();
    }
    private void Start() {
        
    }
    private void Update() {
        this.movement.SetMoveCommand(this.input.direction);
    }
    private void FixedUpdate() {
        this.animator.setSpeed(this.movement.speed);
        
    }
}
