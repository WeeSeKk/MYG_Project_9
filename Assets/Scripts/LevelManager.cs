using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<Transform> obstaclesPositionBottom;
    [SerializeField] List<Transform> obstaclesPositionTop;
    [SerializeField] Transform obstParent;
    [SerializeField] GameObject obstacleGO;
    [SerializeField] GameObject playerGO;
    static readonly Dictionary<Transform, int> obstcFrequenciesBottom = new Dictionary<Transform, int>();
    static readonly Dictionary<Transform, int> obstcFrequenciesTop = new Dictionary<Transform, int>();
    bool gameOver;

    void Start()
    {
        EventManager.gameOver += GameOver;
        EventManager.resetGame += ResetGame;
        InitializeObstcFrequenciesBottom();
        InitializeObstcFrequenciesTop();
    }

    public void StartGame()
    {
        gameOver = false;
        StartCoroutine(ObstaclesSpawner());
        EventManager.GameStart();
        playerGO.SetActive(true);
        
    }

    void InitializeObstcFrequenciesBottom()
    {
        obstcFrequenciesBottom.Clear();

        obstcFrequenciesBottom.Add(obstaclesPositionBottom[0], 25);//Obstc_LOW
        obstcFrequenciesBottom.Add(obstaclesPositionBottom[1], 50);//Obstc_MID
        obstcFrequenciesBottom.Add(obstaclesPositionBottom[2], 100);//Obstc_HIGH
    }

    void InitializeObstcFrequenciesTop()
    {
        obstcFrequenciesBottom.Clear();

        obstcFrequenciesTop.Add(obstaclesPositionTop[0], 100);//Obstc_LOW
        obstcFrequenciesTop.Add(obstaclesPositionTop[1], 100);//Obstc_MID
        obstcFrequenciesTop.Add(obstaclesPositionTop[2], 100);//Obstc_HIGH
    }

    public Transform GenerateObstcPositionBottom()
    {
        int totalWeight = 0;
        foreach (var weight in obstcFrequenciesBottom.Values)
        {
            totalWeight += weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        foreach (var obstc in obstcFrequenciesBottom.Keys)
        {
            if (randomValue < obstcFrequenciesBottom[obstc])
            {
                return obstc;
            }
            randomValue -= obstcFrequenciesBottom[obstc];
        }

        return obstaclesPositionBottom[0];//not supposed to happen
    }

    public Transform GenerateObstcPositionTop()
    {
        int totalWeight = 0;
        foreach (var weight in obstcFrequenciesTop.Values)
        {
            totalWeight += weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        foreach (var obstc in obstcFrequenciesTop.Keys)
        {
            if (randomValue < obstcFrequenciesTop[obstc])
            {
                return obstc;
            }
            randomValue -= obstcFrequenciesTop[obstc];
        }

        return obstaclesPositionTop[0];//not supposed to happen
    }

    public IEnumerator ObstaclesSpawner()
    {
        while(!gameOver)
        {
            Transform topObstaclePosition = GenerateObstcPositionTop();

            Vector3 topPos = topObstaclePosition.position;
            Quaternion toprot = topObstaclePosition.rotation;

            GameObject newObstcTop = ObjectPool.ObstcSpawn(obstacleGO, topPos, toprot);
            newObstcTop.transform.SetParent(obstParent);

            
                if (newObstcTop.transform.GetChild(0).name == "ScoreBoxCollider" && newObstcTop.transform.GetChild(0).gameObject.activeSelf == false) {
                    newObstcTop.transform.GetChild(0).gameObject.SetActive(true);
                }
            

            Transform bottomObstaclePosition = GenerateObstcPositionBottom();

            Vector3 bottomPos = bottomObstaclePosition.position;
            Quaternion bottomrot = bottomObstaclePosition.rotation;

            GameObject newObstcBottom = ObjectPool.ObstcSpawn(obstacleGO, bottomPos, bottomrot);
            newObstcBottom.transform.SetParent(obstParent);
            
            
                if (newObstcBottom.transform.GetChild(0).name == "ScoreBoxCollider" && newObstcBottom.transform.GetChild(0).gameObject.activeSelf == true) {
                    newObstcBottom.transform.GetChild(0).gameObject.SetActive(false);
                }
            
            
            yield return new WaitForSeconds(1f);
        }
    }

    void GameOver()
    {
        StopAllCoroutines();
        gameOver = true;
    }

    void ResetGame()
    {
        gameOver = false;
        StartCoroutine(ObstaclesSpawner());
    }
}
