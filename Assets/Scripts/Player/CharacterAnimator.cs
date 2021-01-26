using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class CharacterAnimator : MonoBehaviour
{

    private Rigidbody mRigid;
    public Animator _animator;
	private int Index;
	private int ArgInt;
	private float ArgFloat;
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
	public void SetAnimatorIdx(int idx)
	{
		if(idx == this.Index)return;
		this.Index = idx;
		this._animator.SetInteger(AnimatorHash.Index, idx);
		this._animator.SetTrigger(AnimatorHash.IndexTrigger);
	}
	public void SetAnimatorInt(int num)
	{
		if (num == this.ArgInt)return;
		this.ArgInt = num;
		this._animator.SetInteger(AnimatorHash.ArgInt, num);
	}
	public void SetAnimatorFloat(float num)
	{
		if (num == this.ArgFloat)return;
		this.ArgFloat = num;
		this._animator.SetFloat(AnimatorHash.ArgFloat, num);
	}
}
