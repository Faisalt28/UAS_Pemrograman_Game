using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    private int maxHP = 100;
    private int currentHP;
    private bool isDead = false;

    private Animator animator;
    private TextMeshProUGUI hpText;
    private AudioSource audioSource;

    public GameObject hitEffectPrefab;
    public AudioClip damageSFX;
    public AudioClip deathSFX;

    void Start()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        var canvas = transform.Find("HP_Canvas");
        hpText = canvas ? canvas.GetComponentInChildren<TextMeshProUGUI>() : null;

        if (!hpText) Debug.LogWarning("HP_Canvas tidak ditemukan.");
        UpdateHPUI();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        int prevHP = currentHP;
        currentHP = Mathf.Max(currentHP - amount, 0);
        UpdateHPUI();

        // Jika masih hidup setelah damage
        if (currentHP > 0)
        {
            animator?.SetTrigger("GetHit");
            SpawnHitEffect();

            if (damageSFX && audioSource)
                audioSource.PlayOneShot(damageSFX);
        }
        else
        {
            // Jika mati, langsung panggil Die tanpa SFX damage
            Die();
        }
    }

    void SpawnHitEffect()
    {
        if (!hitEffectPrefab) return;

        var pos = transform.position + Vector3.up * 1f;
        Destroy(Instantiate(hitEffectPrefab, pos, Quaternion.identity), 1.5f);
    }

    void Die()
    {
        isDead = true;

        animator?.SetTrigger("Die");
        if (hpText) hpText.gameObject.SetActive(false);

        if (deathSFX && audioSource)
            audioSource.PlayOneShot(deathSFX);

        // Nonaktifkan semua script kecuali EnemyHealth
        foreach (var script in GetComponents<MonoBehaviour>())
            if (script != this) script.enabled = false;

        GameManager.Instance.EnemyDefeated();
        Destroy(gameObject, 2f); // Tunggu sedikit agar SFX sempat selesai
    }

    void UpdateHPUI()
    {
        if (hpText) hpText.text = "HP: " + currentHP;
    }

    public bool IsDead() => isDead;
}
