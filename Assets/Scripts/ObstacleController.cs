using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ObstacleController : MonoBehaviour
{
    void OnEnable()
    {
        Invoke("ReturnToPool", 5f);
    }

    void ReturnToPool()
    {
        ObjectPool.ReturnObjectToPool(this.gameObject);
    }
}