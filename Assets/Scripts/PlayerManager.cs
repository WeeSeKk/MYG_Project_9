using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    public float jumpForce;
    bool gameOver;
    
    void Start()
    {
        EventManager.resetGame += ResetVelocity;
        EventManager.gameOver += GameOver;
        EventManager.gameStart += ResetGame;
        EventManager.resetGame += ResetGame;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && !gameOver)
        {
            _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Obstacle") {
            EventManager.GameOver();
            Debug.Log("HIT");
        }
    }

    void ResetVelocity()
    {
        _rigidbody.linearVelocity = Vector2.zero;
        Debug.Log("reset velocity");
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