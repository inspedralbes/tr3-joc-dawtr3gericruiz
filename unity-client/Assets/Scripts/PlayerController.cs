using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuració de Xarxa")]
    public bool isLocalPlayer = true; 

    [Header("Moviment")]
    public float speed = 8f;
    public float jumpForce = 12f;
    private int extraJumps = 1; 
    private int jumpsLeft;
    private bool isFacingRight = true;

    [Header("Estat del Personatge")]
    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    private Animator anim;
    private Rigidbody2D rb;
    private float moveInput;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpsLeft = extraJumps;
    }

    void Update() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        if (isGrounded) jumpsLeft = extraJumps;

        if (anim != null) anim.SetBool("isGrounded", isGrounded);

        if (isLocalPlayer) {
            moveInput = Input.GetAxisRaw("Horizontal");

            if (anim != null) anim.SetFloat("speed", Mathf.Abs(moveInput));

            if (Input.GetKeyDown(KeyCode.Space)) {
                if (isGrounded) {
                    Jump();
                } else if (jumpsLeft > 0) {
                    Jump();
                    jumpsLeft--;
                }
            }

            if (moveInput > 0 && !isFacingRight) {
                Flip();
            } 
            else if (moveInput < 0 && isFacingRight) {
                Flip();
            }
        } 
        else {
            moveInput = 0f; 
            if (anim != null) anim.SetFloat("speed", 0f);
        }
    }

    void FixedUpdate() {
        if (isGrounded || rb.linearVelocity.magnitude < 15f) {
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }
    }

    void Jump() {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}