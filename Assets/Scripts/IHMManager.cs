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
        EventManager.resetGame += GameOverScreen;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GameOverScreen()
    {
        if (gameOverScreenUI.activeSelf == false) {
            gameOverScreenUI.SetActive(true);
        }
        else {
            gameOverScreenUI.SetActive(false);
        }
    }
}