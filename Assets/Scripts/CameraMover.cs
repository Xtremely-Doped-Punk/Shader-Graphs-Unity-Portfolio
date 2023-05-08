using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UnityEngine.InputSystem;

public class CameraMover : MonoBehaviour
{
    [SerializeField] float speed;
    const float k_InputMoveSenstivity = 1f; // new input system, .11f would suit better
    const float k_MouseSensitivityMultiplier = 0.1f; // new input system, 0.01f would suit better
    [SerializeField] float mouseSensitivity = 100f;
    [SerializeField] Camera cam;

    public bool AllowMovement = true;
    Transform camHolderX = null;
    Transform camHolderY = null;
    public void SetHolders (Transform holderX, Transform holderY)
    {
        camHolderX = holderX;
        camHolderY = holderY;
    }
    public void RemoveHolders ()
    {
        camHolderX = null;
        camHolderY = null;
    }

    //InputAction movement;
    float yRotation = 0f;
    float xRotation = 0f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        /*
        movement = new InputAction("PlayerMovement", binding: "<Gamepad>/leftStick");
        movement.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        movement.Enable();
        */
    }

    void Update()
    {
        bool unlockPressed = //Keyboard.current.escapeKey.wasPressedThisFrame;
            Input.GetKeyDown(KeyCode.Escape);
        bool lockPressed = //Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame;
            Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);

        // move
        //var moveDT = movement.ReadValue<Vector2>();
        if (AllowMovement)
        {
            var move = //cam.transform.right * moveDT.x + cam.transform.forward * moveDT.y;
                cam.transform.right * Input.GetAxisRaw("Horizontal") + cam.transform.forward * Input.GetAxisRaw("Vertical");
            cam.transform.position += speed * k_InputMoveSenstivity * Time.deltaTime * move;
        }

        if (unlockPressed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (lockPressed)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.None)
        {
            // look
            var lookDT = mouseSensitivity * k_MouseSensitivityMultiplier *
                //Mouse.current.delta.ReadValue()
                new Vector2 (Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            yRotation -= lookDT.y;
            xRotation += lookDT.x;
            yRotation = Mathf.Clamp(yRotation, -90f, 90f);

            cam.transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0f);

            if (camHolderX != null) camHolderX.localRotation = 
                    Quaternion.Euler(camHolderX.localEulerAngles.x, xRotation, camHolderX.localEulerAngles.z);

            if (camHolderY != null) camHolderY.localRotation =
                    Quaternion.Euler(yRotation, camHolderY.localEulerAngles.y, camHolderY.localEulerAngles.z);
        }
    }
}
