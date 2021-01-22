using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
    private Vector2 direction = Vector2.zero;
	public Vector2 getDirection()
	{
		return this.direction;
	}

     private void Update() 
     {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
		this.direction.Set(x, y);
	}
}
