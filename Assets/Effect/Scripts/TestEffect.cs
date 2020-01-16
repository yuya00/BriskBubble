using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*
 * 
 * 
 *                          ここでエフェクトの挙動を確認する
 *      
 *      
 */

public class TestEffect : MonoBehaviour
{
    public GameObject effect;

    //private GameObject player;

    private float app_timer;
    public float app_timer_max = 1;

    public int EFFECT_NUM = 10;

    public int state = 1;
    public int step = 3;

    // Start is called before the first frame update
    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player");
        app_timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Effect();
    }

    void Effect()
    {
        app_timer += Time.deltaTime;
        if (app_timer > app_timer_max)
        {
            Test();
            app_timer = 0;
        }
    }

    void Test()
    {
        switch(state)
        {
            case 0:
                Apper();    // 位置設定しやんとき
                break;
            case 1:
                Ap2(transform.position);
                break;
        }
    }

    void Apper()
    {
        for (int i = 0; i < EFFECT_NUM; ++i)
        {
            Instantiate(effect, transform.position, effect.transform.rotation);
        }
    }

    void Ap2(Vector3 pos)
    {

        for (int i = 0; i < EFFECT_NUM; ++i)
        {
            for (int j = 0; j < step; ++j)
            {
                Instantiate(effect, new Vector3(pos.x, (pos.y - (step * 0.5f)) + j, pos.z), effect.transform.rotation);
            }
        }
    }


}
