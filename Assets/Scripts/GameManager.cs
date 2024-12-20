using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] Transform obstParent;
    [SerializeField] GameObject player;
    public static GameManager instance;
    int score;

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

    void Start()
    {
        EventManager.gameOver += GameOver;
        EventManager.scroreUP += CountScore;
        score = 0;
    }
    
    void Update()
    {
        
    }

    public void ResetGame()
    {
        score = 0;

        for (int i = 0; i < obstParent.childCount; i ++)
        {
            GameObject child = obstParent.GetChild(i).gameObject;

            if (child.activeSelf == true) {
                ObjectPool.ReturnObjectToPool(child);
            }
        }

        player.transform.position = new Vector3(0,0,0);
        player.transform.rotation = new Quaternion(0,0,0,0);

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        playerRigidbody.useGravity = true;

        EventManager.ResetGame();
    }

    void CountScore()
    {
        score ++;
        Debug.Log("Score == " + score);
    }

    void GameOver()
    {
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        playerRigidbody.useGravity = false;
        DOTween.Clear();
    }
}
