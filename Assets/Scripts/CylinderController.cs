using UnityEngine;

public class CylinderController : MonoBehaviour
{
    public Transform cynlinder;
    Vector3 rot;
    bool gameOver = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rot = new Vector3(- 0.1f,0,0);
        EventManager.gameOver += GameOver;
        EventManager.resetGame += ResetGame;
        EventManager.gameStart += ResetGame;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver) {
            cynlinder.Rotate(rot);
        }
    }

    void GameOver()
    {
        gameOver = true;
    }

    void ResetGame()
    {
        gameOver = false;
    }
}
