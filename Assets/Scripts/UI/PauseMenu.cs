using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;       // Panel popup pause
    public Button resumeButton;         // Tombol Resume
    public Button backButton;           // Tombol Back ke MainMenu
    public Slider volumeSlider;         // Slider Volume
    public AudioSource bgmSource;       // (Opsional) AudioSource BGM

    public static bool isPaused = false;

    void Start()
    {
        // Panel popup disembunyikan dulu
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Pasang listener tombol Resume
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        // Pasang listener tombol Back
        if (backButton != null)
            backButton.onClick.AddListener(BackToTitleScreen);

        // Pasang listener slider volume
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeSlider.value = AudioListener.volume; // sync awal
        }
    }

    // ⚙️ Fungsi tombol Pause (dipanggil lewat ButtonPause)
    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused!");
    }

    // ▶️ Fungsi tombol Resume
    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game Resumed!");
    }

    // 🔙 Fungsi tombol Back ke menu utama
    public void BackToTitleScreen()
    {
        Time.timeScale = 1f;  // pastikan game unpause
        SceneManager.LoadScene("TitleScreen");
    }


    // 🎚️ Atur volume global & bgm
    public void SetVolume(float value)
    {
        AudioListener.volume = value;

        if (bgmSource != null)
            bgmSource.volume = value;

        Debug.Log($"Volume: {value}");
    }
}
