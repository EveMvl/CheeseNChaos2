using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MouseAgent : MonoBehaviour
{
    [Header("Stats")]
    public float hunger = 1f;
    public bool isAdult = true;
    public bool isReadyToMate = false;
    public float detectionRange = 10f;
    public float eatRange = 1.2f;

    [Header("Growth Settings")]
    public float growthTime = 30f;
    private float age = 0f;

    private static readonly Vector3 BASE_SCALE = new Vector3(1f, 1f, 1f);
    private Vector3 initialScale;
    private Vector3 adultScale;

    [Header("Wander Settings")]
    public float wanderRadius = 10f;
    public float waitTimeAtPoint = 2f;

    [Header("Rotation Settings")]
    public float turnSpeed = 5f;
    public float forwardOffset = -113.399f; // 90° = face +X direction by default

    [Header("References")]
    public NavMeshAgent agent;
    private CheesePiece targetCheese;
    private Vector3 wanderTarget;
    private bool waiting = false;
    private bool isEating = false;
    public bool isBreeding = false;
    [Header("Animation")]
    public Animator animator; // Assign in Inspector

    void Awake()
    {
        // ✅ Register this mouse in the manager
        if (MouseManager.Instance != null)
            MouseManager.Instance.RegisterMouse(this);
        Debug.Log("Current mice: " + CurrentMouseCount());

        agent = GetComponent<NavMeshAgent>();
        CheeseManager.OnCheeseListChanged += UpdateTargetCheese;

        initialScale = BASE_SCALE;
        adultScale = BASE_SCALE;

        if (!isAdult)
        {
            transform.localScale = BASE_SCALE * 0.5f;
            age = 0f;
        }
        else
        {
            transform.localScale = adultScale;
        }
        if (animator == null)
        animator = GetComponent<Animator>();
        }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Use agent speed as a float for animation
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    void OnDestroy()
    {
        // ✅ Unregister this mouse in the manager
        if (MouseManager.Instance != null)
            MouseManager.Instance.UnregisterMouse(this);

        CheeseManager.OnCheeseListChanged -= UpdateTargetCheese;
    }

    // Optional helper to get current mouse count
    public int CurrentMouseCount()
    {
        if (MouseManager.Instance != null)
            return MouseManager.Instance.MouseCount;
        return 0;
    }


    void Start()
    {
        InvokeRepeating(nameof(UpdateTargetCheese), 0f, 2f);
    }

    void Update()
    {
        HandleGrowth();

        // Skip all movement logic while breeding
        if (!isBreeding)
        {
            HandleMovementAndTargeting();
            HandleRotation();
        }

        UpdateAnimation();

    }

    // -----------------------------------
    // 🧬 Growth System
    // -----------------------------------
    void HandleGrowth()
    {
        if (!isAdult)
        {
            age += Time.deltaTime;
            float growthRatio = Mathf.Clamp01(age / growthTime);
            transform.localScale = Vector3.Lerp(BASE_SCALE * 0.5f, adultScale, growthRatio);

            if (growthRatio >= 1f)
                BecomeAdult();
        }
    }

    void BecomeAdult()
    {
        isAdult = true;
        transform.localScale = adultScale;
        Debug.Log($"{name} has grown up! 🐭");
    }

    // -----------------------------------
    // 🚶 Movement & Cheese Targeting
    // -----------------------------------
    void HandleMovementAndTargeting()
    {
        if (isEating) return;

        if (targetCheese != null)
        {
            Vector3 cheesePos = targetCheese.transform.position;
            Vector3 dirToCheese = (cheesePos - transform.position).normalized;
            Vector3 stopPoint = cheesePos - dirToCheese * eatRange;
            agent.SetDestination(stopPoint);

            float distance = Vector3.Distance(transform.position, cheesePos);

            if (distance < eatRange * 1.2f)
            {
                agent.isStopped = true;
                TryEatCheese(targetCheese);
            }
            else
            {
                agent.isStopped = false;
            }
        }
        else
        {
            if (!waiting && (!agent.hasPath || agent.remainingDistance < 0.5f))
            {
                wanderTarget = RandomWanderPoint();
                agent.SetDestination(wanderTarget);
                StartCoroutine(WaitAtPoint());
            }
        }
    }

    void UpdateTargetCheese()
    {
        if (!isAdult || isEating || isBreeding) return;

        CheesePiece nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var cheeseTransform in CheeseManager.CheeseList)
        {
            if (cheeseTransform == null) continue;
            CheesePiece cheese = cheeseTransform.GetComponent<CheesePiece>();
            if (cheese == null) continue;

            float dist = Vector3.Distance(transform.position, cheese.transform.position);
            if (dist < minDist && dist <= detectionRange)
            {
                minDist = dist;
                nearest = cheese;
            }
        }

        if (targetCheese == null)
            targetCheese = nearest;
    }

    Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = transform.position + new Vector3(
            Random.Range(-wanderRadius, wanderRadius), 0,
            Random.Range(-wanderRadius, wanderRadius)
        );

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    IEnumerator WaitAtPoint()
    {
        waiting = true;
        yield return new WaitForSeconds(waitTimeAtPoint);
        waiting = false;
    }

    // -----------------------------------
    // 🧀 Eating Logic
    // -----------------------------------
    void TryEatCheese(CheesePiece cheese)
    {
        if (!isAdult || cheese == null || isEating || isBreeding) return;

        isEating = true;
        agent.isStopped = true;
        cheese.AddEater(this);

        if (cheese.eatEffectPrefab != null)
        {
            Vector3 effectPos = (transform.position + cheese.transform.position) / 2f;
            Instantiate(cheese.eatEffectPrefab, effectPos, Quaternion.identity);
        }
    }

    public void OnFinishedEating(CheesePiece cheese)
    {
        isReadyToMate = true;
        hunger = 1f;
        targetCheese = null;
        isEating = false;
        agent.isStopped = false;

        BreedingSystem.Instance.NotifyAte(this);
        Debug.Log($"✅ {name}: Finished eating {cheese.name}");
    }

    public void OnCheeseGone(CheesePiece cheese)
    {
        if (targetCheese == cheese)
        {
            targetCheese = null;
            isEating = false;
            agent.isStopped = false;
            StartCoroutine(WanderAround());
            Debug.Log($"🐭 {name}: OnCheeseGone triggered for {cheese.name}");
        }
    }

    IEnumerator WanderAround()
    {
        wanderTarget = RandomWanderPoint();
        agent.isStopped = false;
        agent.SetDestination(wanderTarget);
        yield return null;
    }

    // -----------------------------------
    // 💞 Breeding Lock
    // -----------------------------------
    public void StartBreeding()
    {
        if (isBreeding) return;
        isBreeding = true;
        isEating = false;
        targetCheese = null;
        CancelInvoke(nameof(UpdateTargetCheese));
        agent.isStopped = true;
        Debug.Log($"{name} is now breeding — cheese targeting disabled 🍷🧀");
    }

    public void EndBreeding()
    {
        if (!isBreeding) return;
        isBreeding = false;
        agent.isStopped = false;
        InvokeRepeating(nameof(UpdateTargetCheese), 0f, 2f);
        Debug.Log($"{name} finished breeding — cheese targeting re-enabled 💪");
    }

    // -----------------------------------
    // 🔄 Rotation
    // -----------------------------------
    void HandleRotation()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 dir = agent.velocity.normalized;
            dir.y = 0f;
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up) *
                                   Quaternion.Euler(0f, forwardOffset, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * turnSpeed
            );
        }
        else if (targetCheese != null && isEating)
        {
            Vector3 dirToCheese = (targetCheese.transform.position - transform.position).normalized;
            dirToCheese.y = 0f;
            Quaternion targetRot = Quaternion.LookRotation(dirToCheese, Vector3.up) *
                                   Quaternion.Euler(0f, forwardOffset, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * turnSpeed
            );
        }
    }

    // -----------------------------------
    // 🧩 Collision Detection
    // -----------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (isAdult && !isEating && !isBreeding && other.CompareTag("Cheese"))
        {
            CheesePiece cheese = other.GetComponent<CheesePiece>();
            if (cheese != null)
            {
                Debug.Log($"{name} entered cheese trigger: {cheese.name}");
                TryEatCheese(cheese);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isAdult && !isEating && !isBreeding && other.CompareTag("Cheese"))
        {
            CheesePiece cheese = other.GetComponent<CheesePiece>();
            if (cheese != null)
                TryEatCheese(cheese);
        }
    }

public void ResumeWanderingAfterSpoil()
{
    // Prevent interrupting other actions
    if (!isEating && !isBreeding)
    {
        StopAllCoroutines();
        StartCoroutine(WanderAround());
    }
}

}
