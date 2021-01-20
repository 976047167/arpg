using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
    private Vector3 direction = Vector3.zero;
	public Vector3 getVector()
	{
		return this.direction;
	}

     private void Update() 
     {
        direction.x = Input.GetAxisRaw("Horizontal");
        direction.z = Input.GetAxisRaw("Vertical");
    }
}
