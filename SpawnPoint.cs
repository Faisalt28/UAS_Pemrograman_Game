using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class SpawnPoint : MonoBehaviour
{
    public GameObject enemyPrefab;
    public AudioClip burnSFX;
    public int maxSpawn = 4;
    public bool onlyOnce = false;

    static bool burnSFXPlaying;
    SphereCollider col;
    GameObject spawnedEnemy;

    int spawnCount = 0;
    bool isRespawning = false;

    void Start()
    {
        if (!enemyPrefab)
        {
            Debug.LogError("Enemy prefab belum diisi.");
            enabled = false;
            return;
        }

        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (spawnCount >= maxSpawn) return;

        if (spawnedEnemy == null)
        {
            SpawnEnemy();
            IgniteArea();
        }
    }

    void Update()
    {
        if (spawnedEnemy && spawnedEnemy.GetComponent<EnemyHealth>()?.IsDead() == true)
        {
            ExtinguishArea();
            spawnedEnemy = null;

            if (!isRespawning && spawnCount < maxSpawn)
            {
                isRespawning = true;
                StartCoroutine(RespawnAfterDelay(2f));
            }
        }
    }

    void SpawnEnemy()
    {
        Vector3 pos = transform.position;
        pos.y = GetGroundY(pos);
        spawnedEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        spawnCount++;
        isRespawning = false;
    }

    void IgniteArea()
    {
        foreach (var c in Physics.OverlapSphere(transform.position, col.radius))
            c.GetComponent<BurnObject>()?.Ignite();

        if (burnSFX && !burnSFXPlaying)
            StartCoroutine(PlayBurnSFX());
    }

    void ExtinguishArea()
    {
        foreach (var c in Physics.OverlapSphere(transform.position, col.radius))
            c.GetComponent<BurnObject>()?.Extinguish();
    }

    IEnumerator PlayBurnSFX()
    {
        burnSFXPlaying = true;

        var sfxObj = new GameObject("BurnSFX");
        sfxObj.transform.position = transform.position;

        var audio = sfxObj.AddComponent<AudioSource>();
        audio.clip = burnSFX;
        audio.Play();

        float duration = 2f;
        float startVol = audio.volume;

        while (audio.volume > 0)
        {
            audio.volume -= startVol * Time.deltaTime / duration;
            yield return null;
        }

        Destroy(sfxObj);
        burnSFXPlaying = false;
    }

    IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (spawnCount < maxSpawn)
        {
            SpawnEnemy();
            IgniteArea();
        }
    }

    float GetGroundY(Vector3 pos)
    {
        return Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 30f)
            ? hit.point.y
            : pos.y;
    }
}
