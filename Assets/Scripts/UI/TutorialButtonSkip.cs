using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipButton : MonoBehaviour
{
    public void SkipToGame()
    {
        SceneManager.LoadScene("Scene1"); 
    }
}
