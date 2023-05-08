using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] FPCamHolder FPCam;
    
    [SerializeField] float _walkSpeed, _sprintBoostSpeed;
    [SerializeField, Range(0, 1)] float _smoothTime = .15f;
    bool CheckEnable => FPCam.isEnabled;
    bool canUpdate = false;

    Vector3 smoothMoveVelocity; Vector3 moveAmount;

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (canUpdate != CheckEnable) canUpdate = CheckEnable;
        
        if (!CheckEnable)
        {
            return;
        }

        Movement();
    }

    private void Movement()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(horizontal, 0, vertical).normalized;

        var shift = Input.GetKey(KeyCode.LeftShift);
        var speed = _walkSpeed + (shift ? _sprintBoostSpeed : 0);

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * speed, ref smoothMoveVelocity, _smoothTime);
    }

    private void FixedUpdate()
    {
        if (!canUpdate) return;

        rb.MovePosition(rb.position 
            + transform.TransformDirection(moveAmount) // convert to local space dir vec
            * Time.fixedDeltaTime);
    }
}
