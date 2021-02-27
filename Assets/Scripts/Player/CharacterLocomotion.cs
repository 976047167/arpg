using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAction;
/// <summary>
/// 控制角色行动
/// 可以放在任何角色上
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class CharacterLocomotion : MonoBehaviour
{

    /// <summary>
    /// 时间缩放，控制单位动画速度
    /// </summary>
    public float TimeScale = 1;
    private PlayerController controller;
    private CharacterAnimator animator;
    private Vector2 InputVector = Vector2.zero;
    private Vector3 InputRotation = Vector3.zero;
    private bool Moving = false;
    private Transform cameraTrans;

    private Vector3 AnimatorDeltaPosition = Vector3.zero;

    public GameActionBase[] actions;
    /// <summary>
    /// 是否动画需要更新
    /// </summary>
    private bool isAnimatorDirty;
    /// <summary>
    /// 该角色是否为玩家
    /// </summary>
    private bool isPlayer;
    /// <summary>
    /// 速度
    /// </summary>
    /// <value></value>
    public Vector3 Velocity { get; private set; }
    /// <summary>
    /// 前一帧的位置
    /// </summary>
    public Vector3 PrevPosition;

    public float YawAngle;
    /// <summary>
    /// 碰撞体的位置
    /// </summary>
    public List<Collider> Colliders;
    /// <summary>
    /// 碰撞体和射线相交点的映射
    /// </summary>
    public Dictionary<RaycastHit, int> ColliderIndexMap;

    /// <summary>
    /// 单个碰撞体投射结果缓冲，减少创建用的开销
    /// </summary>
    private RaycastHit[] RaycastHitsBuffer;

    /// <summary>
    /// 一帧全部碰撞体投射结果缓冲，减少创建用的开销
    /// </summary>
    private RaycastHit[] CombinedRaycastHitsBuffer;

    /// <summary>
    /// 重叠的碰撞器缓冲
    /// </summary>
    private Collider[] OverlapColliderBuffer;
    private void Awake()
    {
        this.controller = this.GetComponent<PlayerController>();
        this.cameraTrans = Camera.main.transform;
        this.animator = this.GetComponent<CharacterAnimator>();

        //刚体组件不参与任何移动的判定，它唯一的用途是告诉unity这不是一个静态的物体
        var rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.mass = 100;
        rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        //遍历所有碰撞体
        var colliders = this.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; ++i)
        {
            //只计算球碰撞和胶囊体碰撞，其他碰撞暂时忽略
            if (!(colliders[i] is CapsuleCollider || colliders[i] is SphereCollider))
            {
                continue;
            }
            //触发器不要
            if (!colliders[i].enabled || colliders[i].isTrigger)
            {
                continue;
            }
            this.Colliders.Add(colliders[i]);
        }
        this.ColliderIndexMap = new Dictionary<RaycastHit, int>(RaycastUtils.RaycastHitEqualityComparer);
        this.RaycastHitsBuffer = new RaycastHit[100];
        this.CombinedRaycastHitsBuffer = new RaycastHit[100 * this.Colliders.Count];
        this.OverlapColliderBuffer = new Collider[100];



        this.actions = new GameActionBase[0];
        this.AddAction(ACTION_TYPE.StartMove);
    }

    /// <summary>
    /// 更新玩家的输入,
    /// 只有玩家控制的角色才会被调用这个接口
    /// </summary>
    /// <param name="input">输入脚本</param>
    public void PlayerUpdate(PlayerInput input)
    {
        this.SetInputVector(input.getDirection());
        this.SetInputRotation(input.getDirection());
        this.UpdateInputActions(input);
    }

    /// <summary>
    /// 由AI或者玩家转传来的角色移动方向的向量；
    /// </summary>
    /// <param name="direction">移动的方向</param>
    public void SetInputVector(Vector2 direction)
    {
        var clampValue = Mathf.Max(Mathf.Abs(direction.x), Mathf.Max(Mathf.Abs(direction.y), 1));
        this.InputVector.y = Mathf.Clamp(direction.magnitude, -clampValue, clampValue);
        this.InputVector.x = 0;
    }
    public Vector2 GetInputVector()
    {
        return this.InputVector;
    }

    /// <summary>
    /// 由AI或者玩家转传来的角色转动方向的向量；
    /// </summary>
    /// <param name="direction">转动的方向</param>
    public void SetInputRotation(Vector2 direction)
    {
        var yaw = 0f;
        if (direction.x != 0 || direction.y != 0)
        {
            var lookRotation = Quaternion.LookRotation(Camera.main.transform.rotation *
                new Vector3(direction.x, 0, direction.y).normalized, this.transform.up);
            yaw = MathUtils.ClampInnerAngle(MathUtils.InverseTransformQuaternion(this.transform.rotation, lookRotation).eulerAngles.y);
        }

        this.InputRotation.Set(0, yaw, 0);
    }
    public Vector3 GetInputRotation()
    {
        return this.InputRotation;
    }



    /// <summary>
    /// 更新角色状态
    /// </summary>
    private void Update()
    {

        this.UpdateAutoActions();
        this.UpdateAnimator();
        this.UpdateMoveState();
        if (!this.isPlayer)
        {
            //玩家控制的角色在onAnimatorMove里调用
            this.UpdatePosAndRota();
        }
    }
    /// <summary>
    /// 角色尝试启动行为
    /// </summary>
    /// <param name="action">被启动的行为</param>
    public void tryActiveAction(GameActionBase action)
    {
        if (!action.Enabled) return;
        if (!action.canActivate()) return;
        if (action.Active) return;
        action.Activavte();
        this.UpdateActionAnimator();
    }
    /// <summary>
    /// 角色尝试停止行为
    /// </summary>
    /// <param name="action">被停止的行为</param>
    public void tryDeactivateAction(GameActionBase action)
    {
        if (!action.Enabled) return;
        if (!action.canDeactivate()) return;
        if (!action.Active) return;
        action.Deactivate();
        this.UpdateActionAnimator();
    }

    /// <summary>
    /// 将玩家输入传入给行为，让其判断是否启动
    /// </summary>
    /// <param name="input">玩家输入</param>
    private void UpdateInputActions(PlayerInput input)
    {
        if (this.actions == null) return;
        for (int i = 0; i < this.actions.Length; i++)
        {

            GameActionBase action = actions[i];
            if (!action.Enabled) continue;
            if (action.Active)
            {
                if (action.canDeactivate(input))
                {
                    this.tryDeactivateAction(action);
                }
            }
            else
            {
                if (action.canActivate(input))
                {
                    this.tryActiveAction(action);
                }
            }
        }

    }

    /// <summary>
    /// 让自动启动的行为根据情况判断是否启用或停止
    /// </summary>
    private void UpdateAutoActions()
    {
        if (this.actions == null) return;
        for (int i = 0; i < this.actions.Length; i++)
        {
            GameActionBase action = actions[i];
            if (!action.Enabled) continue;
            if (action.Active && action.StopType == STOP_TYPE.Automatic)
            {
                this.tryDeactivateAction(action);
            }
            else if (!action.Active && action.StartType == START_TYPE.Automatic)
            {
                this.tryActiveAction(action);
            }
        }

    }
    /// <summary>
    /// 更新Animator的参数
    /// </summary>
    private void UpdateAnimator()
    {

        if (this.animator == null) return;

        this.animator.SetHorizontalMovementParameter(this.InputVector.x, this.TimeScale);
        this.animator.SetForwardMovementParameter(this.InputVector.y, this.TimeScale);
        this.animator.SetYawParameter(this.YawAngle * Constants.YawMultiplier, this.TimeScale);
        this.animator.SetMovingParameter(this.Moving);

        this.UpdateActionAnimator(true);
    }
    public void UpdateActionAnimator(bool immediateUpdate = false)
    {
        //是否立即更新,否则将在下一次fixupdate更新
        if (!immediateUpdate)
        {
            this.isAnimatorDirty = true;
            return;
        }
        //如果没有行为变化，不用更新动画
        if (!this.isAnimatorDirty) return;
        this.isAnimatorDirty = false;
        int idx = 0;
        bool idxChange = false;
        int argInt = 0;
        bool intChange = false;
        float argFloat = 0f;
        bool floatChange = false;
        for (int i = 0; i < this.actions.Length; i++)
        {
            var action = this.actions[i];
            if (!action.Active) continue;
            if (!idxChange && action.AnimatorIndex != -1)
            {
                idx = action.AnimatorIndex;
                idxChange = true;
            }
            //idx可能为0，后续数值为0状态下的动画参数
            if (!intChange && action.AnimatorInt != -1)
            {
                argInt = action.AnimatorInt;
                intChange = true;

            }
            if (!floatChange && action.AnimatorFloat != -1)
            {
                argFloat = action.AnimatorFloat;
                floatChange = true;
            }
        }
        this.animator.SetAnimatorIdx(idx);
        this.animator.SetAnimatorInt(argInt);
        this.animator.SetAnimatorFloat(argFloat);
    }
    public void AddAction(ACTION_TYPE type)
    {
        var action = GameActionFactory.GetAction(type);
        action.Enabled = true;
        int length = this.actions.Length;
        Array.Resize(ref this.actions, length + 1);
        this.actions[length] = action;
        action.Initialize(this);
    }
    private void UpdateMoveState()
    {

        bool temp = this.Moving;
        this.Moving = this.InputVector.magnitude > 0.01f;
        if (this.Moving != temp)
        {
            Notification.Emit<CharacterLocomotion, bool>(GameEvent.OnMoving, this, this.Moving);
        }
    }

    protected void OnAnimatorMove()
    {
        this.AnimatorDeltaPosition += this.animator.GetDeltaPos();
        if (Time.deltaTime == 0) return;
        if (this.AnimatorDeltaPosition.magnitude == 0) return;
        this.UpdatePosAndRota();

    }
    private void UpdatePosAndRota()
    {
        UpdateRotation();
        UpdatePosition();
    }
    /// <summary>
    /// 计算需要旋转的角度
    /// </summary>
    private void UpdateRotation()
    {
        //当前角度
        Quaternion curRoation = transform.rotation;
        //要转到的角度
        Quaternion targetRotation = Quaternion.Slerp(curRoation, curRoation * Quaternion.Euler(this.InputRotation), Constants.RoundSpeed * TimeUtility.DeltaTimeScaled * this.TimeScale);
        //两个角度的差值 力矩
        Quaternion ret = Quaternion.Inverse(curRoation) * targetRotation;


        //记录角度
        if (Mathf.Abs(ret.eulerAngles.y) > 0.1f)
        {
            this.YawAngle = MathUtils.ClampInnerAngle(ret.eulerAngles.y);
        }
        else
        {
            this.YawAngle = 0;
        }

        //结果四舍五入一下
        this.transform.rotation = MathUtils.Round(this.transform.rotation * ret, 1000000);
    }
    private void UpdatePosition()
    {
        //用t的平方可能是之后要与力相乘计算距离
        float deltaTime = this.TimeScale * this.TimeScale * Time.timeScale * TimeUtility.FramerateDeltaTime;
        //计算动画位移，别问我为什么成了力
        Vector3 localMotionForce = this.transform.InverseTransformDirection(this.AnimatorDeltaPosition);
        this.AnimatorDeltaPosition = Vector3.zero;
        //实际方向
        Vector3 motorThrottle = MathUtils.TransformDirection(localMotionForce, this.transform.rotation);

        // MoveDirection += m_ExternalForce * deltaTime + (m_MotorThrottle * (UsingRootMotionPosition ? 1 : deltaTime)) - m_GravityDirection * m_GravityAmount * deltaTime;
        Vector3 MoveDirection = motorThrottle;
        //计算在水平面方向上的碰撞,输出碰撞后的移动值
        this.DeflectHorizontalCollisions(ref MoveDirection);

        this.transform.position = this.transform.position + MoveDirection;

        this.Velocity = (this.transform.position - this.PrevPosition) / (this.TimeScale * Time.deltaTime);
        this.PrevPosition = this.transform.position;
    }
    private void DeflectHorizontalCollisions(ref Vector3 moveDirection)
    {
        //水平偏移量
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
        //移动量不够，则清零世界方向的水平移动
        Vector3 gDir = this.transform.InverseTransformDirection(moveDirection);
        if (horizontalDirection.sqrMagnitude < Constants.ColliderSpacingCubed)
        {
            gDir.x = gDir.z = 0;
            moveDirection = this.transform.TransformDirection(gDir);
            return;
        }
        //检测是否有碰撞
        var hitCount = NonAllocCast(horizontalDirection);
        if (hitCount == 0)
        {
            return;
        }
        float moveDistance = 0f;
        float hitStrength = 0f;
        Vector3 hitMoveDirection = Vector3.zero;
        for (int i = 0; i < hitCount; i++)
        {
            var closestRaycastHit = QuickSelect.SmallestK(this.CombinedRaycastHitsBuffer, hitCount, i, RaycastUtils.RaycastHitComparer);
            int idx = this.ColliderIndexMap[this.CombinedRaycastHitsBuffer[i]];
            Collider starter = this.Colliders[idx];

            //如果距离为0，则两个碰撞器碰撞，需要计算反穿透
            if (closestRaycastHit.distance == 0)
            {
                var offset = Vector3.zero;
                this.ComputePenetration(starter, closestRaycastHit.collider, horizontalDirection, false, out offset);
                if (offset.sqrMagnitude >= Constants.ColliderSpacingCubed)
                {
                    moveDistance = Mathf.Max(0, horizontalDirection.magnitude - offset.magnitude - Constants.ColliderSpacing);
                    moveDirection = Vector3.ProjectOnPlane(horizontalDirection.normalized * moveDistance, Vector3.up)
                                    + Vector3.up * moveDirection.y;
                }
                else
                {
                    moveDirection = Vector3.zero;
                }
                break;
            }

        }

    }
    /// <summary>
    /// 投射检测所有碰撞体
    /// 用来做碰撞预测
    /// </summary>
    /// <param name="direction">投射方向的向量</param>
    /// <returns></returns>
    private int NonAllocCast(Vector3 direction)
    {
        this.ColliderIndexMap.Clear();
        int hitCount = 0;
        for (int i = 0; i < this.Colliders.Count; i++)
        {
            int hitNums;
            if (this.Colliders[i] is CapsuleCollider)
            {
                Vector3 firstEndCap, secondEndCap;
                CapsuleCollider collider = this.Colliders[i] as CapsuleCollider;
                MathUtils.CapsuleColliderEndCaps(collider, collider.transform.position, collider.transform.rotation, out firstEndCap, out secondEndCap);
                //半径
                float radius = collider.radius * MathUtils.ColliderRadiusMultiplier(collider) - Constants.ColliderSpacing;
                hitNums = Physics.CapsuleCastNonAlloc(firstEndCap, secondEndCap, radius, direction.normalized, RaycastHitsBuffer, direction.magnitude + Constants.ColliderSpacing);
            }
            else
            {//只剩下SphereCollider
                SphereCollider collider = this.Colliders[i] as SphereCollider;
                var radius = collider.radius * MathUtils.ColliderRadiusMultiplier(collider) - Constants.ColliderSpacing;
                hitNums = Physics.SphereCastNonAlloc(collider.transform.TransformPoint(collider.center), radius, direction.normalized, RaycastHitsBuffer, direction.magnitude + Constants.ColliderSpacing);
            }
            if (hitNums > 0)
            {
                int validHitCount = 0;
                for (int j = 0; j < hitNums; ++j)
                {
                    if (this.ColliderIndexMap.ContainsKey(RaycastHitsBuffer[j]))
                    {
                        continue;
                    }
                    //检测缓冲数组是否够用
                    if (hitCount + j >= this.CombinedRaycastHitsBuffer.Length)
                    {
                        Debug.LogWarning("投射数组溢出，请分配更多内存");
                        continue;
                    }

                    this.ColliderIndexMap.Add(RaycastHitsBuffer[j], i);
                    this.CombinedRaycastHitsBuffer[hitCount + j] = RaycastHitsBuffer[j];
                    validHitCount += 1;
                }
                hitCount += validHitCount;
            }
        }
        return hitCount;
    }

	/// <summary>
	/// 反穿透封装
	/// </summary>
	/// <param name="firstCollider"></param>
	/// <param name="secondCollider"></param>
	/// <param name="horizontalDirection">水平方向的偏移</param>
	/// <param name="constantVelocity">是否保持速度</param>
	/// <param name="offset">输出offset</param>
	/// <returns>如果为true则表示offset不足以完全反穿透</returns>
    private bool ComputePenetration(Collider firstCollider, Collider secondCollider, Vector3 horizontalDirection, bool constantVelocity, out Vector3 offset)
    {
        var iterations = Constants.MaxOverlapIterations;
        float distance;
        Vector3 direction;
        offset = Vector3.zero;
        var overlap = true;
		//防止没有走overlapCount函数，提前赋值
        this.OverlapColliderBuffer[0] = secondCollider;

        while (iterations > 0)
        {
            if (Physics.ComputePenetration(firstCollider, firstCollider.transform.position + horizontalDirection + offset,
                                                firstCollider.transform.rotation, secondCollider, secondCollider.transform.position, secondCollider.transform.rotation, out direction, out distance))
            {
                offset += direction.normalized * (distance + Constants.ColliderSpacing);
            }
            else
            {
				//如果不需要反穿透，直接返回
                offset = Vector3.zero;
                overlap = false;
                break;
            }
            if (constantVelocity)
            {
                //如果需要保持速度，则原本move的绝对值不变，计算的offset只改变方向不改变模的值。
                offset = (horizontalDirection + offset).normalized * horizontalDirection.magnitude - horizontalDirection;
            }

            //偏移后，检查是水平方向否还有重叠
            if (this.OverlapCount(firstCollider, horizontalDirection + offset) == 0)
            {
                overlap = false;
                break;
            }

            iterations--;
        }
        return overlap;
    }
    /// <summary>
    /// 返回碰撞器的重叠数量
    /// </summary>
    /// <param name="collider">碰撞器</param>
    /// <param name="offset">便宜</param>
    /// <returns></returns>
    protected int OverlapCount(Collider collider, Vector3 offset)
    {
        if (collider is CapsuleCollider)
        {
            Vector3 startEndCap, endEndCap;
            var capsuleCollider = collider as CapsuleCollider;
            MathUtils.CapsuleColliderEndCaps(capsuleCollider, collider.transform.position + offset, collider.transform.rotation, out startEndCap, out endEndCap);
            return Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtils.ColliderRadiusMultiplier(capsuleCollider) - Constants.ColliderSpacing,
                                                    this.OverlapColliderBuffer);
        }
        else
        { // SphereCollider.
            var sphereCollider = collider as SphereCollider;
            return Physics.OverlapSphereNonAlloc(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset,
                                                    sphereCollider.radius * MathUtils.ColliderRadiusMultiplier(sphereCollider) - Constants.ColliderSpacing,
                                                    this.OverlapColliderBuffer);
        }
    }
}
