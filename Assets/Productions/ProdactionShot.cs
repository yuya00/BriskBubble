using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdactionShot : MonoBehaviour
{
    public GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 親の位置に移動
        transform.position = parent.transform.position;

        if (parent.GetComponent<Enemy>().Shot_touch_flg)
            gameObject.SetActive(true);

    }
}
