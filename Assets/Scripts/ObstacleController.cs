using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ObstacleController : MonoBehaviour
{
    void OnEnable()
    {
        Invoke("ReturnToPool", 7f);
    }

    void ReturnToPool()
    {
        ObjectPool.ReturnObjectToPool(this.gameObject);
    }
}