using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{

    private float time = 0;
    private float time_max = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Destroy();
    }

    void Destroy()
    {
        time += Time.deltaTime;
        if(time > time_max)
        {
            time = 0;
            Destroy(gameObject);
        }
    }

}
