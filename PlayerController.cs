using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // Movement
    private float walkSpeed = 2f, runSpeed = 5f, rotationSpeed = 8f, jumpForce = 3.5f;

    // Combat
    private float comboResetTime = 1f;
    private int attackIndex = 0;
    private int maxCombo = 4;

    // States
    private bool isAttacking = false, isHealing = false, isAlive = true;
    private bool isGrounded = true, isRunning = false, shouldJump = false;

    // Refs
    private Rigidbody rb;
    private Animator animator;
    private PlayerHealth playerHealth;

    // Movement & Heal
    private Vector3 move = Vector3.zero;
    private GameObject healingInstance;
    public GameObject HealingSkillPrefab;

    // Attack
    private GameObject nearestEnemy;
    private float lastAttackTime = 0f;

    // SFX Attack
    public AudioClip[] attackSFXs;
    public AudioClip healSFX;

    // Mobile Input
    private float inputX = 0f;
    private float inputZ = 0f;

    [SerializeField] private Joystick joystick;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

        rb.freezeRotation = true;

        if (!HealingSkillPrefab) Debug.LogWarning("HealingSkillPrefab belum diatur.");
        if (!playerHealth) Debug.LogError("PlayerHealth tidak ditemukan.");
    }

    void Update()
    {
        if (!isAlive) return;

        HandleHealing();
        HandleInput();
        HandleComboReset();
    }

    void FixedUpdate()
    {
        if (!isAlive) return;

        float speed = isRunning ? runSpeed : walkSpeed;

        if (!isAttacking && !isHealing)
        {
            rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
            if (move != Vector3.zero)
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(move), rotationSpeed * Time.fixedDeltaTime));
        }

        if (isAttacking && nearestEnemy)
        {
            Vector3 dir = nearestEnemy.transform.position - transform.position;
            dir.y = 0f;
            if (dir != Vector3.zero)
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.fixedDeltaTime));
        }

        animator.applyRootMotion = isAttacking;
        UpdateAnimatorSpeed();
        HandleJump();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isGround", false);
        }
    }

    // ===================== INPUT =====================
    void HandleInput()
    {
        inputX = joystick != null ? joystick.Horizontal : 0f;
        inputZ = joystick != null ? joystick.Vertical : 0f;

        Transform cam = Camera.main.transform;
        Vector3 forward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(cam.right, new Vector3(1, 0, 1)).normalized;

        move = forward * inputZ + right * inputX;
        if (move.magnitude > 1f) move.Normalize();
    }

    // ===================== PUBLIC UI FUNCTIONS =====================
    public void OnRunButtonDown()
    {
        isRunning = true;
    }

    public void OnRunButtonUp()
    {
        isRunning = false;
    }

    public void OnJumpButton()
    {
        if (isGrounded) shouldJump = true;
    }

    public void OnAttackButton()
    {
        if (!isHealing) HandleAttack();
    }

    public void OnHealDown() => StartHeal();
    public void OnHealUp() => StopHeal();

    // ===================== ATTACK =====================
    void HandleAttack()
    {
        if (!isAttacking || Time.time - lastAttackTime <= comboResetTime)
        {
            attackIndex = (attackIndex % maxCombo) + 1;
            animator.SetInteger("attackIndex", attackIndex);
            animator.SetTrigger("doAttack");

            lastAttackTime = Time.time;
            isAttacking = true;

            nearestEnemy = FindNearestEnemy();
            RotateToward(nearestEnemy);

            if (nearestEnemy)
            {
                int damage = GameManager.Instance.playerDamage;
                nearestEnemy.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            }

            int sfxIndex = Mathf.Clamp(attackIndex - 1, 0, attackSFXs.Length - 1);
            if (attackSFXs.Length > 0 && attackSFXs[sfxIndex])
            {
                AudioSource.PlayClipAtPoint(attackSFXs[sfxIndex], transform.position);
            }
        }
    }

    // ===================== HEAL =====================
    void HandleHealing() { }

    void StartHeal()
    {
        isHealing = true;
        animator.SetBool("HealUp", true);
        playerHealth?.StartHealing();

        if (HealingSkillPrefab && !healingInstance)
        {
            healingInstance = Instantiate(HealingSkillPrefab, transform);
            healingInstance.transform.localPosition = Vector3.zero;
        }

        if (healSFX)
        {
            AudioSource.PlayClipAtPoint(healSFX, transform.position);
        }
    }

    void StopHeal()
    {
        isHealing = false;
        animator.SetBool("HealUp", false);
        playerHealth?.StopHealing();

        if (healingInstance)
        {
            ParticleSystem ps = healingInstance.GetComponent<ParticleSystem>();
            if (ps) ps.Stop();

            float delay = ps ? ps.main.startLifetime.constantMax + 0.5f : 0.5f;
            Destroy(healingInstance, delay);
            healingInstance = null;
        }
    }

    // ===================== JUMP =====================
    void HandleJump()
    {
        if (!shouldJump) return;

        animator.SetBool("isGround", true);
        animator.CrossFade("Jump", 0f);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        shouldJump = false;
    }

    // ===================== ANIMATION =====================
    void UpdateAnimatorSpeed()
    {
        if (isGrounded && !isAttacking && !isHealing)
        {
            float speed = move.magnitude * (isRunning ? 1f : 0.5f);
            float smoothed = Mathf.Lerp(animator.GetFloat("Speed"), speed, Time.deltaTime * 10f);
            animator.SetFloat("Speed", smoothed);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void HandleComboReset()
    {
        if (isAttacking && Time.time - lastAttackTime > comboResetTime)
        {
            attackIndex = 0;
            isAttacking = false;
            animator.SetInteger("attackIndex", 0);
            animator.CrossFade("Movement", 0.1f);
        }
    }

    // ===================== UTILITY =====================
    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    void RotateToward(GameObject target)
    {
        if (!target) return;
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
    }

    // ===================== EXTERNAL =====================
    public void DisableControl()
    {
        isAlive = false;
        move = Vector3.zero;
        animator.SetFloat("Speed", 0f);
    }
}
