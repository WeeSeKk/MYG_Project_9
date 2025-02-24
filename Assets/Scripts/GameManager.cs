using UnityEngine;
using DG.Tweening;
using Database;
using IHM;

public class GameManager : MonoBehaviour
{
    [SerializeField] Transform obstParent;
    [SerializeField] GameObject player;
    [SerializeField] GameObject cylinder;
    Quaternion quaternion;
    public static GameManager instance;
    int targetFrameRate = 165;
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
    }

    void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        quaternion = cylinder.transform.rotation;
        EventManager.gameOver += GameOver;
        EventManager.gameStart += StartGame;
        EventManager.scroreUP += CountScore;
        score = 0;
    }

    public void ResetGame(bool reset)
    {
        score = 0;
        IHMManager.instance.UpdateScoreCount(score);

        for (int i = 0; i < obstParent.childCount; i++)
        {
            GameObject child = obstParent.GetChild(i).gameObject;

            if (child.activeSelf == true)
            {
                ObjectPool.ReturnObjectToPool(child);
            }
        }

        cylinder.transform.rotation = quaternion;

        if (reset)
        {
            player.transform.position = new Vector3(0, 0, 0);
            player.transform.rotation = new Quaternion(0, 0, 0, 0);

            Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
            playerRigidbody.useGravity = true;
            EventManager.ResetGame();
        }
        else
        {
            player.SetActive(false);
        }
    }

    void StartGame()
    {
        player.transform.position = new Vector3(0, 0, 0);
        player.transform.rotation = new Quaternion(0, 0, 0, 0);

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        playerRigidbody.useGravity = true;
    }

    void CountScore()
    {
        score++;
        IHMManager.instance.UpdateScoreCount(score);
    }

    async void GameOver()
    {
        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        playerRigidbody.useGravity = false;
        DOTween.Clear();
        await DatabaseManager.instance.UpdatePlayerScore(score);
        await DatabaseManager.instance.UpdatePlayerScoreSQL(score);
    }
}
