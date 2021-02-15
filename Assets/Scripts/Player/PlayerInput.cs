using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{

    private CharacterLocomotion locomotion;
    private Vector2 direction = Vector2.zero;
    private Vector2[] m_SmoothLookBuffer;
    private int m_SmoothLookBufferIndex = 0;
    private int m_SmoothLookBufferCount = 0;
    private Vector2 CurrentLookVector;
    private void Awake()
    {
        this.m_SmoothLookBuffer = new Vector2[Constants.SmoothLookSteps];
        this.locomotion = this.GetComponent<CharacterLocomotion>();
    }
    public Vector2 getDirection()
    {
        return this.direction;
    }
	public Vector2 getLookVector(){
		return this.CurrentLookVector;
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

        //将现在的面朝方向存入缓存中
        this.m_SmoothLookBuffer[m_SmoothLookBufferIndex].x = this.direction.x;
        this.m_SmoothLookBuffer[m_SmoothLookBufferIndex].y = this.direction.y;
        if (m_SmoothLookBufferCount < m_SmoothLookBufferIndex + 1)
        {
            m_SmoothLookBufferCount = m_SmoothLookBufferIndex + 1;
        }

        // Calculate the input smoothing value. The more recent the input value occurred the higher the influence it has on the final smoothing value.
        var weight = 1f;
        var average = Vector2.zero;
        var averageTotal = 0f;
        var deltaTime = this.locomotion.TimeScale * TimeUtility.FramerateDeltaTime;
        for (int i = 0; i < m_SmoothLookBufferCount; ++i)
        {
            var index = m_SmoothLookBufferIndex - i;
            if (index < 0) { index = m_SmoothLookBufferCount + m_SmoothLookBufferIndex - i; }
            average += m_SmoothLookBuffer[index] * weight;
            averageTotal += weight;
            // The deltaTime will be 0 if Unity just started to play after stepping through the editor.
            if (deltaTime > 0)
            {
                weight *= (Constants.SmoothLookWeight / deltaTime);
            }
        }
        m_SmoothLookBufferIndex = (m_SmoothLookBufferIndex + 1) % m_SmoothLookBuffer.Length;
        // Store the averaged input value.
        averageTotal = Mathf.Max(1, averageTotal);
		this.CurrentLookVector = average / averageTotal;
        // Apply any look acceleration. The delta time will be zero on the very first frame.
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
