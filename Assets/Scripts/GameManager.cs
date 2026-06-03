using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int currentLevel = 1; // Starts at 1 by default upon installation

    private void Awake()
    {
        // Singleton pattern: ensures the object persists between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Tells Unity not to destroy this object
        }
        else
        {
            Destroy(gameObject); // Prevents duplicates when returning to MainMenu
        }
    }
}