using UnityEngine;
using TMPro;
using System.Collections;

public class LevelUpPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TextMeshProUGUI levelText;
    public float showDuration = 2.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip levelUpSFX;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void ShowLevelUp(int newLevel)
    {
        if (popupPanel == null) return;

        // kalau ada coroutine lama, stop biar gak tumpuk
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // play sfx
        if (audioSource != null && levelUpSFX != null)
            audioSource.PlayOneShot(levelUpSFX);

        popupPanel.SetActive(true);
        if (levelText != null)
            levelText.text = $"LEVEL UP! #{newLevel}";

        // langsung mulai coroutine tanpa freeze time
        currentRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration); // real-time tetap jalan karena timeScale tetap 1
        popupPanel.SetActive(false);
        currentRoutine = null;
    }
}
