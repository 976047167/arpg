using UnityEngine;
public static class Constants
{
    public static float SpeedAcceleration = 1.0f;
    public static float RoundSpeed = 10f;
    public static float DampingTime = 0.1f;
    public static float YawMultiplier = 7;

    public static int SmoothLookSteps = 20;

    public static Vector2 LookSensitivity = new Vector2(2f, 2f);
    public static float LookSensitivityMultiplier = 1;
    public static float SmoothLookWeight = 0.5f;
    public static float SmoothExponent = 1.05f;
    public static float LookAccelerationThreshold = 0.4f;
	/// <summary>
	/// 预留间隙，如果两个碰撞体真的重叠，就没法用投射了
	/// </summary>
    public static float ColliderSpacing = 0.01f;
    public static float ColliderSpacingCubed = ColliderSpacing * ColliderSpacing * ColliderSpacing * ColliderSpacing;
    public static float MaxStepHeight = 0.35f;
    public static float SlopeLimit = 40f;
    public static float SlopeLimitSpacing = 0.3f;
    public static int MaxOverlapIterations = 6;
	/// <summary>
	/// 空白raycast，用来清空raycast用
	/// </summary>
	/// <returns></returns>
	public static RaycastHit BlankRaycastHit = new RaycastHit();

	public static float MotorSlopeForceDown = 1.25f;
	public static float MotorSlopeForceUp = 1f;
	public static float GravityMagnitude = 3.92f;

}
public static class TimeUtility
{
    public static float DeltaTimeScaled
    {
        get { return Time.deltaTime * Time.timeScale; }
    }
    public static float FramerateDeltaTime
    {
        get { return Time.deltaTime * TargetFramerate; }
    }
    private const int TargetFramerate = 60;


}
public static class AnimatorHash
{
    public static int Index = Animator.StringToHash("Index");
    public static int IndexTrigger = Animator.StringToHash("IndexTrigger");
    public static int ArgInt = Animator.StringToHash("ArgInt");
    public static int ArgFloat = Animator.StringToHash("ArgFloat");
    public static int HorizontalMovementHash = Animator.StringToHash("HorizontalMovement");
    public static int ForwardMovementHash = Animator.StringToHash("ForwardMovement");
    public static int MovingHash = Animator.StringToHash("Moving");
    public static int YawHash = Animator.StringToHash("Yaw");
    public static int OnCharacterGroundedHash = Animator.StringToHash("OnCharacterGrounded");

}
public static class GameEvent
{
    public static int OnMoving = "onMoving".GetHashCode();
    public static int AnimationEvent = "AnimationEvent".GetHashCode();
}