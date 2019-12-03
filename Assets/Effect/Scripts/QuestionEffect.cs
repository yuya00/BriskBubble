using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class QuestionEffect : MonoBehaviour
{
    private float destroy_timer;              // 壊すまでの時間
    public float destroy_timer_max = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        destroy_timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(destroy_timer_max);
    }

    void Destroy(float timer_max)
    {
        destroy_timer += Time.deltaTime;
        if (destroy_timer > timer_max)
        {
            Destroy(gameObject);
        }
    }
}