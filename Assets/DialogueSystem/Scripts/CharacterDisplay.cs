using UnityEngine;
using Yarn.Unity;

public class CharacterCommands : MonoBehaviour
{
    
[YarnCommand("showCharacter")]
public void ShowCharacter(string characterName) {
    if (characterName == name) {
        gameObject.SetActive(true);
        Debug.Log($"{characterName} is now shown!");
    }
}


    [YarnCommand("hideCharacter")]
    public void HideCharacter(string characterName)
    {
        if (characterName == name)
        {
            gameObject.SetActive(false);
            Debug.Log($"{characterName} is now hidden!");
        }
    }
}
