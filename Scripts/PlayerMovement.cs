using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public AudioClip footStepSFX;
    private Rigidbody rb;
    private Vector2 movementInput;
    private bool isGrounded;
    private @PlayerInput playerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new @PlayerInput();
        playerInput.Enable();
        StartCoroutine(PlayFootStep());
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    void OnJump()
    {
        if (isGrounded) 
        {
            rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
        }
    }
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }
    void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }
    void MovePlayer()
    {
        Vector3 direction = transform.right * movementInput.x + transform.forward * movementInput.y;
        direction.Normalize();
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
    }
    void OnSprint()
    {
        moveSpeed = 15f;
    }
    void OnSprintRelease()
    {
        moveSpeed = 5f;
    }
    IEnumerator PlayFootStep()
    {
        while (true)
        {
            if(rb.linearVelocity.magnitude > 0.1 && isGrounded)
            {
                AudioManager.Instance.PlaySFX(footStepSFX);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
