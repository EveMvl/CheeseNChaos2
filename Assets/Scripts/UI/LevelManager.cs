using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int[] ratTargets = { 15, 25, 39 }; // target tiap level
    public float levelDuration = 120f;        // waktu tambahan tiap level

    [Header("Runtime State")]
    private int currentRats = 0;
    private float gameTimer;
    private bool isPlaying = true;

    [Header("References")]
    public WinLoseManager winLoseManager;
    public LevelUpPopup levelUpPopup;
    public AudioSource audioSource;
    public AudioClip levelUpSFX;
    public AudioClip winSFX;
    public AudioClip loseSFX;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentRats = 0;
        gameTimer = levelDuration;
        isPlaying = true;

        UIManager.Instance.UpdateRatUI(currentRats, GetCurrentTarget());
        UIManager.Instance.UpdateTimer(gameTimer);
        UIManager.Instance.UpdateLevelText(currentLevel);
    }

    private void Update()
    {
        if (!isPlaying) return;

        gameTimer -= Time.deltaTime;
        UIManager.Instance.UpdateTimer(gameTimer);

        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            isPlaying = false;

            if (audioSource && loseSFX) audioSource.PlayOneShot(loseSFX);
            winLoseManager?.ShowWinLose(false);
        }
    }

    public void OnRatCollected()
    {
        if (!isPlaying) return;

        // pastikan tidak double count
        currentRats = Mathf.Min(currentRats + 1, GetCurrentTarget());
        UIManager.Instance.UpdateRatUI(currentRats, GetCurrentTarget());

        if (currentRats >= GetCurrentTarget())
            NextLevel();
    }

    private int GetCurrentTarget()
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, ratTargets.Length - 1);
        return ratTargets[index];
    }

private void NextLevel()
{
    currentLevel++;

    if (currentLevel > ratTargets.Length)
    {
        // Level terakhir
        isPlaying = false;
        if (audioSource && winSFX) audioSource.PlayOneShot(winSFX);
        UIManager.Instance.UpdateLevelText("END"); // string
        winLoseManager?.ShowWinLose(true);
        return;
    }

    // LevelUp Popup
    levelUpPopup?.ShowLevelUp(currentLevel);
    if (audioSource && levelUpSFX) audioSource.PlayOneShot(levelUpSFX);

    // Tambahkan waktu 120 detik + sisa timer sebelumnya
    gameTimer += levelDuration;

    // reset tikus level baru
    currentRats = 0;

    // Update UI
    UIManager.Instance.UpdateRatUI(currentRats, GetCurrentTarget());
    UIManager.Instance.UpdateTimer(gameTimer);
    UIManager.Instance.UpdateLevelText(currentLevel); // int untuk level normal

    Debug.Log($"🚀 Level {currentLevel} - Target: {GetCurrentTarget()}, Time: {gameTimer}");
}


    public void PauseGame(bool pause)
    {
        isPlaying = !pause;
    }

    public bool IsPlaying() => isPlaying;
}
