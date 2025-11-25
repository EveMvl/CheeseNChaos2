using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ClickToStart : MonoBehaviour
{
    public float delayBeforeClickable = 3f; // seconds before click is allowed
    private bool canClick = false;

    void Start()
    {
        Invoke(nameof(EnableClick), delayBeforeClickable);
    }

    void EnableClick()
    {
        canClick = true;
    }

    void Update()
    {
        if (!canClick) return;

        // Check for mouse left button press
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartGame();
        }

        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        Debug.Log("Game Started!");
        SceneManager.LoadScene("TutorialScene");
    }
}
