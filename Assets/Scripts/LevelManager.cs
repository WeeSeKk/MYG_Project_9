using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<GameObject> obstacles;
    [SerializeField] Transform obstParent;
    [SerializeField] Transform obstSpanwPosBottom;
    [SerializeField] Transform obstSpanwPosTop;
    static readonly Dictionary<GameObject, int> obstcFrequencies = new Dictionary<GameObject, int>();
    bool gameOver;

    void Start()
    {
        EventManager.gameOver += GameOver;
        EventManager.resetGame += ResetGame;
        InitializeObstcFrequencies();
        StartCoroutine(ObstaclesSpawner());
    }
    void InitializeObstcFrequencies()
    {
        obstcFrequencies.Clear();

        obstcFrequencies.Add(obstacles[0], 100);//Obstc_LOW
        obstcFrequencies.Add(obstacles[1], 100);//Obstc_MID
        obstcFrequencies.Add(obstacles[2], 100);//Obstc_HIGH
    }

    public GameObject GenerateObstc()
    {
        int totalWeight = 0;
        foreach (var weight in obstcFrequencies.Values)
        {
            totalWeight += weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        foreach (var obstc in obstcFrequencies.Keys)
        {
            if (randomValue < obstcFrequencies[obstc])
            {
                return obstc;
            }
            randomValue -= obstcFrequencies[obstc];
        }

        return obstacles[0];//not supposed to happen
    }

    public IEnumerator ObstaclesSpawner()
    {
        while(!gameOver)//top y 14 // bottom y -9
        {
            GameObject topObstacle = GenerateObstc();

            //Vector3 topPos = new Vector3(15, 14, 0);
            //Quaternion toprot = new Quaternion(0, 0, 180, 0);

            Vector3 topPos = obstSpanwPosTop.position;
            Quaternion toprot = obstSpanwPosTop.rotation;

            GameObject newObstcTop = ObjectPool.ObstcSpawn(topObstacle, topPos, toprot);
            newObstcTop.transform.SetParent(obstParent);

            for (int i = 0; i < newObstcTop.transform.childCount; i ++) {
                if (newObstcTop.transform.GetChild(i).name == "ScoreCollider" && newObstcTop.transform.GetChild(i).gameObject.activeSelf == false) {
                    newObstcTop.transform.GetChild(i).gameObject.SetActive(true);
                }
            }

            //if obstc == high reduce chance of another high spawning if low difficulty

            GameObject bottomObstacle = GenerateObstc();

            //Vector3 bottomPos = new Vector3(15, -9, 0);
            //Quaternion bottomrot = new Quaternion(0, 0, 0, 0);

            Vector3 bottomPos = obstSpanwPosBottom.position;
            Quaternion bottomrot = obstSpanwPosBottom.rotation;

            GameObject newObstcBottom = ObjectPool.ObstcSpawn(bottomObstacle, bottomPos, bottomrot);
            newObstcBottom.transform.SetParent(obstParent);
            
            for (int i = 0; i < newObstcBottom.transform.childCount; i ++) {
                if (newObstcBottom.transform.GetChild(i).name == "ScoreCollider" && newObstcBottom.transform.GetChild(i).gameObject.activeSelf == true) {
                    newObstcBottom.transform.GetChild(i).gameObject.SetActive(false);
                }
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
