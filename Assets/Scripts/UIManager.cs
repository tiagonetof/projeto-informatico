using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    
    // Method to switch scenes
    public void LoadARScene()
    {
        Debug.Log("O botão foi clicado com sucesso!");
        SceneManager.LoadScene("AR");
    }
}