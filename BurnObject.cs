using UnityEngine;
using TMPro;

public class BurnObject : MonoBehaviour
{
    public GameObject fireEffectPrefab;
    private GameObject fireEffectInstance;
    private bool isBurning = false;

    public int maxHP = 100;
    private int currentHP;

    public TextMeshProUGUI hpText;

    public MonoBehaviour areaManager;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }

    public void Ignite()
    {
        if (isBurning || fireEffectPrefab == null) return;

        isBurning = true;

        // Tampilkan efek api
        fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity, transform);

        // Mulai damage tiap detik
        InvokeRepeating(nameof(TakeBurnDamage), 1f, 1f);
    }

    public void Extinguish()
    {
        if (!isBurning) return;

        isBurning = false;

        CancelInvoke(nameof(TakeBurnDamage));

        if (fireEffectInstance != null)
        {
            ParticleSystem ps = fireEffectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                float delay = ps.main.startLifetime.constantMax + 0.5f;
                Destroy(fireEffectInstance, delay);
            }
            else
            {
                Destroy(fireEffectInstance);
            }
        }

        fireEffectInstance = null;
    }

    void TakeBurnDamage()
    {
        currentHP -= 1;
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = "HP: " + currentHP.ToString();
        }
    }

    void Die()
    {
        Extinguish();

        if (areaManager != null)
        {
            var method = areaManager.GetType().GetMethod("NotifyAreaDestroyed");
            if (method != null)
            {
                method.Invoke(areaManager, null);
            }
        }

        Destroy(gameObject);
    }
}
