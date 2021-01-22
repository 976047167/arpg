using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class CharacterAnimator : MonoBehaviour
{

    private Rigidbody mRigid;
    public Animator _animator;
    private void Awake()
    {
        mRigid = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }
    private void Start() {
        
    }

    private void Update()
    {

    }
}
