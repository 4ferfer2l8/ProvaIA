using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float invulnerabilityTime = 1f;


    private float invulnerableUntil;
    private bool isDead;

    bool isCrouching;
    bool isRunning;
    Animator animator;
    Rigidbody2D rb;

    Vector2 input;
    Vector2 lastDirection = Vector2.down; // começa olhando para baixo

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(isDead) return;
        
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        isRunning = Input.GetKey(KeyCode.LeftShift) && input != Vector2.zero && !isCrouching;
        animator.SetBool("isRunning", isRunning);

        bool isAttacking = IsAttacking();
        bool isSpecial = IsSpecial();
        bool isRolling = IsRolling();

        // só atualiza a direção se estiver andando e não estiver atacando
        if (input != Vector2.zero && !isAttacking)
            lastDirection = input;

        animator.SetFloat("moveX", input.x);
        animator.SetFloat("moveY", input.y);
        animator.SetFloat("lastX", lastDirection.x);
        animator.SetFloat("lastY", lastDirection.y);
        animator.SetBool("isMoving", input != Vector2.zero);
        animator.SetBool("isCrouching", isCrouching);

        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
            animator.SetTrigger("attack");

        if (Input.GetKeyDown(KeyCode.F) && !isSpecial)
            animator.SetTrigger("special");

        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
            animator.SetTrigger("roll");

        if (Input.GetKeyDown(KeyCode.K)) 
            Die();
    }

    void FixedUpdate()
    {
        if (isDead || IsInState("Hurt") || IsInState("Attack") || IsInState("Special") || IsInState("Roll"))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        float speed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);
        rb.linearVelocity = input * speed;
    }

    bool IsInState(string name)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
    }

    bool IsSpecial()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Special");
    }

    bool IsRolling()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Roll");
    }

    void OnTriggerEnter2D(Collider2D other) { TryTakeDamage(other); }
    void OnTriggerStay2D(Collider2D other)  { TryTakeDamage(other); }

    void TryTakeDamage(Collider2D other)
    {
        if (isDead) return;
        if (!other.CompareTag("Enemy")) return;
        if (Time.time < invulnerableUntil) return;

        invulnerableUntil = Time.time + invulnerabilityTime;
        animator.SetTrigger("hurt");
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("die");
    }
}