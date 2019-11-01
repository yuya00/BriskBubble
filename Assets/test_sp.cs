using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_sp : MonoBehaviour
{
    public Transform chara_ray;          //レイを飛ばす位置(地面判別に使用)
    public float len = 2.5f;
    public float min = 1;
    // Start is called before the first frame update
    void Start()
    {
        //chara_ray = transform.Find("CharaRay");
    }
    int dt = 0;
    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(chara_ray.position, chara_ray.position + new Vector3(0,-1,1) * (1 * len), Color.green);
        if(dt++ > 60 * min)
        {
            Destroy(gameObject);
            dt = 0;
        }
    }
}
