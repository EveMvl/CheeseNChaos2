using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text ratCountText;
    public TMP_Text levelText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateRatUI(int current, int target)
    {
        if (ratCountText != null)
            ratCountText.text = $"{current}/{target}";
    }

    /*
    private void UpdateRatUI()
    {
        if (ratCountText != null)
        {
            int ratCount = 0;
            if (MouseManager.Instance != null)
                ratCount = MouseManager.Instance.MouseCount;

            ratCountText.text = ratCount + "/15";
        }
    }
    */

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(time);
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            timerText.text = $"{minutes:00}:{remainingSeconds:00}";
        }
    }
    public void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = $"Level {level}";
    }

    public void UpdateLevelText(string text)
    {
        if (levelText != null)
            levelText.text = text;
    }
}
