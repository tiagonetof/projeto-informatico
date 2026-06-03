using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    
    // Method to switch scenes
    public void LoadMainMenuScene()
    {
        Debug.Log("O botão foi clicado com sucesso!");
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadARScene()
    {
        Debug.Log("O botão foi clicado com sucesso!");
        SceneManager.LoadScene("AR");
    }

    public void LoadInventoryScene()
    {
        Debug.Log("O botão foi clicado com sucesso!");
        SceneManager.LoadScene("Inventory");
    }

    public void LoadSettingsScene()
    {
        Debug.Log("O botão foi clicado com sucesso!");
        SceneManager.LoadScene("Settings");
    }
}