using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();
    /**
        <summary>
        <param> </param>
        <returns> Add gameobjects to the pool </returns>
        </summary>
    **/
    public static GameObject ObstcSpawn(GameObject gameObject, Vector3 spawnPos, Quaternion spawnRot)
    {
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookUpString == gameObject.name);

        if(pool == null)
        {
            pool = new PooledObjectInfo() {LookUpString = gameObject.name};
            ObjectPools.Add(pool);
        }

        GameObject spawnAbleObject = null;
        
        foreach (GameObject obj in pool.InactiveObjects)//look for inactive object in the pool 
        {
            if (obj != null)
            {
                spawnAbleObject = obj;
                break;
            }
        }

        if (spawnAbleObject == null)//if there is no inactive object then create one
        {
            spawnAbleObject = Instantiate(gameObject, spawnPos, spawnRot);
        }
        else//if there is an inactive object then reactive it
        {
            spawnAbleObject.transform.position = spawnPos;
            spawnAbleObject.transform.rotation = spawnRot;
            pool.InactiveObjects.Remove(spawnAbleObject);
            spawnAbleObject.SetActive(true);
        }
        return spawnAbleObject;
    }
    /**
        <summary>
        <param> </param>
        <returns> Remove gameobjects to the pool </returns>
        </summary>
    **/
    public static void ReturnObjectToPool(GameObject gameObject)
    {
        string goName = gameObject.name.Substring(0, gameObject.name.Length - 7);//removing the "(Clone)" frome the new instantiate gameobject

        PooledObjectInfo pool = ObjectPools.Find(p => p.LookUpString == goName);

        if(pool == null)
        {
            //Debug.LogWarning("ERROR " + gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            pool.InactiveObjects.Add(gameObject);
        }
    }
}

public class PooledObjectInfo
{
    public string LookUpString;
    public List<GameObject> InactiveObjects = new List<GameObject>();
}  