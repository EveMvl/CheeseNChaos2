using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheesePiece : MonoBehaviour
{
    [Header("Eating Settings")]
    public int requiredEaters = 2;          // Minimum mice to eat
    public float eatDuration = 3f;          // Time it takes to eat
    public float lonelyTimeout = 8f;        // Time before single mouse gives up
    public GameObject eatEffectPrefab;      // Optional particle effect

    [Header("Spoil Settings")]
    public float spoilTime = 15f;           // Seconds until cheese goes bad
    public float fadeDuration = 2f;         // Fade-out time before disappearing
    public GameObject spoilEffectPrefab;    // Optional rot/disappear effect

    private List<MouseAgent> currentEaters = new List<MouseAgent>();
    private bool isBeingEaten = false;
    private bool isSpoiled = false;
    private Renderer cheeseRenderer;
    private Coroutine spoilCoroutine;
    private Coroutine lonelyCoroutine;

    void Start()
    {
        cheeseRenderer = GetComponentInChildren<Renderer>();
        spoilCoroutine = StartCoroutine(SpoilTimer());
    }

    public void AddEater(MouseAgent mouse)
    {
        if (isSpoiled || isBeingEaten) return;
        if (currentEaters.Contains(mouse)) return;

        currentEaters.Add(mouse);
        Debug.Log($"🧀 Eater added: {mouse.name} ({currentEaters.Count}/{requiredEaters})");

        // Cancel lonely wait if enough mice
        if (lonelyCoroutine != null && currentEaters.Count >= requiredEaters)
        {
            StopCoroutine(lonelyCoroutine);
            lonelyCoroutine = null;
        }

        // Start eating if enough mice
        if (!isBeingEaten)
        {
            if (currentEaters.Count >= requiredEaters)
            {
                // Free extra mice beyond requiredEaters
                if (currentEaters.Count > requiredEaters)
                {
                    for (int i = requiredEaters; i < currentEaters.Count; i++)
                    {
                        MouseAgent extra = currentEaters[i];
                        if (extra != null)
                            extra.OnCheeseGone(this);
                    }
                    currentEaters = currentEaters.GetRange(0, requiredEaters);
                }

                // Stop spoil timer
                if (spoilCoroutine != null)
                {
                    StopCoroutine(spoilCoroutine);
                    spoilCoroutine = null;
                }

                // 🔔 Notify *all other* mice that this cheese is now taken
                NotifyAllMiceCheeseTaken();

                StartCoroutine(EatCoroutine());
            }
            else
            {
                // Wait for partner
                if (lonelyCoroutine == null)
                    lonelyCoroutine = StartCoroutine(LonelyMouseWait());
            }
        }
    }

    private IEnumerator LonelyMouseWait()
    {
        yield return new WaitForSeconds(lonelyTimeout);

        if (currentEaters.Count < requiredEaters)
        {
            Debug.Log("😿 Not enough eaters — mice give up and move on.");

            foreach (var mouse in currentEaters)
            {
                if (mouse != null)
                    mouse.OnCheeseGone(this);
            }

            currentEaters.Clear();
            Debug.Log($"🚫 Cheese {name}: clearing all eaters (count {currentEaters.Count})");

            lonelyCoroutine = null;
        }
    }

    public void RemoveEater(MouseAgent mouse)
    {
        if (currentEaters.Contains(mouse))
        {
            currentEaters.Remove(mouse);
            Debug.Log($"🐭 Eater removed: {mouse.name} ({currentEaters.Count}/{requiredEaters})");
        }
    }

private IEnumerator EatCoroutine()
{
    isBeingEaten = true;
    Debug.Log($"🍽️ Cheese at {transform.position} is being eaten by {currentEaters.Count} mice...");

    if (eatEffectPrefab != null)
        Instantiate(eatEffectPrefab, transform.position, Quaternion.identity);

    yield return new WaitForSeconds(eatDuration);

    // Notify the current eaters
    foreach (var mouse in currentEaters)
    {
        if (mouse != null)
        {
            Debug.Log($"✅ {mouse.name} finished eating.");
            mouse.OnFinishedEating(this);
        }
    }

    // 🔔 Tell everyone else this cheese is gone (resume wandering)
    MouseAgent[] allMice = FindObjectsByType<MouseAgent>(FindObjectsSortMode.None);
    foreach (var m in allMice)
    {
        if (m != null && !currentEaters.Contains(m))
        {
            m.OnCheeseGone(this); // reset their states + wander again
        }
    }

    CheeseManager.RemoveCheese(transform);
    Destroy(gameObject);
}


    private IEnumerator SpoilTimer()
    {
        yield return new WaitForSeconds(spoilTime);

        if (!isBeingEaten)
        {
            isSpoiled = true;
            Debug.Log($"💀 Cheese at {transform.position} spoiled and fading away...");

            if (spoilEffectPrefab != null)
                Instantiate(spoilEffectPrefab, transform.position, Quaternion.identity);

            yield return StartCoroutine(FadeAndDestroy());
        }
    }

    private void NotifyEatersOnSpoil()
    {
        foreach (var mouse in currentEaters)
        {
            if (mouse != null)
                mouse.OnCheeseGone(this);
        }
        currentEaters.Clear();
        Debug.Log($"🚫 Cheese {name}: clearing all eaters (count {currentEaters.Count})");

    }

    /// <summary>
    /// Notify all mice in the scene that this cheese is now taken
    /// (so they don't freeze or try to join mid-bite)
    /// </summary>
    private void NotifyAllMiceCheeseTaken()
    {
        MouseAgent[] allMice = FindObjectsByType<MouseAgent>(FindObjectsSortMode.None);
        foreach (var mouse in allMice)
        {
            if (mouse == null) continue;
            if (!currentEaters.Contains(mouse))
            {
                mouse.OnCheeseGone(this);
            }
        }
    }

    private IEnumerator FadeAndDestroy()
    {
        if (cheeseRenderer != null)
        {
            Material mat = cheeseRenderer.material;
            Color originalColor = mat.color;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // 🧀 Notify mice that cheese is gone
        NotifyEatersOnSpoil();

        // 🐭 Resume wandering for all mice in scene
        MouseAgent[] allMice = FindObjectsByType<MouseAgent>(FindObjectsSortMode.None);
        foreach (var mouse in allMice)
        {
            if (mouse != null)
            {
                // Make sure they resume wandering if idle
                mouse.ResumeWanderingAfterSpoil();
            }
        }

        CheeseManager.RemoveCheese(transform);
        Destroy(gameObject);
    }
}
