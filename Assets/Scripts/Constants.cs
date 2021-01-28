using UnityEngine;
public static class Constants
{
	public static float SpeedAcceleration = 1.0f;
	public static float RoundSpeed = 0.1f;
}
public static class AnimatorHash
{
	public static int Index = Animator.StringToHash("Index");
	public static int IndexTrigger = Animator.StringToHash("IndexTrigger");
	public static int ArgInt = Animator.StringToHash("ArgInt");
	public static int ArgFloat = Animator.StringToHash("ArgFloat");
}
public static class GameEvent{
	public static int OnMoving ="onMoving".GetHashCode();
	public static int AnimationEvent ="AnimationEvent".GetHashCode();
}