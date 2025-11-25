using UnityEngine;
using UnityEngine.UI;

public class ClickSound : MonoBehaviour
{
    public AudioSource sfxSource;

    public void PlayClickSound()
    {
        Debug.Log("SFX clicked!");
        sfxSource.Play();
    }
}