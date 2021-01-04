using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
 
    private PlayerController controller;
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
    public void SetMove(Vector3 direction){
        this.movement = direction;
		
    }
    private void FixedUpdate() {
		this.Move();
	}
	private void Move()
	{
        Vector3 target = this.cameraTrans.forward * movement.z + this.cameraTrans.right * movement.x;
        target.y = 0;
        RoundView(target);
		rigidbody.velocity = target * Constants.MaxSpeed;
	}

	/// <summary>
    /// 旋转视角
    /// </summary>
    private void RoundView(Vector3 target)
    {
        if (target == Vector3.zero) return;
        Quaternion direction = Quaternion.LookRotation(target);
        transform.rotation = Quaternion.Slerp(transform.rotation, direction, 0.2f);
    }
}
