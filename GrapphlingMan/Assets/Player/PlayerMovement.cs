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
    public float swingJumpForce = 8f;       // Сила прыжка с верёвки

    [Header("Joystick")]
    public Joystick joystick;
    public bool isDead;
    public bool freeze;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float jumpBufferTimer;
    private bool swinging;
    private Vector3 grapplePoint;
    private float pullForce;
    private float swingForce;
    private float maxFallSpeed = -25f;

    // Ссылка на Grappling чтобы остановить верёвку при прыжке
    public Grappling grappling;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
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
            StopSwing(); // явно сбрасываем состояние свинга в PM
            return;
        }

        if (!swinging && !freeze)
        {
            float x = Input.GetAxis("Horizontal") + (joystick != null ? joystick.Horizontal : 0f);
            float z = Input.GetAxis("Vertical") + (joystick != null ? joystick.Vertical : 0f);

            Vector3 move = (transform.right * x + transform.forward * z) * MoveSpeed;
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