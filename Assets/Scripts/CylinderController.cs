using UnityEngine;

public class CylinderController : MonoBehaviour
{
    public Transform cynlinder;
    Vector3 rot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rot = new Vector3(- 0.04f,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        cynlinder.Rotate(rot);
    }
}
