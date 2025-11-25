using UnityEngine;
using TMPro;

public class TitleTextFade3D : MonoBehaviour
{
    public TextMeshPro titleText; // <-- 3D TextMeshPro
    public float delayBeforeFade = 2f;
    public float fadeDuration = 2f;

    void Start()
    {
        if (titleText != null)
        {
            Color c = titleText.color;
            c.a = 0f;
            titleText.color = c;
            Invoke(nameof(StartFadeIn), delayBeforeFade);
        }
    }

    void StartFadeIn()
    {
        StartCoroutine(FadeInText());
    }

    System.Collections.IEnumerator FadeInText()
    {
        float t = 0f;
        Color c = titleText.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            titleText.color = c;
            yield return null;
        }
        c.a = 1f;
        titleText.color = c;
    }
}
