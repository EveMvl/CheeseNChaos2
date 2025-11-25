using UnityEngine;
using Yarn.Unity;

public class YarnCommands : MonoBehaviour
{
    [YarnCommand("showCharacter")]
    public static void ShowCharacter(string characterName)
    {
        var character = GameObject.Find(characterName);

        if (character != null)
        {
            character.SetActive(true);
            Debug.Log($"Menampilkan karakter: {characterName}");
        }
        else
        {
            Debug.LogError($"Character '{characterName}' tidak ditemukan di scene!");
        }
    }
}
