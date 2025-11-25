using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class CannibalMouseAgent : MouseAgent
{
    [Header("Cannibal Settings")]
    public float attackRange = 1.5f;
    public float eatDuration = 2f;
    public GameObject explosionPrefab;
    public GameObject eatingPrefab;

    private MouseAgent targetMouse;
    private bool isEatingTarget = false;

    void Update()
    {
        if (!isEatingTarget)
        {
            FindTarget();
            ChaseTarget();
            HandleRotation();
        }

        UpdateAnimation();
    }

    void FindTarget()
    {
        if (targetMouse != null) return;

        List<MouseAgent> mice = MouseManager.Instance.GetAllMice();
        foreach (var m in mice)
        {
            if (m == null || m == this || m.isBreeding) continue;
            targetMouse = m;
            break; // pick the first available mouse
        }
    }

    void ChaseTarget()
    {
        if (targetMouse == null) return;

        Vector3 dir = (targetMouse.transform.position - transform.position);
        float dist = dir.magnitude;

        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(targetMouse.transform.position);
        }
        else
        {
            // Attack
            agent.isStopped = true;
            StartCoroutine(EatTarget());
        }
    }

    IEnumerator EatTarget()
    {
        if (isEatingTarget) yield break;

        isEatingTarget = true;

        // Spawn eating effect at mouth
        if (eatingPrefab != null)
            Instantiate(eatingPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);

        // Target disappears with explosion
        if (targetMouse != null)
        {
            if (explosionPrefab != null)
                Instantiate(explosionPrefab, targetMouse.transform.position, Quaternion.identity);

            // Unregister from MouseManager before destroying
            MouseManager.Instance?.UnregisterMouse(targetMouse);

            Destroy(targetMouse.gameObject);
            targetMouse = null;
        }

        yield return new WaitForSeconds(eatDuration);

        isEatingTarget = false; // back to wandering/hunting
    }

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
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Use NavMeshAgent speed for animation
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
}
