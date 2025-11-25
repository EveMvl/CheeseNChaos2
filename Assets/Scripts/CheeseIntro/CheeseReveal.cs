using UnityEngine;
using System.Collections;

public class CheeseReveal : MonoBehaviour
{
    [Header("References")]
    public GameObject cheese;              // the cheese mesh
    public Light cheeseLight;              // spotlight or point light
    public ParticleSystem cheeseParticles; // particle effect

    [Header("Timings")]
    public float cheeseDelay = 0f;     // when cheese appears
    public float lightDelay = 1f;      // when spotlight starts
    public float particleDelay = 1.5f; // when particles start

    [Header("Fade Durations")]
    public float cheeseFade = 1f;      // fade-in for cheese material
    public float lightFade = 2f;       // how long the spotlight ramps up
    public float lightIntensity = 4f;  // target spotlight intensity

    private Material[] cheeseMats;

    void Start()
    {
        // Grab all materials on the cheese mesh
        if (cheese != null)
        {
            Renderer rend = cheese.GetComponent<Renderer>();
            if (rend != null)
            {
                cheeseMats = rend.materials; // creates unique copies at runtime
                foreach (Material m in cheeseMats)
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color c = m.GetColor("_BaseColor");
                        c.a = 0f; // start invisible
                        m.SetColor("_BaseColor", c);
                    }
                }
            }
        }

        if (cheeseLight != null) cheeseLight.intensity = 0f;
        if (cheeseParticles != null) cheeseParticles.Stop();

        // Start the reveal sequence
        StartCoroutine(RevealSequence());
    }

    IEnumerator RevealSequence()
    {
        // 1. Cheese fade-in
        yield return new WaitForSeconds(cheeseDelay);
        if (cheeseMats != null)
            yield return StartCoroutine(FadeMaterialAlpha(0f, 1f, cheeseFade));

        // 2. Spotlight fade-in
        yield return new WaitForSeconds(lightDelay - cheeseDelay);
        if (cheeseLight != null)
            yield return StartCoroutine(FadeLightIntensity(0f, lightIntensity, lightFade));

        // 3. Particle effect
        yield return new WaitForSeconds(particleDelay - lightDelay);
        if (cheeseParticles != null)
            cheeseParticles.Play();
    }

    IEnumerator FadeMaterialAlpha(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            foreach (Material m in cheeseMats)
            {
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = alpha;
                    m.SetColor("_BaseColor", c);
                }
            }
            yield return null;
        }
    }

    IEnumerator FadeLightIntensity(float from, float to, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            cheeseLight.intensity = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
    }

    // Optional fade-out trigger you can call later
    public void HideCheese(float fadeDuration = 1f)
    {
        StartCoroutine(FadeMaterialAlpha(1f, 0f, fadeDuration));
        if (cheeseLight != null)
            StartCoroutine(FadeLightIntensity(cheeseLight.intensity, 0f, fadeDuration));
        if (cheeseParticles != null)
            cheeseParticles.Stop();
    }
}
