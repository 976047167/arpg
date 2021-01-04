using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
 
    private PlayerController controller;
    private Vector3 movement = Vector3.zero;
    private Rigidbody rigidbody;
    private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
        this.controller = this.GetComponent<PlayerController>();
    }
    private void Start() {
    }
    public void SetMove(Vector3 motion){
        this.movement = motion;
    }
    private void FixedUpdate() {
        this.rigidbody.velocity=this.movement* 10;
    }
}
