using UnityEngine;
using YG;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 3f;
    public float jumpHeight = 1.4f;
    public float gravity = -23f;

    [Header("Ladder")]
    public float climbSpeed = 3f;
    public bool isOnLadder;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Jump")]
    public float jumpBufferTime = 0.15f;
    public float swingJumpForce = 8f;

    [Header("Joystick")]
    public Joystick joystick;
    public bool isDead;
    public bool freeze;

    [Header("Hands")]
    public GameObject hands; // Ссылка на GameObject с руками
    [Header("Idle Settings")]
    public float idleShakeAmount = 0.005f; // Амплитуда тряски в состоянии покоя
    public float idleShakeSpeed = 2f; // Скорость тряски в состоянии покоя
    [Header("Walk Settings")]
    public float walkShakeAmount = 0.015f; // Амплитуда тряски при ходьбе
    public float walkShakeSpeed = 4f; // Скорость тряски при ходьбе
    [Header("Run Settings")]
    public float runShakeAmount = 0.03f; // Амплитуда тряски при беге
    public float runShakeSpeed = 6f; // Скорость тряски при беге
    [Header("Jump Settings")]
    public float jumpShakeAmount = 0.05f; // Амплитуда тряски при прыжке
    public float jumpShakeSpeed = 8f; // Скорость тряски при прыжке

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float jumpBufferTimer;
    private bool swinging;
    private Vector3 grapplePoint;
    private float pullForce;
    private float swingForce;
    private float maxFallSpeed = -25f;

    private Vector3 initialHandsPosition;
    private float currentShakeAmount;
    private float currentShakeSpeed;
    private bool isJumping;
    private float shakeTimer;

    public Grappling grappling;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (hands != null)
        {
            initialHandsPosition = hands.transform.localPosition;
        }
    }

    private void Update()
    {
        if (YG2.isPauseGame || isDead) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (swinging && jumpBufferTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(swingJumpForce * -2f * gravity);
            jumpBufferTimer = 0f;
            if (grappling != null)
                grappling.StopGrappleExternal();
            StopSwing();
            return;
        }

        if (!swinging && !freeze)
        {
            float x = Input.GetAxis("Horizontal") + (joystick != null ? joystick.Horizontal : 0f);
            float z = Input.GetAxis("Vertical") + (joystick != null ? joystick.Vertical : 0f);

            bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
            //bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

            if (!isGrounded)
            {
                currentShakeAmount = jumpShakeAmount;
                currentShakeSpeed = jumpShakeSpeed;
                isJumping = true;
            }
            //else if (isRunning)
            //{
            //    currentShakeAmount = runShakeAmount;
            //    currentShakeSpeed = runShakeSpeed;
            //    isJumping = false;
            //}
            else if (isMoving)
            {
                currentShakeAmount = walkShakeAmount;
                currentShakeSpeed = walkShakeSpeed;
                isJumping = false;
            }
            else
            {
                currentShakeAmount = idleShakeAmount;
                currentShakeSpeed = idleShakeSpeed;
                isJumping = false;
            }

            Vector3 move = (transform.right * x + transform.forward * z) * MoveSpeed;
            //if (isRunning)
            //{
            //    move *= 1.5f; 
            //}
            controller.Move(move * Time.deltaTime);

            if (isGrounded)
            {
                if (velocity.y < 0f)
                    velocity.y = -2f;

                if (jumpBufferTimer > 0f)
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    jumpBufferTimer = 0f;
                }
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
                if (velocity.y < maxFallSpeed)
                    velocity.y = maxFallSpeed;
            }

            controller.Move(velocity * Time.deltaTime);
        }

        if (hands != null)
        {
            shakeTimer += Time.deltaTime * currentShakeSpeed;
            float shakeOffset = Mathf.Sin(shakeTimer) * currentShakeAmount;
            hands.transform.localPosition = initialHandsPosition + new Vector3(0, shakeOffset, 0);
        }
    }

    public void StartSwing(Vector3 point, float pull, float swing)
    {
        swinging = true;
        freeze = true;
        grapplePoint = point;
        pullForce = pull;
        swingForce = swing;
        velocity = Vector3.zero;
    }

    public void StopSwing()
    {
        swinging = false;
        freeze = false;
        velocity.y = Mathf.Max(velocity.y, -10f);
    }

    private void FixedUpdate()
    {
        if (!swinging) return;

        Vector3 toPoint = grapplePoint - transform.position;
        Vector3 dir = toPoint.normalized;
        controller.Move(dir * pullForce * Time.fixedDeltaTime);

        float input = Input.GetAxis("Horizontal") + (joystick != null ? joystick.Horizontal : 0f);
        controller.Move(transform.right * input * swingForce * Time.fixedDeltaTime);
    }
}
