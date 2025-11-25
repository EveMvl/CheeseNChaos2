using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseButton : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene"); // ganti sesuai nama scene menu
    }
}
