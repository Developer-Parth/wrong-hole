using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float lookSensitivity = 0.1f;
    public float gravity = -20f;
    public Transform cameraTransform;
    private CharacterController controller;
    private float xRotation = 0f;
    private float yVelocity;
    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        startPos = transform.position;
        startRot = transform.rotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (transform.position.y < -30f && !GameManager.MissionOver)
        {
            controller.enabled = false;
            transform.position = startPos;
            transform.rotation = startRot;
            controller.enabled = true;
            yVelocity = 0f;
            Debug.Log("PLAYER fell off world, respawned");
        }
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null) return;

        if (mouse != null)
        {
            Vector2 look = mouse.delta.ReadValue() * lookSensitivity;
            transform.Rotate(Vector3.up * look.x);
            xRotation -= look.y;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            if (cameraTransform != null) cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        float mx = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f)
                 + (kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.leftArrowKey.isPressed ? 1f : 0f);
        float mz = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f)
                 + (kb.upArrowKey.isPressed ? 1f : 0f) - (kb.downArrowKey.isPressed ? 1f : 0f);
        Vector3 move = transform.right * mx + transform.forward * mz;
        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        if (kb.pageUpKey.wasPressedThisFrame)
        {
            controller.enabled = false;
            transform.position += transform.forward * 25f;
            controller.enabled = true;
        }

        if (controller.isGrounded && yVelocity < 0f) yVelocity = -2f;
        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move.normalized * speed;
        velocity.y = yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
