using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
    public Vector3 movement;
        

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.z = Input.GetAxisRaw("Vertical");
    }
}
