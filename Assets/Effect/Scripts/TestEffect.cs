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
            Apper();
            app_timer = 0;
        }
    }

    void Apper()
    {
        for (int i = 0; i < EFFECT_NUM; ++i)
        {
            Instantiate(effect, transform.position, effect.transform.rotation);
        }
    }


}
