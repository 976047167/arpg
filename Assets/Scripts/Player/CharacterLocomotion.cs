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
	/// <summary>
	/// 用来计算的质量值，不是给刚体组件用的
	/// </summary>
	private float Mass = 100;

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
	private Vector3 MotorThrottle;
	public float YawAngle;
	/// <summary>
	/// 碰撞体的位置
	/// </summary>
	private List<Collider> Colliders;
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
	/// <summary>
	/// 是否在地面
	/// </summary>
	private bool grounded;
	/// <summary>
	/// 射向地面的射线碰撞结果
	/// </summary>
	private RaycastHit GroundRaycastHit;
	/// <summary>
	///  GroundRaycastHit的起点
	/// </summary>
	private Vector3 GroundRaycastOrigin;
	/// <summary>
	/// 角色接地的物体
	/// </summary>
	private Transform GroundHitTransform;
	private float SlopeFactor = 1.0f;
	private float SkinWidth = 0.08f;
	//减少gc用的变量，通常是Singlecast的结果值
	private RaycastHit RaycastHit;
	//用来手动计算击中法线的变量
	private Ray FixRay = new Ray();
	private float Radius;
	/// <summary>
	/// 重力参数，决定下落的速度
	/// 如果在空中，此数值会逐渐增大
	/// </summary>
	private float GravityAmount;
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
			//触发器不要
			if (!colliders[i].enabled || colliders[i].isTrigger)
			{
				continue;
			}
			//只计算球碰撞和胶囊体碰撞，其他碰撞暂时忽略
			if (!(colliders[i] is CapsuleCollider || colliders[i] is SphereCollider))
			{
				continue;
			}
			//todo:后期加一个mask检测，只检测身体的，忽略武器的碰撞
			//身体有多个检测碰撞时，取最小值半径。
			var radius = float.MaxValue;
			if (colliders[i] is CapsuleCollider)
			{
				radius = (colliders[i] as CapsuleCollider).radius;
			}
			else
			{ // SphereCollider.
				radius = (colliders[i] as SphereCollider).radius;
			}
			if (radius < this.Radius)
			{
				this.Radius = radius;
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

	private void OnAnimatorMove()
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
		//用t的平方是之后要与力相乘计算距离
		float deltaTime = this.TimeScale * this.TimeScale * Time.timeScale * TimeUtility.FramerateDeltaTime;
		//计算动画位移
		Vector3 localMotionForce = this.transform.InverseTransformDirection(this.AnimatorDeltaPosition);
		this.AnimatorDeltaPosition = Vector3.zero;
		//实际方向
		this.MotorThrottle = this.transform.TransformDirection(localMotionForce) * this.SlopeFactor;

		// MoveDirection += m_ExternalForce * deltaTime + (m_MotorThrottle * (UsingRootMotionPosition ? 1 : deltaTime)) - m_GravityDirection * m_GravityAmount * deltaTime;
		Vector3 moveDirection = this.MotorThrottle - Vector3.down * this.GravityAmount * deltaTime; ;
		//计算在水平面方向上的碰撞,输出碰撞后的移动值
		this.DeflectHorizontalCollisions(ref moveDirection);
		//计算在垂直面方向上的碰撞,输出碰撞后的移动值
		this.DeflectVerticalCollisions(ref moveDirection);

		this.transform.position = this.transform.position + moveDirection;

		this.Velocity = (this.transform.position - this.PrevPosition) / (this.TimeScale * Time.deltaTime);
		this.PrevPosition = this.transform.position;
	}
	private void DeflectHorizontalCollisions(ref Vector3 moveDirection)
	{
		//水平偏移量
		Vector3 horizontalDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
		//移动量不够，则清零本地坐标方向的水平移动
		Vector3 localDir = this.transform.InverseTransformDirection(moveDirection);
		if (horizontalDirection.sqrMagnitude < Constants.ColliderSpacingCubed)
		{
			localDir.x = localDir.z = 0;
			moveDirection = this.transform.TransformDirection(localDir);
			return;
		}
		//检测是否有碰撞
		var hitCount = this.NonAllocCast(horizontalDirection, Vector3.zero);
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
			Collider origin = this.Colliders[idx];

			//如果距离为0，则两个碰撞器重叠，可能是速度过快产生穿透，需要计算反穿透
			//否则就Constants.ColliderSpacing中，可以计算力的相互作用
			if (closestRaycastHit.distance == 0)
			{
				var offset = Vector3.zero;
				this.ComputePenetration(origin, closestRaycastHit.collider, horizontalDirection, false, out offset);
				if (offset.sqrMagnitude >= Constants.ColliderSpacingCubed)
				{
					moveDistance = Mathf.Max(0, horizontalDirection.magnitude - offset.magnitude - Constants.ColliderSpacing);
					moveDirection = Vector3.ProjectOnPlane(horizontalDirection.normalized * moveDistance, Vector3.up)
									+ Vector3.up * localDir.y;
				}
				else
				{
					moveDirection = Vector3.zero;
				}
				//计算过反穿透后，就不必计算后面的物理量
				break;
			}
			//如果第一个不是穿透，则后面的循环就全部都不是穿透（因为它最短）。

			// 对其他刚体施加力,检测是否推动
			GameObject hitGameObject = closestRaycastHit.transform.gameObject;
			Rigidbody hitRigidbody = hitGameObject.GetComponentInParent<Rigidbody>();
			bool canStep = true;
			if (hitRigidbody != null)
			{
				var radius = (origin is CapsuleCollider ?
								((origin as CapsuleCollider).radius * MathUtils.ColliderRadiusMultiplier(origin as CapsuleCollider)) :
								((origin as SphereCollider).radius * MathUtils.ColliderRadiusMultiplier(origin)));
				canStep = !PushRigidbody(hitRigidbody, horizontalDirection, closestRaycastHit.point, radius);
			}
			//如果没有推动，说明可能是斜坡或者阶梯
			//如果是斜坡或者阶梯，不必做物理计算
			if (canStep && this.grounded)
			{
				// 计算移动点的斜坡高度是否可以上坡
				var groundPoint = this.transform.InverseTransformPoint(closestRaycastHit.point);
				if (groundPoint.y <= Constants.MaxStepHeight + Constants.ColliderSpacing)
				{
					//有个bug，如果射线击中球体边缘，raycasthit的法线方向会有问题，这里手动计算法线。
					this.FixRay.direction = horizontalDirection.normalized;
					this.FixRay.origin = closestRaycastHit.point - this.FixRay.direction * (Constants.ColliderSpacing + 0.1f);
					if (!Physics.Raycast(this.FixRay, out this.RaycastHit, (Constants.ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore))
					{
						this.RaycastHit = closestRaycastHit;
					}
					//斜率
					var slope = Vector3.Angle(Vector3.up, this.RaycastHit.normal);
					if (slope <= Constants.SlopeLimit + Constants.SlopeLimitSpacing)
					{
						//斜率够小(斜坡的情况)，可以直接上
						continue;
					}

					//如果斜率过大，可能是台阶
					//用高一点的射线检测碰撞，如果没碰到，或者长度更长，说明是个台阶
					if (SingleCast(origin, horizontalDirection, (Constants.MaxStepHeight - Constants.ColliderSpacing) * Vector3.up))
					{
						if ((this.RaycastHit.distance - Constants.ColliderSpacing) < horizontalDirection.magnitude)
						{
							//是台阶的情况
							this.FixRay.direction = horizontalDirection.normalized;
							this.FixRay.origin = closestRaycastHit.point - this.FixRay.direction * (Constants.ColliderSpacing + 0.1f);
							var normal = this.RaycastHit.normal;
							if (Physics.Raycast(this.FixRay, out this.RaycastHit, (Constants.ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore))
							{
								normal = this.RaycastHit.normal;
							}
							//计算台阶的斜率
							slope = Vector3.Angle(Vector3.up, normal);
							if (slope <= Constants.SlopeLimit + Constants.SlopeLimitSpacing)
							{
								continue;
							}
						}
					}
					else
					{
						//就一层的台阶，直接上
						groundPoint.y = 0;
						groundPoint = this.transform.TransformPoint(groundPoint);
						var direction = groundPoint - this.transform.position;
						if (this.OverlapCount(origin, (direction.normalized * (direction.magnitude + this.Radius * 0.5f)) + (Constants.MaxStepHeight - Constants.ColliderSpacing) * Vector3.up) == 0)
						{
							continue;
						}
					}

				}
			}
			// 不是斜坡的情况，碰到墙壁
			// 如果碰到墙壁，应该沿着墙的方向移动
			var hitNormal = Vector3.ProjectOnPlane(closestRaycastHit.normal, Vector3.up).normalized;
			var targetDirection = Vector3.Cross(hitNormal, Vector3.up).normalized;
			var closestPoint = MathUtils.ClosestPointOnCollider(this.transform, origin, closestRaycastHit.point, moveDirection, true, false);
			if ((Vector3.Dot(Vector3.Cross(Vector3.ProjectOnPlane(this.transform.position - closestPoint, Vector3.up).normalized, horizontalDirection).normalized, Vector3.up)) > 0)
			{
				targetDirection = -targetDirection;
			}
			//碰撞的情况下“动方向的距离“和“碰撞预留间隙“的和必然大于等于“投射距离“,即horizontalDirection.magnitudes+ Constants.ColliderSpacing >= closestRaycastHit.distance。
			//但是如果物体够小，在胶囊体下方，投射距离就会可能大于运动距离,即closestRaycastHit.distance > horizontalDirection.magnitudes。
			//这种情况也视为碰撞(移动后两物体实际距离在预留间隙中)，取较小值为移动距离
			moveDistance = Mathf.Min(closestRaycastHit.distance - Constants.ColliderSpacing, horizontalDirection.magnitude);
			if (moveDistance < 0.001f || Vector3.Angle(Vector3.up, this.GroundRaycastHit.normal) > Constants.SlopeLimit + Constants.SlopeLimitSpacing)
			{
				moveDistance = 0;
			}
			//根据物理材质计算摩擦力
			var dynamicFrictionValue = Mathf.Clamp01(1 - MathUtils.FrictionValue(origin.material, closestRaycastHit.collider.material, true));
			//力量系数为1-cos值
			hitStrength = 1 - Vector3.Dot(horizontalDirection.normalized, -hitNormal);
			hitMoveDirection = targetDirection * (horizontalDirection.magnitude - moveDistance) * (hitStrength < 0.1f ? 0 : 1.5f) * dynamicFrictionValue;
			if (hitMoveDirection.magnitude <= Constants.ColliderSpacing)
			{
				hitMoveDirection = Vector3.zero;
				hitStrength = 0;
			}
			moveDirection = (horizontalDirection.normalized * moveDistance) + hitMoveDirection + Vector3.up * localDir.y;
			break;
		}
		this.ResetCombinedRaycastHits();

		//在墙角可能被挤出去
		//做第二次检测，确保被第一次弹出的方向的位置是可以使用的
		if (hitStrength > 0.0001f)
		{
			hitCount = this.NonAllocCast(hitMoveDirection, Vector3.zero);
			for (int i = 0; i < hitCount; ++i)
			{
				var closestRaycastHit = QuickSelect.SmallestK(this.CombinedRaycastHitsBuffer, hitCount, i, RaycastUtils.RaycastHitComparer);
				int idx = this.ColliderIndexMap[this.CombinedRaycastHitsBuffer[i]];
				Collider origin = this.Colliders[idx];
				if (this.grounded)
				{
					var groundPoint = this.transform.InverseTransformPoint(closestRaycastHit.point);
					if (groundPoint.y > Constants.ColliderSpacing && groundPoint.y <= Constants.MaxStepHeight + Constants.ColliderSpacing)
					{
						var hitGameObject = closestRaycastHit.transform.gameObject;
						this.FixRay.direction = hitMoveDirection.normalized;
						this.FixRay.origin = closestRaycastHit.point - this.FixRay.direction * (Constants.ColliderSpacing + 0.1f);
						if (!Physics.Raycast(this.FixRay, out this.RaycastHit, (Constants.ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore))
						{
							this.RaycastHit = closestRaycastHit;
						}
						var slope = Vector3.Angle(Vector3.up, this.RaycastHit.normal);
						if (slope <= Constants.SlopeLimit + Constants.SlopeLimitSpacing)
						{
							continue;
						}

						if (SingleCast(origin, hitMoveDirection, (Constants.MaxStepHeight - Constants.ColliderSpacing) * Vector3.up))
						{
							if ((this.RaycastHit.distance - Constants.ColliderSpacing) < hitMoveDirection.magnitude)
							{
								this.FixRay.direction = hitMoveDirection.normalized;
								this.FixRay.origin = closestRaycastHit.point - this.FixRay.direction * (Constants.ColliderSpacing + 0.1f);
								var normal = this.RaycastHit.normal;
								if (Physics.Raycast(this.FixRay, out this.RaycastHit, (Constants.ColliderSpacing + 0.11f), 1 << hitGameObject.layer, QueryTriggerInteraction.Ignore))
								{
									normal = this.RaycastHit.normal;
								}
								//计算台阶的斜率
								slope = Vector3.Angle(Vector3.up, normal);
								if (slope <= Constants.SlopeLimit + Constants.SlopeLimitSpacing)
								{
									continue;
								}
							}
						}
						else
						{
							groundPoint.y = 0;
							groundPoint = this.transform.TransformPoint(groundPoint);
							var direction = groundPoint - this.transform.position;
							if (OverlapCount(origin, (direction.normalized * (direction.magnitude + this.Radius * 0.5f)) + Vector3.up * (Constants.MaxStepHeight - Constants.ColliderSpacing)) == 0)
							{
								continue;
							}
						}
					}
				}

				//计算第一次和第二次的联合角度和距离
				var moveDistanceContribution = moveDistance * Mathf.Cos(Vector3.Angle(horizontalDirection.normalized, hitMoveDirection.normalized) * Mathf.Deg2Rad);
				var hitMoveDistance = Mathf.Min(closestRaycastHit.distance - moveDistanceContribution - hitMoveDirection.magnitude - Constants.ColliderSpacing, hitMoveDirection.magnitude);
				if (hitMoveDistance < 0.001f || Vector3.Angle(Vector3.up, this.GroundRaycastHit.normal) > Constants.SlopeLimit + Constants.SlopeLimitSpacing)
				{
					hitMoveDistance = 0;
				}

				moveDirection = (horizontalDirection.normalized * moveDistance) + hitMoveDirection.normalized * hitMoveDistance + Vector3.up * localDir.y;
				break;
			}
			this.ResetCombinedRaycastHits();
		}
	}

	private void DeflectVerticalCollisions(ref Vector3 moveDirection)
	{
		//本地坐标
		var localMoveDirection = this.transform.InverseTransformDirection(moveDirection);
		if (localMoveDirection.y > 0)
		{
			var horizontalDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
			var hitCount = this.NonAllocCast(Vector3.up * (localMoveDirection.y + Constants.ColliderSpacing), horizontalDirection);
			if (hitCount > 0)
			{
				//垂直方向不存在台阶斜坡墙壁之类的需要额外计算，所以只要计算反穿透即可
				var closestRaycastHit = QuickSelect.SmallestK(this.CombinedRaycastHitsBuffer, hitCount, 0, RaycastUtils.RaycastHitComparer);
				if (closestRaycastHit.distance == 0)
				{
					//同水平方向一样，先进行反穿透处理
					int idx = this.ColliderIndexMap[closestRaycastHit];
					Collider origin = this.Colliders[idx];
					//先尝试保持速度能否出来
					var overlap = this.ComputePenetration(origin, closestRaycastHit.collider, horizontalDirection, true, out var offset);
					if (overlap)
					{
						//不行就直接弹出
						overlap = this.ComputePenetration(origin, closestRaycastHit.collider, horizontalDirection, false, out offset);
					}
					if (!overlap)
					{
						localMoveDirection.y = this.transform.InverseTransformDirection(offset).y;
					}
					else
					{
						localMoveDirection.y = 0;
					}
				}
				else
				{
					//不穿透，就停在碰撞器前
					localMoveDirection.y = Mathf.Max(0, closestRaycastHit.distance - Constants.ColliderSpacing);
				}
				//输出结果
				moveDirection = this.transform.TransformDirection(localMoveDirection);
			}
		}
		//向下投射,地面检测
		var grounded = this.CheckGround(ref moveDirection);
		localMoveDirection = this.transform.InverseTransformDirection(moveDirection);

		//计算重力参数
		var accumulateGravity = true;
		var verticalOffset = 0f;
		if (this.GroundRaycastHit.distance != 0)
		{
			//负向离地高度、需要角色垂直移动的长度。
			verticalOffset = this.transform.InverseTransformDirection(this.GroundRaycastHit.point - this.GroundRaycastOrigin).y + Constants.ColliderSpacing;
			if (Mathf.Abs(verticalOffset) < 0.0001f)
			{
				verticalOffset = 0;
			}
			//在地面或者落地、起跳的瞬间
			if ((this.grounded || grounded) &&
			localMoveDirection.y < this.SkinWidth &&
			verticalOffset > -Constants.ColliderSpacing)
			{
				//进行移动
				localMoveDirection.y += verticalOffset;
				//防止角色穿过地面
				if (localMoveDirection.y < -this.GroundRaycastHit.distance)
				{
					localMoveDirection.y = -this.GroundRaycastHit.distance + Constants.ColliderSpacing;
				}
				accumulateGravity = false;
				grounded = true;
			}
		}

		//位移结果
		moveDirection = this.transform.TransformDirection(localMoveDirection);

		if (accumulateGravity)
		{ 
			//如果角色再空中，增大重力参数，使其在下一帧下落更快
			this.GravityAmount += (Constants.GravityMagnitude * -0.001f) / Time.timeScale;
		}
		else if (grounded)
		{ 
			//向下施加人物的重力
			if (this.GroundRaycastHit.rigidbody != null)
			{
				this.GroundRaycastHit.rigidbody.AddForceAtPosition(Vector3.down * ((this.Mass / this.GroundRaycastHit.rigidbody.mass) + this.GravityAmount) / Time.deltaTime, this.transform.position + moveDirection, ForceMode.Force);
			}

			//落地后重力参数归0
			this.GravityAmount = 0;
		}

		this.grounded = grounded;
	}

	/// <summary>
	/// 检测角色运动后是否落地
	/// 不改变this.grounded的值
	/// 由垂直碰撞去改变（因为有重力计算）
	/// </summary>
	/// <returns></returns>
	private bool CheckGround(ref Vector3 moveDirection)
	{
		this.GroundRaycastHit = Constants.BlankRaycastHit;

		var verticalMoveDirection = this.transform.InverseTransformDirection(moveDirection).y;
		var horizontalDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
		//向下投射检测
		var hitCount = this.NonAllocCast((Mathf.Abs(verticalMoveDirection) + Constants.MaxStepHeight + Constants.ColliderSpacing) * Vector3.down, horizontalDirection);
		var grounded = false;
		var closestRaycastHit = QuickSelect.SmallestK(this.CombinedRaycastHitsBuffer, hitCount, 0, RaycastUtils.RaycastHitComparer);
		var activeCollider = this.Colliders[this.ColliderIndexMap[closestRaycastHit]];
		//计算重叠
		if (closestRaycastHit.distance == 0)
		{
			var horizontalStep = false;
			//如果水平方向有移动的话，就检测台阶
			if (horizontalDirection.sqrMagnitude >= 0.000001f)
			{
				if (this.SingleCast(activeCollider, horizontalDirection, Vector3.zero))
				{
					//计算击中点的相对位置
					var groundPoint = this.transform.InverseTransformPoint(this.RaycastHit.point);
					if (groundPoint.y <= Constants.MaxStepHeight + Constants.ColliderSpacing)
					{
						//如果移动方向可以踩上去，且踩上去后没有碰撞，则是在台阶的情况（台阶范围内碰撞，都算接地）
						if (this.OverlapCount(activeCollider, horizontalDirection + Vector3.up * (Constants.MaxStepHeight - Constants.ColliderSpacing)) == 0)
						{
							horizontalStep = true;
						}
					}
				}
			}
			var offset = Vector3.zero;
			// 上台阶的情况下必定要计算新位移，所以跳过反穿透计算，减少计算量
			var overlap = !horizontalStep && this.ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalDirection, true, out offset);
			if (overlap)
			{
				overlap = this.ComputePenetration(activeCollider, closestRaycastHit.collider, horizontalDirection, false, out offset);
			}

			//如果要上台阶，需要计算新位移
			if (!overlap)
			{
				//通过反穿透的值，计算出地面的位置
				var localOffset = this.transform.InverseTransformDirection(offset);
				var verticalOffset = 0f;//地面的位置
										//垂直方向上的反穿透先存起来，后面(在计算垂直碰撞时)统一计算
				if (localOffset.y > 0)
				{
					verticalOffset = localOffset.y;
					localOffset.y = 0;
				}
				//先根据反穿透量移动
				moveDirection += this.transform.TransformDirection(localOffset);
				//计算台阶
				//角色反穿透水平移动后，加上一层台阶，向下投射，击中点是移动后的新的地面高度
				if (this.SingleCast(activeCollider, Vector3.down * (Constants.MaxStepHeight + verticalOffset + Constants.ColliderSpacing),
									Vector3.ProjectOnPlane(moveDirection, Vector3.up) + this.transform.up * (Constants.MaxStepHeight + verticalOffset + Constants.ColliderSpacing)))
				{
					//地面射线为踩上一阶台阶后向下投射的射线(为了赋给GroundRaycastHit)
					closestRaycastHit = this.RaycastHit;
				}
			}
			else
			{
				// 进行了反穿透后还有重叠，这种情况下停止移动
				moveDirection = Vector3.zero;
			}
		}
		//如果没碰撞，就是向下的射线，如果碰撞检测了，就是移动一层台阶再向下射
		this.GroundRaycastHit = closestRaycastHit;
		//投射的起点（最后一个参数为ture，否则一半以下都陷地的话，最近点会在上方，应当为下方的点）
		//这个函数返回值不一定是最近点，当point在球体部分（胶囊体内部的球体也算）的下半部时，点的位置为（y=point向下与球的交点,xz=球心）
		this.GroundRaycastOrigin = MathUtils.ClosestPointOnCollider(this.transform, activeCollider, this.GroundRaycastHit.point, moveDirection, false, true);
		//距离地面的向量
		var lenDir = this.transform.InverseTransformDirection(this.GroundRaycastHit.point - this.GroundRaycastOrigin);
		//长度在皮肤宽度内，接地(实际陷地)，否则悬空
		grounded = lenDir.y >= -this.SkinWidth;
		this.ResetCombinedRaycastHits();

		//更新接地体
		if (grounded != this.grounded || this.GroundRaycastHit.transform != this.GroundHitTransform)
		{
			this.GroundHitTransform = grounded ? this.GroundRaycastHit.transform : null;
		}
		//更新斜坡缓速
		this.UpdateSlopeFactor();

		return grounded;
	}
	/// <summary>
	/// 斜坡缓速
	/// </summary>
	private void UpdateSlopeFactor()
	{
		if (!this.grounded)
		{
			this.SlopeFactor = 1;
			return;
		}

		this.SlopeFactor = 1 + (1 - (Vector3.Angle(this.GroundRaycastHit.normal, this.MotorThrottle) / 90));

		if (Mathf.Abs(1 - this.SlopeFactor) < 0.01f)
		{
			this.SlopeFactor = 1;
		}
		else if (this.SlopeFactor > 1)
		{
			//下坡
			this.SlopeFactor = Constants.MotorSlopeForceDown / this.SlopeFactor;
		}
		else
		{
			//上坡
			this.SlopeFactor *= Constants.MotorSlopeForceUp;
		}
	}



	/// <summary>
	/// 投射检测所有碰撞体
	/// 用来做碰撞预测
	/// </summary>
	/// <param name="direction"></param>
	/// <param name="direction">投射方向的向量</param>
	/// <param name="offset">位置上的偏移量</param>
	/// <returns></returns>
	private int NonAllocCast(Vector3 direction, Vector3 offset)
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
				MathUtils.CapsuleColliderEndCaps(collider, collider.transform.position + offset, collider.transform.rotation, out firstEndCap, out secondEndCap);
				//半径
				float radius = collider.radius * MathUtils.ColliderRadiusMultiplier(collider) - Constants.ColliderSpacing;
				hitNums = Physics.CapsuleCastNonAlloc(firstEndCap, secondEndCap, radius, direction.normalized, RaycastHitsBuffer, direction.magnitude + Constants.ColliderSpacing);
			}
			else
			{//只剩下SphereCollider
				SphereCollider collider = this.Colliders[i] as SphereCollider;
				var radius = collider.radius * MathUtils.ColliderRadiusMultiplier(collider) - Constants.ColliderSpacing;
				hitNums = Physics.SphereCastNonAlloc(collider.transform.TransformPoint(collider.center) + offset, radius, direction.normalized, RaycastHitsBuffer, direction.magnitude + Constants.ColliderSpacing);
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
	/// <param name="offset">偏移</param>
	/// <returns></returns>
	private int OverlapCount(Collider collider, Vector3 offset)
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
	/// <summary>
	/// 推动刚体
	/// </summary>
	/// <param name="targetRigidbody"></param>
	/// <param name="moveDirection"></param>
	/// <param name="point"></param>
	/// <param name="radius"></param>
	/// <returns>是否推动</returns>
	private bool PushRigidbody(Rigidbody targetRigidbody, Vector3 moveDirection, Vector3 point, float radius)
	{
		if (targetRigidbody.isKinematic)
		{
			return false;
		}

		targetRigidbody.AddForceAtPosition((moveDirection / Time.deltaTime) * (this.Mass / targetRigidbody.mass) * 0.01f, point, ForceMode.VelocityChange);
		return targetRigidbody.velocity.sqrMagnitude > 0.1f;
	}
	/// <summary>
	/// 射线碰撞检测
	/// </summary>
	/// <param name="collider"></param>
	/// <param name="direction"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	private bool SingleCast(Collider collider, Vector3 direction, Vector3 offset)
	{
		if (collider is CapsuleCollider)
		{
			Vector3 startEndCap, endEndCap;
			var capsuleCollider = collider as CapsuleCollider;
			MathUtils.CapsuleColliderEndCaps(capsuleCollider, capsuleCollider.transform.position + offset, capsuleCollider.transform.rotation, out startEndCap, out endEndCap);
			var radius = capsuleCollider.radius * MathUtils.ColliderRadiusMultiplier(capsuleCollider) - Constants.ColliderSpacing;
			return Physics.CapsuleCast(startEndCap, endEndCap, radius, direction.normalized, out this.RaycastHit, direction.magnitude + Constants.ColliderSpacing);
		}
		else
		{ // SphereCollider.
			var sphereCollider = collider as SphereCollider;
			var radius = sphereCollider.radius * MathUtils.ColliderRadiusMultiplier(sphereCollider) - Constants.ColliderSpacing;
			return Physics.SphereCast(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
															out this.RaycastHit, direction.magnitude + Constants.ColliderSpacing);
		}
	}
	/// <summary>
	/// 清空buffer，一般和NonAllocCast()成对使用
	/// </summary>
	private void ResetCombinedRaycastHits()
	{
		if (this.CombinedRaycastHitsBuffer == null)
		{
			return;
		}

		for (int i = 0; i < this.CombinedRaycastHitsBuffer.Length; ++i)
		{
			if (this.CombinedRaycastHitsBuffer[i].collider == null)
			{
				break;
			}

			this.CombinedRaycastHitsBuffer[i] = Constants.BlankRaycastHit;
		}
	}
}
