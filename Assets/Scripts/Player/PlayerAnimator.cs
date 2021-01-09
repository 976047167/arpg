using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerAnimator : MonoBehaviour
{

    private Rigidbody mRigid;
    public Animator _animator;
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
    public void setSpeed(float speed){
        this._animator.SetFloat("speed",speed);

    }

}
