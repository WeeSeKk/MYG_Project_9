using UnityEngine;
using DG.Tweening;

public class ObstacleController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        transform.DOMoveX(-15f, 2f).SetEase(Ease.Linear).OnComplete(() => {

            this.gameObject.transform.DOKill();
            ObjectPool.ReturnObjectToPool(this.gameObject);
            //Destroy(this.gameObject);
        });
    }
}