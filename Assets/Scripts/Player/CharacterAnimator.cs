using UnityEngine;
[DisallowMultipleComponent]
public class CharacterAnimator : MonoBehaviour
{

    public Animator _animator;
	private CharacterLocomotion locomotion;
	private Vector3 AnimatorDeltaPosition = Vector3.zero;
	private int Index;
	private int ArgInt;

	private float ArgFloat;
	private void Awake()
    {
        _animator = this.GetComponent<Animator>();
		locomotion = this.GetComponent<CharacterLocomotion>();
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
	public void ExecuteEvent(string arg1)
	{
		Notification.Emit<CharacterLocomotion, string>(GameEvent.AnimationEvent, this.locomotion, arg1);
	}
 	protected void OnAnimatorMove()
	{
		this.AnimatorDeltaPosition += this._animator.deltaPosition;
		if (Time.deltaTime == 0) return;
		if(this.AnimatorDeltaPosition.magnitude == 0)return;


	}
}
