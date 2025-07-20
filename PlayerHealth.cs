using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private int maxHP = 100;
    private int currentHP;

    public TextMeshProUGUI hpText;
    public GameObject hitEffectPrefab;

    public AudioClip damageSFX;
    public AudioClip deathSFX;

    private AudioSource audioSource;
    private Animator animator;
    private bool isDead = false;
    private PlayerController playerController;

    private Coroutine healingCoroutine;
    private int healAmount = 2;
    private float healInterval = 0.2f;
    private bool isHealing = false;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();

        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (!animator) Debug.LogError("Animator tidak ditemukan.");
        if (!playerController) Debug.LogError("PlayerController tidak ditemukan.");

        // Tampilkan HP hanya jika game sedang berjalan
        if (hpText != null)
            hpText.gameObject.SetActive(Time.timeScale == 1f);
    }

    void Update()
    {
        // Pastikan HP UI muncul saat game sedang berjalan dan player belum mati
        if (!isDead && Time.timeScale == 1f && hpText != null && !hpText.gameObject.activeSelf)
        {
            hpText.gameObject.SetActive(true);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        currentHP = Mathf.Max(currentHP, 0);
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Die();
            return;
        }

        if (animator) animator.SetTrigger("GetHit");
        if (damageSFX) audioSource.PlayOneShot(damageSFX);
        SpawnHitEffect();
    }

    void Die()
    {
        isDead = true;

        if (animator) animator.SetTrigger("Death");
        if (playerController) playerController.DisableControl();
        if (deathSFX) audioSource.PlayOneShot(deathSFX);

        StopHealing(); // <== penting: hentikan healing saat mati

        if (hpText != null)
            hpText.gameObject.SetActive(false); // <== sembunyikan UI HP

        GameManager.Instance.GameOver();
    }

    void UpdateHPUI()
    {
        if (hpText)
            hpText.text = "HP: " + currentHP;
    }

    public bool IsDead()
    {
        return isDead;
    }

    void SpawnHitEffect()
    {
        if (hitEffectPrefab)
        {
            Vector3 effectPos = transform.position + Vector3.up * 1f;
            Destroy(Instantiate(hitEffectPrefab, effectPos, Quaternion.identity), 2f);
        }
    }

    public void StartHealing()
    {
        if (!isHealing && !isDead)
        {
            healingCoroutine = StartCoroutine(HealOverTime());
            isHealing = true;
        }
    }

    public void StopHealing()
    {
        if (healingCoroutine != null)
        {
            StopCoroutine(healingCoroutine);
            healingCoroutine = null;
            isHealing = false;
        }
    }

    IEnumerator HealOverTime()
    {
        while (true)
        {
            if (currentHP < maxHP)
            {
                currentHP += healAmount;
                currentHP = Mathf.Min(currentHP, maxHP);
                UpdateHPUI();
            }
            yield return new WaitForSeconds(healInterval);
        }
    }
}
