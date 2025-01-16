using UnityEngine;

public class OnTriggerReturnToPool : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Obstacle") {
            ObjectPool.ReturnObjectToPool(other.gameObject);
        }
    }
}
