using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TutorialSequence : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How long to wait before allowing click (in seconds)")]
    public float delayBeforeClickable = 3f;

    private bool canClick = false;

    void Start()
    {
        // Wait a bit before allowing player to click
        Invoke(nameof(EnableClick), delayBeforeClickable);
    }

    void EnableClick()
    {
        canClick = true;
    }

    void Update()
    {
        if (!canClick) return;

        // ✅ Handle both mouse and touch input
        if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
        {
            BeginGame();
        }
    }

    private void BeginGame()
    {
        Debug.Log("🎬 Tutorial complete — loading main scene...");
        SceneManager.LoadScene("Scene1"); // Make sure "Screen1" is added in Build Settings
    }
}
