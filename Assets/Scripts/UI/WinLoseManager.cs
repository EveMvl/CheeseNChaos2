using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance;

    [Header("UI References")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip winSFX;
    public AudioClip loseSFX;

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Pastikan pop-up gak muncul pas awal main
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    /// <summary>
    /// Method lama, biar script lain gak error.
    /// </summary>
    public void ShowWinLose(bool isWin)
    {
        ShowResult(isWin);
    }

    /// <summary>
    /// Versi baru yang dipanggil internal.
    /// </summary>
    public void ShowResult(bool isWin)
    {
        if (gameEnded) return;
        gameEnded = true;

        // Pause game

        if (isWin)
        {
            if (winPanel != null) winPanel.SetActive(true);
            if (audioSource != null && winSFX != null)
                audioSource.PlayOneShot(winSFX);
        }
        else
        {
            if (losePanel != null) losePanel.SetActive(true);
            if (audioSource != null && loseSFX != null)
                audioSource.PlayOneShot(loseSFX);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScreen"); // ubah sesuai nama scene title lo
    }
}
