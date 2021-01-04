using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimator : MonoBehaviour
{
    public bool canMove = true;
    public bool canSkill = false;

    private Rigidbody mRigid;
    private Animator _animator;
    private PlayerController controller;

    private void Awake()
    {
        mRigid = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        this.controller = this.GetComponent<PlayerController>();
    }
    private void Start() {
        
    }

    private void Update()
    {

    }

}
