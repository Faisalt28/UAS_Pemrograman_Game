using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    // Fire Attack
    public GameObject firePrefab;
    public AudioClip fireSFX;
    private float summonInterval = 0.8f;   // Frekuensi serangan
    private float fireLifetime = 2f;
    private int fireCount = 2;             // Jumlah bola api per serangan
    private int fireDamage = 10;

    // Movement
    private float chaseRange = 20f;
    private float stopRange = 5f;
    private float speed = 4f;

    private Transform player;
    private PlayerHealth playerHealth;
    private float lastSummon;

    void Start()
    {
        if (!Application.isPlaying) return;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (!playerObj || !firePrefab)
        {
            Debug.LogError("Player atau prefab belum diatur.");
            enabled = false;
            return;
        }

        player = playerObj.transform;
        playerHealth = playerObj.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (!player || playerHealth == null || playerHealth.IsDead()) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < chaseRange && distance > stopRange)
            MoveTowardPlayer();

        // Serang terus tanpa memperhatikan jarak
        if (Time.time - lastSummon >= summonInterval)
        {
            SummonFire();
            lastSummon = Time.time;
        }
    }

    void MoveTowardPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    void SummonFire()
    {
        for (int i = 0; i < fireCount; i++)
        {
            // Posisi acak di sekitar player (jarak aman 1–3 meter)
            Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
            Vector3 basePos = new Vector3(player.position.x + offset.x, player.position.y + 1f, player.position.z + offset.z);

            // Turunkan ke tanah
            float groundY = GetGroundY(basePos);
            Vector3 finalPos = new Vector3(basePos.x, groundY, basePos.z);

            var fire = Instantiate(firePrefab, finalPos, Quaternion.identity);
            Destroy(fire, fireLifetime);

            if (fireSFX)
                AudioSource.PlayClipAtPoint(fireSFX, finalPos);

            // Damage jika player sangat dekat (masuk radius)
            float distanceToPlayer = Vector3.Distance(finalPos, player.position);
            if (distanceToPlayer < 1.2f) // lebih kecil dari versi OP
            {
                playerHealth.TakeDamage(fireDamage);
            }
        }
    }

    float GetGroundY(Vector3 position)
    {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 20f))
        {
            return hit.point.y;
        }
        return position.y;
    }
}
