using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class BreedingSystem : MonoBehaviour
{
    public static BreedingSystem Instance;

    [Header("Prefabs & Settings")]
    public GameObject mousePrefab;
    public GameObject cannibalPrefab;
    public Transform spawnParent;
    public float babySpawnOffset = 0.5f;
    public float spinDuration = 2f;
    public float parentSpinSpeed = 720f;
    public float babySpinSpeed = 1440f;
    public float stopThreshold = 0.1f;

    private List<MouseAgent> readyMice = new List<MouseAgent>();

    public event System.Action OnBabySpawned;

    void Awake()
    {
        Instance = this;
    }

    public void NotifyAte(MouseAgent mouse)
    {
        if (!mouse.isAdult || !mouse.isReadyToMate) return;

        if (!readyMice.Contains(mouse))
            readyMice.Add(mouse);

        TryBreed();
    }

    void TryBreed()
    {
        while (readyMice.Count >= 2)
        {
            MouseAgent parentA = readyMice[0];
            MouseAgent parentB = readyMice[1];
            readyMice.RemoveRange(0, 2);

            StartCoroutine(HandlePairBreeding(parentA, parentB));
        }
    }

    private IEnumerator HandlePairBreeding(MouseAgent a, MouseAgent b)
    {
        if (a == null || b == null) yield break;

        NavMeshAgent agentA = a.GetComponent<NavMeshAgent>();
        NavMeshAgent agentB = b.GetComponent<NavMeshAgent>();

        while ((agentA != null && (agentA.pathPending || agentA.remainingDistance > stopThreshold)) ||
               (agentB != null && (agentB.pathPending || agentB.remainingDistance > stopThreshold)))
        {
            if (a == null || b == null) yield break;
            yield return null;
        }

        if (a == null || b == null) yield break;

        if (agentA) agentA.isStopped = true;
        if (agentB) agentB.isStopped = true;

        Vector3 midPoint = (a.transform.position + b.transform.position) / 2f;

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            if (a != null) a.transform.Rotate(Vector3.up, parentSpinSpeed * Time.deltaTime);
            if (b != null) b.transform.Rotate(Vector3.up, -parentSpinSpeed * Time.deltaTime);
            yield return null;
        }

        if (a == null || b == null) yield break;

        Vector3 spawnPos = midPoint + Vector3.up * babySpawnOffset;
        GameObject prefabToSpawn = (Random.Range(0, 4) == 0 && cannibalPrefab != null)
                                   ? cannibalPrefab
                                   : mousePrefab;

        GameObject baby = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, spawnParent);
        MouseAgent babyAgent = baby.GetComponent<MouseAgent>();

        if (babyAgent != null)
        {
            babyAgent.isAdult = false;
            babyAgent.hunger = 0f;
            babyAgent.isReadyToMate = false;

            float spin = 0f;
            while (spin < spinDuration)
            {
                spin += Time.deltaTime;
                if (baby != null) baby.transform.Rotate(Vector3.up, babySpinSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // 🔔 Notifikasi sistem bahwa ada bayi baru
        OnBabySpawned?.Invoke();

        // ⚡ Hanya tambah score untuk bayi baru normal, kanibal dimakan tidak memanggil OnRatCollected lagi
        if (babyAgent != null && !(babyAgent is CannibalMouseAgent))
        {
            LevelManager.Instance?.OnRatCollected();
        }

        if (a != null) { if (agentA) agentA.isStopped = false; a.isReadyToMate = false; }
        if (b != null) { if (agentB) agentB.isStopped = false; b.isReadyToMate = false; }
    }
}
