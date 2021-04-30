using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    private CharacterLocomotion Locomotion;
    private Vector2 direction = Vector2.zero;
    private Vector2[] SmoothLookBuffer;
    private int SmoothLookBufferIndex = 0;
    private int SmoothLookBufferCount = 0;
    private Vector2 CurrentLookVector;
	private void Awake()
    {
        this.SmoothLookBuffer = new Vector2[Constants.SmoothLookSteps];
        this.Locomotion = this.GetComponent<CharacterLocomotion>();
    }
    public Vector2 getDirection()
    {
        return this.direction;
    }

	/// <summary>
	/// 面朝方向
	/// </summary>
	/// <returns></returns>
	public Vector2 getLookVector(){
		return this.CurrentLookVector;
	}

	public bool GetJump(){
		return Input.GetKeyDown(KeyCode.Space);
	}
    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        this.direction.Set(x, y);

    }
    private void FixedUpdate()
    {
        if (!Application.isFocused)
        {
            return;
        }

        this.direction.x = Input.GetAxisRaw("Horizontal");
        this.direction.y = Input.GetAxisRaw("Vertical");

        //将现在的面朝方向的目标存入缓存中
        this.SmoothLookBuffer[SmoothLookBufferIndex].x = this.direction.x;
        this.SmoothLookBuffer[SmoothLookBufferIndex].y = this.direction.y;
        if (SmoothLookBufferCount < SmoothLookBufferIndex + 1)
        {
            SmoothLookBufferCount = SmoothLookBufferIndex + 1;
        }

        // 通过计算连续输入的权重，计算朝向
        var weight = 1f;
        var average = Vector2.zero;
        var averageTotal = 0f;
        var deltaTime = this.Locomotion.TimeScale * TimeUtility.FramerateDeltaTime;
        for (int i = 0; i < SmoothLookBufferCount; ++i)
        {
            var index = SmoothLookBufferIndex - i;
            if (index < 0) { index = SmoothLookBufferCount + SmoothLookBufferIndex - i; }
            average += SmoothLookBuffer[index] * weight;
            averageTotal += weight;
            //unity调试断点暂停，之后点运行会导致deltaTime为0
            if (deltaTime > 0)
            {
                weight *= (Constants.SmoothLookWeight / deltaTime);
            }
        }
        SmoothLookBufferIndex = (SmoothLookBufferIndex + 1) % SmoothLookBuffer.Length;
        averageTotal = Mathf.Max(1, averageTotal);//最小为1
		this.CurrentLookVector = average / averageTotal;
        var lookAcceleration = 0f;
        if (Constants.LookAccelerationThreshold > 0 && deltaTime != 0)
        {
            var accX = Mathf.Abs(this.CurrentLookVector.x);
            var accY = Mathf.Abs(this.CurrentLookVector.y);
            lookAcceleration = Mathf.Sqrt((accX * accX) + (accY * accY)) / deltaTime;
            if (lookAcceleration > Constants.LookAccelerationThreshold)
            {
                lookAcceleration = Constants.LookAccelerationThreshold;
            }
        }

        // 计算最终值
        this.CurrentLookVector.x *= ((Constants.LookSensitivity.x * Constants.LookSensitivityMultiplier) + lookAcceleration) * TimeUtility.FramerateDeltaTime;
        this.CurrentLookVector.y *= ((Constants.LookSensitivity.y * Constants.LookSensitivityMultiplier) + lookAcceleration) * TimeUtility.FramerateDeltaTime;
		//增强对比
        this.CurrentLookVector.x = Mathf.Sign(this.CurrentLookVector.x) * Mathf.Pow(Mathf.Abs(this.CurrentLookVector.x), Constants.SmoothExponent);
        this.CurrentLookVector.y = Mathf.Sign(this.CurrentLookVector.y) * Mathf.Pow(Mathf.Abs(this.CurrentLookVector.y), Constants.SmoothExponent);

    }
}
