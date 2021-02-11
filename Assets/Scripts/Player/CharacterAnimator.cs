using UnityEngine;
[DisallowMultipleComponent]
public class CharacterAnimator : MonoBehaviour
{

    public Animator _animator;
    private CharacterLocomotion locomotion;

    private int Index;
    private int ArgInt;

    private float ArgFloat;
    private float HorizontalMovement;
    private float ForwardMovement;
    private bool Moving;

    private void Awake()
    {
        _animator = this.GetComponent<Animator>();
        locomotion = this.GetComponent<CharacterLocomotion>();
    }
    private void Start()
    {

    }

    private void Update()
    {

    }
    public void SetAnimatorIdx(int idx)
    {
        if (idx == this.Index) return;
        this.Index = idx;
        this._animator.SetInteger(AnimatorHash.Index, idx);
        this._animator.SetTrigger(AnimatorHash.IndexTrigger);
    }
    public void SetAnimatorInt(int num)
    {
        if (num == this.ArgInt) return;
        this.ArgInt = num;
        this._animator.SetInteger(AnimatorHash.ArgInt, num);
    }
    public void SetAnimatorFloat(float num)
    {
        if (num == this.ArgFloat) return;
        this.ArgFloat = num;
        this._animator.SetFloat(AnimatorHash.ArgFloat, num);
    }
    public void ExecuteEvent(string arg1)
    {
        Notification.Emit<CharacterLocomotion, string>(GameEvent.AnimationEvent, this.locomotion, arg1);
    }
    public Vector3 GetDeltaPos()
    {
        return this._animator.deltaPosition;
    }
    public bool SetHorizontalMovementParameter(float value, float timeScale)
    {
        var change = this.HorizontalMovement != value;
        if (change)
        {
            this._animator.SetFloat(AnimatorHash.HorizontalMovementHash, value, Constants.DampingTime, TimeUtility.DeltaTimeScaled / timeScale);
            this.HorizontalMovement = this._animator.GetFloat(AnimatorHash.HorizontalMovementHash);
            if (Mathf.Abs(this.HorizontalMovement) < 0.001f)
            {
                this.HorizontalMovement = 0;
            }
        }
        return change;
    }
    public bool SetForwardMovementParameter(float value, float timeScale)
    {
        var change = this.ForwardMovement != value;
        if (change)
        {
            this._animator.SetFloat(AnimatorHash.ForwardMovementHash, value, Constants.DampingTime, TimeUtility.DeltaTimeScaled / timeScale);
            this.ForwardMovement = this._animator.GetFloat(AnimatorHash.ForwardMovementHash);
            if (Mathf.Abs(this.ForwardMovement) < 0.001f)
            {
                this.ForwardMovement = 0;
            }
        }
        return change;
    }

    public bool SetMovingParameter(bool value)
    {
        var change = this.Moving != value;
        if (change)
        {
			this._animator.SetBool(AnimatorHash.MovingHash,value);
			this.Moving = value;
        }
        return change;
    }
}
