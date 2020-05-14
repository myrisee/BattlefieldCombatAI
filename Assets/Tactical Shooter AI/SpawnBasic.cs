using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBasic : MonoBehaviour
{
    public GameObject prefab;

    public float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Spawn", 1, spawnTime);
    }


    void Spawn()
    {
        Instantiate(prefab, this.transform.position, Quaternion.identity);
    }


}
