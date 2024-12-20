using UnityEngine;

public class IHMManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverScreenUI;
    public static IHMManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        //DontDestroyOnLoad(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventManager.gameOver += GameOverScreen;
        EventManager.resetGame += ResetGame;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GameOverScreen()
    {
        gameOverScreenUI.SetActive(true);
    }

    void ResetGame()
    {
        gameOverScreenUI.SetActive(false);
    }
}