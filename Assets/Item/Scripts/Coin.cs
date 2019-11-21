using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Coin : MonoBehaviour
{
    public Vector3 rot = new Vector3(30, 80, 30);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Rotator();
    }

    void Rotator()
    {
        // 一定の角度で回転させる
        transform.Rotate(new Vector3(rot.x, rot.y, rot.z) * Time.deltaTime);
    }
}
