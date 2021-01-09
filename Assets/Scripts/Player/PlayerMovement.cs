using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private PlayerController controller;
    private Vector3 moveCommand = Vector3.zero;
    private Vector3 movement = Vector3.zero;
    private Rigidbody rigidbody;
	private Transform cameraTrans;
	private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
        this.controller = this.GetComponent<PlayerController>();
		this.cameraTrans = Camera.main.transform;
	}
    private void Start() {
    }
    public void SetMoveCommand(Vector3 direction){
        this.moveCommand = direction;
    }
    private void FixedUpdate() {
		this.Move();
	}
	private void Move()
	{
        if(this.moveCommand.Equals(Vector3.zero)){
            this.movement = Vector3.zero;
            this.speed =0;
            return;
        }
        
        this.movement =this.cameraTrans.forward * moveCommand.z + this.cameraTrans.right * moveCommand.x;
        this.movement.y = 0;
        this.movement = this.movement.normalized;
        this.speed =Mathf.Lerp(this.speed,0.5f,0.3f);
        RoundView(this.movement);
		rigidbody.velocity = this.movement *this.speed* Constants.MaxWalkSpeed;
	}

	/// <summary>
    /// 旋转视角
    /// </summary>
    private void RoundView(Vector3 target)
    {
        if (target == Vector3.zero) return;
        Quaternion direction = Quaternion.LookRotation(target);
        transform.rotation = Quaternion.Slerp(transform.rotation, direction, Constants.RoundSpeed);
    }
}
