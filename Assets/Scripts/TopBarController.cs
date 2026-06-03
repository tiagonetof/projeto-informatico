using UnityEngine;
using TMPro;

public class TopBarController : MonoBehaviour
{
    // Reference to the Text component inside the level button
    public TMP_Text levelButtonText; 

    void Start()
    {
        // Fetches the current level from GameManager and updates the button text
        if (GameManager.Instance != null)
        {
            Debug.Log("Sucesso: Encontrei o GameManager! O nível é: " + GameManager.Instance.currentLevel);
            levelButtonText.text = "Level " + GameManager.Instance.currentLevel;
        }
        else
        {
            Debug.LogError("Erro: Não encontrei o GameManager! Começaste o jogo a partir do MainMenu?");
        }
    }

    // This function will be triggered when the user clicks the level button
    public void OpenInfoPopup()
    {
        Debug.Log("Level button clicked! Opening info popup...");
        // TODO: Add code to activate the popup UI panel here later
    }
}