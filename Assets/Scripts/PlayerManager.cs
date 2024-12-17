using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Rigidbody _rigidbody;
    public float jumpForce;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("JUMP!");
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
}